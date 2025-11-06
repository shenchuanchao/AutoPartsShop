using AutoPartsShop.Core.Interfaces;
using AutoPartsShop.Domain.Common;
using AutoPartsShop.Domain.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AutoPartsShop.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ILogger<ProductController> _logger;
        private readonly IProductService _productService;
        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // GET: /Product
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            ProductQuery productQuery = new ProductQuery
            {
                Page = 1,
                PageSize = 20
            };
            var products = await _productService.GetProductsAsync(productQuery);
            return Ok(products);
        }
        /// <summary>
        /// Get product by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        //获取热门商品
        [HttpGet]
        [Route("hot/{totalNums:int}")]
        public async Task<IActionResult> GetHotProducts(int totalNums)
        {
            var products = await _productService.GetHotProductsAsync(totalNums);
            return Ok(products);
        }





    }
}
