using Menova.Areas.Admin.Models;
using Menova.Data.Services;
using Microsoft.AspNetCore.Mvc;

namespace Menova.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;

        public DashboardController(
            IProductService productService,
            IOrderService orderService,
            IUserService userService)
        {
            _productService = productService;
            _orderService = orderService;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            // Tạo view model cho dashboard
            var viewModel = new DashboardViewModel
            {
                TotalProducts = await _productService.GetTotalProductCountAsync(),
                TotalOrders = await _orderService.GetTotalOrderCountAsync(),
                TotalUsers = await _userService.GetTotalUserCountAsync(),
                TotalRevenue = await _orderService.GetTotalRevenueAsync(),
                RecentOrders = await _orderService.GetRecentOrdersAsync(5),
                TopSellingProducts = await _productService.GetTopSellingProductsAsync(5),
                LowStockProducts = await _productService.GetLowStockProductsAsync(5),

                // Demo data cho biểu đồ
                SalesChartData = new ChartData
                {
                    Labels = new List<string> { "Tháng 1", "Tháng 2", "Tháng 3", "Tháng 4", "Tháng 5", "Tháng 6" },
                    Sales = new List<decimal> { 12000000, 19000000, 15000000, 25000000, 22000000, 30000000 },
                    Orders = new List<int> { 100, 150, 120, 180, 160, 200 }
                }
            };

            return View(viewModel);
        }
    }



}
