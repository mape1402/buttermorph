namespace ButterMorph.PerformanceTests;

/// <summary>
/// Verifies that the performance test project is discoverable.
/// </summary>
public sealed class ProjectBootstrapTests
{
    /// <summary>
    /// Confirms that the performance test project can execute tests.
    /// </summary>
    [Fact]
    public void PerformanceTestProjectRuns()
    {
        Assert.True(true);
    }
}
