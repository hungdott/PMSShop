using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMSShop.ViewModels.Catalog.ProductImages
{
    public class ProductImageCreateRequest
    {
        public int SortOrder { get; set; }
        public string Caption { get; set; }
        public bool IsDefault { get; set; }
        public IFormFile ImageFile { get; set; }
    }
}
