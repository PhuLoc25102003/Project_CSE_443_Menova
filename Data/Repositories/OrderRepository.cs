using Menova.Models;
using Microsoft.EntityFrameworkCore;

namespace Menova.Data.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> GetOrderWithDetailsAsync(int orderId, int userId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(v => v.Size)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(v => v.Color)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);
        }

        public async Task<Order> GetOrderWithDetailsForAdminAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(v => v.Size)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(v => v.Color)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<IEnumerable<Order>> GetAllWithDetailsAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> CreateOrderFromCartAsync(int userId, string shippingAddress, string phoneNumber, string paymentMethod, string notes)
        {
            // Get user's cart
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                throw new Exception("Cart is empty");
            }

            // Calculate total
            //decimal subtotal = cart.CartItems.Sum(ci =>
            //    (ci.Product.DiscountPrice ?? ci.Product.Price + (ci.ProductVariant.AdditionalPrice ?? 0)) * ci.Quantity);
            decimal shippingFee = 30000; // Default shipping fee
            decimal totalAmount = 1 + shippingFee;

            // Create order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                ShippingAddress = shippingAddress,
                PhoneNumber = phoneNumber,
                OrderStatus = "Pending",
                PaymentMethod = paymentMethod,
                PaymentStatus = paymentMethod == "COD" ? "Pending" : "Paid",
                ShippingFee = shippingFee,
                Notes = notes,
                OrderDetails = new List<OrderDetail>()
            };

            // Create order details
            foreach (var item in cart.CartItems)
            {
                var unitPrice = 1; 
                //item.Product.DiscountPrice ?? item.Product.Price + (item.ProductVariant.AdditionalPrice ?? 0);
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    VariantId = item.VariantId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    Discount = 0, // No discount for this demo
                    Subtotal = unitPrice * item.Quantity
                };

                order.OrderDetails.Add(orderDetail);
            }

            await _context.Orders.AddAsync(order);

            // Clear cart
            _context.CartItems.RemoveRange(cart.CartItems);

            await _context.SaveChangesAsync();

            return order;
        }

        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                throw new Exception("Order not found");
            }

            order.OrderStatus = status;
            if (status == "Delivered" && order.PaymentMethod == "COD")
            {
                order.PaymentStatus = "Paid";
            }

            await _context.SaveChangesAsync();
        }
    }

}
