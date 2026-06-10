namespace ButterMorph.IntegrationTests;

/// <summary>
/// Verifies that the integration test project is discoverable.
/// </summary>
public sealed class ProjectBootstrapTests
{
    /// <summary>
    /// Confirms that the integration test project can execute tests.
    /// </summary>
    [Fact]
    public void IntegrationTestProjectRuns()
    {
        Assert.True(true);
    }
}
