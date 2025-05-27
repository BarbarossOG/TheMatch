using Microsoft.AspNetCore.Mvc;
using TheMatch.Models;
using Microsoft.AspNetCore.Authorization;

namespace TheMatch.Controllers
{
    public class CommunityController : Controller
    {
        private readonly ILogger<CommunityController> _logger;

        public CommunityController(ILogger<CommunityController> logger)
        {
            _logger = logger;
        }

        [Authorize]
        public IActionResult Members()
        {
            return View();
        }

        [Authorize]
        public IActionResult Likes()
        {
            return View();
        }

        [Authorize]
        public IActionResult Chats()
        {
            return View();
        }
    }
} 