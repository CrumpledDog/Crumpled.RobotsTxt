using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

using RobotsTxt;

using static RobotsTxt.RobotsTxtOptionsBuilder;

namespace Crumpled.RobotsTxt
{
	public static partial class IUmbracoBuilderExtensions
	{
		public static IUmbracoBuilder AddCrumpledRobotsTxt(this IUmbracoBuilder builder)
		{
            var robotsTxtOptions = GetRobotsTxtOptions(builder.Config);

            // Check if there are any valid (non-null) sites configured
            var validSites = robotsTxtOptions.Sites?.Where(x => x.Value != null).ToList();
            
            if (validSites != null && validSites.Any())
            {
                foreach (var site in validSites)
                {
                    var ruleSet = robotsTxtOptions.RuleSets?.GetValueOrDefault(site.Value.RuleSet);
                    var sitemapUrl = GetSitemapUrl(site.Value.SiteMapDomain);
                    builder.Services.AddStaticRobotsTxt(robotBuilder => robotBuilder.BuildRulesFromConfig(ruleSet, sitemapUrl).ForHostnames(site.Value.HostNames.Split(',')));
                }

                // Add catch-all fallback for unmatched domains - blocks all bots for safety
                builder.Services.AddStaticRobotsTxt(robotBuilder => 
                    robotBuilder.AddSection(section => 
                        section.AddUserAgent("*").Disallow("/")));
            }
            else
            {
                // Check if running on Umbraco Cloud live environment
                var isUmbracoCloudLive = Environment.GetEnvironmentVariable("UMBRACO__CLOUD__DEPLOY__ENVIRONMENTNAME")?.Equals("live", StringComparison.OrdinalIgnoreCase) ?? false;

                if (isUmbracoCloudLive)
                {
                    // Default to allowing all bots on Umbraco Cloud live environment
                    builder.Services.AddStaticRobotsTxt(robotBuilder => 
                        robotBuilder.AddSection(section => 
                            section.AddUserAgent("*").Allow("/")));
                }
                else
                {
                    // Default to blocking all bots when no Sites are configured
                    builder.Services.AddStaticRobotsTxt(robotBuilder => 
                        robotBuilder.AddSection(section => 
                            section.AddUserAgent("*").Disallow("/")));
                }
            }

            builder.Services.Configure<UmbracoPipelineOptions>(options =>
            {
                options.AddFilter(new UmbracoPipelineFilter("robots.txt")
                {
                    PreRouting = app => app.UseRobotsTxt()
                });
            });

            return builder;
        }

        public static SectionBuilder Allow(this SectionBuilder section, string[] path)
        {
            foreach (var p in path)
            {
                section.Allow(p);
            }
            return section;
        }

        public static SectionBuilder Disallow(this SectionBuilder section, string[] path)
        {
            foreach (var p in path)
            {
                section.Disallow(p);
            }
            return section;
        }

        private static RobotsTxtOptionsBuilder BuildRulesFromConfig(this RobotsTxtOptionsBuilder builder, RuleSet? ruleSet, string? sitemapUrl)
        {
            if (ruleSet?.Allow != null)
            {
                foreach (var allowRule in ruleSet.Allow)
                {
                    builder.AddSection(section => section.AddUserAgent(allowRule.Key).Allow(allowRule.Value));
                }
            }

            if (ruleSet?.Disallow != null)
            {
                foreach (var disAllowRule in ruleSet.Disallow)
                {
                    builder.AddSection(section => section.AddUserAgent(disAllowRule.Key).Disallow(disAllowRule.Value));
                }
            }

            if (sitemapUrl != null)
            {
                builder.AddSitemap(sitemapUrl);
            }

            return builder;
        }

        private static RobotsTxtOptions GetRobotsTxtOptions(this IConfiguration configuration)
        {
            var robotsTxtOptions = new RobotsTxtOptions();
            var robotsTxtOptionsSection = GetRobotsTxtOptionsSection(configuration);
            robotsTxtOptionsSection.Bind(robotsTxtOptions);

            return robotsTxtOptions;
        }

        private static string GetSitemapUrl(string sitemapDomain)
        {
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

        private static IConfigurationSection GetRobotsTxtOptionsSection(this IConfiguration configuration)
        {
            var robotsTxtOptionsSection = configuration.GetSection("Crumpled").GetSection(RobotsTxtOptions.RobotsTxtSection);
            return robotsTxtOptionsSection;
        }
    }
}