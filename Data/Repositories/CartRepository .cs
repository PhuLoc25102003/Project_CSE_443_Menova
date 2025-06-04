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

        public async Task<Cart> GetCartWithItemsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }
            
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

        public async Task<CartViewModel> GetCartViewModelAsync(string userId)
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

        public async Task AddItemToCartAsync(string userId, int productId, int variantId, int quantity)
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
            var cartItem = await _context.CartItems
                .Include(ci => ci.ProductVariant)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

            if (cartItem == null)
            {
                throw new Exception("Cart item not found");
            }

            // Check stock availability when increasing quantity
            if (quantity > cartItem.Quantity)
            {
                // We will handle this at the service layer
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

        public async Task ClearCartAsync(string userId)
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

        public async Task MergeCartsAsync(string sessionCartId, string userId)
        {
            // Get session cart
            var sessionCart = await GetCartWithItemsAsync(sessionCartId);
            if (sessionCart == null || !sessionCart.CartItems.Any())
            {
                return; // No session cart or it's empty
            }

            // Get user cart
            var userCart = await GetCartWithItemsAsync(userId);
            if (userCart == null)
            {
                // Create new cart for user if none exists
                userCart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    CartItems = new List<CartItem>()
                };
                await _context.Carts.AddAsync(userCart);
                await _context.SaveChangesAsync();
            }

            // Merge cart items
            foreach (var sessionItem in sessionCart.CartItems)
            {
                var existingItem = userCart.CartItems
                    .FirstOrDefault(ci => ci.ProductId == sessionItem.ProductId && ci.VariantId == sessionItem.VariantId);

                if (existingItem != null)
                {
                    // Update quantity if item already exists in user cart
                    existingItem.Quantity += sessionItem.Quantity;
                    existingItem.UpdatedAt = DateTime.Now;
                }
                else
                {
                    // Add session item to user cart
                    var newItem = new CartItem
                    {
                        CartId = userCart.CartId,
                        ProductId = sessionItem.ProductId,
                        VariantId = sessionItem.VariantId,
                        Quantity = sessionItem.Quantity,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    await _context.CartItems.AddAsync(newItem);
                }
            }

            // Remove session cart
            _context.CartItems.RemoveRange(sessionCart.CartItems);
            _context.Carts.Remove(sessionCart);

            userCart.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task<CartItem> GetCartItemAsync(int cartItemId)
        {
            return await _context.CartItems
                .Include(ci => ci.Product)
                .Include(ci => ci.ProductVariant)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);
        }

        public async Task<int> GetTotalReservedQuantityForVariantAsync(int variantId, int? excludeCartItemId = null)
        {
            var query = _context.CartItems.Where(ci => ci.VariantId == variantId);
            
            // Exclude a specific cart item if needed (e.g., when updating quantity)
            if (excludeCartItemId.HasValue)
            {
                query = query.Where(ci => ci.CartItemId != excludeCartItemId.Value);
            }
            
            // Sum up quantities from active carts (created/updated within the last 60 minutes)
            var cutoffTime = DateTime.UtcNow.AddMinutes(-60);
            
            // Sum up quantities from all recent and active cart items containing this variant
            return await query
                .Join(_context.Carts,
                      ci => ci.CartId,
                      c => c.CartId,
                      (ci, c) => new { CartItem = ci, Cart = c })
                .Where(x => x.Cart.UpdatedAt >= cutoffTime) // Only consider recently active carts
                .SumAsync(x => x.CartItem.Quantity);
        }
    }
}
