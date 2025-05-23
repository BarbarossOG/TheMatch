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

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Single()
        {
            return View();
        }

        public IActionResult Members()
        {
            return View();
        }

        public IActionResult Likes()
        {
            return View();
        }
    }
} 