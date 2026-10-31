namespace Crumpled.RobotsTxt.Tests.Integration;

[Collection("CloudLive")]
public class CloudLiveEnvironmentTests : IClassFixture<UmbracoCloudLiveWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CloudLiveEnvironmentTests(UmbracoCloudLiveWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CloudLive_NoSitesConfigured_AllowsAllBots()
    {
        // Act
        var response = await _client.GetAsync("/robots.txt");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Contains("User-agent: *", content);
        Assert.Contains("Allow: /", content);
        Assert.DoesNotContain("Disallow: /", content);
    }

    [Fact]
    public async Task CloudLive_ReturnsTextPlain()
    {
        // Act
        var response = await _client.GetAsync("/robots.txt");

        // Assert
        Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);
    }
}
