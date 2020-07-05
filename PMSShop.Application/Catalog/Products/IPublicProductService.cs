using PMSShop.ViewModels.Catalog.Common;
using PMSShop.ViewModels.Catalog.Products;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PMSShop.Application.Catalog.Products
{
    public interface IPublicProductService
    {
        Task<PagedResult<ProductViewModel>> GetAllByCategoryID(GetPublicProductPagingRequest request);
        Task<List<ProductViewModel>> GetAll();
    }
}
