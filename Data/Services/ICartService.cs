using Menova.Models.ViewModels;

namespace Menova.Data.Services
{
    public interface ICartService
    {
        Task<CartViewModel> GetCartAsync(string userId);
        Task AddToCartAsync(string userId, int productId, int variantId, int quantity);
        Task UpdateCartItemAsync(int cartItemId, int quantity);
        Task RemoveCartItemAsync(int cartItemId);
        Task ClearCartAsync(string userId);
        Task MergeCartsAsync(string sessionCartId, string userId);
        Task<bool> CheckStockAvailabilityAsync(int variantId, int requestedQuantity);
    }
}
