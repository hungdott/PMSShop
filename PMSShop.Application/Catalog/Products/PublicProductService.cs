using PMSShop.Data.EF;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using PMSShop.Utilities.LinqExtesions;
using Microsoft.EntityFrameworkCore;
using PMSShop.ViewModels.Catalog.Products;
using PMSShop.ViewModels.Catalog.Common;


namespace PMSShop.Application.Catalog.Products
{
    public class PublicProductService : IPublicProductService
    {
        private readonly PMSShopDbContext _context;
        public PublicProductService(PMSShopDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProductViewModel>> GetAll()
        {
            var query = from p in _context.Products
                         join pt in _context.ProductTranslations on p.Id equals pt.ProductId
                         join pic in _context.ProductInCategories on p.Id equals pic.ProductId
                         join ct in _context.Categories on pic.CategoryId equals ct.Id
                         //where pt.Name.Contains(request.Keyword)
                         select new { p, pt, pic };
                         
           
            var data = await query.Select(x => new ProductViewModel()
                            {
                                Id = x.p.Id,
                                Name = x.pt.Name,
                                Description = x.pt.Description,
                                DateCreated = x.p.DateCreated,
                                Details = x.pt.Details,
                                LanguageId = x.pt.LanguageId,
                                OriginalPrice = x.p.OriginalPrice,
                                Price = x.p.Price,
                                SeoAlias = x.pt.SeoAlias,
                                SeoDescription = x.pt.SeoDescription,
                                SeoTitle = x.pt.SeoTitle,
                                Stock = x.p.Stock,
                                ViewCount = x.p.ViewCount
                            }).ToListAsync();
            return data;
        }

        public async Task<PagedResult<ProductViewModel>> GetAllByCategoryID(GetPublicProductPagingRequest request)
        {
            var query = (from p in _context.Products
                         join pt in _context.ProductTranslations on p.Id equals pt.ProductId
                         join pic in _context.ProductInCategories on p.Id equals pic.ProductId
                         join ct in _context.Categories on pic.CategoryId equals ct.Id
                         //where pt.Name.Contains(request.Keyword)
                         select new { p, pt, pic })
                         .WhereIf(request.CategoryId.HasValue
                                  && request.CategoryId.Value > 0,
                                  x => request.CategoryId == x.pic.CategoryId)
                         .AsQueryable();
            int totalRow = await query.CountAsync();
            var data = query.Skip((request.PageIndex - 1) * request.PageSize)
                            .Take(request.PageSize)
                            .Select(x => new ProductViewModel()
                            {
                                Id = x.p.Id,
                                Name = x.pt.Name,
                                Description = x.pt.Description,
                                DateCreated = x.p.DateCreated,
                                Details = x.pt.Details,
                                LanguageId = x.pt.LanguageId,
                                OriginalPrice = x.p.OriginalPrice,
                                Price = x.p.Price,
                                SeoAlias = x.pt.SeoAlias,
                                SeoDescription = x.pt.SeoDescription,
                                SeoTitle = x.pt.SeoTitle,
                                Stock = x.p.Stock,
                                ViewCount = x.p.ViewCount
                            }).ToListAsync();
            var pageResult = new PagedResult<ProductViewModel>()
            {
                TotalRecord = totalRow,
                Items = await data
            };
            return pageResult;
        }

       
    }
}
