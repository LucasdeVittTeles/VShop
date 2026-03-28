using AutoMapper;
using VShop.ProductApi.Context;
using VShop.ProductApi.DTOs;
using VShop.ProductApi.Models;
using VShop.ProductApi.Repositories.Interfaces;
using VShop.ProductApi.Services.Interfaces;

namespace VShop.ProductApi.Services
{
    public class CategoryService : ICategoryService
    {

        private readonly IMapper _mapper;
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(AppDbContext context, ICategoryRepository categoryRepository, IMapper mapper)
        {
            _mapper = mapper;
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<CategoryDTO>> GetCategories()
        {
            var categoriesEntity = await _categoryRepository.GetAll();
            return _mapper.Map<IEnumerable<CategoryDTO>>(categoriesEntity);
        }

        public async Task<IEnumerable<CategoryDTO>> GetCategoriesProducts()
        {
            var categoriesProductsEntity = await _categoryRepository.GetCategoriesProducts();
            return _mapper.Map<IEnumerable<CategoryDTO>>(categoriesProductsEntity);
        }

        public async Task<CategoryDTO> GetCategoryById(int id)
        {
            var category = await _categoryRepository.GetById(id);
            return _mapper.Map<CategoryDTO>(category);
        }

        public async Task CreateCategory(CategoryDTO categoryDTO)
        {
            Category category = _mapper.Map<Category>(categoryDTO);
            await _categoryRepository.Create(category);
            categoryDTO.Id = category.Id;
        }


        public async Task UpdateCategory(CategoryDTO categoryDTO)
        {
            Category category = _mapper.Map<Category>(categoryDTO);
            await _categoryRepository.Update(category);

        }

        public async Task RemoveCategory(int id)
        {
            var category = _categoryRepository.GetById(id).Result;
            await _categoryRepository.Delete(category.Id);
        }

    }
}
