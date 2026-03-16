using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Crumpled.RobotsTxt.Core;

namespace Crumpled.RobotsTxt.Providers;

/// <summary>
/// Dynamic robots.txt provider that supports hot reload of configuration changes.
/// </summary>
internal class DynamicRobotsTxtProvider : IRobotsTxtProvider
{
    private readonly IOptionsMonitor<RobotsTxtOptions> _optionsMonitor;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DynamicRobotsTxtProvider(
        IOptionsMonitor<RobotsTxtOptions> optionsMonitor,
        IHttpContextAccessor httpContextAccessor)
    {
        _optionsMonitor = optionsMonitor;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<RobotsTxtResult> GetResultAsync(CancellationToken cancellationToken)
    {
        var options = _optionsMonitor.CurrentValue;
        var httpContext = _httpContextAccessor.HttpContext;

        var robotsTxtContent = BuildRobotsTxtContent(options, httpContext);
        var contentBytes = Encoding.UTF8.GetBytes(robotsTxtContent).AsMemory();

        var result = new RobotsTxtResult(contentBytes, maxAge: 3600); // 1 hour cache
        return Task.FromResult(result);
    }

    private string BuildRobotsTxtContent(RobotsTxtOptions options, HttpContext? httpContext)
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

        // Add Allow rules with Content-Signal
        // Order: specific user agents first, wildcard "*" last (robots.txt best practice)
        if (ruleSet.Allow != null)
        {
            var orderedRules = ruleSet.Allow
                .OrderBy(r => r.Key == "*" ? 1 : 0)  // Wildcard last
                .ThenBy(r => r.Key, StringComparer.OrdinalIgnoreCase); // Then alphabetically

            foreach (var allowRule in orderedRules)
            {
                var userAgent = allowRule.Key;
                var ruleValue = allowRule.Value;

                string[]? paths = null;
                ContentSignalConfig? agentContentSignal = null;

                if (ruleValue is AllowRule complexRule)
                {
                    paths = complexRule.Paths;
                    agentContentSignal = complexRule.ContentSignal;
                }
                else if (ruleValue is string[] stringArray)
                {
                    paths = stringArray;
                }

                if (paths != null)
                {
                    builder.AppendLine($"User-agent: {userAgent}");

                    // Apply agent-specific ContentSignal if present, otherwise use default from RuleSet
                    var contentSignalToApply = agentContentSignal ?? ruleSet.ContentSignal;
                    if (contentSignalToApply != null)
                    {
                        AppendContentSignal(builder, contentSignalToApply);
                    }

                    foreach (var path in paths)
                    {
                        builder.AppendLine($"Allow: {path}");
                    }

                    builder.AppendLine();
                }
            }
        }

        // Add Disallow rules (no Content-Signal)
        // Order: specific user agents first, wildcard "*" last (robots.txt best practice)
        if (ruleSet.Disallow != null)
        {
            var orderedRules = ruleSet.Disallow
                .OrderBy(r => r.Key == "*" ? 1 : 0)  // Wildcard last
                .ThenBy(r => r.Key, StringComparer.OrdinalIgnoreCase); // Then alphabetically

            foreach (var disallowRule in orderedRules)
            {
                builder.AppendLine($"User-agent: {disallowRule.Key}");

                foreach (var path in disallowRule.Value)
                {
                    builder.AppendLine($"Disallow: {path}");
                }

                builder.AppendLine();
            }
        }

        // Add sitemap
        if (!string.IsNullOrWhiteSpace(sitemapUrl))
        {
            builder.AppendLine($"Sitemap: {sitemapUrl}");
        }

        return builder.ToString();
    }

    private static void AppendContentSignal(StringBuilder builder, ContentSignalConfig contentSignal)
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
            var path = !string.IsNullOrWhiteSpace(contentSignal.Path) ? $" path=\"{contentSignal.Path}\"" : "";
            builder.AppendLine($"Content-Signal:{path} {string.Join(", ", signals)}");
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
