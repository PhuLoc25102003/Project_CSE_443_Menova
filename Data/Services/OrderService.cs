using Menova.Models;

namespace Menova.Data.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            return await _unitOfWork.Orders.GetUserOrdersAsync(userId);
        }

        public async Task<Order> GetOrderDetailsAsync(int orderId, int userId)
        {
            return await _unitOfWork.Orders.GetOrderWithDetailsAsync(orderId, userId);
        }

        public async Task<Order> CreateOrderAsync(int userId, string shippingAddress, string phoneNumber, string paymentMethod, string notes)
        {
            return await _unitOfWork.Orders.CreateOrderFromCartAsync(userId, shippingAddress, phoneNumber, paymentMethod, notes);
        }

        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            await _unitOfWork.Orders.UpdateOrderStatusAsync(orderId, status);
        }

        public async Task<int> GetTotalOrderCountAsync()
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();
            return orders.Count();
        }

        public async Task<List<Order>> GetRecentOrdersAsync(int count)
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();
            return orders
                .OrderByDescending(o => o.OrderDate)
                .Take(count)
                .ToList();
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            var orders = await _unitOfWork.Orders.GetAllAsync();
            return orders.Sum(o => o.TotalAmount);
        }

    }

}
