using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VShop.CartApi.Models.ViewModels;
using VShop.Web.Models;
using VShop.Web.Services.Interfaces;

namespace VShop.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService _productService;
        private readonly ICartService _cartService;

        public HomeController(ILogger<HomeController> logger, IProductService productService, ICartService cartService)
        {
            _logger = logger;
            this._productService = productService;
            this._cartService = cartService;
        }

        public async Task<ActionResult> Index()
        {
            var products = await _productService.GetAllProducts(string.Empty);

            if (products is null)
            {
                return View("Error");
            }

            return View(products);
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ProductViewModel>> ProductDetails(int id)
        {

            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var product = await _productService.FindProductById(id, accessToken);

            if (product is null)
            {
                return View("Error");
            }

            return View(product);

        }


        [HttpPost]
        [ActionName("ProductDetails")]
        [Authorize]
        public async Task<ActionResult<ProductViewModel>> ProductDetailsPost(ProductViewModel productVM)
        {

            var accessToken = await HttpContext.GetTokenAsync("access_token");

            CartViewModel cart = new CartViewModel()
            {

                CartHeader = new CartHeaderViewModel
                {
                    UserId = User.Claims.Where(u => u.Type == "sub")?.FirstOrDefault()?.Value
                }


            };

            CartItemViewModel cartItem = new CartItemViewModel()
            {
                Quantity = productVM.Quantity,
                ProductId = productVM.Id,
                Product = await _productService.FindProductById(productVM.Id, accessToken)
            };

            List<CartItemViewModel> cartItemViewModels = new List<CartItemViewModel>();

            cartItemViewModels.Add(cartItem);

            cart.CartItems = cartItemViewModels;

            var result = await _cartService.AddItemToCartAsync(cart, accessToken);

            if (result is null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(productVM);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        public async Task<IActionResult> Login()
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");

            Console.WriteLine("TOKEN COMPLETO: " + accessToken);

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Logout()
        {
            return SignOut("Cookies", "oidc");
        }

    }
}
