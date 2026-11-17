namespace Crumpled.RobotsTxt.Tests.Integration;

[Collection("CloudDev")]
public class CloudDevEnvironmentTests : IClassFixture<UmbracoCloudDevWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CloudDevEnvironmentTests(UmbracoCloudDevWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CloudDev_NoSitesConfigured_BlocksAllBots()
    {
        // Act
        var response = await _client.GetAsync("/robots.txt");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains("User-agent: *", content);
        Assert.Contains("Disallow: /", content);
    }

    [Fact]
    public async Task CloudDev_ReturnsTextPlain()
    {
        // Act
        var response = await _client.GetAsync("/robots.txt");

        // Assert
        Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task CloudDev_DoesNotAllowBots()
    {
        // Act
        var response = await _client.GetAsync("/robots.txt");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        // In dev environment with no sites configured, should not contain explicit "Allow: /"
        if (content.Contains("Allow: /"))
        {
            // If it contains Allow, it should be for specific patterns, not the root
            Assert.DoesNotContain("User-agent: *\r\nAllow: /", content);
        }
    }
}
