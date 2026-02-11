namespace Crumpled.RobotsTxt.Core;

public interface IRobotsTxtProvider
{
    Task<RobotsTxtResult> GetResultAsync(CancellationToken cancellationToken);
}
