using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VShop.CartApi.Models.ViewModels;
using VShop.Web.Services;
using VShop.Web.Services.Interfaces;

namespace VShop.Web.Controllers
{
    public class CartController : Controller
    {

        private readonly ICartService _cartService;
        private readonly ICouponService _couponService;

        public CartController(ICartService cartService, ICouponService couponService)
        {
            this._cartService = cartService;
            _couponService = couponService;
        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(CartViewModel cartVM)
        {

            if (ModelState.IsValid)
            {

                var result = await _cartService.ApplyCouponAsync(cartVM, await GetAcessToken());

                if (result)
                {
                    return RedirectToAction(nameof(Index));
                }


            }

            return View();

        }

        [HttpPost]
        public async Task<IActionResult> DeleteCoupon()
        {

            var result = await _cartService.RemoveCouponAsync(GetUserId(), await GetAcessToken());

            if (result)
            {
                return RedirectToAction(nameof(Index));
            }

            return View();

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

            return View(cartVM);
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
                Console.WriteLine($">>> CouponCode no CartHeader: '{cart.CartHeader.CouponCode}'");

                var coupon = await _couponService.GetDiscountCoupon(cart.CartHeader.CouponCode, await GetAcessToken());

                Console.WriteLine($">>> Coupon retornado: {coupon?.CouponCode} - Desconto: {coupon?.Discount}");

                if (coupon?.CouponCode is not null)
                {
                    cart.CartHeader.Discount = coupon.Discount;
                }
                foreach (var item in cart.CartItems)
                {
                    cart.CartHeader.TotalAmount += (item.Product.Price * item.Quantity);
                }
                cart.CartHeader.TotalAmount = cart.CartHeader.TotalAmount -
                                             (cart.CartHeader.TotalAmount *
                                              cart.CartHeader.Discount) / 100;
            }
            return cart;
        }

        private async Task<string> GetAcessToken()
        {
            return await HttpContext.GetTokenAsync("access_token");
        }

        private string GetUserId()
        {
            return User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value;
        }

    }
}
