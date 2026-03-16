namespace Crumpled.RobotsTxt
{
    public class RobotsTxtOptions
    {
        public const string RobotsTxtSection = "RobotsTxt";
        public bool DisableComposer { get; set; } = false;
        public string? DefaultRuleset { get; set; }
        public Dictionary<string, RuleSet>? RuleSets { get; set; }
        public Dictionary<string, SiteItem>? Sites { get; set; } = null;
    }

    public class RuleSet
    {
        public bool IncludeContentSignalInstructions { get; set; } = false;
        public ContentSignalConfig? ContentSignal { get; set; }
        public Dictionary<string, object>? Allow { get; set; }
        public Dictionary<string, string[]>? Disallow { get; set; }
    }

    public class AllowRule
    {
        public string[]? Paths { get; set; }
        public ContentSignalConfig? ContentSignal { get; set; }
    }

    public class SiteItem
    {
        public required string HostNames { get; set; }
        public string? SiteMapDomain { get; set; }
        public required string RuleSet { get; set; }
    }

    public class ContentSignalConfig
    {
        public string? Path { get; set; }
        public bool? AiTrain { get; set; }
        public bool? Search { get; set; }
        public bool? AiInput { get; set; }
    }
}
