using Menova.Models.ViewModels;
using Menova.Models;
using Microsoft.EntityFrameworkCore;

namespace Menova.Data.Repositories
{
    public class CartRepository : Repository<Cart>, ICartRepository
    {
        public CartRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Cart> GetCartWithItemsAsync(int userId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(v => v.Size)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(v => v.Color)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<CartViewModel> GetCartViewModelAsync(int userId)
        {
            var cart = await GetCartWithItemsAsync(userId);

            if (cart == null)
            {
                // Create new cart for user if none exists
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CartItems = new System.Collections.Generic.List<CartItem>()
                };

                await _context.Carts.AddAsync(cart);
                await _context.SaveChangesAsync();

                return new CartViewModel
                {
                    Cart = cart,
                    Items = new System.Collections.Generic.List<CartItemViewModel>(),
                    Subtotal = 0,
                    ShippingFee = 30000
                };
            }

            var viewModel = new CartViewModel
            {
                Cart = cart,
                Items = cart.CartItems?.Select(ci => new CartItemViewModel
                {
                    CartItemId = ci.CartItemId,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    ProductImage = ci.Product.ImageUrl ??
                        (ci.Product.Images != null && ci.Product.Images.Any(i => i.IsPrimary)
                        ? ci.Product.Images.FirstOrDefault(i => i.IsPrimary).ImageUrl
                        : "/images/placeholder.png"),
                    VariantId = ci.VariantId,
                    SizeName = ci.ProductVariant.Size.Name,
                    ColorName = ci.ProductVariant.Color.Name,
                    ColorCode = ci.ProductVariant.Color.ColorCode,
                    Quantity = ci.Quantity,
                    UnitPrice = (ci.Product.DiscountPrice > 0) ? ci.Product.DiscountPrice : (ci.Product.Price + (ci.ProductVariant.AdditionalPrice ?? 0))
                }).ToList() ?? new System.Collections.Generic.List<CartItemViewModel>(),
                ShippingFee = 30000 
            };

            viewModel.Subtotal = viewModel.Items.Sum(i => i.Subtotal);
            return viewModel;
        }

        public async Task AddItemToCartAsync(int userId, int productId, int variantId, int quantity)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _context.Carts.AddAsync(cart);
                await _context.SaveChangesAsync();
            }

            // Check if item already exists in cart
            var existingItem = cart.CartItems?
                .FirstOrDefault(ci => ci.ProductId == productId && ci.VariantId == variantId);

            if (existingItem != null)
            {
                // Update existing item
                existingItem.Quantity += quantity;
                existingItem.UpdatedAt = DateTime.Now;
            }
            else
            {
                // Add new item
                var newItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    VariantId = variantId,
                    Quantity = quantity,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                await _context.CartItems.AddAsync(newItem);
            }

            cart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCartItemAsync(int cartItemId, int quantity)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);

            if (cartItem == null)
            {
                throw new Exception("Cart item not found");
            }

            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = quantity;
                cartItem.UpdatedAt = DateTime.Now;
            }

            var cart = await _context.Carts.FindAsync(cartItem.CartId);
            cart.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task RemoveCartItemAsync(int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);

            if (cartItem == null)
            {
                throw new Exception("Cart item not found");
            }

            _context.CartItems.Remove(cartItem);

            var cart = await _context.Carts.FindAsync(cartItem.CartId);
            cart.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
        }

        public async Task ClearCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null && cart.CartItems.Any())
            {
                _context.CartItems.RemoveRange(cart.CartItems);
                cart.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }
    }

}
