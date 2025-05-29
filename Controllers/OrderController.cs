using System.Threading.Tasks;
using Menova.Data;
using Menova.Data.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Menova.Models;

namespace Menova.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _orderService.GetUserOrdersAsync(user.Id);
            return View(orders);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetOrderDetailsAsync(id, user.Id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string shippingAddress, string phoneNumber, string paymentMethod, string notes)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var order = await _orderService.CreateOrderAsync(user.Id, shippingAddress, phoneNumber, paymentMethod, notes);
                return RedirectToAction("Detail", new { id = order.OrderId });
            }
            catch (System.Exception ex)
            {
                // Log the error
                ModelState.AddModelError(string.Empty, ex.Message);
                return RedirectToAction("Checkout", "Cart");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            await _orderService.UpdateOrderStatusAsync(id, status);
            return RedirectToAction("Detail", new { id });
        }
    }
}
