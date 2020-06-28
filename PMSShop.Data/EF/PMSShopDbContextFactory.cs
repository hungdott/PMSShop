using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace PMSShop.Data.EF
{
    public class PMSShopDbContextFactory : IDesignTimeDbContextFactory<PMSShopDbContext>
    {
        public PMSShopDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var connectionString = configuration.GetConnectionString("PMSShopDb");

            var optionsBuilder = new DbContextOptionsBuilder<PMSShopDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new PMSShopDbContext(optionsBuilder.Options);
        }
    }
}
