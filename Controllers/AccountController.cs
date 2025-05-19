using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TheMatch.Models;

namespace TheMatch.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult VerifyAccount()
        {
            return View();
        }

        public IActionResult AccountInfo()
        {
            return View();
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        public IActionResult CloseAccount()
        {
            return View();
        }

        public IActionResult FriendRequest()
        {
            return View();
        }

        public IActionResult Notification()
        {
            return View();
        }

        public IActionResult PrivacySettings()
        {
            return View();
        }

        public IActionResult Settings()
        {
            return View();
        }
    }
} 