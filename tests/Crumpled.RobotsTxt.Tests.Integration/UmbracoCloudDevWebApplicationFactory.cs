using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Crumpled.RobotsTxt.Tests.Integration;

/// <summary>
/// WebApplicationFactory for testing Umbraco Cloud Development environment behavior
/// Simulates: UMBRACO__CLOUD__DEPLOY__ENVIRONMENTNAME=development
/// </summary>
public class UmbracoCloudDevWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath;
#if NET8_0
    private static readonly string _seedDbPath = Path.Combine(AppContext.BaseDirectory, "Umbraco.seed.v13.sqlite.db");
#else
    private static readonly string _seedDbPath = Path.Combine(AppContext.BaseDirectory, "Umbraco.seed.v17.sqlite.db");
#endif

    public UmbracoCloudDevWebApplicationFactory()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"UmbracoCloudDevTest_{Guid.NewGuid()}.db");
        
        // Copy committed seed database to test location for fast startup (if it exists)
        if (File.Exists(_seedDbPath))
        {
            File.Copy(_seedDbPath, _dbPath, true);
        }
        
        // Set the Umbraco Cloud environment variable
        Environment.SetEnvironmentVariable("UMBRACO__CLOUD__DEPLOY__ENVIRONMENTNAME", "development");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Suppress noisy shutdown errors in tests
        builder.ConfigureLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.None);
            logging.AddFilter((category, level) => 
            {
                // Suppress all Fatal/Critical logs (shutdown errors)
                if (level >= LogLevel.Critical)
                    return false;
                // Allow other logs through
                return true;
            });
        });
        
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var testConfig = new Dictionary<string, string>
            {
                ["ConnectionStrings:umbracoDbDSN"] = $"Data Source={_dbPath}",
                ["ConnectionStrings:umbracoDbDSN_ProviderName"] = "Microsoft.Data.Sqlite",
                
                // Speed up startup
                ["Umbraco:CMS:Content:Notifications:MaxProcessingDelayMilliseconds"] = "0",
                
                // Suppress Serilog Fatal logs from shutdown
                ["Serilog:MinimumLevel:Override:Microsoft.Extensions.Hosting"] = "6",
                ["Serilog:MinimumLevel:Override:Microsoft.Hosting.Lifetime"] = "6",
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
