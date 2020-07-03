using PMSShop.ViewModels.Catalog.Common;
using PMSShop.ViewModels.Catalog.Products;
using PMSShop.ViewModels.Catalog.Products.Public;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PMSShop.Application.Catalog.Products
{
    public interface IPublicProductService
    {
        Task<PagedResult<ProductViewModel>> GetAllByCategoryID(GetProductPagingRequest request);
    }
}
