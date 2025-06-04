using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Menova.Data.Services;
using Menova.Models;
using Menova.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace Menova.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("Admin/OrderManagement")]
    public class OrderManagementController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderManagementController> _logger;

        public OrderManagementController(IOrderService orderService, ILogger<OrderManagementController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index(string statusFilter = "", string searchQuery = "", 
            DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20)
        {
            try
            {
                if (!User.IsInRole("Admin"))
                {
                    _logger.LogWarning("Unauthorized access attempt to OrderManagementController.Index");
                    return RedirectToAction("AccessDenied", "Account");
                }
                
                var statusCounts = await _orderService.GetOrderCountsByStatusAsync();
                
                var totalRevenue = await _orderService.GetTotalRevenueAsync();
                
                var orders = await GetFilteredOrdersAsync(statusFilter, searchQuery, startDate, endDate);
                
                var totalItems = orders.Count;
                var paginatedOrders = orders
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
                
                var viewModel = new OrderManagementViewModel
                {
                    Orders = paginatedOrders ?? new List<Order>(),
                    StatusFilter = statusFilter ?? "",
                    SearchQuery = searchQuery ?? "",
                    StartDate = startDate,
                    EndDate = endDate,
                    StatusCounts = statusCounts ?? new Dictionary<string, int>(),
                    TotalRevenue = totalRevenue,
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = page,
                        ItemsPerPage = pageSize,
                        TotalItems = totalItems
                    }
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OrderManagementController.Index");
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi tải dữ liệu đơn hàng.");
                
                var emptyViewModel = new OrderManagementViewModel
                {
                    Orders = new List<Order>(),
                    StatusFilter = statusFilter ?? "",
                    SearchQuery = searchQuery ?? "",
                    StatusCounts = new Dictionary<string, int>(),
                    TotalRevenue = 0,
                    Pagination = new PaginationInfo
                    {
                        CurrentPage = 1,
                        ItemsPerPage = 20,
                        TotalItems = 0
                    }
                };
                
                return View(emptyViewModel);
            }
        }

        [HttpGet]
        [Route("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                if (!User.IsInRole("Admin"))
                {
                    _logger.LogWarning($"Unauthorized access attempt to OrderManagementController.Details for order {id}");
                    return RedirectToAction("AccessDenied", "Account");
                }
                
                var order = await _orderService.GetOrderDetailsForAdminAsync(id);
                
                if (order == null)
                {
                    return NotFound();
                }

                var viewModel = new OrderDetailViewModel
                {
                    Order = order,
                    AvailableStatuses = GetOrderStatusOptions()
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in OrderManagementController.Details for order {id}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải thông tin chi tiết đơn hàng.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Route("UpdateStatus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                if (!User.IsInRole("Admin"))
                {
                    _logger.LogWarning($"Unauthorized access attempt to OrderManagementController.UpdateStatus for order {id}");
                    return RedirectToAction("AccessDenied", "Account");
                }
                
                var validStatuses = new[] { "Pending", "Processing", "Shipping", "Delivered", "Received", "Cancelled" };
                if (!validStatuses.Contains(status))
                {
                    _logger.LogWarning($"Invalid status {status} in OrderManagementController.UpdateStatus for order {id}");
                    TempData["ErrorMessage"] = "Trạng thái không hợp lệ.";
                    return RedirectToAction("Details", new { id });
                }
                
                await _orderService.UpdateOrderStatusAsync(id, status);
                _logger.LogInformation($"Order {id} status updated to {status} by admin {User.Identity.Name}");
                TempData["SuccessMessage"] = "Trạng thái đơn hàng đã được cập nhật thành công.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in OrderManagementController.UpdateStatus for order {id}");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật trạng thái đơn hàng.";
            }
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [Route("api/UpdateStatusAjax")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatusAjax(int id, string status)
        {
            try
            {
                if (!User.IsInRole("Admin"))
                {
                    _logger.LogWarning($"Unauthorized AJAX access attempt to update order {id} status");
                    return Json(new { success = false, message = "Không có quyền thực hiện thao tác này" });
                }
                
                var validStatuses = new[] { "Pending", "Processing", "Shipping", "Delivered", "Received", "Cancelled" };
                if (!validStatuses.Contains(status))
                {
                    _logger.LogWarning($"Invalid status {status} in AJAX update for order {id}");
                    return Json(new { success = false, message = "Trạng thái không hợp lệ" });
                }
                
                await _orderService.UpdateOrderStatusAsync(id, status);
                _logger.LogInformation($"Order {id} status updated to {status} automatically by system");
                
                return Json(new { 
                    success = true, 
                    message = $"Đơn hàng đã được cập nhật thành trạng thái {status} thành công",
                    newStatus = status
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in AJAX status update for order {id}");
                return Json(new { success = false, message = "Lỗi cập nhật: " + ex.Message });
            }
        }

        [HttpPost]
        [Route("Delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogWarning($"Attempt to delete order {id} was rejected as per business requirements");
            TempData["ErrorMessage"] = "Không được phép xóa đơn hàng.";
            return RedirectToAction("Index");
        }
        
        [HttpGet]
        [Route("DiagnosticDetails/{id:int}")]
        public async Task<IActionResult> DiagnosticDetails(int id)
        {
            try
            {
                if (!User.IsInRole("Admin"))
                {
                    _logger.LogWarning($"Unauthorized access attempt to OrderManagementController.DiagnosticDetails for order {id}");
                    return RedirectToAction("AccessDenied", "Account");
                }
                
                var order = await _orderService.GetOrderDetailsForAdminAsync(id);
                
                if (order == null)
                {
                    return Content("Order not found");
                }

                var orderDetailCount = order.OrderDetails?.Count() ?? 0;
                var hasNullProducts = order.OrderDetails?.Any(od => od.Product == null) ?? false;
                
                string diagnosticInfo = $"Order ID: {order.OrderId}\n" +
                                      $"Order Status: {order.OrderStatus}\n" +
                                      $"Order Details Count: {orderDetailCount}\n" +
                                      $"Has Null Products: {hasNullProducts}\n\n";
                
                if (orderDetailCount > 0)
                {
                    diagnosticInfo += "Order Details:\n";
                    foreach (var detail in order.OrderDetails)
                    {
                        diagnosticInfo += $"- Detail ID: {detail.OrderDetailId}\n" +
                                         $"  Product ID: {detail.ProductId}\n" +
                                         $"  Product Name: {detail.Product?.Name ?? "NULL"}\n" +
                                         $"  Variant ID: {detail.VariantId}\n" +
                                         $"  Quantity: {detail.Quantity}\n" +
                                         $"  Unit Price: {detail.UnitPrice}\n\n";
                    }
                }
                
                return Content(diagnosticInfo, "text/plain");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in OrderManagementController.DiagnosticDetails for order {id}");
                return Content($"Error: {ex.Message}\n{ex.StackTrace}", "text/plain");
            }
        }

        [HttpGet]
        [Route("Export")]
        public async Task<IActionResult> ExportOrders(string statusFilter = "", string searchQuery = "", 
            DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                if (!User.IsInRole("Admin"))
                {
                    _logger.LogWarning("Unauthorized access attempt to OrderManagementController.ExportOrders");
                    return RedirectToAction("AccessDenied", "Account");
                }
                
                var orders = await GetFilteredOrdersAsync(statusFilter, searchQuery, startDate, endDate);
                
                _logger.LogInformation($"Admin {User.Identity.Name} exported orders with filters: status={statusFilter}, query={searchQuery}");
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OrderManagementController.ExportOrders");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xuất đơn hàng.";
                return RedirectToAction("Index");
            }
        }

        private async Task<List<Order>> GetFilteredOrdersAsync(string statusFilter, string searchQuery, 
            DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var allOrders = (await _orderService.GetRecentOrdersAsync(1000))?.ToList() ?? new List<Order>();
                
                var filteredOrders = allOrders;
                
                if (!string.IsNullOrEmpty(statusFilter))
                {
                    filteredOrders = filteredOrders.Where(o => o.OrderStatus?.ToLower() == statusFilter.ToLower()).ToList();
                }
                
                if (startDate.HasValue)
                {
                    filteredOrders = filteredOrders.Where(o => o.OrderDate >= startDate.Value).ToList();
                }
                
                if (endDate.HasValue)
                {
                    filteredOrders = filteredOrders.Where(o => o.OrderDate <= endDate.Value.AddDays(1)).ToList();
                }
                
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    filteredOrders = filteredOrders.Where(o => 
                        o.OrderId.ToString().Contains(searchQuery) || 
                        (o.User?.FullName?.Contains(searchQuery) ?? false) ||
                        (o.User?.PhoneNumber?.Contains(searchQuery) ?? false) ||
                        (o.PhoneNumber?.Contains(searchQuery) ?? false)
                    ).ToList();
                }
                
                return filteredOrders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetFilteredOrdersAsync");
                return new List<Order>();
            }
        }

        private List<OrderStatusOption> GetOrderStatusOptions()
        {
            return new List<OrderStatusOption>
            {
                new OrderStatusOption { Value = "Pending", Text = "Chờ xử lý" },
                new OrderStatusOption { Value = "Processing", Text = "Đang xử lý" },
                new OrderStatusOption { Value = "Shipping", Text = "Đang giao hàng" },
                new OrderStatusOption { Value = "Delivered", Text = "Đã giao hàng" },
                new OrderStatusOption { Value = "Received", Text = "Đã nhận hàng" },
                new OrderStatusOption { Value = "Cancelled", Text = "Đã hủy" }
            };
        }
    }
}

