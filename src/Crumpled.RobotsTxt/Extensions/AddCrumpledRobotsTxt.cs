using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace Crumpled.RobotsTxt
{
	public static partial class IUmbracoBuilderExtensions
	{
		public static IUmbracoBuilder AddCrumpledRobotsTxt(this IUmbracoBuilder builder)
		{
			return builder;
		}
	}
}
