namespace Crumpled.RobotsTxt
{
    public class RobotsTxtOptions
    {
        public const string RobotsTxtSection = "RobotsTxt";
        public bool IsProduction { get; set; }
        public IEnumerable<string>? ProductionHostsExclude { get; set; } = null;
        public Dictionary<string, string>? Allow { get; set; }
        public Dictionary<string, string>? Disallow { get; set; }
        public string? SitemapDomain { get; set; }
        public IEnumerable<SiteMapDomainItem>? SiteMapDomains { get; set; } = null;

    }

    public class SiteMapDomainItem
    {
        public required string HostNames { get; set; }
        public required string SiteMapDomain { get; set; }
    }
}
