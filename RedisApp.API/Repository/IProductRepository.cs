using RedisApp.API.Models;

namespace RedisApp.API.Repository
{
    public interface IProductRepository
    {


        // Ürünleri döneceğiz
        Task<List<Product>> GetAsync(); 
        

        //Bir ürün getirme
        Task<Product> GetByIdAsync(int id);


        // Bür ürün oluşturma
        Task<Product> CreateAsync(Product product); 
    }
}
