using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TheMatch.Models;
using TheMatch.Models.Dtos;
using Microsoft.AspNetCore.Authorization;

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

        [Authorize]
        public IActionResult VerifyAccount()
        {
            return View();
        }

        [Authorize]
        public IActionResult Profileinfo()
        {
            return View();
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        public IActionResult CloseAccount()
        {
            return View();
        }

        [Authorize]
        public IActionResult AccountInfoUser()
        {
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            ViewBag.Email = email;
            return View();
        }

        [Authorize]
        public IActionResult Settings()
        {
            return View();
        }

        [Authorize]
        public IActionResult TestUser()
        {
            return View();
        }

        [Authorize]
        public IActionResult Logout()
        {
            return View();
        }

    }
} 