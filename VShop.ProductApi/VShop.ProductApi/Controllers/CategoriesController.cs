using Microsoft.AspNetCore.Mvc;
using VShop.ProductApi.DTOs;
using VShop.ProductApi.Services.Interfaces;

namespace VShop.ProductApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {

        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]

        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategories()
        {
            var categoriesDTO = await _categoryService.GetCategories();

            if (categoriesDTO is null)
                return NotFound("Categories not found");

            return Ok(categoriesDTO);

        }

        [HttpGet("products")]

        public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetCategoriesProducts()
        {
            var categoriesDTO = await _categoryService.GetCategoriesProducts();

            if (categoriesDTO is null)
                return NotFound("Categories not found");

            return Ok(categoriesDTO);

        }


        [HttpGet("{id:int}", Name = "GetCategory")]

        public async Task<ActionResult<CategoryDTO>> GetCategory(int id)
        {
            var categoryDTO = await _categoryService.GetCategoryById(id);

            if (categoryDTO == null)
                return NotFound("Category not found");

            return Ok(categoryDTO);

        }


        [HttpPost]
        public async Task<ActionResult> CreateCategory([FromBody] CategoryDTO categoryDTO)
        {

            if (categoryDTO == null)
            {
                return BadRequest("Invalid data!");
            }

            await _categoryService.CreateCategory(categoryDTO);

            return new CreatedAtRouteResult("GetCategory", new { id = categoryDTO.Id }, categoryDTO);

        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateCategory(int id, [FromBody] CategoryDTO categoryDTO)
        {

            if (id != categoryDTO.Id)
            {
                return BadRequest("Invalid id!");
            }

            if (categoryDTO == null)
            {
                return BadRequest("Invalid data!");
            }

            await _categoryService.UpdateCategory(categoryDTO);

            return Ok(categoryDTO);

        }


        [HttpDelete("{id:int}")]

        public async Task<ActionResult<CategoryDTO>> DeleteCategory(int id)
        {

            var categoryDTO = await _categoryService.GetCategoryById(id);

            if (categoryDTO is null)
            {
                return NotFound("Category not found");
            }

            await _categoryService.RemoveCategory(id);

            return Ok(categoryDTO);

        }

    }
}
