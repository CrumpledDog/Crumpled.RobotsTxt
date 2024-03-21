namespace Crumpled.RobotsTxt
{
    public class RobotsTxtOptions
    {
        public const string RobotsTxtSection = "RobotsTxt";
        public bool IsProduction { get; set; }
        public Dictionary<string, string>? Allow { get; set; }
        public Dictionary<string, string>? Disallow { get; set; }
        public string? SitemapDomain { get; set; }
        public Dictionary<string, string[]>? SiteMapDomains { get; set; }
    }
}
