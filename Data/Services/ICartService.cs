using Menova.Models.ViewModels;

namespace Menova.Data.Services
{
    public interface ICartService
    {
        Task<CartViewModel> GetCartAsync(int userId);
        Task AddToCartAsync(int userId, int productId, int variantId, int quantity);
        Task UpdateCartItemAsync(int cartItemId, int quantity);
        Task RemoveCartItemAsync(int cartItemId);
        Task ClearCartAsync(int userId);
    }
}
