using Menova.Models;

namespace Menova.Data.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
        Task<Order> GetOrderWithDetailsAsync(int orderId, int userId);
        Task<Order> CreateOrderFromCartAsync(int userId, string shippingAddress, string phoneNumber, string paymentMethod, string notes);
        Task UpdateOrderStatusAsync(int orderId, string status);
    }
}
