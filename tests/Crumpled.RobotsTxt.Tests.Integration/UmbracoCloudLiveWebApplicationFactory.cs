using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Crumpled.RobotsTxt.Tests.Integration;

/// <summary>
/// WebApplicationFactory for testing Umbraco Cloud Live environment behavior
/// Simulates: UMBRACO__CLOUD__DEPLOY__ENVIRONMENTNAME=live
/// </summary>
public class UmbracoCloudLiveWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath;
#if NET8_0
    private static readonly string _seedDbPath = Path.Combine(AppContext.BaseDirectory, "Umbraco.seed.v13.sqlite.db");
#else
    private static readonly string _seedDbPath = Path.Combine(AppContext.BaseDirectory, "Umbraco.seed.v17.sqlite.db");
#endif

    public UmbracoCloudLiveWebApplicationFactory()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"UmbracoCloudLiveTest_{Guid.NewGuid()}.db");
        
        // Copy committed seed database to test location for fast startup (if it exists)
        if (File.Exists(_seedDbPath))
        {
            File.Copy(_seedDbPath, _dbPath, true);
        }
        
        // Set the Umbraco Cloud environment variable
        Environment.SetEnvironmentVariable("UMBRACO__CLOUD__DEPLOY__ENVIRONMENTNAME", "live");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Suppress noisy shutdown errors in tests
        builder.ConfigureLogging(logging =>
        {
            logging.AddFilter("Microsoft.Extensions.Hosting", LogLevel.None);
            logging.AddFilter((category, level) => 
            {
                // Suppress shutdown-related errors
                if (category?.Contains("ApplicationLifetime") == true && level >= LogLevel.Error)
                    return false;
                return true;
            });
        });
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var testConfig = new Dictionary<string, string>
            {
                ["ConnectionStrings:umbracoDbDSN"] = $"Data Source={_dbPath}",
                ["ConnectionStrings:umbracoDbDSN_ProviderName"] = "Microsoft.Data.Sqlite",
                
                // Enable unattended install
                ["Umbraco:CMS:Unattended:InstallUnattended"] = "true",
                ["Umbraco:CMS:Unattended:UpgradeUnattended"] = "true",
                ["Umbraco:CMS:Unattended:UnattendedUserName"] = "test",
                ["Umbraco:CMS:Unattended:UnattendedUserEmail"] = "test@test.com",
                ["Umbraco:CMS:Unattended:UnattendedUserPassword"] = "Test1234567!",
                
                ["Umbraco:CMS:Global:InstallMissingDatabase"] = "true",
                ["Umbraco:CMS:Global:Id"] = Guid.NewGuid().ToString(),
                ["Umbraco:CMS:Content:Notifications:MaxProcessingDelayMilliseconds"] = "0",
                
                // No sites configured - should use Cloud Live default (allow all)
            };

            config.AddInMemoryCollection(testConfig!);
        });

        builder.UseEnvironment("CloudTest");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        
        // Clean up environment variable
        Environment.SetEnvironmentVariable("UMBRACO__CLOUD__DEPLOY__ENVIRONMENTNAME", null);
        
        if (disposing && File.Exists(_dbPath))
        {
            try
            {
                File.Delete(_dbPath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
