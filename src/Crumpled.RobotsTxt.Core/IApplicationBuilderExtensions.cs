using Microsoft.AspNetCore.Builder;

namespace Crumpled.RobotsTxt.Core;

public static class IApplicationBuilderExtensions
{
    public static void UseRobotsTxt(this IApplicationBuilder app)
    {
        app.UseMiddleware<RobotsTxtMiddleware>();
    }
}
