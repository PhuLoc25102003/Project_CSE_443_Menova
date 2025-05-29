using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Menova.Models;
using System.Threading.Tasks;
using Menova.Areas.Admin.Models;
using Menova.Data.Services;

namespace Menova.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            IProductService productService,
            IOrderService orderService,
            IUserService userService,
            UserManager<ApplicationUser> userManager)
        {
            _productService = productService;
            _orderService = orderService;
            _userService = userService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string dateRange = "week")
        {
            // Get current authenticated user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "" });
            }

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
                SalesChartData = await GetSalesChartDataAsync(dateRange),
                OrderStatistics = await GetOrderStatisticsAsync()
            };

            ViewData["SelectedDateRange"] = dateRange;
            ViewData["CurrentUser"] = currentUser;
            return View(viewModel);
        }

        [HttpGet]
        public async Task<JsonResult> GetChartData(string dateRange = "week")
        {
            var chartData = await GetSalesChartDataAsync(dateRange);
            return Json(chartData);
        }

        private async Task<Dictionary<string, int>> GetOrderStatisticsAsync()
        {
            return await _orderService.GetOrderCountsByStatusAsync();
        }

        private async Task<ChartData> GetSalesChartDataAsync(string dateRange)
        {
            DateTime startDate;
            DateTime endDate = DateTime.Now;
            List<string> labels = new List<string>();
            List<decimal> sales = new List<decimal>();
            List<int> orders = new List<int>();

            // Xác định khoảng thời gian dựa trên dateRange
            switch (dateRange)
            {
                case "today":
                    startDate = DateTime.Today;
                    // Lấy tất cả đơn hàng trong ngày hôm nay
                    var todayOrders = await _orderService.GetOrdersInDateRangeAsync(startDate, endDate);
                    
                    // Lấy dữ liệu theo giờ trong ngày
                    for (int hour = 0; hour < 24; hour++)
                    {
                        var hourStart = startDate.AddHours(hour);
                        var hourEnd = hourStart.AddHours(1);
                        
                        var hourOrders = todayOrders
                            .Where(o => o.OrderDate.Hour == hour)
                            .ToList();
                        
                        labels.Add($"{hour}h");
                        sales.Add(hourOrders.Sum(o => o.TotalAmount));
                        orders.Add(hourOrders.Count);
                    }
                    break;

                case "week":
                    startDate = DateTime.Today.AddDays(-6);
                    // Lấy tất cả đơn hàng trong tuần
                    var weekOrders = await _orderService.GetOrdersInDateRangeAsync(startDate, endDate);
                    
                    // Lấy dữ liệu theo ngày trong tuần
                    for (int day = 0; day < 7; day++)
                    {
                        var dayDate = startDate.AddDays(day);
                        var nextDay = dayDate.AddDays(1);
                        
                        var dayOrders = weekOrders
                            .Where(o => o.OrderDate.Date == dayDate.Date)
                            .ToList();
                        
                        labels.Add(dayDate.ToString("dd/MM"));
                        sales.Add(dayOrders.Sum(o => o.TotalAmount));
                        orders.Add(dayOrders.Count);
                    }
                    break;

                case "month":
                    startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    var daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);
                    
                    // Lấy tất cả đơn hàng trong tháng
                    var monthOrders = await _orderService.GetOrdersInDateRangeAsync(startDate, endDate);
                    
                    // Lấy dữ liệu theo ngày trong tháng
                    for (int day = 0; day < daysInMonth; day++)
                    {
                        var dayDate = startDate.AddDays(day);
                        if (dayDate > endDate) break;
                        
                        var dayOrders = monthOrders
                            .Where(o => o.OrderDate.Date == dayDate.Date)
                            .ToList();
                        
                        labels.Add(dayDate.ToString("dd"));
                        sales.Add(dayOrders.Sum(o => o.TotalAmount));
                        orders.Add(dayOrders.Count);
                    }
                    break;

                case "year":
                    startDate = new DateTime(DateTime.Now.Year, 1, 1);
                    
                    // Lấy tất cả đơn hàng trong năm
                    var yearOrders = await _orderService.GetOrdersInDateRangeAsync(startDate, endDate);
                    
                    // Lấy dữ liệu theo tháng trong năm
                    for (int month = 1; month <= 12; month++)
                    {
                        var monthStart = new DateTime(startDate.Year, month, 1);
                        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                        if (monthStart > endDate) break;
                        if (monthEnd > endDate) monthEnd = endDate;
                        
                        var monthOrdersData = yearOrders
                            .Where(o => o.OrderDate.Month == month)
                            .ToList();
                        
                        labels.Add($"T{month}");
                        sales.Add(monthOrdersData.Sum(o => o.TotalAmount));
                        orders.Add(monthOrdersData.Count);
                    }
                    break;

                default:
                    // Mặc định là tuần
                    return await GetSalesChartDataAsync("week");
            }

            return new ChartData
            {
                Labels = labels,
                Sales = sales,
                Orders = orders
            };
        }
    }
}
