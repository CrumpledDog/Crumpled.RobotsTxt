namespace Crumpled.RobotsTxt.Tests.Integration;

public class RobotsTxtTests : IClassFixture<UmbracoWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RobotsTxtTests(UmbracoWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RobotsTxt_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/robots.txt");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task RobotsTxt_DevelopmentSite_BlocksAllBots()
    {
        // Arrange
        _client.DefaultRequestHeaders.Host = "localhost:44389";

        // Act
        var response = await _client.GetAsync("/robots.txt");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("User-agent: *", content);
        Assert.Contains("Disallow: /", content);
    }

    [Fact]
    public async Task RobotsTxt_ProductionSite_AllowsAllBots()
    {
        // Arrange
        _client.DefaultRequestHeaders.Host = "localhost:44390";

        // Act
        var response = await _client.GetAsync("/robots.txt");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("User-agent: *", content);
        Assert.Contains("Allow: /", content);
    }

    [Fact]
    public async Task RobotsTxt_ProductionSite_IncludesSitemap()
    {
        // Arrange
        _client.DefaultRequestHeaders.Host = "localhost:44390";

        // Act
        var response = await _client.GetAsync("/robots.txt");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("Sitemap:", content);
    }

    [Fact]
    public async Task RobotsTxt_UnmatchedDomain_UsesSafeDefault()
    {
        // Arrange
        _client.DefaultRequestHeaders.Host = "unknown.example.com";

        // Act
        var response = await _client.GetAsync("/robots.txt");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains("User-agent: *", content);
        // Should block by default (safe default)
        Assert.Contains("Disallow: /", content);
    }
}
