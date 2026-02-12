using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Crumpled.RobotsTxt.Tests.Integration;

public class UmbracoWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath;
    private static readonly string _seedDbPath = Path.Combine(AppContext.BaseDirectory, "Umbraco.seed.sqlite.db");

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
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Use file-based SQLite database for testing (in-memory doesn't work with Umbraco's connection pooling)
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
                
                // Use file database
                ["Umbraco:CMS:Global:InstallMissingDatabase"] = "true",
                
                // Disable analytics
                ["Umbraco:CMS:Global:Id"] = Guid.NewGuid().ToString(),
                
                // Speed up startup
                ["Umbraco:CMS:Content:Notifications:MaxProcessingDelayMilliseconds"] = "0",
                
                // Robots.txt test configuration - multi-site setup
                // Site configurations
                ["Crumpled:RobotsTxt:Sites:Development:HostNames"] = "localhost:44389",
                ["Crumpled:RobotsTxt:Sites:Development:SiteMapDomain"] = "localhost:44389",
                ["Crumpled:RobotsTxt:Sites:Development:RuleSet"] = "DevelopmentRules",
                
                ["Crumpled:RobotsTxt:Sites:Production:HostNames"] = "localhost:44390", 
                ["Crumpled:RobotsTxt:Sites:Production:SiteMapDomain"] = "localhost:44390",
                ["Crumpled:RobotsTxt:Sites:Production:RuleSet"] = "ProductionRules",
                
                // Define rulesets
                ["Crumpled:RobotsTxt:RuleSets:DevelopmentRules:Disallow:*:0"] = "/",
                
                ["Crumpled:RobotsTxt:RuleSets:ProductionRules:Allow:*:0"] = "/",
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
