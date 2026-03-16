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

        // Production ruleset now includes Content Signal instructions
        Assert.Contains("# As a condition of accessing this website", content);
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

    [Fact]
    public async Task RobotsTxt_ProductionSite_IncludesSiteSpecificContentSignal()
    {
        // Arrange
        _client.DefaultRequestHeaders.Host = "localhost:44390";

        // Act
        var response = await _client.GetAsync("/robots.txt");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Contains("User-agent: *", content);
        Assert.Contains("Content-Signal:", content);
        Assert.Contains("ai-train=yes", content);
        Assert.Contains("search=yes", content);
        Assert.Contains("ai-input=yes", content);
        Assert.Contains("Allow: /", content);

        // OAI-SearchBot with override
        Assert.Contains("User-agent: OAI-SearchBot", content);
        Assert.Contains("ai-train=no", content);
    }

    [Fact]
    public async Task RobotsTxt_DevelopmentSite_IncludesRulesetContentSignal()
    {
        // Arrange
        _client.DefaultRequestHeaders.Host = "localhost:44389";

        // Act
        var response = await _client.GetAsync("/robots.txt");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - NonProduction allows specific bots but no ContentSignal directives since no default or agent-specific ContentSignal
        Assert.Contains("User-agent: SemrushBot", content);
        Assert.Contains("Allow: /", content);
        Assert.Contains("Disallow: /", content); // Blocks everything else

        // NonProduction does NOT include Content Signal instructions
        Assert.DoesNotContain("# As a condition of accessing this website, you agree to abide by", content);
    }

    [Fact]
    public async Task RobotsTxt_UnmatchedDomain_UsesDefaultRulesetWithContentSignal()
    {
        // Arrange
        _client.DefaultRequestHeaders.Host = "unknown.example.com";

        // Act
        var response = await _client.GetAsync("/robots.txt");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - unmatched domain uses DefaultRuleset (NonProduction)
        Assert.Contains("User-agent: SemrushBot", content);
        Assert.Contains("Allow: /", content);

        // NonProduction does NOT include Content Signal instructions
        Assert.DoesNotContain("# As a condition of accessing this website, you agree to abide by", content);
    }

    [Fact]
    public async Task RobotsTxt_ProductionSite_GooglebotPathSpecificContentSignal()
    {
        // Arrange
        _client.DefaultRequestHeaders.Host = "localhost:44390";

        // Act
        var response = await _client.GetAsync("/robots.txt");
        var content = await response.Content.ReadAsStringAsync();

        // Assert - googlebot should have path-specific rules with Content-Signal
        Assert.Contains("User-agent: googlebot", content);
        Assert.Contains("Content-Signal: ai-train=no, search=yes, ai-input=no", content);
        Assert.Contains("Allow: /blog", content);
        Assert.Contains("Allow: /news", content);
    }

    [Fact]
    public async Task RobotsTxt_ProductionSite_UserAgentOrderingIsCorrect()
    {
        // Arrange
        _client.DefaultRequestHeaders.Host = "localhost:44390";

        // Act
        var response = await _client.GetAsync("/robots.txt");
        var content = await response.Content.ReadAsStringAsync();

        // Normalize line endings for cross-platform compatibility
        content = content.Replace("\r\n", "\n");

        // Assert - specific user agents should appear before wildcard "*"
        var googlebotIndex = content.IndexOf("User-agent: googlebot", StringComparison.Ordinal);
        var oaiSearchBotIndex = content.IndexOf("User-agent: OAI-SearchBot", StringComparison.Ordinal);
        var wildcardAllowIndex = content.IndexOf("User-agent: *\nContent-Signal:", StringComparison.Ordinal);
        var wildcardDisallowIndex = content.IndexOf("User-agent: *\nDisallow:", StringComparison.Ordinal);

        // All should be found
        Assert.True(googlebotIndex > 0, "googlebot user-agent not found");
        Assert.True(oaiSearchBotIndex > 0, "OAI-SearchBot user-agent not found");
        Assert.True(wildcardAllowIndex > 0, "Wildcard Allow section not found");
        Assert.True(wildcardDisallowIndex > 0, "Wildcard Disallow section not found");

        // Specific user agents should come before wildcard in Allow rules
        Assert.True(googlebotIndex < wildcardAllowIndex, "googlebot should appear before wildcard * in Allow rules");
        Assert.True(oaiSearchBotIndex < wildcardAllowIndex, "OAI-SearchBot should appear before wildcard * in Allow rules");

        // Wildcard Allow should come before wildcard Disallow
        Assert.True(wildcardAllowIndex < wildcardDisallowIndex, "Wildcard Allow should appear before wildcard Disallow");
    }
}
