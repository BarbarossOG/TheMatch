using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TheMatch.Data;
using TheMatch.Models;
using TheMatch.Models.Dtos;

namespace TheMatch.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatApiController : ControllerBase
    {
        private readonly TheMatchContext _context;

        public ChatApiController(TheMatchContext context)
        {
            _context = context;
        }

        // 1. Получить список пользователей, с кем есть переписка
        [HttpGet("dialogs")]
        public async Task<IActionResult> GetDialogs()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Пользователи.FirstOrDefaultAsync(u => u.ЭлектроннаяПочта == email);
            if (user == null) return Unauthorized();
            var myId = user.IdПользователя;

            var dialogUserIds = await _context.Переписка
                .Where(m => m.IdОтправителя == myId || m.IdПолучателя == myId)
                .Select(m => m.IdОтправителя == myId ? m.IdПолучателя : m.IdОтправителя)
                .Distinct()
                .ToListAsync();

            var users = await _context.Пользователи
                .Where(u => dialogUserIds.Contains(u.IdПользователя))
                .Select(u => new {
                    u.IdПользователя,
                    u.Имя,
                    Фото = u.ИзображенияПрофиля.FirstOrDefault(i => i.Основное).Ссылка
                })
                .ToListAsync();

            return Ok(users);
        }

        // 2. Получить сообщения между двумя пользователями
        [HttpGet("messages/{userId}")]
        public async Task<IActionResult> GetMessages(int userId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Пользователи.FirstOrDefaultAsync(u => u.ЭлектроннаяПочта == email);
            if (user == null) return Unauthorized();
            var myId = user.IdПользователя;

            var messages = await _context.Переписка
                .Where(m => (m.IdОтправителя == myId && m.IdПолучателя == userId) || (m.IdОтправителя == userId && m.IdПолучателя == myId))
                .OrderBy(m => m.ДатаОтправки)
                .Select(m => new {
                    m.IdСообщения,
                    m.IdОтправителя,
                    m.IdПолучателя,
                    m.Текст,
                    m.ДатаОтправки,
                    m.Прочитано
                })
                .ToListAsync();

            return Ok(messages);
        }

        // 3. Отправить сообщение
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Пользователи.FirstOrDefaultAsync(u => u.ЭлектроннаяПочта == email);
            if (user == null) return Unauthorized();
            var myId = user.IdПользователя;

            if (string.IsNullOrWhiteSpace(dto.Текст) || dto.Текст.Length > 250)
                return BadRequest("Текст сообщения не должен быть пустым и не должен превышать 250 символов.");

            var message = new Переписка
            {
                IdОтправителя = myId,
                IdПолучателя = dto.IdПолучателя,
                Текст = dto.Текст,
                ДатаОтправки = DateTime.Now,
                Прочитано = false
            };
            _context.Переписка.Add(message);
            await _context.SaveChangesAsync();
            return Ok(new { message.IdСообщения });
        }

        // 4. Пометить сообщения как прочитанные
        [HttpPost("read/{userId}")]
        public async Task<IActionResult> MarkAsRead(int userId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Пользователи.FirstOrDefaultAsync(u => u.ЭлектроннаяПочта == email);
            if (user == null) return Unauthorized();
            var myId = user.IdПользователя;

            var unread = await _context.Переписка
                .Where(m => m.IdОтправителя == userId && m.IdПолучателя == myId && !m.Прочитано)
                .ToListAsync();
            foreach (var msg in unread)
                msg.Прочитано = true;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
} 