using System.Threading.Tasks;
using Menova.Data;
using Menova.Data.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Menova.Models;
using System;

namespace Menova.Controllers
{
    [Authorize]  // Yêu cầu đăng nhập cho tất cả các hành động trong controller này
    [Route("orders")]  // Định nghĩa route prefix cho controller
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
        }

        [Route("")]
        [Route("index")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Order") });
            }

            // Chỉ lấy đơn hàng của người dùng hiện tại
            var orders = await _orderService.GetUserOrdersAsync(user.Id);
            return View(orders);
        }

        [Route("detail/{id:int}")]
        public async Task<IActionResult> Detail(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Detail", "Order", new { id }) });
            }

            // Sử dụng GetOrderDetailsAsync để đảm bảo chỉ lấy đơn hàng của người dùng hiện tại
            var order = await _orderService.GetOrderDetailsAsync(id, user.Id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền xem đơn hàng này.";
                return RedirectToAction("Index");
            }

            return View(order);
        }

        [HttpPost]
        [Route("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string shippingAddress, string phoneNumber, string paymentMethod, string notes)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "Cart") });
            }

            try
            {
                var order = await _orderService.CreateOrderAsync(user.Id, shippingAddress, phoneNumber, paymentMethod, notes);
                
                // Thêm thông báo OrderSuccess để hiển thị SweetAlert
                TempData["OrderSuccess"] = "Đơn hàng của bạn đã được đặt thành công. Chúng tôi sẽ xử lý trong thời gian sớm nhất!";
                
                // Chuyển hướng về trang chủ thay vì trang giỏ hàng (đã trống)
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // Sử dụng ErrorMessage để hiển thị thông báo lỗi với SweetAlert
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Checkout", "Cart");
            }
        }

        [HttpPost]
        [Route("cancel/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Lấy thông tin đơn hàng
                var order = await _orderService.GetOrderDetailsAsync(id, user.Id);
                
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền hủy đơn hàng này.";
                    return RedirectToAction("Index");
                }

                // Chỉ cho phép hủy đơn hàng ở trạng thái "Pending" hoặc "Processing"
                if (order.OrderStatus?.ToLower() == "pending" || order.OrderStatus?.ToLower() == "processing")
                {
                    await _orderService.UpdateOrderStatusAsync(id, "Cancelled");
                    TempData["SuccessMessage"] = "Đơn hàng đã được hủy thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể hủy đơn hàng ở trạng thái hiện tại.";
                }
                
                return RedirectToAction("Detail", new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi hủy đơn hàng: " + ex.Message;
                return RedirectToAction("Detail", new { id });
            }
        }

        [HttpPost]
        [Route("confirm-received/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmReceived(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                // Lấy thông tin đơn hàng
                var order = await _orderService.GetOrderDetailsAsync(id, user.Id);
                
                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền xác nhận đơn hàng này.";
                    return RedirectToAction("Index");
                }

                // Chỉ cho phép xác nhận đơn hàng ở trạng thái "Delivered"
                if (order.OrderStatus?.ToLower() == "delivered")
                {
                    await _orderService.UpdateOrderStatusAsync(id, "Received");
                    TempData["SuccessMessage"] = "Cảm ơn bạn đã xác nhận đã nhận hàng.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xác nhận đã nhận hàng ở trạng thái hiện tại.";
                }
                
                return RedirectToAction("Detail", new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xác nhận đã nhận hàng: " + ex.Message;
                return RedirectToAction("Detail", new { id });
            }
        }
    }
}
