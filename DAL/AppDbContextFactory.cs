using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace DAL
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbcontext>
    {
        public AppDbcontext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("../API/appsettings.json", optional: true)
                .Build();

            var builder = new DbContextOptionsBuilder<AppDbcontext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // استخدام UseSqlServer بدلاً من UseNpgsql
            // وإذا كنت تستخدم NetTopologySuite مع SQL Server:
            builder.UseSqlServer(connectionString, o => o.UseNetTopologySuite());

            return new AppDbcontext(builder.Options);
        }
    }
}