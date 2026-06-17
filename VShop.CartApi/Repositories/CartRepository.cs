using AutoMapper;
using Microsoft.EntityFrameworkCore;
using VShop.CartApi.Context;
using VShop.CartApi.DTOs;
using VShop.CartApi.Models;

namespace VShop.CartApi.Repositories;

public class CartRepository : ICartRepository
{

    private readonly AppDbContext _context;
    private IMapper _mapper;

    public CartRepository(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CartDTO> GetCartByUserIdAsync(string userId)
    {

        Cart cart = new Cart
        {
            CartHeader = await _context.CartHeaders.FirstOrDefaultAsync(c => c.UserId == userId),
        };

        cart.CartItems = _context.CartItems.Where(c => c.CartHeaderId == cart.CartHeader.Id)
            .Include(c => c.Product);

        return _mapper.Map<CartDTO>(cart);

    }

    public async Task<bool> DeleteItemCartAsync(int cartItemId)
    {

        try
        {

            CartItem cartItem = await _context.CartItems.FirstOrDefaultAsync(c => c.Id == cartItemId);

            int total = _context.CartItems.Where(c => c.CartHeaderId == cartItem.CartHeaderId).Count();

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            if (total == 1)
            {

                var cartHeaderRemove = await _context.CartHeaders.FirstOrDefaultAsync(c => c.Id == cartItem.CartHeaderId);

                _context.CartHeaders.Remove(cartHeaderRemove);
                await _context.SaveChangesAsync();
            }

            return true;

        }
        catch (Exception ex)
        {
            return false;
        }

    }


    public async Task<bool> CleanCartAsync(string userId)
    {

        var cartHeader = await _context.CartHeaders.FirstOrDefaultAsync(c => c.UserId == userId);

        if (cartHeader is not null)
        {
            _context.CartItems.RemoveRange(_context.CartItems.Where(c => c.CartHeaderId == cartHeader.Id));
            _context.CartHeaders.Remove(cartHeader);
            await _context.SaveChangesAsync();
            return true;

        }
        return false;
    }

    public async Task<CartDTO> UpdateCartAsync(CartDTO cartDTO)
    {

        Console.WriteLine($"[{DateTime.Now}] Iniciando UpdateCartAsync");
        Cart cart = _mapper.Map<Cart>(cartDTO);

        Console.WriteLine($"[{DateTime.Now}] Chamando SaveProductInDataBase");
        //salvar produto no banco se ele não existir
        await SaveProductInDataBase(cartDTO, cart);

        Console.WriteLine($"[{DateTime.Now}] SaveProductInDataBase concluído");
        //verifica se o CartHeader é nulo
        var cartHeader = await _context.CartHeaders.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == cart.CartHeader.UserId);

        if (cartHeader is null)
        {
            //criar o header e os itens
            await CreateCartHeaderAndItems(cart);
        }
        else
        {
            //atualizar a quantidade de itens
            await UpdateQuantityAndItems(cartDTO, cart, cartHeader);
        }
        return _mapper.Map<CartDTO>(cart);
    }

    private async Task UpdateQuantityAndItems(CartDTO cartDTO, Cart cart, CartHeader cartHeader)
    {
        var cartItem = await _context.CartItems.AsNoTracking().FirstOrDefaultAsync(
                               p => p.ProductId == cartDTO.CartItems.FirstOrDefault().ProductId
                               && p.CartHeaderId == cartHeader.Id);
        if (cartItem is null)
        {
            cart.CartItems.FirstOrDefault().CartHeaderId = cartHeader.Id;
            cart.CartItems.FirstOrDefault().Product = null;
            _context.CartItems.Add(cart.CartItems.FirstOrDefault());
            await _context.SaveChangesAsync();
        }
        else
        {
            var newQuantity = cart.CartItems.FirstOrDefault().Quantity;

            // ✅ busca COM tracking para o EF saber atualizar
            var trackedItem = await _context.CartItems.FirstOrDefaultAsync(p => p.Id == cartItem.Id);
            trackedItem.Quantity = cartItem.Quantity + newQuantity;

            await _context.SaveChangesAsync(); // sem precisar de .Update()
        }
    }

    private async Task CreateCartHeaderAndItems(Cart cart)
    {
        _context.CartHeaders.Add(cart.CartHeader);
        await _context.SaveChangesAsync();

        cart.CartItems.FirstOrDefault().CartHeaderId = cart.CartHeader.Id;
        cart.CartItems.FirstOrDefault().Product = null;

        _context.CartItems.Add(cart.CartItems.FirstOrDefault());

        await _context.SaveChangesAsync();

    }

    private async Task SaveProductInDataBase(CartDTO cartDTO, Cart cart)
    {

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == cartDTO.CartItems.FirstOrDefault().ProductId);

        if (product is null)
        {
            _context.Products.Add(cart.CartItems.FirstOrDefault().Product);
            await _context.SaveChangesAsync();
        }

    }


    //fazer depois


    public async Task<bool> ApplyCouponAsync(string userId, string couponCode)
    {
        var cartHeaderApplyCoupon = await _context.CartHeaders.FirstOrDefaultAsync(c => c.UserId == userId);

        if (cartHeaderApplyCoupon is not null)
        {

            cartHeaderApplyCoupon.CouponCode = couponCode;

            _context.CartHeaders.Update(cartHeaderApplyCoupon);

            await _context.SaveChangesAsync();

            return true;

        }

        return false;
    }

    public async Task<bool> DeleteCouponAsync(string userId)
    {
        var cartHeaderDeleteCupon = await _context.CartHeaders.FirstOrDefaultAsync(c => c.UserId == userId);

        if (cartHeaderDeleteCupon is not null)
        {

            cartHeaderDeleteCupon.CouponCode = "";

            _context.CartHeaders.Update(cartHeaderDeleteCupon);

            await _context.SaveChangesAsync();

            return true;

        }

        return false;
    }


}
