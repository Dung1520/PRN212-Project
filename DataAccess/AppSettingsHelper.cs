using BusinessObjects;
using Microsoft.Extensions.Configuration;

namespace DataAccess
{
    public static class AppSettingsHelper
    {
        private static readonly IConfigurationRoot _configuration;

        static AppSettingsHelper()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }

        public static string GetConnectionString(string name = "DefaultConnection")
        {
            return _configuration.GetConnectionString(name) ?? string.Empty;
        }

        public static DefaultAdminSettings GetDefaultAdmin()
        {
            var admin = new DefaultAdminSettings();
            _configuration.GetSection("DefaultAdmin").Bind(admin);
            return admin;
        }
    }
}