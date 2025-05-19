using Microsoft.AspNetCore.Mvc;
using TheMatch.Models;

namespace TheMatch.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(ILogger<ProfileController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SingleProfile()
        {
            return View();
        }

        public IActionResult SingleProfile2()
        {
            return View();
        }

        public IActionResult SingleProfile3()
        {
            return View();
        }

        public IActionResult NotFound()
        {
            return View();
        }

        public IActionResult Members()
        {
            return View();
        }

        public IActionResult Membership()
        {
            return View();
        }
    }
} 