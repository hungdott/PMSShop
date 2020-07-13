using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PMSShop.Application.Catalog.Products;
using PMSShop.ViewModels.Catalog.ProductImages;
using PMSShop.ViewModels.Catalog.Products;

namespace PMSShop.BackendApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService managerProductService)
        {
            _productService = managerProductService;
        }

        [HttpGet("{languageId}")]
        public async Task<IActionResult> GetAllPaging(string languageId, [FromQuery] GetPublicProductPagingRequest request)
        {
            var products = await _productService.GetAllByCategoryID(languageId, request);
            return Ok(products);
        }

        [HttpGet("{productId}/{languageId}")]
        public async Task<IActionResult> GetById(int productId, string languageId)
        {
            var product = await _productService.GetById(productId, languageId);
            if (product == null) return BadRequest("Cannot find product");
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductCreateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var productId = await _productService.Create(request);
            if (productId == 0)
                return BadRequest();
            var product = await _productService.GetById(productId, request.LanguageId);
            return CreatedAtAction(nameof(GetById), new { id = productId }, product);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromForm] ProductUpdateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var affectedResult = await _productService.Update(request);
            if (affectedResult == 0)
                return BadRequest();

            return Ok();
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> Delete([FromForm] int productId)
        {
            var affectedResult = await _productService.Delete(productId);
            if (affectedResult == 0)
                return BadRequest();

            return Ok();
        }

        [HttpPatch("{productId}/{newprice}")]
        public async Task<IActionResult> UpdatePrice([FromQuery] int productId, decimal newprice)
        {
            var isSuccess = await _productService.UpdatePrice(productId, newprice);
            if (isSuccess == true)
                return Ok();

            return BadRequest();
        }

        //image
        [HttpPost("{productId}/images")]
        public async Task<IActionResult> CreateImage(int productId, [FromForm] ProductImageCreateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var imageId = await _productService.AddImage(productId, request);
            if (imageId == 0)
                return BadRequest();
            var image = await _productService.GetImageById(imageId);
            return CreatedAtAction(nameof(GetImageById), new { id = imageId }, image);
        }

        [HttpGet("{productId}/images/{imageId}")]
        public async Task<IActionResult> GetImageById(int productId, int imageId)
        {
            var image = await _productService.GetImageById(imageId);
            if (image == null) return BadRequest("Cannot find image");
            return Ok(image);
        }

        [HttpPut("{productId}/images/{imageId}")]
        public async Task<IActionResult> UpdateImage(int imageId, ProductImageUpdateRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var image = await _productService.UpdateImage(imageId, request);
            if (image == 0) return BadRequest("Cannot update image");
            return Ok();
        }

        [HttpDelete("{productId}/images/{imageId}")]
        public async Task<IActionResult> RemoveImage(int imageId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var image = await _productService.RemoveImage(imageId);
            if (image == 0) return BadRequest("Cannot remove image");
            return Ok();
        }
    }
}