using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMSShop.ViewModels.Catalog.Products.Manager
{
    public class ProductCreateRequest
    {
        public int Id { set; get; }
        public decimal Price { set; get; }
        public decimal OriginalPrice { set; get; }
        public int Stock { set; get; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Detail { get; set; }
        public string SeoDescription { get; set; }
        public string SeoTIttle { get; set; }
        public string LanguageId { get; set; }
        public string SeoAlias { set; get; }
        public IFormFile ThumbnailImage { get; set; }
    }
}
