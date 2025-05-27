using Microsoft.AspNetCore.Mvc;
using TheMatch.Models;
using TheMatch.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using TheMatch.Models.Dtos;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.Data.SqlClient;
using BCrypt.Net;

namespace TheMatch.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountApiController : ControllerBase
    {
        private readonly TheMatchContext _context;

        public AccountApiController(TheMatchContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _context.Пользователи.AnyAsync(u => u.ЭлектроннаяПочта == dto.ЭлектроннаяПочта))
                return BadRequest("Пользователь с такой почтой уже существует");

            var user = new Пользователи
            {
                Имя = dto.Имя,
                Пол = dto.Пол,
                Рост = dto.Рост,
                ДатаРождения = dto.ДатаРождения,
                Местоположение = dto.Местоположение,
                Описание = dto.Описание,
                УровеньЗаработка = dto.УровеньЗаработка,
                Жильё = dto.Жильё,
                НаличиеДетей = dto.НаличиеДетей,
                ЭлектроннаяПочта = dto.ЭлектроннаяПочта,
                Пароль = BCrypt.Net.BCrypt.HashPassword(dto.Пароль)
            };

            try
            {
                _context.Пользователи.Add(user);
                await _context.SaveChangesAsync();
                return Ok("Пользователь успешно зарегистрирован");
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message;
                return BadRequest($"Ошибка при сохранении: {ex.Message} {(inner != null ? "| SQL: " + inner : "")}");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _context.Пользователи
                .FirstOrDefaultAsync(u => u.ЭлектроннаяПочта == dto.ЭлектроннаяПочта);
            if (user == null)
                return Unauthorized("Пользователь не найден");

            if (!BCrypt.Net.BCrypt.Verify(dto.Пароль, user.Пароль))
                return Unauthorized("Неверный пароль");

            // Создаём куку
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.IdПользователя.ToString()),
                new Claim(ClaimTypes.Name, user.Имя),
                new Claim(ClaimTypes.Email, user.ЭлектроннаяПочта)
            };
            var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true // кука будет жить между сессиями браузера
            };

            await HttpContext.SignInAsync("MyCookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

            return Ok("Вход выполнен успешно");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return Ok("Выход выполнен");
        }

        [HttpGet("profileinfo")]
        public async Task<IActionResult> GetProfileInfo()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return Unauthorized();
            var user = await _context.Пользователи.FirstOrDefaultAsync(u => u.ЭлектроннаяПочта == email);
            if (user == null) return NotFound();

            // Получаем интересы через представление
            var interests = await _context.ПользователиНазванияУвлечений
                .Where(x => x.IdПользователя == user.IdПользователя)
                .Select(x => x.НазваниеУвлечения)
                .ToListAsync();

            var dto = new ProfileInfoDto
            {
                Имя = user.Имя,
                Рост = user.Рост,
                ДатаРождения = user.ДатаРождения.ToDateTime(TimeOnly.MinValue),
                Местоположение = user.Местоположение,
                Описание = user.Описание,
                УровеньЗаработка = user.УровеньЗаработка,
                Жильё = user.Жильё,
                НаличиеДетей = user.НаличиеДетей ? "Есть" : "Нет",
                Интересы = interests
            };
            return Ok(dto);
        }

        [HttpPost("profileinfo")]
        public async Task<IActionResult> UpdateProfileInfo([FromBody] ProfileInfoDto dto)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return Unauthorized();
            var user = await _context.Пользователи.FirstOrDefaultAsync(u => u.ЭлектроннаяПочта == email);
            if (user == null) return NotFound();

            user.Имя = dto.Имя;
            user.Рост = dto.Рост;
            user.ДатаРождения = dto.ДатаРождения.HasValue
                ? DateOnly.FromDateTime(dto.ДатаРождения.Value)
                : user.ДатаРождения;
            user.Местоположение = dto.Местоположение;
            user.Описание = dto.Описание;
            user.УровеньЗаработка = dto.УровеньЗаработка;
            user.Жильё = dto.Жильё;
            user.НаличиеДетей = dto.НаличиеДетей == "Есть";

            // --- Обновление интересов ---
            if (dto.Интересы != null)
            {
                var allHobbies = await _context.Увлечения
                    .Where(h => dto.Интересы.Contains(h.НазваниеУвлечения))
                    .ToListAsync();
                var oldLinks = _context.УвлеченияПользователяs.Where(x => x.ID_Пользователя == user.IdПользователя);
                _context.УвлеченияПользователяs.RemoveRange(oldLinks);
                foreach (var hobby in allHobbies)
                {
                    _context.УвлеченияПользователяs.Add(new УвлеченияПользователя
                    {
                        ID_Пользователя = user.IdПользователя,
                        ID_Увлечения = hobby.IdУвлечения
                    });
                }
            }
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("allhobbies")]
        public async Task<IActionResult> GetAllHobbies()
        {
            var hobbies = await _context.Увлечения
                .Select(x => x.НазваниеУвлечения)
                .ToListAsync();
            return Ok(hobbies);
        }

        [HttpPost("uploadprofilephoto")]
        public async Task<IActionResult> UploadProfilePhoto()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return Unauthorized();
            var user = await _context.Пользователи.FirstOrDefaultAsync(u => u.ЭлектроннаяПочта == email);
            if (user == null) return NotFound();

            var file = Request.Form.Files.FirstOrDefault();
            if (file == null || file.Length == 0)
                return BadRequest("Файл не выбран");
            if (file.Length > 2 * 1024 * 1024)
                return BadRequest("Максимальный размер файла 2 МБ");

            // Проверка размера изображения (только 265x350) -- удалено, оставлена только JS-проверка

            var ext = System.IO.Path.GetExtension(file.FileName).ToLower();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                return BadRequest("Только JPG/PNG");

            // Определяем минимальный свободный номер фото (1 или 2)
            var userPhotos = await _context.ИзображенияПрофиля.Where(x => x.ID_Пользователя == user.IdПользователя).ToListAsync();
            var usedNums = userPhotos.Select(x => {
                var fname = System.IO.Path.GetFileNameWithoutExtension(x.Ссылка);
                var parts = fname.Split('_');
                if (parts.Length == 2 && int.TryParse(parts[1], out int n)) return n; else return 0;
            }).Where(n => n > 0).ToList();
            int nextNum = Enumerable.Range(1, 2).First(n => !usedNums.Contains(n));
            var fileName = $"{user.IdПользователя}_{nextNum}{ext}";
            var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles", fileName);
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            // Сбросить "основное" у других фото
            foreach (var p in userPhotos) p.Основное = false;
            // Добавить запись
            var photo = new ИзображенияПрофиля
            {
                ID_Пользователя = user.IdПользователя,
                Ссылка = $"/images/profiles/{fileName}",
                Основное = true
            };
            _context.ИзображенияПрофиля.Add(photo);
            await _context.SaveChangesAsync();
            return Ok(new { url = photo.Ссылка });
        }

        [HttpGet("profilephoto")]
        public async Task<IActionResult> GetProfilePhoto()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return Unauthorized();
            var user = await _context.Пользователи.FirstOrDefaultAsync(u => u.ЭлектроннаяПочта == email);
            if (user == null) return NotFound();
            var photo = await _context.ИзображенияПрофиля.FirstOrDefaultAsync(x => x.ID_Пользователя == user.IdПользователя && x.Основное);
            if (photo == null)
                return Ok(new { url = "/images/avatars/standart.jpg" }); // default
            return Ok(new { url = photo.Ссылка });
        }

        [HttpGet("profilephotos")]
        public async Task<IActionResult> GetProfilePhotos()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return Unauthorized();
            var user = await _context.Пользователи.FirstOrDefaultAsync(u => u.ЭлектроннаяПочта == email);
            if (user == null) return NotFound();
            var photos = await _context.ИзображенияПрофиля
                .Where(x => x.ID_Пользователя == user.IdПользователя)
                .OrderBy(x => x.ID_Изображения)
                .Select(x => new { id = x.ID_Изображения, url = x.Ссылка, isMain = x.Основное })
                .ToListAsync();
            return Ok(photos);
        }

        [HttpPost("deleteprofilephoto")]
        public async Task<IActionResult> DeleteProfilePhoto([FromBody] int photoId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return Unauthorized();
            var user = await _context.Пользователи.FirstOrDefaultAsync(u => u.ЭлектроннаяПочта == email);
            if (user == null) return NotFound();
            var photo = await _context.ИзображенияПрофиля.FirstOrDefaultAsync(x => x.ID_Изображения == photoId && x.ID_Пользователя == user.IdПользователя);
            if (photo == null) return NotFound();
            // Удаляем файл физически
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", photo.Ссылка.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
            _context.ИзображенияПрофиля.Remove(photo);
            await _context.SaveChangesAsync();
            // Если удалили основное фото, сделать основным другое (если есть)
            var other = await _context.ИзображенияПрофиля.FirstOrDefaultAsync(x => x.ID_Пользователя == user.IdПользователя);
            if (other != null && !other.Основное)
            {
                other.Основное = true;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return Unauthorized();
            var user = await _context.Пользователи.FirstOrDefaultAsync(u => u.ЭлектроннаяПочта == email);
            if (user == null) return NotFound();
            if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.Пароль))
                return BadRequest("Старый пароль неверен");
            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest("Пароли не совпадают");
            if (dto.NewPassword.Length < 6)
                return BadRequest("Пароль должен быть не короче 6 символов");
            user.Пароль = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("deleteaccount")]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto dto)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return Unauthorized();
            var user = await _context.Пользователи.FirstOrDefaultAsync(u => u.ЭлектроннаяПочта == email);
            if (user == null) return NotFound();
            // Можно сохранить причину удаления в отдельную таблицу, если нужно
            _context.Пользователи.Remove(user);
            await _context.SaveChangesAsync();
            await HttpContext.SignOutAsync("MyCookieAuth");
            return Ok();
        }

        [HttpGet("alltraits")]
        public async Task<IActionResult> GetAllTraits()
        {
            var traits = await _context.ЧертыХарактера
                .Select(x => new { id = x.IdЧертыХарактера, name = x.НазваниеЧерты })
                .ToListAsync();
            return Ok(traits);
        }

        public class TraitValueDto
        {
            public byte TraitId { get; set; }
            public decimal Value { get; set; }
        }

        [HttpPost("savetesttraits")]
        public async Task<IActionResult> SaveTestTraits([FromBody] List<TraitValueDto> traits)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return Unauthorized();
            var user = await _context.Пользователи.FirstOrDefaultAsync(u => u.ЭлектроннаяПочта == email);
            if (user == null) return NotFound();

            foreach (var t in traits)
            {
                var existing = await _context.ЧертыПользователя.FirstOrDefaultAsync(x => x.IdПользователя == user.IdПользователя && x.IdЧертыХарактера == t.TraitId);
                if (existing != null)
                {
                    existing.Значение = t.Value;
                    _context.Entry(existing).State = EntityState.Modified;
                }
                else
                {
                    _context.ЧертыПользователя.Add(new Models.ЧертыПользователя
                    {
                        IdПользователя = user.IdПользователя,
                        IdЧертыХарактера = t.TraitId,
                        Значение = t.Value
                    });
                }
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("usertraits")]
        public async Task<IActionResult> GetUserTraits()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return Unauthorized();
            var user = await _context.Пользователи.FirstOrDefaultAsync(u => u.ЭлектроннаяПочта == email);
            if (user == null) return NotFound();
            var traits = await _context.ЧертыПользователя
                .Where(x => x.IdПользователя == user.IdПользователя)
                .Select(x => new {
                    id = x.IdЧертыХарактера,
                    name = x.IdЧертыХарактераNavigation.НазваниеЧерты,
                    value = x.Значение
                }).ToListAsync();
            return Ok(traits);
        }

        [HttpPost("searchmembers")]
        public async Task<IActionResult> SearchMembers([FromBody] MemberSearchDto filter)
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email)) return Unauthorized();
                var currentUser = await _context.Пользователи.FirstOrDefaultAsync(u => u.ЭлектроннаяПочта == email);
                if (currentUser == null) return NotFound();
                var oppositeGender = currentUser.Пол == "М" ? "Ж" : "М";
                var today = DateTime.Today;
                var query = _context.Пользователи
                    .Where(u => u.Пол == oppositeGender && u.IdПользователя != currentUser.IdПользователя);
                if (!string.IsNullOrEmpty(filter.City))
                    query = query.Where(u => u.Местоположение.Trim().ToLower() == filter.City.Trim().ToLower());
                if (filter.HeightMin > 0)
                    query = query.Where(u => u.Рост >= filter.HeightMin);
                if (filter.HeightMax > 0)
                    query = query.Where(u => u.Рост <= filter.HeightMax);
                var users = await query.ToListAsync();
                if (filter.AgeMin > 0 && filter.AgeMax > 0)
                {
                    var minBirth = today.AddYears(-filter.AgeMax);
                    var maxBirth = today.AddYears(-filter.AgeMin);
                    users = users.Where(u => u.ДатаРождения.ToDateTime(TimeOnly.MinValue) >= minBirth && u.ДатаРождения.ToDateTime(TimeOnly.MinValue) <= maxBirth).ToList();
                }
                // --- В выборке анкет ---
                var blockedIds = await _context.ЖурналПриложения
                    .Where(x => x.IdПользователя1 == currentUser.IdПользователя && x.IdТипВзаимодействия == 6)
                    .Select(x => x.IdПользователя2)
                    .ToListAsync();
                var disliked = await _context.ЖурналПриложения
                    .Where(x => x.IdПользователя1 == currentUser.IdПользователя && x.IdТипВзаимодействия == 3)
                    .ToListAsync();
                var dislikedIds = disliked
                    .Where(x => (DateTime.Now - x.ДатаИВремя).TotalDays < 3)
                    .Select(x => x.IdПользователя2)
                    .ToList();
                users = users
                    .Where(u => !blockedIds.Contains(u.IdПользователя) && !dislikedIds.Contains(u.IdПользователя))
                    .ToList();
                // Считаем совпадения по интересам
                var userIds = users.Select(u => u.IdПользователя).ToList();
                var userHobbies = await _context.ПользователиНазванияУвлечений.Where(x => userIds.Contains(x.IdПользователя)).ToListAsync();
                var photos = await _context.ИзображенияПрофиля
                    .Where(x => userIds.Contains(x.ID_Пользователя))
                    .OrderBy(x => x.ID_Изображения)
                    .ToListAsync();
                // --- ЧЕРТЫ ХАРАКТЕРА ---
                var myTraits = await _context.ЧертыПользователя
                    .Where(x => x.IdПользователя == currentUser.IdПользователя)
                    .ToDictionaryAsync(x => x.IdЧертыХарактера, x => x.Значение);
                var allTraits = await _context.ЧертыПользователя
                    .Where(x => userIds.Contains(x.IdПользователя))
                    .ToListAsync();
                var traitsByUser = allTraits
                    .GroupBy(x => x.IdПользователя)
                    .ToDictionary(g => g.Key, g => g.ToDictionary(t => t.IdЧертыХарактера, t => t.Значение));
                var result = users.Select(u => {
                    var interests = userHobbies.Where(h => h.IdПользователя == u.IdПользователя).Select(h => h.НазваниеУвлечения).ToList();
                    int matchCount = 0;
                    if (filter.Interests != null && filter.Interests.Count > 0)
                        matchCount = interests.Intersect(filter.Interests).Count();
                    var photo = photos.FirstOrDefault(p => p.ID_Пользователя == u.IdПользователя)?.Ссылка ?? "/images/avatars/standart.jpg";
                    double compatibility = 0;
                    if (traitsByUser.TryGetValue(u.IdПользователя, out var otherTraits))
                        compatibility = CompatibilityHelper.CalculateCompatibility(myTraits, otherTraits);
                    var userPhotos = photos
                        .Where(p => p.ID_Пользователя == u.IdПользователя)
                        .Select(p => p.Ссылка)
                        .ToList();
                    return new {
                        id = u.IdПользователя,
                        имя = u.Имя,
                        интересы = interests,
                        рост = u.Рост,
                        уровеньЗаработка = u.УровеньЗаработка,
                        жильё = u.Жильё,
                        наличиеДетей = u.НаличиеДетей ? "Есть" : "Нет",
                        город = u.Местоположение,
                        возраст = today.Year - u.ДатаРождения.Year - (today.DayOfYear < u.ДатаРождения.DayOfYear ? 1 : 0),
                        совпадениеИнтересов = matchCount,
                        фото = photo,
                        фотографии = userPhotos,
                        описание = u.Описание,
                        совместимость = compatibility
                    };
                }).OrderByDescending(x => x.совпадениеИнтересов).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpGet("allcities")]
        public IActionResult GetAllCities()
        {
            var cities = new List<string> { "Рязань", "Москва", "Санкт-Петербург" };
            return Ok(cities);
        }

        [HttpPost("userinteraction")]
        public async Task<IActionResult> UserInteraction([FromBody] UserInteractionDto dto)
        {
            try
            {
                var email = User.FindFirstValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(email)) return Unauthorized();
                var user = await _context.Пользователи.FirstOrDefaultAsync(u => u.ЭлектроннаяПочта == email);
                if (user == null) return NotFound();
                var target = await _context.Пользователи.FirstOrDefaultAsync(u => u.IdПользователя == dto.TargetUserId);
                if (target == null) return NotFound();
                var now = DateTime.Now;
                // Можно не дублировать одинаковые действия подряд (например, не писать 2 лайка подряд)
                var last = await _context.ЖурналПриложения
                    .Where(x => x.IdПользователя1 == user.IdПользователя && x.IdПользователя2 == target.IdПользователя)
                    .OrderByDescending(x => x.ДатаИВремя)
                    .FirstOrDefaultAsync();
                if (last == null || last.IdТипВзаимодействия != dto.InteractionTypeId)
                {
                    var entry = new Models.ЖурналПриложения
                    {
                        IdПользователя1 = user.IdПользователя,
                        IdПользователя2 = target.IdПользователя,
                        IdТипВзаимодействия = dto.InteractionTypeId,
                        ДатаИВремя = now
                    };
                    _context.ЖурналПриложения.Add(entry);
                    await _context.SaveChangesAsync();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }

        [HttpGet("mutuallikes")]
        public async Task<IActionResult> GetMutualLikes()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return Unauthorized();
            var user = await _context.Пользователи.FirstOrDefaultAsync(u => u.ЭлектроннаяПочта == email);
            if (user == null) return NotFound();

            // Находим id пользователей, которым я поставил лайк (или суперлайк)
            var myLikes = await _context.ЖурналПриложения
                .Where(x => x.IdПользователя1 == user.IdПользователя && (x.IdТипВзаимодействия == 2 || x.IdТипВзаимодействия == 1))
                .Select(x => x.IdПользователя2)
                .ToListAsync();

            // Находим id пользователей, которые поставили лайк (или суперлайк) мне
            var likesToMe = await _context.ЖурналПриложения
                .Where(x => x.IdПользователя2 == user.IdПользователя && (x.IdТипВзаимодействия == 2 || x.IdТипВзаимодействия == 1))
                .Select(x => x.IdПользователя1)
                .ToListAsync();

            // Взаимные лайки — пересечение
            var mutualIds = myLikes.Intersect(likesToMe).ToList();

            if (!mutualIds.Any())
                return Ok(new List<object>());

            var users = await _context.Пользователи
                .Where(u => mutualIds.Contains(u.IdПользователя))
                .ToListAsync();

            var photos = await _context.ИзображенияПрофиля
                .Where(p => mutualIds.Contains(p.ID_Пользователя))
                .GroupBy(p => p.ID_Пользователя)
                .Select(g => new { Id = g.Key, Photo = g.OrderBy(p => p.ID_Изображения).FirstOrDefault().Ссылка })
                .ToListAsync();

            var result = users.Select(u => new {
                id = u.IdПользователя,
                имя = u.Имя,
                фото = photos.FirstOrDefault(p => p.Id == u.IdПользователя)?.Photo ?? "/images/avatars/standart.jpg"
            }).ToList();

            return Ok(result);
        }

        public class ResetPasswordDto
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string Secret { get; set; }
        }

        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (dto.Secret != "password")
                return BadRequest("Секретное слово неверно");
            var email = dto.Email?.Trim().ToLower();
            var user = await _context.Пользователи.FirstOrDefaultAsync(u => u.ЭлектроннаяПочта.ToLower() == email);
            if (user == null)
                return BadRequest("Пользователь с таким email не найден");
            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
                return BadRequest("Пароль должен быть не короче 6 символов");
            user.Пароль = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("likedtome")]
        public async Task<IActionResult> GetLikedToMe()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email)) return Unauthorized();
            var user = await _context.Пользователи.FirstOrDefaultAsync(u => u.ЭлектроннаяПочта == email);
            if (user == null) return NotFound();

            // Пользователи, которые лайкнули меня (или суперлайк)
            var likesToMe = await _context.ЖурналПриложения
                .Where(x => x.IdПользователя2 == user.IdПользователя && (x.IdТипВзаимодействия == 2 || x.IdТипВзаимодействия == 1))
                .Select(x => x.IdПользователя1)
                .ToListAsync();

            // Кому я уже поставил лайк/дизлайк/блокировку
            var myActions = await _context.ЖурналПриложения
                .Where(x => x.IdПользователя1 == user.IdПользователя && likesToMe.Contains(x.IdПользователя2) && (x.IdТипВзаимодействия == 2 || x.IdТипВзаимодействия == 3 || x.IdТипВзаимодействия == 6))
                .Select(x => x.IdПользователя2)
                .ToListAsync();

            // Оставляем только тех, кому я ещё не ответил
            var pendingIds = likesToMe.Except(myActions).ToList();
            if (!pendingIds.Any())
                return Ok(new List<object>());

            var users = await _context.Пользователи
                .Where(u => pendingIds.Contains(u.IdПользователя))
                .ToListAsync();

            var photos = await _context.ИзображенияПрофиля
                .Where(p => pendingIds.Contains(p.ID_Пользователя))
                .GroupBy(p => p.ID_Пользователя)
                .Select(g => new { Id = g.Key, Photo = g.OrderBy(p => p.ID_Изображения).FirstOrDefault().Ссылка })
                .ToListAsync();

            var result = users.Select(u => new {
                id = u.IdПользователя,
                имя = u.Имя,
                фото = photos.FirstOrDefault(p => p.Id == u.IdПользователя)?.Photo ?? "/images/avatars/standart.jpg"
            }).ToList();

            return Ok(result);
        }
    }
} 