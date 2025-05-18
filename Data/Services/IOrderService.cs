using Menova.Models;

namespace Menova.Data.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
        Task<Order> GetOrderDetailsAsync(int orderId, int userId);
        Task<Order> CreateOrderAsync(int userId, string shippingAddress, string phoneNumber, string paymentMethod, string notes);
        Task UpdateOrderStatusAsync(int orderId, string status);

        Task<int> GetTotalOrderCountAsync();
        Task<List<Order>> GetRecentOrdersAsync(int count);
        Task<decimal> GetTotalRevenueAsync();
    }
}
