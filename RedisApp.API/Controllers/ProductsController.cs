using Microsoft.AspNetCore.Mvc;
using RedisApp.API.Models;
using RedisApp.API.Repository;
using RedisApp.API.Services;
using RedisExampleApp.Cache;
using StackExchange.Redis;

namespace RedisApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : Controller
    {

        //private readonly IProductRepository _productRepository;

        //private readonly IDatabase _database;

        private readonly IProductService _productService;



        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }



        [HttpGet]
        // Tüm verileri yansıtır.
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _productService.GetAsync());
        }


        [HttpGet("{id}")]
        // İstenen veriyi yansıtır.
        public async Task<IActionResult> GetById(int id)
        {
            return Ok(await _productService.GetByIdAsync(id));
        }



        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            return Created(string.Empty,await _productService.CreateAsync(product));
        }




    }
}
