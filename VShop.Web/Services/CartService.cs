using System.Text;
using System.Text.Json;
using VShop.CartApi.Models.ViewModels;
using VShop.Web.Services.Interfaces;

namespace VShop.Web.Services
{
    public class CartService : ICartService
    {

        private readonly IHttpClientFactory _clientFactory;
        private readonly JsonSerializerOptions _options;
        private const string apiEndpoint = "/api/cart/";
        private CartViewModel _cartVM = new CartViewModel();

        public CartService(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<CartViewModel> GetCartByUserIdAsync(string userId, string token)
        {

            var client = _clientFactory.CreateClient("CartApi");

            PutTokenInHeaderAuthorization(token, client);

            using (var response = await client.GetAsync($"{apiEndpoint}/getcart/{userId}"))
            {
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadAsStreamAsync();

                    _cartVM = await JsonSerializer.DeserializeAsync<CartViewModel>(apiResponse, _options);
                }
                else
                {
                    return null;
                }
            }
            return _cartVM;
        }

        public async Task<CartViewModel> AddItemToCartAsync(CartViewModel cartVM, string token)
        {
            var client = _clientFactory.CreateClient("CartApi");

            PutTokenInHeaderAuthorization(token, client);

            var content = new StringContent(JsonSerializer.Serialize(cartVM), Encoding.UTF8, "application/json");

            using (var response = await client.PostAsync($"{apiEndpoint}/addcart/", content))
            {
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadAsStreamAsync();

                    _cartVM = await JsonSerializer.DeserializeAsync<CartViewModel>(apiResponse, _options);
                }
                else
                {
                    return null;
                }
            }
            return _cartVM;
        }


        public async Task<CartViewModel> UpdateCartAsync(CartViewModel cartVM, string token)
        {
            var client = _clientFactory.CreateClient("CartApi");

            PutTokenInHeaderAuthorization(token, client);

            CartViewModel cartUpdated = new CartViewModel();

            var content = new StringContent(JsonSerializer.Serialize(cartVM), Encoding.UTF8, "application/json");

            using (var response = await client.PutAsJsonAsync($"{apiEndpoint}/updatecart", content))
            {
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadAsStreamAsync();

                    cartUpdated = await JsonSerializer.DeserializeAsync<CartViewModel>(apiResponse, _options);
                }
                else
                {
                    return null;
                }
            }
            return cartUpdated;
        }


        public async Task<bool> RemoveItemCartAsync(int cartId, string token)
        {

            var client = _clientFactory.CreateClient("CartApi");

            PutTokenInHeaderAuthorization(token, client);

            using (var response = await client.DeleteAsync($"{apiEndpoint}/deleteCart/{cartId}"))
            {
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }

            return false;

        }




        public async Task<bool> ClearCartAsync(string userId, string token)
        {
            throw new NotImplementedException();
        }
        public Task<bool> ApplyCouponAsync(CartViewModel cartVM, string couponCode, string token)
        {
            throw new NotImplementedException();
        }

        public Task<CartViewModel> CheckoutAsync(CartHeaderViewModel cartHeader, string token)
        {
            throw new NotImplementedException();
        }
        public Task<bool> RemoveCouponAsync(int userId, string token)
        {
            throw new NotImplementedException();
        }

        private static void PutTokenInHeaderAuthorization(string token, HttpClient client)
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }


    }
}
