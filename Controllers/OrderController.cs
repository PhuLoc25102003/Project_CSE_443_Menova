using System.Threading.Tasks;
using Menova.Data;
using Menova.Data.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Menova.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            // For demonstration purposes - normally would use user authentication
            int userId = 1; // Placeholder for actual user ID from auth

            var orders = await _orderService.GetUserOrdersAsync(userId);
            return View(orders);
        }

        public async Task<IActionResult> Detail(int id)
        {
            // For demonstration purposes - normally would use user authentication
            int userId = 1; // Placeholder for actual user ID from auth

            var order = await _orderService.GetOrderDetailsAsync(id, userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Create(string shippingAddress, string phoneNumber, string paymentMethod, string notes)
        {
            // For demonstration purposes - normally would use user authentication
            int userId = 1; // Placeholder for actual user ID from auth

            try
            {
                var order = await _orderService.CreateOrderAsync(userId, shippingAddress, phoneNumber, paymentMethod, notes);
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
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            // This would typically be an admin function
            await _orderService.UpdateOrderStatusAsync(id, status);
            return RedirectToAction("Detail", new { id });
        }

    }
}
