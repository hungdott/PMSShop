using Microsoft.EntityFrameworkCore;
using PMSShop.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMSShop.Data.EF
{
    class PMSShopDbContext : DbContext
    {
        public PMSShopDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}
