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
                Пароль = HashPassword(dto.Пароль)
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

            if (user.Пароль != HashPassword(dto.Пароль))
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
                return Ok(new { url = "/images/avatars/standart.png" }); // default
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
            if (user.Пароль != HashPassword(dto.OldPassword))
                return BadRequest("Старый пароль неверен");
            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest("Пароли не совпадают");
            if (dto.NewPassword.Length < 6)
                return BadRequest("Пароль должен быть не короче 6 символов");
            user.Пароль = HashPassword(dto.NewPassword);
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
                    query = query.Where(u => u.Местоположение == filter.City);
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
                // Считаем совпадения по интересам
                var userIds = users.Select(u => u.IdПользователя).ToList();
                var userHobbies = await _context.ПользователиНазванияУвлечений.Where(x => userIds.Contains(x.IdПользователя)).ToListAsync();
                var photos = await _context.ИзображенияПрофиля.Where(x => userIds.Contains(x.ID_Пользователя) && x.Основное).ToListAsync();
                var result = users.Select(u => {
                    var interests = userHobbies.Where(h => h.IdПользователя == u.IdПользователя).Select(h => h.НазваниеУвлечения).ToList();
                    int matchCount = 0;
                    if (filter.Interests != null && filter.Interests.Count > 0)
                        matchCount = interests.Intersect(filter.Interests).Count();
                    var photo = photos.FirstOrDefault(p => p.ID_Пользователя == u.IdПользователя)?.Ссылка ?? "/images/avatars/standart.png";
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
                        фото = photo
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

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
} 