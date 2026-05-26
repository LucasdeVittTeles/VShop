using 

namespace VShop.CartApi.Models;

public class CartItem
{

    public int Id { get; set; }
    public int Quantity { get; set; }
    public int ProductId { get; set; }
    public int CartHeaderId { get; set; }
    public Product product { get; set; }
    public CartHeader CartHeader { get; set; }

}
