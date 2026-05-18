using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Crumpled.RobotsTxt.Tests.Integration;

public class UmbracoWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath;
    private static readonly string _seedDbPath = Path.Combine(AppContext.BaseDirectory, "Umbraco.seed.v18.sqlite.db");

    public UmbracoWebApplicationFactory()
    {
        // Use a unique test database path
        _dbPath = Path.Combine(Path.GetTempPath(), $"UmbracoTest_{Guid.NewGuid()}.db");

        // Copy committed seed database to test location for fast startup (if it exists)
        if (File.Exists(_seedDbPath))
        {
            File.Copy(_seedDbPath, _dbPath, true);
        }
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
            // Use file-based SQLite database for testing (in-memory doesn't work with Umbraco's connection pooling)
            var testConfig = new Dictionary<string, string>
            {
                ["ConnectionStrings:umbracoDbDSN"] = $"Data Source={_dbPath}",
                ["ConnectionStrings:umbracoDbDSN_ProviderName"] = "Microsoft.Data.Sqlite",

                // Speed up startup
                ["Umbraco:CMS:Content:Notifications:MaxProcessingDelayMilliseconds"] = "0",

                // Suppress Serilog Fatal logs from shutdown
                ["Serilog:MinimumLevel:Override:Microsoft.Extensions.Hosting"] = "6",
                ["Serilog:MinimumLevel:Override:Microsoft.Hosting.Lifetime"] = "6",

                // Add test configuration for duplicate disallow paths
                ["Crumpled:RobotsTxt:RuleSets:DuplicateTest:Allow:PowerMapper:0"] = "/",
                ["Crumpled:RobotsTxt:RuleSets:DuplicateTest:Disallow:*:0"] = "/",
                ["Crumpled:RobotsTxt:RuleSets:DuplicateTest:Disallow:*:1"] = "/",
                ["Crumpled:RobotsTxt:Sites:DuplicateTest:HostNames"] = "localhost:44391",
                ["Crumpled:RobotsTxt:Sites:DuplicateTest:RuleSet"] = "DuplicateTest",
            };

            config.AddInMemoryCollection(testConfig!);
        });

        builder.UseEnvironment("Development");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

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
