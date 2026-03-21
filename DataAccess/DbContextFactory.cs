using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataAccess
{
    public static class DbContextFactory
    {
        private static readonly IConfigurationRoot _configuration;

        static DbContextFactory()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public static LctmsDbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<LctmsDbContext>();
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseSqlServer(connectionString);

            return new LctmsDbContext(optionsBuilder.Options);
        }
    }
}