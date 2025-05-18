using Menova.Data;
using Menova.Data.Services;
using Menova.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Menova.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _userService.AuthenticateAsync(username, password);

            if (user != null)
            {
                // In a real app, would use ASP.NET Core Identity or set auth cookie
                // For demo, just redirect to home
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user, string password)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _userService.RegisterUserAsync(user, password);
                    if (result)
                    {
                        return RedirectToAction("Login");
                    }
                }
                return View(user);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(user);
            }
        }

        public async Task<IActionResult> Profile()
        {
            // For demonstration purposes - normally would use user authentication
            int userId = 1; // Placeholder for actual user ID from auth

            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(User userUpdates)
        {
            // For demonstration purposes - normally would use user authentication
            int userId = 1; // Placeholder for actual user ID from auth

            try
            {
                // Make sure we're updating the correct user
                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound();
                }

                user.FullName = userUpdates.FullName;
                user.PhoneNumber = userUpdates.PhoneNumber;

                await _userService.UpdateUserProfileAsync(user);

                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("Profile", userUpdates);
            }
        }

    }
}
