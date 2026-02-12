namespace Crumpled.RobotsTxt.Core;

public sealed class RobotsTxtResult
{
    public RobotsTxtResult(Memory<byte> content, int maxAge)
    {
        Content = content;
        MaxAge = maxAge;
    }

    public int MaxAge { get; }
    public Memory<byte> Content { get; }
}
