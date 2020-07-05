﻿using PMSShop.Data.EF;
using PMSShop.Data.Entities;
using PMSShop.Utilities.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PMSShop.Utilities.LinqExtesions;
using Microsoft.EntityFrameworkCore;
using PMSShop.ViewModels.Catalog.Products;
using PMSShop.ViewModels.Catalog.Common;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.IO;
using PMSShop.Application.Common;
using Microsoft.VisualBasic;
using System.Security.Cryptography.X509Certificates;

namespace PMSShop.Application.Catalog.Products
{
    public class ManagerProductService : IManagerProductService
    {
        private readonly PMSShopDbContext _context;
        private readonly IStorageService _storageService;
        public ManagerProductService(PMSShopDbContext context, IStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        public async Task<int> AddImages(int productId, List<IFormFile> files)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == productId);
            if (product == null) throw new PMSShopException("Product không tồn tại");
            foreach (var file in files)
            {
                var productImage = new ProductImage()
                {
                    ImagePath = await this.SaveFile(file),
                    Caption = file.FileName,
                    ProductId = productId,
                    DateCreated = DateTime.Now,
                    IsDefault = true,
                    FileSize = file.Length,
                    SortOrder = 1
                };
                _context.ProductImages.Add(productImage);
            }
            return await _context.SaveChangesAsync();
        }

        public async Task AddViewCount(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            product.ViewCount += 1;
            await _context.SaveChangesAsync();
        }

        public async Task<int> Create(ProductCreateRequest request)
        {
            var product = new Product()
            {

                Price = request.Price,
                OriginalPrice = request.OriginalPrice,
                Stock = request.Stock,
                ViewCount = 0,
                DateCreated = DateTime.Now,
                ProductTranslations = new List<ProductTranslation>()
                {
                    new ProductTranslation()
                    {
                        Name = request.Name,
                        Description = request.Description,
                        Details = request.Detail,
                        SeoAlias = request.SeoAlias,
                        SeoDescription = request.SeoDescription,
                        SeoTitle = request.SeoTIttle,
                        LanguageId = request.LanguageId
                    }
                }
            };

            if (request.ThumbnailImage != null)
            {
                var thumbnailImage = await _context.ProductImages.FirstOrDefaultAsync(x => x.IsDefault == true && x.ProductId == request.Id);
                if (thumbnailImage != null)
                {
                    thumbnailImage.FileSize = request.ThumbnailImage.Length;
                    thumbnailImage.ImagePath = await this.SaveFile(request.ThumbnailImage);
                    _context.ProductImages.Update(thumbnailImage);
                }

                product.ProductImages = new List<ProductImage>()
                {
                    new ProductImage()
                    {
                        Caption = request.Name,
                        DateCreated = DateTime.Now,
                        FileSize = request.ThumbnailImage.Length,
                        ImagePath = await this.SaveFile(request.ThumbnailImage),
                        IsDefault = true,
                        SortOrder = 1

                    }
                };
            }
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return product.Id;
        }

        public async Task<int> Delete(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new PMSShopException($"Cannot find a product: {productId}");
            var images = await _context.ProductImages.Where(x => x.ProductId == productId).ToListAsync();
            foreach (var image in images)
            {
                await _storageService.DeleteFileAsync(image.ImagePath);
            }
            _context.Products.Remove(product);
            return await _context.SaveChangesAsync();
        }

        public async Task<PagedResult<ProductViewModel>> GetAllPaging(GetManagerProductPagingRequest request)
        {
            var query = (from p in _context.Products
                         join pt in _context.ProductTranslations on p.Id equals pt.ProductId
                         join pic in _context.ProductInCategories on p.Id equals pic.ProductId
                         join ct in _context.Categories on pic.CategoryId equals ct.Id
                         //where pt.Name.Contains(request.Keyword)
                         select new { p, pt, pic })
                         .WhereIf(!String.IsNullOrEmpty(request.Keyword), x => x.pt.Name.Contains(request.Keyword))
                         .WhereIf(request.CategoryIds.Count > 0, x => request.CategoryIds.Contains(x.pic.CategoryId))
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

        public async Task<ProductViewModel> GetById(int productId,string languageId)
        {
            var product = await _context.Products.FindAsync(productId);
            var productTranslation = await _context.ProductTranslations.FirstOrDefaultAsync(x => x.ProductId == productId
            && x.LanguageId == languageId);

            var productViewModel = new ProductViewModel()
            {
                Id = product.Id,
                DateCreated = product.DateCreated,
                Description = productTranslation != null ? productTranslation.Description : null,
                LanguageId = productTranslation.LanguageId,
                Details = productTranslation != null ? productTranslation.Details : null,
                Name = productTranslation != null ? productTranslation.Name : null,
                OriginalPrice = product.OriginalPrice,
                Price = product.Price,
                SeoAlias = productTranslation != null ? productTranslation.SeoAlias : null,
                SeoDescription = productTranslation != null ? productTranslation.SeoDescription : null,
                SeoTitle = productTranslation != null ? productTranslation.SeoTitle : null,
                Stock = product.Stock,
                ViewCount = product.ViewCount
            };
            return productViewModel;
        }

        public async Task<List<ProductImageViewModel>> GetListImage(int productId)
        {
            var lstImage = await (from productImage in _context.ProductImages
                                  join product in _context.Products on productImage.ProductId equals product.Id into tbl_proJoin
                                  from projoin in tbl_proJoin.DefaultIfEmpty()
                                  where projoin.Id == productId
                                  select new ProductImageViewModel()
                                  {
                                      Id = productImage.Id,
                                      FilePath = productImage.ImagePath,
                                      IsDefault = productImage.IsDefault,
                                      FileSize = productImage.FileSize
                                  }).ToListAsync(); 
            return lstImage;
        }

        public async Task<int> RemoveImages(int imageId)
        {
            var productImage = await _context.ProductImages.FirstOrDefaultAsync(x => x.Id == imageId);
            if (productImage == null) throw new PMSShopException("Ảnh không tồn tại");
            await _storageService.DeleteFileAsync(productImage.ImagePath);
            _context.ProductImages.Remove(productImage);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> Update(ProductUpdateRequest request)
        {
            var product = await _context.Products.FindAsync(request.Id);
            var productTranslations = await _context.ProductTranslations
                                              .FirstOrDefaultAsync(x => x.ProductId == request.Id
                                                                      && x.LanguageId == request.LanguageId);
            if (product == null || productTranslations == null) throw new PMSShopException($"Cannot find product with id: {request.Id}");
            productTranslations.Name = request.Name;
            productTranslations.SeoAlias = request.SeoAlias;
            productTranslations.SeoDescription = request.SeoDescription;
            productTranslations.SeoTitle = request.SeoTitle;
            productTranslations.Description = request.Description;
            productTranslations.Details = request.Details;
            //Save image
            if (request.ThumbnailImage != null)
            {
                var thumbnailImage = await _context.ProductImages.FirstOrDefaultAsync(i => i.IsDefault == true && i.ProductId == request.Id);
                if (thumbnailImage != null)
                {
                    thumbnailImage.FileSize = request.ThumbnailImage.Length;
                    thumbnailImage.ImagePath = await this.SaveFile(request.ThumbnailImage);
                    _context.ProductImages.Update(thumbnailImage);
                }
            }
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateImage(int imageId, string caption, bool isDefault)
        {
            var productImage = await _context.ProductImages.FirstOrDefaultAsync(x => x.Id == imageId);
            if (productImage == null) throw new PMSShopException("Ảnh không tồn tại");
            productImage.Caption = caption;
            productImage.IsDefault = isDefault;
            _context.ProductImages.Update(productImage);
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdatePrice(int productId, decimal newPrice)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new PMSShopException($"Cannot find product with id: {productId}");
            product.Price = newPrice;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateStock(int productId, int addedQuantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) throw new PMSShopException($"Cannot find product with id: {productId}");
            product.Stock += addedQuantity;
            return await _context.SaveChangesAsync() > 0;
        }
        private async Task<string> SaveFile(IFormFile file)
        {
            var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
            await _storageService.SaveFileAsync(file.OpenReadStream(), fileName);
            return fileName;
        }

    }
}
