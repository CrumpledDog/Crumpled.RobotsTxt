namespace Crumpled.RobotsTxt
{
    public class RobotsTxtOptions
    {
        public const string RobotsTxtSection = "RobotsTxt";
        public Dictionary<string, RuleSet>? RuleSets { get; set; }
        public Dictionary<string, SiteItem>? Sites { get; set; } = null;
    }

    public class RuleSet
    {
        public Dictionary<string, string[]>? Allow { get; set; }
        public Dictionary<string, string[]>? Disallow { get; set; }
    }

    public class SiteItem
    {
        public required string HostNames { get; set; }
        public required string SiteMapDomain { get; set; }
        public required string RuleSet { get; set; }
    }
}
