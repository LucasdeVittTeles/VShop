using VShop.CartApi.Models;

namespace VShop.CartApi.DTOs
{
    public class CartItemDTO
    {

        public int Id { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public int CartHeaderId { get; set; }
        public ProductDTO Product { get; set; }

    }
}
