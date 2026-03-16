using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

using Crumpled.RobotsTxt.Core;
using Crumpled.RobotsTxt.Providers;

#pragma warning disable IDE0130
namespace Crumpled.RobotsTxt
#pragma warning restore IDE0130
{
    public static partial class UmbracoBuilderExtensions
    {
        public static IUmbracoBuilder AddCrumpledRobotsTxt(this IUmbracoBuilder builder)
        {
            // Register options with DI container for hot reload support
            builder.Services.Configure<RobotsTxtOptions>(
                builder.Config.GetSection("Crumpled").GetSection(RobotsTxtOptions.RobotsTxtSection));

            // Use PostConfigure to handle the custom parsing of Allow dictionaries
            builder.Services.PostConfigure<RobotsTxtOptions>(options =>
            {
                if (options.RuleSets != null)
                {
                    var configSection = builder.Config.GetSection("Crumpled")
                        .GetSection(RobotsTxtOptions.RobotsTxtSection)
                        .GetSection("RuleSets");

                    foreach (var ruleSetSection in configSection.GetChildren())
                    {
                        var ruleSetName = ruleSetSection.Key;
                        if (options.RuleSets.TryGetValue(ruleSetName, out var ruleSet))
                        {
                            var allowSection = ruleSetSection.GetSection("Allow");
                            if (allowSection.Exists())
                            {
                                ruleSet.Allow = ParseAllowRules(allowSection);
                            }
                        }
                    }
                }
            });

            // Register dynamic provider for hot reload support
            builder.Services.AddScoped<IRobotsTxtProvider, DynamicRobotsTxtProvider>();

            builder.Services.Configure<UmbracoPipelineOptions>(options =>
            {
                options.AddFilter(new UmbracoPipelineFilter("robots.txt")
                {
                    PreRouting = app => app.UseRobotsTxt()
                });
            });

            return builder;
        }

        private static Dictionary<string, object> ParseAllowRules(IConfigurationSection allowSection)
        {
            var result = new Dictionary<string, object>();

            foreach (var child in allowSection.GetChildren())
            {
                var userAgent = child.Key;

                // Check if this is a simple array (has numeric keys) or a complex object (has Paths/ContentSignal)
                var pathsChild = child.GetSection("Paths");
                if (pathsChild.Exists())
                {
                    // Complex single format with Paths and optionally ContentSignal
                    var allowRule = ParseAllowRule(child);
                    result[userAgent] = allowRule;
                }
                else
                {
                    // Check if this is an array of complex objects (array elements have "Paths")
                    var childElements = child.GetChildren().ToList();
                    if (childElements.Any() && childElements.All(c => c.GetSection("Paths").Exists()))
                    {
                        // Complex multiple format - array of AllowRule objects
                        var allowRules = childElements.Select(ParseAllowRule).ToArray();
                        result[userAgent] = allowRules;
                    }
                    else
                    {
                        // Simple array format - array of path strings
                        var paths = child.Get<string[]>();
                        if (paths != null)
                        {
                            result[userAgent] = paths;
                        }
                    }
                }
            }

            return result;
        }

        private static AllowRule ParseAllowRule(IConfigurationSection section)
        {
            var allowRule = new AllowRule
            {
                Paths = section.GetSection("Paths").Get<string[]>()
            };

            var contentSignalChild = section.GetSection("ContentSignal");
            if (contentSignalChild.Exists())
            {
                allowRule.ContentSignal = new ContentSignalConfig
                {
                    AiTrain = contentSignalChild.GetValue<bool?>("AiTrain"),
                    Search = contentSignalChild.GetValue<bool?>("Search"),
                    AiInput = contentSignalChild.GetValue<bool?>("AiInput")
                };
            }

            var crawlDelay = section.GetValue<int?>("CrawlDelay");
            if (crawlDelay.HasValue)
            {
                allowRule.CrawlDelay = crawlDelay;
            }

            return allowRule;
        }
    }
}
