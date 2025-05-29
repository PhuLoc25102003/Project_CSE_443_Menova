using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Menova.Data;
using Menova.Data.Services;
using Menova.Models;
using Menova.Areas.Admin.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Menova.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderManagementController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderManagementController> _logger;

        public OrderManagementController(IOrderService orderService, ILogger<OrderManagementController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string statusFilter = "", string searchQuery = "", 
            DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 20)
        {
            try
            {
                // Get total order count by status
                var statusCounts = await _orderService.GetOrderCountsByStatusAsync();
                
                // Get total revenue
                var totalRevenue = await _orderService.GetTotalRevenueAsync();
                
                // Get orders for the current page based on filters
                var orders = await GetFilteredOrdersAsync(statusFilter, searchQuery, startDate, endDate);
                
                // Apply pagination
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
                        // TotalPages được tính toán tự động trong PaginationInfo
                    }
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OrderManagementController.Index");
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi tải dữ liệu đơn hàng.");
                
                // Return a minimal view model to avoid null reference exceptions
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

        public async Task<IActionResult> Details(int id)
        {
            try
            {
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
                _logger.LogError(ex, "Error in OrderManagementController.Details");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải thông tin chi tiết đơn hàng.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                await _orderService.UpdateOrderStatusAsync(id, status);
                TempData["SuccessMessage"] = "Trạng thái đơn hàng đã được cập nhật thành công.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OrderManagementController.UpdateStatus");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật trạng thái đơn hàng.";
            }
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // Orders cannot be deleted as per business requirements
            // We keep this method to prevent errors but redirect with a message
            TempData["ErrorMessage"] = "Không được phép xóa đơn hàng.";
            return RedirectToAction("Index");
        }

        private async Task<List<Order>> GetFilteredOrdersAsync(string statusFilter, string searchQuery, 
            DateTime? startDate, DateTime? endDate)
        {
            try
            {
                // Get all orders
                var allOrders = (await _orderService.GetRecentOrdersAsync(1000))?.ToList() ?? new List<Order>();
                
                // Apply filters
                var filteredOrders = allOrders;
                
                // Filter by status
                if (!string.IsNullOrEmpty(statusFilter))
                {
                    filteredOrders = filteredOrders.Where(o => o.OrderStatus?.ToLower() == statusFilter.ToLower()).ToList();
                }
                
                // Filter by date range
                if (startDate.HasValue)
                {
                    filteredOrders = filteredOrders.Where(o => o.OrderDate >= startDate.Value).ToList();
                }
                
                if (endDate.HasValue)
                {
                    filteredOrders = filteredOrders.Where(o => o.OrderDate <= endDate.Value.AddDays(1)).ToList();
                }
                
                // Filter by search query (order ID, customer name, phone, etc.)
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