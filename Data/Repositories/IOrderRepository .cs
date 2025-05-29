using Menova.Models;

namespace Menova.Data.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetUserOrdersAsync(string userId);
        Task<Order> GetOrderWithDetailsAsync(int orderId, string userId);
        Task<Order> GetOrderWithDetailsForAdminAsync(int orderId);
        Task<Order> CreateOrderFromCartAsync(string userId, string shippingAddress, string phoneNumber, string paymentMethod, string notes);
        Task UpdateOrderStatusAsync(int orderId, string status);
        Task<IEnumerable<Order>> GetAllWithDetailsAsync();
    }
}
