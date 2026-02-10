using Microsoft.Extensions.Configuration;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Crumpled.RobotsTxt
{
    public class RobotsTxtComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            var robotsTxtOptions = new RobotsTxtOptions();
            var section = builder.Config.GetSection("Crumpled").GetSection(RobotsTxtOptions.RobotsTxtSection);
            section.Bind(robotsTxtOptions);

            // Only register if composer is not disabled
            if (!robotsTxtOptions.DisableComposer)
            {
                builder.AddCrumpledRobotsTxt();
            }
        }
    }
}
