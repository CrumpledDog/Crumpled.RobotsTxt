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

            if (robotsTxtOptions.Sites != null)
            {
                var siteItems = robotsTxtOptions.Sites.Select(x => x.Value);

                foreach (var site in siteItems)
                {
                    var ruleSet = robotsTxtOptions.RuleSets?.GetValueOrDefault(site.RuleSet);
                    var sitemapUrl = GetSitemapUrl(site.SiteMapDomain);
                    builder.Services.AddStaticRobotsTxt(robotBuilder => robotBuilder.BuildRulesFromConfig(ruleSet, sitemapUrl).ForHostnames(site.HostNames.Split(',')));
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