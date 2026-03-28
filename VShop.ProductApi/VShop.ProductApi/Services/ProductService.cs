using AutoMapper;
using VShop.ProductApi.Context;
using VShop.ProductApi.DTOs;
using VShop.ProductApi.Models;
using VShop.ProductApi.Repositories.Interfaces;
using VShop.ProductApi.Services.Interfaces;

namespace VShop.ProductApi.Services
{
    public class ProductService : IProductService
    {

        private readonly IMapper _mapper;
        private readonly IProductRepository _productRepository;

        public ProductService(AppDbContext context, IProductRepository productRepository, IMapper mapper)
        {
            _mapper = mapper;
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<ProductDTO>> GetProducts()
        {
            var products = await _productRepository.GetAll();
            return _mapper.Map<IEnumerable<ProductDTO>>(products);
        }

        public async Task<ProductDTO> GetProductsById(int id)
        {
            var product = await _productRepository.GetById(id);
            return _mapper.Map<ProductDTO>(product);
        }

        public async Task CreateProduct(ProductDTO productDTO)
        {
            Product product = _mapper.Map<Product>(productDTO);
            await _productRepository.Create(product);
            productDTO.Id = product.Id;
        }

        public async Task UpdateProduct(ProductDTO productDTO)
        {
            Product product = _mapper.Map<Product>(productDTO);
            await _productRepository.Update(product);
        }

        public async Task RemoveProduct(int id)
        {
            var product = _productRepository.GetById(id).Result;
            await _productRepository.Delete(product.Id);
        }
    }
}
