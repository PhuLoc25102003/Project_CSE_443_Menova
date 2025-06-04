using Menova.Models.ViewModels;
using Menova.Models;

namespace Menova.Data.Repositories
{
    public interface ICartRepository : IRepository<Cart>
    {
        Task<Cart> GetCartWithItemsAsync(string userId);
        Task<CartViewModel> GetCartViewModelAsync(string userId);
        Task AddItemToCartAsync(string userId, int productId, int variantId, int quantity);
        Task UpdateCartItemAsync(int cartItemId, int quantity);
        Task RemoveCartItemAsync(int cartItemId);
        Task ClearCartAsync(string userId);
        Task MergeCartsAsync(string sessionCartId, string userId);
        Task<CartItem> GetCartItemAsync(int cartItemId);
        Task<int> GetTotalReservedQuantityForVariantAsync(int variantId, int? excludeCartItemId = null);
    }
}
