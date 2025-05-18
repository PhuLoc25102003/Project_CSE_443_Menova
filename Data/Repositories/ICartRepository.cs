using Menova.Models.ViewModels;
using Menova.Models;

namespace Menova.Data.Repositories
{
    public interface ICartRepository : IRepository<Cart>
    {
        Task<Cart> GetCartWithItemsAsync(int userId);
        Task<CartViewModel> GetCartViewModelAsync(int userId);
        Task AddItemToCartAsync(int userId, int productId, int variantId, int quantity);
        Task UpdateCartItemAsync(int cartItemId, int quantity);
        Task RemoveCartItemAsync(int cartItemId);
        Task ClearCartAsync(int userId);
    }

}
