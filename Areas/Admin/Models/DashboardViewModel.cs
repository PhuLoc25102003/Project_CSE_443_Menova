using Menova.Models;

namespace Menova.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<Order> RecentOrders { get; set; }
        public List<ProductSalesSummary> TopSellingProducts { get; set; }
        public List<Product> LowStockProducts { get; set; }
        public ChartData SalesChartData { get; set; }
    }

    public class ProductSalesSummary
    {
        public Product Product { get; set; }
        public int TotalSales { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ChartData
    {
        public List<string> Labels { get; set; }
        public List<decimal> Sales { get; set; }
        public List<int> Orders { get; set; }
    }

}
