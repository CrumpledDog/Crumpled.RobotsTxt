using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

using RobotsTxt;

using Crumpled.RobotsTxt.Enums;

namespace Crumpled.RobotsTxt
{
	public static partial class IUmbracoBuilderExtensions
	{
		public static IUmbracoBuilder AddCrumpledRobotsTxt(this IUmbracoBuilder builder, string? sitemapDomain = null)
		{
            var robotsTxtOptions = GetRobotsTxtOptions(builder.Config, sitemapDomain, null);

            if (robotsTxtOptions.Domains != null)
            {
                var domainItems = robotsTxtOptions.Domains.Select(x => x.Value);

                foreach (var domain in domainItems)
                {
                    var robotsTxtOptionsVariant = GetRobotsTxtOptions(builder.Config, domain.SiteMapDomain, domain.IsProduction);
                    builder.Services.AddStaticRobotsTxt(robotBuilder => robotBuilder.BuildRulesFromConfig(robotsTxtOptionsVariant).ForHostnames(domain.HostNames.Split(',')));
                }
            }
            else if (robotsTxtOptions.SiteMapDomains != null)
            {
                foreach (var domain in robotsTxtOptions.SiteMapDomains)
                {
                    var robotsTxtOptionsVariant = GetRobotsTxtOptions(builder.Config, domain.SiteMapDomain, null);
                    builder.Services.AddStaticRobotsTxt(robotBuilder => robotBuilder.BuildRulesFromConfig(robotsTxtOptionsVariant).ForHostnames(domain.HostNames.Split(',')));
                }
            }
            else
            {
                builder.Services.AddStaticRobotsTxt(robotBuilder => robotBuilder.BuildRulesFromConfig(robotsTxtOptions));
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

        private static RobotsTxtOptionsBuilder BuildRulesFromConfig(this RobotsTxtOptionsBuilder builder, RobotsTxtOptions robotsTxtOptions)
        {
            if (robotsTxtOptions.Allow != null)
                foreach (var allowRule in robotsTxtOptions.Allow)
                {
                    builder.AddSection(section => section.AddUserAgent(allowRule.Key).Allow(allowRule.Value));
                }

            builder.BuildStandardRules(robotsTxtOptions, RuleType.Allow);

            if (robotsTxtOptions.Disallow != null)
                foreach (var disAllowRule in robotsTxtOptions.Disallow)
                {
                    builder.AddSection(section => section.AddUserAgent(disAllowRule.Key).Disallow(disAllowRule.Value));
                }

            builder.BuildStandardRules(robotsTxtOptions, RuleType.Disallow);

            if (robotsTxtOptions.SitemapDomain != null)
            {
                var siteMapUrl = robotsTxtOptions.SitemapDomain + "sitemap.xml";

                builder.AddSitemap(siteMapUrl);
            }

            return builder;
        }

        private static RobotsTxtOptionsBuilder BuildStandardRules(this RobotsTxtOptionsBuilder builder, RobotsTxtOptions robotsTxtOptions, RuleType ruleType)
        {
            if (!robotsTxtOptions.IsProduction)
            {
                if (ruleType == RuleType.Allow)
                {
                    builder.AddSection(section =>
                            section.AddUserAgent("SemrushBot")
                                .Allow("/")
                        )
                        .AddSection(section =>
                            section.AddUserAgent("SemrushBot-SA")
                                .Allow("/")
                        )
                        .AddSection(section =>
                            section.AddUserAgent("SemrushBot-Desktop")
                                .Allow("/")
                        )
                        .AddSection(section =>
                            section.AddUserAgent("SemrushBot-Mobile")
                                .Allow("/")
                        )
                        .AddSection(section =>
                            section.AddUserAgent("SiteAuditBot")
                                .Allow("/")
                        ).AddSection(section =>
                            section.AddUserAgent("Twitterbot")
                                .Allow("/")
                        ).AddSection(section =>
                            section.AddUserAgent("facebookexternalhit")
                                .Allow("/")
                        )
                        .AddSection(section =>
                            section.AddUserAgent("PowerMapper")
                                .Allow("/")
                        );
                }
                else if (ruleType == RuleType.Disallow)
                {
                    builder.AddSection(section =>
                        section
                            .AddUserAgent("*")
                            .Disallow("/"));
                }
            }
            else
            {
                if (ruleType == RuleType.Allow){
                    builder
                        .AddSection(section =>
                        section
                            .AddUserAgent("*")
                            .Allow("/")
                    );
                } else if (ruleType == RuleType.Disallow)
                {
                    // nada
                }
            }

            return builder;
        }

        private static RobotsTxtOptions GetRobotsTxtOptions(this IConfiguration configuration, string? sitemapDomain, bool? isProduction)
        {
            var robotsTxtOptions = new RobotsTxtOptions();
            var robotsTxtOptionsSection = GetRobotsTxtOptionsSection(configuration);
            robotsTxtOptionsSection.Bind(robotsTxtOptions);

            if (isProduction != null)
            {
                robotsTxtOptions.IsProduction = (bool)isProduction;
            }

            if (sitemapDomain != null)
            {
                if (!sitemapDomain.EndsWith("/"))
                {
                    sitemapDomain += "/";
                }

                if (!sitemapDomain.StartsWith("http"))
                {
                    sitemapDomain = "https://" + sitemapDomain;
                }

                robotsTxtOptions.SitemapDomain = sitemapDomain;
            }

            return robotsTxtOptions;
        }

        private static IConfigurationSection GetRobotsTxtOptionsSection(this IConfiguration configuration)
        {
            var robotsTxtOptionsSection = configuration.GetSection("Crumpled").GetSection(RobotsTxtOptions.RobotsTxtSection);
            return robotsTxtOptionsSection;
        }
    }
}