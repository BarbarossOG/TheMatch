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

        public IActionResult Profileinfo()
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

        public IActionResult AccountInfoUser()
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            ViewBag.Email = email;
            return View();
        }

        public IActionResult Settings()
        {
            return View();
        }

        public IActionResult TestUser()
        {
            return View();
        }

        public IActionResult Logout()
        {
            return View();
        }

    }
} 