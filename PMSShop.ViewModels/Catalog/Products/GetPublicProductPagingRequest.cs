using PMSShop.ViewModels.Catalog.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMSShop.ViewModels.Catalog.Products
{
    public class GetPublicProductPagingRequest : PagingRequestBase
    {
        public int? CategoryId { get; set; }
    }
}
