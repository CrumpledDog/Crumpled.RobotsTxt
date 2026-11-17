namespace Crumpled.RobotsTxt.Tests.Integration;

/// <summary>
/// Ensures CloudLive tests run in isolation from other cloud tests
/// </summary>
[CollectionDefinition("CloudLive", DisableParallelization = true)]
public class CloudLiveCollection
{
}

/// <summary>
/// Ensures CloudDev tests run in isolation from other cloud tests
/// </summary>
[CollectionDefinition("CloudDev", DisableParallelization = true)]
public class CloudDevCollection
{
}
