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
        Assert.Contains("Content-Signal: /blog ai-train=no, search=yes, ai-input=no", content);
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
        var wildcardIndex = content.IndexOf("User-agent: *\n", StringComparison.Ordinal);

        // All should be found
        Assert.True(googlebotIndex > 0, "googlebot user-agent not found");
        Assert.True(oaiSearchBotIndex > 0, "OAI-SearchBot user-agent not found");
        Assert.True(wildcardIndex > 0, "Wildcard user-agent section not found");

        // Specific user agents should come before wildcard
        Assert.True(googlebotIndex < wildcardIndex, "googlebot should appear before wildcard *");
        Assert.True(oaiSearchBotIndex < wildcardIndex, "OAI-SearchBot should appear before wildcard *");

        // Verify wildcard section contains both Allow and Disallow (combined in one section)
        var wildcardSection = content.Substring(wildcardIndex);
        var nextUserAgentIndex = wildcardSection.IndexOf("\nUser-agent:", 1, StringComparison.Ordinal);
        if (nextUserAgentIndex > 0)
        {
            wildcardSection = wildcardSection.Substring(0, nextUserAgentIndex);
        }

        Assert.Contains("Content-Signal:", wildcardSection);
        Assert.Contains("Allow: /", wildcardSection);
        Assert.Contains("Disallow:", wildcardSection);
    }

    [Fact]
    public async Task RobotsTxt_ProductionSite_IncludesCrawlDelay()
    {
        // Arrange
        _client.DefaultRequestHeaders.Host = "localhost:44390";

        // Act
        var response = await _client.GetAsync("/robots.txt");
        var content = await response.Content.ReadAsStringAsync();

        // Normalize line endings for cross-platform compatibility
        content = content.Replace("\r\n", "\n");

        // Assert - googlebot should have Crawl-delay directive
        Assert.Contains("User-agent: googlebot", content);

        // Extract googlebot section
        var googlebotIndex = content.IndexOf("User-agent: googlebot", StringComparison.Ordinal);
        var googlebotSection = content.Substring(googlebotIndex);
        var nextUserAgentIndex = googlebotSection.IndexOf("\nUser-agent:", 1, StringComparison.Ordinal);
        if (nextUserAgentIndex > 0)
        {
            googlebotSection = googlebotSection.Substring(0, nextUserAgentIndex);
        }

        // Verify Crawl-delay appears before path-specific Content-Signal and Allow
        Assert.Contains("Crawl-delay: 2", googlebotSection);

        var crawlDelayIndex = googlebotSection.IndexOf("Crawl-delay:", StringComparison.Ordinal);
        var contentSignalIndex = googlebotSection.IndexOf("Content-Signal:", StringComparison.Ordinal);
        var allowIndex = googlebotSection.IndexOf("Allow:", StringComparison.Ordinal);

        Assert.True(crawlDelayIndex < contentSignalIndex, "Crawl-delay should appear before path-specific Content-Signal");
        Assert.True(contentSignalIndex < allowIndex, "Path-specific Content-Signal should appear before Allow");
    }

    [Fact]
    public async Task RobotsTxt_ProductionSite_SupportsPathSpecificContentSignals()
    {
        // Arrange
        _client.DefaultRequestHeaders.Host = "localhost:44390";

        // Act
        var response = await _client.GetAsync("/robots.txt");
        var content = await response.Content.ReadAsStringAsync();

        // Normalize line endings for cross-platform compatibility
        content = content.Replace("\r\n", "\n");

        // Assert - bingbot should have multiple Content-Signal directives with paths
        Assert.Contains("User-agent: bingbot", content);

        // Extract bingbot section
        var bingbotIndex = content.IndexOf("User-agent: bingbot", StringComparison.Ordinal);
        var bingbotSection = content.Substring(bingbotIndex);
        var nextUserAgentIndex = bingbotSection.IndexOf("\nUser-agent:", 1, StringComparison.Ordinal);
        if (nextUserAgentIndex > 0)
        {
            bingbotSection = bingbotSection.Substring(0, nextUserAgentIndex);
        }

        // Verify we have three Content-Signal directives - two with paths, one without (root)
        Assert.Contains("Content-Signal: /blog ai-train=yes", bingbotSection);
        Assert.Contains("Content-Signal: /news ai-train=yes", bingbotSection);
        Assert.Contains("Content-Signal: ai-train=no", bingbotSection);

        // Verify each Content-Signal is followed by its corresponding Allow directive
        var blogContentSignalIndex = bingbotSection.IndexOf("Content-Signal: /blog", StringComparison.Ordinal);
        var blogAllowIndex = bingbotSection.IndexOf("Allow: /blog", StringComparison.Ordinal);
        var newsContentSignalIndex = bingbotSection.IndexOf("Content-Signal: /news", StringComparison.Ordinal);
        var newsAllowIndex = bingbotSection.IndexOf("Allow: /news", StringComparison.Ordinal);
        var rootContentSignalIndex = bingbotSection.IndexOf("Content-Signal: ai-train=no", StringComparison.Ordinal);
        var rootAllowLines = bingbotSection.Split('\n').Where(l => l.Trim() == "Allow: /").ToList();
        Assert.Single(rootAllowLines); // Should have exactly one "Allow: /" directive

        var rootAllowIndex = bingbotSection.LastIndexOf("Allow: /", StringComparison.Ordinal);

        // Verify ordering: Content-Signal /blog, Allow /blog, Content-Signal /news, Allow /news, Content-Signal (root), Allow /
        Assert.True(blogContentSignalIndex < blogAllowIndex, "Content-Signal /blog should appear before Allow /blog");
        Assert.True(blogAllowIndex < newsContentSignalIndex, "Allow /blog should appear before Content-Signal /news");
        Assert.True(newsContentSignalIndex < newsAllowIndex, "Content-Signal /news should appear before Allow /news");
        Assert.True(newsAllowIndex < rootContentSignalIndex, "Allow /news should appear before Content-Signal (root)");
        Assert.True(rootContentSignalIndex < rootAllowIndex, "Content-Signal (root) should appear before Allow /");
    }

    [Fact]
    public async Task RobotsTxt_DuplicateDisallowPaths_AreDeduplicatedInOutput()
    {
        // Arrange - test site with duplicate "/" in Disallow array
        _client.DefaultRequestHeaders.Host = "localhost:44391";

        // Act
        var response = await _client.GetAsync("/robots.txt");
        var content = await response.Content.ReadAsStringAsync();

        // Normalize line endings for cross-platform compatibility
        content = content.Replace("\r\n", "\n");

        // Assert
        Assert.Contains("User-agent: PowerMapper", content);
        Assert.Contains("User-agent: *", content);

        // Extract wildcard section
        var wildcardIndex = content.IndexOf("User-agent: *", StringComparison.Ordinal);
        Assert.True(wildcardIndex >= 0, "Should contain User-agent: *");

        var wildcardSection = content.Substring(wildcardIndex);
        
        // Count occurrences of "Disallow: /" in the wildcard section
        var disallowCount = 0;
        var index = 0;
        while ((index = wildcardSection.IndexOf("Disallow: /\n", index, StringComparison.Ordinal)) >= 0)
        {
            disallowCount++;
            index += "Disallow: /\n".Length;
        }

        // Should only appear once, even though the configuration has it twice
        Assert.Equal(1, disallowCount);
    }
}
