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

        // Проверка взаимного лайка
        private async Task<bool> HasMutualLike(int myId, int otherId)
        {
            var myLike = await _context.ЖурналПриложения.AnyAsync(x => x.IdПользователя1 == myId && x.IdПользователя2 == otherId && (x.IdТипВзаимодействия == 1 || x.IdТипВзаимодействия == 2));
            var theirLike = await _context.ЖурналПриложения.AnyAsync(x => x.IdПользователя1 == otherId && x.IdПользователя2 == myId && (x.IdТипВзаимодействия == 1 || x.IdТипВзаимодействия == 2));
            return myLike && theirLike;
        }

        // 1. Получить список пользователей, с кем есть переписка (только взаимные лайки)
        [HttpGet("dialogs")]
        public async Task<IActionResult> GetDialogs()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Пользователи.FirstOrDefaultAsync(u => u.ЭлектроннаяПочта == email);
            if (user == null) return Unauthorized();
            var myId = user.IdПользователя;

            // Найти всех пользователей с взаимным лайком (как в GetMutualLikes)
            var myLikes = await _context.ЖурналПриложения
                .Where(x => x.IdПользователя1 == myId && (x.IdТипВзаимодействия == 2 || x.IdТипВзаимодействия == 1))
                .Select(x => x.IdПользователя2)
                .ToListAsync();

            var likesToMe = await _context.ЖурналПриложения
                .Where(x => x.IdПользователя2 == myId && (x.IdТипВзаимодействия == 2 || x.IdТипВзаимодействия == 1))
                .Select(x => x.IdПользователя1)
                .ToListAsync();

            var mutualIds = myLikes.Intersect(likesToMe).ToList();

            if (!mutualIds.Any())
                return Ok(new List<object>());

            var users = await _context.Пользователи
                .Where(u => mutualIds.Contains(u.IdПользователя))
                .Select(u => new {
                    IdПользователя = u.IdПользователя,
                    Имя = u.Имя,
                    Фото = u.ИзображенияПрофиля.FirstOrDefault(i => i.Основное).Ссылка
                })
                .ToListAsync();

            return Ok(users);
        }

        // 2. Получить сообщения между двумя пользователями (только если есть взаимный лайк)
        [HttpGet("messages/{userId}")]
        public async Task<IActionResult> GetMessages(int userId)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Пользователи.FirstOrDefaultAsync(u => u.ЭлектроннаяПочта == email);
            if (user == null) return Unauthorized();
            var myId = user.IdПользователя;

            if (!await HasMutualLike(myId, userId))
                return Ok(new List<object>()); // Нет взаимного лайка — пусто

            var messages = await _context.Переписка
                .Where(m => (m.IdОтправителя == myId && m.IdПолучателя == userId) || (m.IdОтправителя == userId && m.IdПолучателя == myId))
                .OrderBy(m => m.ДатаОтправки)
                .Select(m => new {
                    IdСообщения = m.IdСообщения,
                    IdОтправителя = m.IdОтправителя,
                    IdПолучателя = m.IdПолучателя,
                    Текст = m.Текст,
                    ДатаОтправки = m.ДатаОтправки,
                    Прочитано = m.Прочитано
                })
                .ToListAsync();

            return Ok(messages);
        }

        // 3. Отправить сообщение (только если есть взаимный лайк)
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await _context.Пользователи.FirstOrDefaultAsync(u => u.ЭлектроннаяПочта == email);
            if (user == null) return Unauthorized();
            var myId = user.IdПользователя;

            if (!await HasMutualLike(myId, dto.IdПолучателя))
                return BadRequest("Вы можете переписываться только с пользователями, с которыми у вас взаимная симпатия.");

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

        // Получить имя и фото пользователя по id (для чата)
        [HttpGet("userinfo/{id}")]
        public async Task<IActionResult> GetUserInfo(int id)
        {
            var user = await _context.Пользователи.FirstOrDefaultAsync(u => u.IdПользователя == id);
            if (user == null) return NotFound();
            var photo = await _context.ИзображенияПрофиля
                .Where(x => x.ID_Пользователя == id && x.Основное)
                .Select(x => x.Ссылка)
                .FirstOrDefaultAsync();
            return Ok(new {
                id = user.IdПользователя,
                имя = user.Имя,
                фото = photo ?? "/images/avatars/avatar.png"
            });
        }

        // Получить id текущего пользователя
        [HttpGet("myid")]
        public IActionResult GetMyId()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Пользователи.FirstOrDefault(u => u.ЭлектроннаяПочта == email);
            if (user == null) return Unauthorized();
            return Ok(new { id = user.IdПользователя });
        }
    }
} 