using Microsoft.AspNetCore.Mvc;
using TheMatch.Models;
using TheMatch.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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