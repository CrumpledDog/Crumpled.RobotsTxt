using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Crumpled.RobotsTxt.Tests.Integration;

/// <summary>
/// WebApplicationFactory for testing Umbraco Cloud Development environment behavior
/// Simulates: UMBRACO__CLOUD__DEPLOY__ENVIRONMENTNAME=development
/// </summary>
public class UmbracoCloudDevWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbPath;

    public UmbracoCloudDevWebApplicationFactory()
    {
        _dbPath = Path.Combine(Path.GetTempPath(), $"UmbracoCloudDevTest_{Guid.NewGuid()}.db");
        
        // Set the Umbraco Cloud environment variable
        Environment.SetEnvironmentVariable("UMBRACO__CLOUD__DEPLOY__ENVIRONMENTNAME", "development");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
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
                
                // No sites configured - should use safe default (block all)
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
