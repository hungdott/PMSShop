using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMSShop.Application.Catalog.Products;
using PMSShop.ViewModels.Catalog.Products;

namespace PMSShop.BackendApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IPublicProductService _publicProductService;
        private readonly IManagerProductService _managerProductService;
        public ProductController(IPublicProductService publicProductService, IManagerProductService managerProductService)
        {
            _publicProductService = publicProductService;
            _managerProductService = managerProductService;
        }

        [HttpGet("{languageId}")]
        public async Task<IActionResult> Get(string languageId)
        {
            var products = await _publicProductService.GetAll(languageId);
            return Ok(products);
        }
        [HttpGet("public-paging/{languageId}")]
        public async Task<IActionResult> Get([FromQuery] GetPublicProductPagingRequest request)
        {
            var products = await _publicProductService.GetAllByCategoryID(request);
            return Ok(products);
        }
        [HttpGet("{productId}/{languageId}")]
        public async Task<IActionResult> GetById(int productId,string languageId)
        {
            var product = await _managerProductService.GetById(productId, languageId);
            if (product == null) return BadRequest("Cannot find product");
            return Ok(product);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductCreateRequest request)
        {
            var productId = await _managerProductService.Create(request);
            if (productId == 0)
                return BadRequest();
            var product = await _managerProductService.GetById(productId, request.LanguageId);
            return CreatedAtAction(nameof(GetById), new { id = productId }, product);
        }
        [HttpPut]
        public async Task<IActionResult> Update([FromForm] ProductUpdateRequest request)
        {
            var affectedResult = await _managerProductService.Update(request);
            if (affectedResult == 0)
                return BadRequest();

            return Ok();
        }
        [HttpDelete("{productId}")]
        public async Task<IActionResult> Update([FromForm] int productId)
        {
            var affectedResult = await _managerProductService.Delete(productId);
            if (affectedResult == 0)
                return BadRequest();
           
            return Ok();
        }
        [HttpPut("price/{productId}/{newprice}")]
        public async Task<IActionResult> UpdatePrice([FromQuery] int productId, decimal newprice)
        {
            var isSuccess = await _managerProductService.UpdatePrice(productId, newprice);
            if (isSuccess == true)
                return Ok();

            return BadRequest();
        }
    }
}
