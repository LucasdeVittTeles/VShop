using VShop.CartApi.Models.ViewModels;
using VShop.Web.Models;

namespace VShop.Web.Services.Interfaces;

public interface ICartService
{

    Task<CartViewModel> GetCartByUserIdAsync(string userId, string token);
    Task<CartViewModel> AddItemToCartAsync(CartViewModel cartVM, string token);
    Task<CartViewModel> UpdateCartAsync(CartViewModel cartVM, string token);
    Task<bool> RemoveItemCartAsync(int cartId, string token);

    //implementação futura
    Task<bool> ApplyCouponAsync(CartViewModel cartVM, string couponCode, string token);
    Task<bool> RemoveCouponAsync(int userId, string token);
    Task<bool> ClearCartAsync(string userId, string token);

    Task<CartViewModel> CheckoutAsync(CartHeaderViewModel cartHeader, string token);

}
