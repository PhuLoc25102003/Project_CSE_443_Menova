using Menova.Models;

namespace Menova.Data.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetUserOrdersAsync(string userId);
        Task<Order> GetOrderDetailsAsync(int orderId, string userId);
        Task<Order> GetOrderDetailsForAdminAsync(int orderId);
        Task<Order> CreateOrderAsync(string userId, string shippingAddress, string phoneNumber, string paymentMethod, string notes);
        Task UpdateOrderStatusAsync(int orderId, string status);

        Task<int> GetTotalOrderCountAsync();
        Task<List<Order>> GetRecentOrdersAsync(int count);
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetTotalRevenueWithoutShippingAsync();
        Task<List<Order>> GetOrdersInDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetOrderCountsByStatusAsync();
    }
}
