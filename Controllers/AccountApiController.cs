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
            user.Рост = (byte)(dto.Рост ?? 0);
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
                // Получаем все id увлечений по названиям
                var allHobbies = await _context.Увлечения
                    .Where(h => dto.Интересы.Contains(h.НазваниеУвлечения))
                    .ToListAsync();

                // Удаляем старые связи
                var oldLinks = _context.УвлеченияПользователяs.Where(x => x.ID_Пользователя == user.IdПользователя);
                _context.УвлеченияПользователяs.RemoveRange(oldLinks);

                // Добавляем новые связи
                foreach (var hobby in allHobbies)
                {
                    _context.УвлеченияПользователяs.Add(new УвлеченияПользователя
                    {
                        ID_Пользователя = user.IdПользователя,
                        ID_Увлечения = hobby.IdУвлечения
                    });
                }
            }

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