using Microsoft.AspNetCore.Mvc;
using VShop.ProductApi.DTOs;
using VShop.ProductApi.Services.Interfaces;

namespace VShop.ProductApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {

        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts()
        {
            var productsDTO = await _productService.GetProducts();

            if (productsDTO is null)
                return NotFound("Products not found");

            return Ok(productsDTO);

        }


        [HttpGet("{id:int}", Name = "GetProduct")]
        public async Task<ActionResult<ProductDTO>> GetProduct(int id)
        {
            var productDTO = await _productService.GetProductsById(id);

            if (productDTO == null)
                return NotFound("product not found");

            return Ok(productDTO);

        }


        [HttpPost]
        public async Task<ActionResult> CreateProduct([FromBody] ProductDTO productDTO)
        {

            if (productDTO == null)
            {
                return BadRequest("Invalid data!");
            }

            await _productService.CreateProduct(productDTO);

            return new CreatedAtRouteResult("GetProduct", new { id = productDTO.Id }, productDTO);

        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateProduct(int id, [FromBody] ProductDTO productDTO)
        {

            if (id != productDTO.Id)
            {
                return BadRequest("Invalid id!");
            }

            if (productDTO == null)
            {
                return BadRequest("Invalid data!");
            }

            await _productService.UpdateProduct(productDTO);

            return Ok(productDTO);

        }


        [HttpDelete("{id:int}")]

        public async Task<ActionResult<ProductDTO>> DeleteProduct(int id)
        {

            var productDTO = await _productService.GetProductsById(id);

            if (productDTO is null)
            {
                return NotFound("Product not found");
            }

            await _productService.RemoveProduct(id);

            return Ok(productDTO);

        }


    }
}
