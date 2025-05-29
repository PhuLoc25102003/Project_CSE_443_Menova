using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Menova.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace Menova.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            
            // Create a dictionary to store user roles
            var userRoles = new Dictionary<string, string>();
            
            // For each user, get their roles and store in the dictionary
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.FirstOrDefault() ?? "Khách hàng";
            }
            
            // Pass the user roles to the view
            ViewBag.UserRoles = userRoles;
            
            return View(users);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = await _roleManager.Roles.ToListAsync();
            ViewBag.UserRoles = userRoles;

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, string fullName, string email, string phoneNumber, bool isActive, string[] roles)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.FullName = fullName;
            user.Email = email;
            user.UserName = email; // Keep username and email the same
            user.PhoneNumber = phoneNumber;
            user.IsActive = isActive;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                var userRoles = await _userManager.GetRolesAsync(user);
                ViewBag.Roles = await _roleManager.Roles.ToListAsync();
                ViewBag.UserRoles = userRoles;
                return View(user);
            }

            // Update roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            // Remove roles not in the new list
            foreach (var role in currentRoles)
            {
                if (!roles.Contains(role))
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                }
            }
            // Add new roles
            foreach (var role in roles)
            {
                if (!await _userManager.IsInRoleAsync(user, role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }

            TempData["SuccessMessage"] = "User updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if it's the last admin
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                var admins = await _userManager.GetUsersInRoleAsync("Admin");
                if (admins.Count <= 1)
                {
                    TempData["ErrorMessage"] = "Cannot delete the last admin user.";
                    return RedirectToAction("Index");
                }
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting user.";
            }

            return RedirectToAction("Index");
        }
    }
} 