﻿using Menova.Models;
using Microsoft.EntityFrameworkCore;

namespace Menova.Data.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(string userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order> GetOrderWithDetailsAsync(int orderId, string userId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(v => v.Size)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(v => v.Color)
                .Include(o => o.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.UserId == userId);

            // Ensure Product is not null for display
            if (order?.OrderDetails != null)
            {
                foreach (var orderDetail in order.OrderDetails)
                {
                    if (orderDetail.Product == null)
                    {
                        // Create a placeholder product for display purposes
                        orderDetail.Product = new Product
                        {
                            Name = "Sản phẩm không tồn tại",
                            ImageUrl = "https://via.placeholder.com/80",
                            ProductId = orderDetail.ProductId
                        };
                    }
                }
            }

            return order;
        }

        public async Task<Order> GetOrderWithDetailsForAdminAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(v => v.Size)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(v => v.Color)
                .Include(o => o.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            // Ensure Product is not null for display
            if (order?.OrderDetails != null)
            {
                foreach (var orderDetail in order.OrderDetails)
                {
                    if (orderDetail.Product == null)
                    {
                        // Create a placeholder product for display purposes
                        orderDetail.Product = new Product
                        {
                            Name = "Sản phẩm không tồn tại",
                            ImageUrl = "https://via.placeholder.com/80",
                            ProductId = orderDetail.ProductId
                        };
                    }
                }
            }

            return order;
        }

        public async Task<IEnumerable<Order>> GetAllWithDetailsAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(v => v.Color)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                        .ThenInclude(v => v.Size)
                .AsNoTracking()
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // Ensure Product is not null for display
            foreach (var order in orders)
            {
                if (order.OrderDetails != null)
                {
                    foreach (var orderDetail in order.OrderDetails)
                    {
                        if (orderDetail.Product == null)
                        {
                            // Create a placeholder product for display purposes
                            orderDetail.Product = new Product
                            {
                                Name = "Sản phẩm không tồn tại",
                                ImageUrl = "https://via.placeholder.com/80",
                                ProductId = orderDetail.ProductId
                            };
                        }
                    }
                }
            }

            return orders;
        }

        public async Task<Order> CreateOrderFromCartAsync(string userId, string shippingAddress, string phoneNumber, string paymentMethod, string notes)
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

            // Lock all needed product variants to prevent race conditions with concurrent checkouts
            var variantIds = cart.CartItems.Select(ci => ci.VariantId).ToList();
            var lockedVariants = await _context.ProductVariants
                .Where(pv => variantIds.Contains(pv.ProductVariantId))
                .ToListAsync();

            // Dictionary to store current stock for quick lookup
            var variantStockDict = lockedVariants.ToDictionary(v => v.ProductVariantId, v => v.StockQuantity);

            // Check if any other users have reserved quantities of these products
            var reservedQuantities = new Dictionary<int, int>();
            foreach (var variantId in variantIds)
            {
                // Exclude current user's cart from reservations
                // We're using a new method to calculate reserved quantities by other users
                int reservedByOthers = await GetReservedQuantityByOtherUsersAsync(variantId, userId);
                reservedQuantities[variantId] = reservedByOthers;
            }

            // Check stock availability for all items before processing
            foreach (var item in cart.CartItems)
            {
                if (!variantStockDict.TryGetValue(item.VariantId, out int currentStock))
                {
                    throw new Exception($"Variant with ID {item.VariantId} not found");
                }

                int reservedByOthers = reservedQuantities.GetValueOrDefault(item.VariantId, 0);
                int availableStock = currentStock - reservedByOthers;
                
                // Check if enough stock is available
                if (availableStock < item.Quantity)
                {
                    string message = availableStock <= 0 
                        ? $"Sản phẩm '{item.Product.Name}' đã hết hàng hoặc đã được đặt hết bởi người dùng khác" 
                        : $"Sản phẩm '{item.Product.Name}' chỉ còn {availableStock} trong kho";
                    throw new Exception(message);
                }
            }

            // Calculate total
             decimal subtotal = cart.CartItems.Sum(ci =>
                (ci.Product.DiscountPrice > 0 ? ci.Product.DiscountPrice : ci.Product.Price + (ci.ProductVariant?.AdditionalPrice ?? 0)) * ci.Quantity);
            decimal shippingFee = 40000; // Shipping fee as shown in the screenshot
            decimal totalAmount = subtotal + shippingFee;

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

            // Create order details and decrease stock
            foreach (var item in cart.CartItems)
            {
                var unitPrice = item.Product.DiscountPrice > 0 
                    ? item.Product.DiscountPrice 
                    : item.Product.Price + (item.ProductVariant?.AdditionalPrice ?? 0);
                    
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
                
                // Decrease the stock quantity
                var variant = lockedVariants.First(v => v.ProductVariantId == item.VariantId);
                variant.StockQuantity -= item.Quantity;
                
                // If stock is depleted, mark variant as inactive
                if (variant.StockQuantity <= 0)
                {
                    variant.StockQuantity = 0;
                    variant.IsActive = false; // Deactivate if no stock to prevent new orders
                }
            }

            await _context.Orders.AddAsync(order);

            // Clear cart
            _context.CartItems.RemoveRange(cart.CartItems);

            await _context.SaveChangesAsync();

            return order;
        }

        private async Task<int> GetReservedQuantityByOtherUsersAsync(int variantId, string excludeUserId)
        {
            var cutoffTime = DateTime.UtcNow.AddMinutes(-60);

            return await _context.CartItems
                .Where(ci => ci.VariantId == variantId)
                .Join(_context.Carts,
                    ci => ci.CartId,
                    c => c.CartId,
                    (ci, c) => new { CartItem = ci, Cart = c })
                .Where(x => x.Cart.UserId != excludeUserId && x.Cart.UpdatedAt >= cutoffTime)
                .SumAsync(x => x.CartItem.Quantity);
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
