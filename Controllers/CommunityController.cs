using Microsoft.AspNetCore.Mvc;
using TheMatch.Models;

namespace TheMatch.Controllers
{
    public class CommunityController : Controller
    {
        private readonly ILogger<CommunityController> _logger;

        public CommunityController(ILogger<CommunityController> logger)
        {
            _logger = logger;
        }

        public IActionResult Members()
        {
            return View();
        }

        public IActionResult Likes()
        {
            return View();
        }

        public IActionResult Chats()
        {
            return View();
        }
    }
} 