using StackExchange.Redis;
using RedisApp.API.Models;
using RedisExampleApp.Cache;
using System.Text.Json;

namespace RedisApp.API.Repository
{
    //Uygulamanın herhangi bir yerinde IProductRepository ile karşılaşırsak ProductRepsitoryWithCacheDecorator den nesne verecek.
    // ProductRepsitoryWithCacheDecorator (bu sınıf) içerisinde karşılaşırsa ProductRepository'den nesne örneği vermesi gerekli.

    public class ProductRepsitoryWithCacheDecorator : IProductRepository
    {
        //Değişkenimiz
        private const string productKey = "productCaches";
        private readonly IProductRepository _productrepository;
        private readonly RedisService _redisService;
        private readonly IDatabase _cacheRepository;
        private ProductRepository productRepository;
        private RedisService redisService;

        public ProductRepsitoryWithCacheDecorator(ProductRepository productRepository, RedisService redisService)
        {
            this.productRepository = productRepository;
            this.redisService = redisService;
        }

        private ProductRepsitoryWithCacheDecorator(IProductRepository productrepository, RedisService redisService) 
        {
            _productrepository = productrepository;
            _redisService = redisService;
            _cacheRepository = _redisService.GetDb(2);
        }




        public async Task<Product> CreateAsync(Product product) // Hem gerçek db eklemesi lazım hem de cache eklemesi lazım
        {
            var newProduct = await _productrepository.CreateAsync(product);


            //Eğer data cache'de var ise
            if (!await _cacheRepository.KeyExistsAsync(productKey)) 
            {
                // cache'ye ekledik product'ın Id'si ile
                await _cacheRepository.HashSetAsync(productKey, product.Id, JsonSerializer.Serialize(newProduct));
                
            }

            return newProduct;

        }



        public async Task<List<Product>> GetAsync()
        {
            if (!await _cacheRepository.KeyExistsAsync(productKey)) // Eğer datalar cache'de yoksa
               return await LoadToCacheFromDbAsync(); // Db den cache'ye yükle arkasından dön.


            // Eğer cache'de varsa buradan devam ediyor
            
            var products = new List<Product>();

            var cacheProducts = await _cacheRepository.HashGetAllAsync(productKey);


            foreach (var item in cacheProducts.ToList())  // Tüm datayı bana ver --> HashGetAllAsync, prductKey deki data.
            {
                var product = JsonSerializer.Deserialize<Product>(item.Value); // item name değil value kısmını alıyoruz.

                products.Add(product);
            }

            return products;
        }




        // Id ile aldığımız metod
        public async Task<Product> GetByIdAsync(int id)
        {
            if(_cacheRepository.KeyExists(productKey)) // cache var ise
            {
                var product = await _cacheRepository.HashGetAsync(productKey, id);
                return product.HasValue ? JsonSerializer.Deserialize<Product>(product) : null;
                // Product'un tüm cachelerini desrilaize ediyor.
            }

            var products = await LoadToCacheFromDbAsync();

            return products.FirstOrDefault(x => x.Id == id);
        }




        // Datayı cacheleyen ve geri dönen metod
        private async Task<List<Product>> LoadToCacheFromDbAsync()
        {
            var products = await _productrepository.GetAsync();

            products.ForEach(p =>
            {
                _cacheRepository.HashSetAsync(productKey, p.Id, JsonSerializer.Serialize(p)); // Key ile birlikte tüm product datasını serialize ediyr.
                                                                     // Bu sayede ben Id üzerinden çok hızlı bir şekilde redisten datayı isteyebiliriz.
            });

            return products;
        }
    }
}
