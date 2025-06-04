using Menova.Models.ViewModels;

namespace Menova.Data.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CartService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CartViewModel> GetCartAsync(string userId)
        {
            // Đảm bảo userId không null hoặc rỗng
            if (string.IsNullOrEmpty(userId))
            {
                // Trả về giỏ hàng trống cho anonymous user
                return new CartViewModel
                {
                    Items = new List<CartItemViewModel>(),
                    Subtotal = 0,
                    ShippingFee = 30000
                };
            }
            
            return await _unitOfWork.Carts.GetCartViewModelAsync(userId);
        }

        public async Task AddToCartAsync(string userId, int productId, int variantId, int quantity)
        {
            // Check stock before adding to cart
            var variant = await _unitOfWork.ProductVariants.GetVariantWithDetailsAsync(variantId);
            if (variant == null)
            {
                throw new Exception($"Variant with ID {variantId} not found");
            }
            
            // Get all active cart items for this variant
            var reservedQuantity = await _unitOfWork.Carts.GetTotalReservedQuantityForVariantAsync(variantId);
            
            // Calculate real available quantity (stock minus items in other active carts)
            int availableQuantity = variant.StockQuantity - reservedQuantity;
            
            // Get the current cart to check for existing items
            var cart = await _unitOfWork.Carts.GetCartWithItemsAsync(userId);
            int existingQuantity = 0;
            
            if (cart != null && cart.CartItems != null)
            {
                var existingItem = cart.CartItems.FirstOrDefault(ci => ci.VariantId == variantId);
                if (existingItem != null)
                {
                    existingQuantity = existingItem.Quantity;
                    // Add back the user's own reserved quantity since it's already counted in the total reserved
                    availableQuantity += existingQuantity;
                }
            }
            
            // Check if total quantity exceeds available stock
            if (availableQuantity < quantity)
            {
                throw new Exception(availableQuantity <= 0 
                    ? $"Sản phẩm '{variant.Product?.Name}' đã hết hàng hoặc đã được đặt hết bởi người dùng khác" 
                    : $"Sản phẩm '{variant.Product?.Name}' chỉ còn {availableQuantity} có thể thêm vào giỏ hàng");
            }
            
            await _unitOfWork.Carts.AddItemToCartAsync(userId, productId, variantId, quantity);
        }

        public async Task UpdateCartItemAsync(int cartItemId, int quantity)
        {
            // Get cart item first to check variant and current quantity
            var cartItem = await _unitOfWork.Carts.GetCartItemAsync(cartItemId);
            if (cartItem == null)
            {
                throw new Exception("Cart item not found");
            }
            
            // Only check stock if quantity is being increased
            if (quantity > cartItem.Quantity)
            {
                var variant = await _unitOfWork.ProductVariants.GetVariantWithDetailsAsync(cartItem.VariantId);
                if (variant == null)
                {
                    throw new Exception("Product variant not found");
                }
                
                // Get all active cart items for this variant except the current one
                var reservedQuantity = await _unitOfWork.Carts.GetTotalReservedQuantityForVariantAsync(variant.ProductVariantId, cartItemId);
                
                // Calculate real available quantity
                int availableQuantity = variant.StockQuantity - reservedQuantity;
                
                // Add the current cart item quantity since we're only checking the additional quantity
                availableQuantity += cartItem.Quantity;
                
                if (quantity > availableQuantity)
                {
                    throw new Exception($"Không đủ số lượng hàng khả dụng. Chỉ còn {availableQuantity} sản phẩm.");
                }
            }
            
            await _unitOfWork.Carts.UpdateCartItemAsync(cartItemId, quantity);
        }

        public async Task RemoveCartItemAsync(int cartItemId)
        {
            await _unitOfWork.Carts.RemoveCartItemAsync(cartItemId);
        }

        public async Task ClearCartAsync(string userId)
        {
            await _unitOfWork.Carts.ClearCartAsync(userId);
        }

        public async Task MergeCartsAsync(string sessionCartId, string userId)
        {
            await _unitOfWork.Carts.MergeCartsAsync(sessionCartId, userId);
        }
        
        public async Task<bool> CheckStockAvailabilityAsync(int variantId, int requestedQuantity)
        {
            var variant = await _unitOfWork.ProductVariants.GetVariantWithDetailsAsync(variantId);
            if (variant == null)
            {
                return false;
            }
            
            // Get all active cart items for this variant
            var reservedQuantity = await _unitOfWork.Carts.GetTotalReservedQuantityForVariantAsync(variantId);
            
            // Calculate real available quantity
            int availableQuantity = variant.StockQuantity - reservedQuantity;
            
            return availableQuantity >= requestedQuantity;
        }
    }
}
