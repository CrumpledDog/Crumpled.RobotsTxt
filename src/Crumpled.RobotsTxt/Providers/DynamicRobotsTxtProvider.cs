using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using Crumpled.RobotsTxt.Core;

namespace Crumpled.RobotsTxt.Providers;

/// <summary>
/// Dynamic robots.txt provider that supports hot reload of configuration changes.
/// </summary>
internal class DynamicRobotsTxtProvider(
    IOptionsMonitor<RobotsTxtOptions> optionsMonitor,
    IHttpContextAccessor httpContextAccessor)
    : IRobotsTxtProvider
{
    public Task<RobotsTxtResult> GetResultAsync(CancellationToken cancellationToken)
    {
        var options = optionsMonitor.CurrentValue;
        var httpContext = httpContextAccessor.HttpContext;

        var robotsTxtContent = BuildRobotsTxtContent(options, httpContext);
        var contentBytes = Encoding.UTF8.GetBytes(robotsTxtContent).AsMemory();

        var maxAge = (int)options.MaxAge.TotalSeconds;
        var result = new RobotsTxtResult(contentBytes, maxAge: maxAge);
        return Task.FromResult(result);
    }

    private static string BuildRobotsTxtContent(RobotsTxtOptions options, HttpContext? httpContext)
    {
        RuleSet? ruleSet = null;
        string? sitemapUrl = null;

        // Try to match hostname to a configured site
        if (httpContext != null && options.Sites != null)
        {
            var host = httpContext.Request.Host.Value;
            var matchingSite = options.Sites.Values
                .FirstOrDefault(site => site.HostNames
                    .Split(',')
                    .Any(hostname => hostname.Trim().Equals(host, StringComparison.OrdinalIgnoreCase)));

            if (matchingSite != null)
            {
                ruleSet = options.RuleSets?.GetValueOrDefault(matchingSite.RuleSet);
                sitemapUrl = GetSitemapUrl(matchingSite.SiteMapDomain);
            }
        }

        // Fall back to default ruleset if no match
        if (ruleSet == null && !string.IsNullOrEmpty(options.DefaultRuleset))
        {
            ruleSet = options.RuleSets?.GetValueOrDefault(options.DefaultRuleset);
        }

        // Build the robots.txt content
        if (ruleSet == null)
        {
            // Check if running on Umbraco Cloud live environment
            var isUmbracoCloudLive = Environment.GetEnvironmentVariable("UMBRACO__CLOUD__DEPLOY__ENVIRONMENTNAME")
                ?.Equals("live", StringComparison.OrdinalIgnoreCase) ?? false;

            if (isUmbracoCloudLive)
            {
                // Default to allowing all bots on Umbraco Cloud live environment
                return "User-agent: *\nAllow: /\n";
            }
            else
            {
                // Default to blocking all bots when no Sites are configured
                return "User-agent: *\nDisallow: /\n";
            }
        }

        var builder = new StringBuilder();

        // Add Content Signal instructions if enabled in the ruleset
        if (ruleSet.IncludeContentSignalInstructions)
        {
            builder.AppendLine("# As a condition of accessing this website, you agree to abide by");
            builder.AppendLine("# the following content signals:");
            builder.AppendLine("#");
            builder.AppendLine("# (a)  If a content-signal = yes, you may collect content for the");
            builder.AppendLine("# corresponding use.");
            builder.AppendLine("# (b)  If a content-signal = no, you may not collect content for");
            builder.AppendLine("# the corresponding use.");
            builder.AppendLine("# (c)  If the website operator does not include a content signal");
            builder.AppendLine("# for a corresponding use, the website operator neither grants nor");
            builder.AppendLine("# restricts permission via content signal with respect to the");
            builder.AppendLine("# corresponding use.");
            builder.AppendLine("#");
            builder.AppendLine("# The content signals and their meanings are:");
            builder.AppendLine("#");
            builder.AppendLine("# search: building a search index and providing search results");
            builder.AppendLine("# (e.g., returning hyperlinks and short excerpts from your");
            builder.AppendLine("# website's contents).  Search does not include providing");
            builder.AppendLine("# AI-generated search summaries.");
            builder.AppendLine("# ai-input: inputting content into one or more AI models (e.g.,");
            builder.AppendLine("# retrieval augmented generation, grounding, or other real-time");
            builder.AppendLine("# taking of content for generative AI search answers).");
            builder.AppendLine("# ai-train: training or fine-tuning AI models.");
            builder.AppendLine("#");
            builder.AppendLine("# ANY RESTRICTIONS EXPRESSED VIA CONTENT SIGNALS ARE EXPRESS");
            builder.AppendLine("# RESERVATIONS OF RIGHTS UNDER ARTICLE 4 OF THE EUROPEAN UNION");
            builder.AppendLine("# DIRECTIVE 2019/790 ON COPYRIGHT AND RELATED RIGHTS IN THE");
            builder.AppendLine("# DIGITAL SINGLE MARKET.");
            builder.AppendLine();
        }

        // Combine Allow and Disallow rules by user-agent
        // Order: specific user agents first, wildcard "*" last (robots.txt best practice)
        var allUserAgents = new HashSet<string>();

        if (ruleSet.Allow != null)
        {
            foreach (var key in ruleSet.Allow.Keys)
                allUserAgents.Add(key);
        }

        if (ruleSet.Disallow != null)
        {
            foreach (var key in ruleSet.Disallow.Keys)
                allUserAgents.Add(key);
        }

        var orderedUserAgents = allUserAgents
            .OrderBy(ua => ua == "*" ? 1 : 0)  // Wildcard last
            .ThenBy(ua => ua, StringComparer.OrdinalIgnoreCase); // Then alphabetically

        foreach (var userAgent in orderedUserAgents)
        {
            builder.AppendLine($"User-agent: {userAgent}");

            // Collect all AllowRules for this user agent
            var allowRules = new List<AllowRule>();
            string[]? simplePaths = null;

            if (ruleSet.Allow != null && ruleSet.Allow.TryGetValue(userAgent, out var allowValue))
            {
                if (allowValue is AllowRule[] allowRuleArray)
                {
                    allowRules.AddRange(allowRuleArray);
                }
                else if (allowValue is AllowRule singleRule)
                {
                    allowRules.Add(singleRule);
                }
                else if (allowValue is string[] stringArray)
                {
                    simplePaths = stringArray;
                }
            }

            // Determine if we should use RuleSet-level Content-Signal
            // Only use it if no AllowRule has a path-specific ContentSignal
            var hasPathSpecificContentSignal = allowRules.Any(r => r.ContentSignal != null);

            if (!hasPathSpecificContentSignal && ruleSet.ContentSignal != null)
            {
                AppendContentSignal(builder, ruleSet.ContentSignal, null);
            }

            // Add Crawl-delay if any AllowRule specifies it
            var crawlDelay = allowRules.FirstOrDefault(r => r.CrawlDelay.HasValue)?.CrawlDelay;
            if (crawlDelay.HasValue)
            {
                builder.AppendLine($"Crawl-delay: {crawlDelay.Value}");
            }

            // Add Disallow rules first (convention: Disallow before Allow)
            if (ruleSet.Disallow != null && ruleSet.Disallow.TryGetValue(userAgent, out var disallowPaths))
            {
                foreach (var path in disallowPaths)
                {
                    builder.AppendLine($"Disallow: {path}");
                }
            }

            // Add path-specific Allow rules with Content-Signal
            foreach (var allowRule in allowRules.Where(r => r.ContentSignal != null))
            {
                // Output Content-Signal with first path from this rule
                var firstPath = allowRule.Paths?.FirstOrDefault();
                if (allowRule.ContentSignal != null)
                {
                    AppendContentSignal(builder, allowRule.ContentSignal, firstPath);
                }

                // Output all Allow directives for this rule
                if (allowRule.Paths != null)
                {
                    foreach (var path in allowRule.Paths)
                    {
                        builder.AppendLine($"Allow: {path}");
                    }
                }
            }

            // Add Allow rules without Content-Signal
            foreach (var allowRule in allowRules.Where(r => r.ContentSignal == null))
            {
                if (allowRule.Paths != null)
                {
                    foreach (var path in allowRule.Paths)
                    {
                        builder.AppendLine($"Allow: {path}");
                    }
                }
            }

            // Add simple path-based Allow rules
            if (simplePaths != null)
            {
                foreach (var path in simplePaths)
                {
                    builder.AppendLine($"Allow: {path}");
                }
            }

            builder.AppendLine();
        }

        // Add sitemap
        if (!string.IsNullOrWhiteSpace(sitemapUrl))
        {
            builder.AppendLine($"Sitemap: {sitemapUrl}");
        }

        return builder.ToString();
    }

    private static void AppendContentSignal(StringBuilder builder, ContentSignalConfig contentSignal, string? path = null)
    {
        var signals = new List<string>();

        if (contentSignal.AiTrain.HasValue)
        {
            signals.Add($"ai-train={YesNo(contentSignal.AiTrain.Value)}");
        }

        if (contentSignal.Search.HasValue)
        {
            signals.Add($"search={YesNo(contentSignal.Search.Value)}");
        }

        if (contentSignal.AiInput.HasValue)
        {
            signals.Add($"ai-input={YesNo(contentSignal.AiInput.Value)}");
        }

        if (signals.Any())
        {
            var pathPrefix = !string.IsNullOrWhiteSpace(path) ? $"{path} " : "";
            builder.AppendLine($"Content-Signal: {pathPrefix}{string.Join(", ", signals)}");
        }
    }

    private static string YesNo(bool value) => value ? "yes" : "no";

    private static string? GetSitemapUrl(string? sitemapDomain)
    {
        if (string.IsNullOrWhiteSpace(sitemapDomain))
        {
            return null;
        }

        if (!sitemapDomain.EndsWith("/"))
        {
            sitemapDomain += "/";
        }

        if (!sitemapDomain.StartsWith("http"))
        {
            sitemapDomain = "https://" + sitemapDomain;
        }

        return sitemapDomain + "sitemap.xml";
    }
}
