using System;
using System.Linq;
using System.Threading.Tasks;
using Menova.Data;
using Menova.Models.ViewModels;
using Menova.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Menova.Data.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Menova.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ApplicationDbContext _context;
        private const string SessionCartKey = "CartSessionKey";

        public CartController(ICartService cartService, UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, ApplicationDbContext context)
        {
            _cartService = cartService;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Check if user is logged in
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    // Nếu không tìm thấy user, đăng xuất và chuyển hướng về login
                    return RedirectToAction("Logout", "Account");
                }
                
                var viewModel = await _cartService.GetCartAsync(user.Id);
                return View(viewModel);
            }
            else
            {
                // Redirect to login page with return URL
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Cart") });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int variantId, int quantity, bool buyNow = false)
        {
            if (quantity <= 0)
            {
                quantity = 1;
            }

            try
            {
                string userId;
                
                // Stock checks now happen in CartService
                if (User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        // Nếu không tìm thấy user, đăng xuất và chuyển hướng về login
                        return RedirectToAction("Logout", "Account");
                    }
                    
                    userId = user.Id;
                }
                else
                {
                    // For anonymous users, use session cart
                    userId = GetOrCreateCartSessionId();
                }
                
                await _cartService.AddToCartAsync(userId, productId, variantId, quantity);

                TempData["CartMessage"] = "Sản phẩm đã được thêm vào giỏ hàng";
                
                // Redirect based on buyNow parameter
                if (buyNow)
                {
                    return RedirectToAction("Checkout");
                }
                else
                {
                    return RedirectToAction("Detail", "Product", new { id = productId });
                }
            }
            catch (Exception ex)
            {
                TempData["CartMessage"] = ex.Message;
                TempData["CartError"] = true;
                return RedirectToAction("Detail", "Product", new { id = productId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCart(int cartItemId, int quantity)
        {
            try
            {
                // Xác minh rằng cartItem thuộc về người dùng hiện tại
                var cartItem = await _unitOfWork.Carts.GetCartItemAsync(cartItemId);
                
                if (cartItem != null)
                {
                    var cart = await _context.Carts.FindAsync(cartItem.CartId);
                    
                    if (cart == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy giỏ hàng";
                        return RedirectToAction("Index");
                    }
                    
                    // Chỉ cho phép cập nhật nếu là giỏ hàng của người dùng hiện tại
                    if (User.Identity.IsAuthenticated)
                    {
                        var user = await _userManager.GetUserAsync(User);
                        if (user == null || cart.UserId != user.Id)
                        {
                            TempData["ErrorMessage"] = "Bạn không có quyền cập nhật giỏ hàng này";
                            return RedirectToAction("Index");
                        }
                    }
                    else
                    {
                        string sessionId = GetOrCreateCartSessionId();
                        if (cart.UserId != sessionId)
                        {
                            TempData["ErrorMessage"] = "Bạn không có quyền cập nhật giỏ hàng này";
                            return RedirectToAction("Index");
                        }
                    }
                    
                    await _cartService.UpdateCartItemAsync(cartItemId, quantity);
                    TempData["CartMessage"] = "Giỏ hàng đã được cập nhật";
                }
                else
                {
                    TempData["CartMessage"] = "Không tìm thấy sản phẩm trong giỏ hàng";
                    TempData["CartError"] = true;
                }
            }
            catch (Exception ex)
            {
                TempData["CartMessage"] = "Không thể cập nhật giỏ hàng: " + ex.Message;
                TempData["CartError"] = true;
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            try
            {
                // Xác minh rằng cartItem thuộc về người dùng hiện tại
                var cartItem = await _unitOfWork.Carts.GetCartItemAsync(cartItemId);
                
                if (cartItem != null)
                {
                    var cart = await _context.Carts.FindAsync(cartItem.CartId);
                    
                    if (cart == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy giỏ hàng";
                        return RedirectToAction("Index");
                    }
                    
                    // Chỉ cho phép xóa nếu là giỏ hàng của người dùng hiện tại
                    if (User.Identity.IsAuthenticated)
                    {
                        var user = await _userManager.GetUserAsync(User);
                        if (user == null || cart.UserId != user.Id)
                        {
                            TempData["ErrorMessage"] = "Bạn không có quyền xóa sản phẩm khỏi giỏ hàng này";
                            return RedirectToAction("Index");
                        }
                    }
                    else
                    {
                        string sessionId = GetOrCreateCartSessionId();
                        if (cart.UserId != sessionId)
                        {
                            TempData["ErrorMessage"] = "Bạn không có quyền xóa sản phẩm khỏi giỏ hàng này";
                            return RedirectToAction("Index");
                        }
                    }
                    
                    await _cartService.RemoveCartItemAsync(cartItemId);
                    TempData["CartMessage"] = "Sản phẩm đã được xóa khỏi giỏ hàng";
                }
                else
                {
                    TempData["CartMessage"] = "Không tìm thấy sản phẩm trong giỏ hàng";
                    TempData["CartError"] = true;
                }
            }
            catch (Exception ex)
            {
                TempData["CartMessage"] = "Không thể xóa sản phẩm khỏi giỏ hàng: " + ex.Message;
                TempData["CartError"] = true;
            }
            
            return RedirectToAction("Index");
        }

        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "Cart") });
            }

            // Merge anonymous cart with user cart if exists
            if (HttpContext.Session.GetString(SessionCartKey) != null)
            {
                string cartSessionId = HttpContext.Session.GetString(SessionCartKey);
                await _cartService.MergeCartsAsync(cartSessionId, user.Id);
                HttpContext.Session.Remove(SessionCartKey);
            }

            ViewBag.Cart = await _cartService.GetCartAsync(user.Id);
            return View();
        }

        private string GetOrCreateCartSessionId()
        {
            string cartSessionId = HttpContext.Session.GetString(SessionCartKey);
            
            if (string.IsNullOrEmpty(cartSessionId))
            {
                cartSessionId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString(SessionCartKey, cartSessionId);
            }
            
            return cartSessionId;
        }
    }
}
