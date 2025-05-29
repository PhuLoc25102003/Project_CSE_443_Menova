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
            return await _unitOfWork.Carts.GetCartViewModelAsync(userId);
        }

        public async Task AddToCartAsync(string userId, int productId, int variantId, int quantity)
        {
            await _unitOfWork.Carts.AddItemToCartAsync(userId, productId, variantId, quantity);
        }

        public async Task UpdateCartItemAsync(int cartItemId, int quantity)
        {
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
    }
}
