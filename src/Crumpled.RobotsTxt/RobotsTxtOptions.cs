namespace Crumpled.RobotsTxt
{
    public class RobotsTxtOptions
    {
        public const string RobotsTxtSection = "RobotsTxt";
        public bool IsProduction { get; set; }
        public Dictionary<string, string[]>? Allow { get; set; }
        public Dictionary<string, string[]>? Disallow { get; set; }
        public Dictionary<string, DomainItem>? Domains { get; set; } = null;
    }

    public class DomainItem
    {
        public required string HostNames { get; set; }
        public required string SiteMapDomain { get; set; }
        public bool IsProduction { get; set; }
    }
}
