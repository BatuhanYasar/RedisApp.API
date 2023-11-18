using RedisApp.API.Models;
using RedisApp.API.Repository;

namespace RedisApp.API.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;


        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }



        public async Task<Product> CreateAsync(Product product)
        {
           return await _productRepository.CreateAsync(product);
        }


        public async Task<List<Product>> GetAsync()
        {
            return await _productRepository.GetAsync();
        }


        public async Task<Product> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            return product;
        }

    }
}
