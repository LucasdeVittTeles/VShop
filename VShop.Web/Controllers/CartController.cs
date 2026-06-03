using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using VShop.CartApi.Models.ViewModels;
using VShop.Web.Services.Interfaces;

namespace VShop.Web.Controllers
{
    public class CartController : Controller
    {

        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            this._cartService = cartService;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {

            CartViewModel? cartVM = await GetCartByUser();

            if (cartVM is null)
            {
                ModelState.AddModelError("CartNotFound", "Does not exist a cart yet...Come on Shopping...");
                return View("/Views/Cart/CartNotFound.cshtml");
            }

            return View();
        }

        public async Task<IActionResult> RemoveItem(int id)
        {
            var result = await _cartService.RemoveItemCartAsync(id, await GetAcessToken());

            if (result)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(id);
        }

        private async Task<CartViewModel?> GetCartByUser()
        {
            var cart = await _cartService.GetCartByUserIdAsync(GetUserId(), await GetAcessToken());

            if (cart?.CartHeader is not null)
            {
                foreach (var item in cart.CartItems)
                {
                    cart.CartHeader.TotalAmount += (item.Product.Price * item.Quantity);
                }
            }
            return cart;

        }

        private async Task<string> GetAcessToken()
        {
            return await HttpContext.GetTokenAsync("acess_token");
        }

        private string GetUserId()
        {
            return User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value;
        }

    }
}
