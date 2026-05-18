using Microsoft.AspNetCore.Mvc;
using VShop.Web.Models;
using VShop.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using VShop.Web.Roles;

namespace VShop.Web.Controllers
{
    public class ProductsController : Controller
    {

        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductsController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductViewModel>>> Index()
        {

            var result = await _productService.GetAllProducts();

            if (result is null)
            {
                return View("Error");
            }

            return View(result);
        }

        [HttpGet]
        public async Task<ActionResult> CreateProduct()
        {

            ViewBag.CategoryId = new SelectList(await _categoryService.GetAllCategories(), "Id", "Name");

            return View();
        }

       
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> CreateProduct(ProductViewModel productViewModel)
        {

            if (ModelState.IsValid)
            {
                var result = await _productService.CreateProduct(productViewModel);

                if (result != null)
                {
                    return RedirectToAction(nameof(Index));
                }

            }
            else
            {
                ViewBag.CategoryId = new SelectList(await _categoryService.GetAllCategories(), "Id", "Name");
            }

            return View(productViewModel);
        }

       
        [HttpGet]
        public async Task<IActionResult> UpdateProduct(int id)
        {
            ViewBag.CategoryId = new SelectList(await _categoryService.GetAllCategories(), "Id", "Name");

            var result = await _productService.FindProductById(id);

            if (result is null)
            {
                return View("Error");
            }

            return View(result);
        }

        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateProduct(ProductViewModel productViewModel)
        {

            if (ModelState.IsValid)
            {

                var result = await _productService.UpdateProduct(productViewModel);

                if (result is not null)
                {
                    return RedirectToAction(nameof(Index));
                }

            }

            return View(productViewModel);
        }


        [HttpGet]
        [Authorize]
        public async Task<ActionResult<ProductViewModel>> DeleteProduct(int id)
        {

            var result = await _productService.FindProductById(id);

            if (result is null)
                return View("Error");

            return View(result);

        }

        [HttpPost(), ActionName("DeleteProduct")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _productService.DeleteProductById(id);

            if (!result)
            {
                return View("Error");
            }

            return RedirectToAction("Index");

        }

    }
}
