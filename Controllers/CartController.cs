using System;
using System.Linq;
using Menova.Data;
using Menova.Models.ViewModels;
using Menova.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Menova.Data.Services;


namespace Menova.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            // For demonstration purposes - normally would use user authentication
            // and get user ID from claims
            int userId = 1; // Placeholder for actual user ID from auth

            var viewModel = await _cartService.GetCartAsync(userId);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int variantId, int quantity)
        {
            // For demonstration purposes - normally would use user authentication
            int userId = 1; // Placeholder for actual user ID from auth

            await _cartService.AddToCartAsync(userId, productId, variantId, quantity);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCart(int cartItemId, int quantity)
        {
            await _cartService.UpdateCartItemAsync(cartItemId, quantity);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            await _cartService.RemoveCartItemAsync(cartItemId);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Checkout()
        {
            // For demonstration purposes - normally would use user authentication
            int userId = 1; // Placeholder for actual user ID from auth

            ViewBag.Cart = await _cartService.GetCartAsync(userId);
            return View();
        }

    }
}
