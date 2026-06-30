namespace ButterMorph.Abstractions;

/// <summary>
/// Defines graph navigation behavior.
/// </summary>
public interface INavigationEngine
{
    /// <summary>
    /// Selects a node from the execution context using a path.
    /// </summary>
    /// <param name="context">The execution context.</param>
    /// <param name="path">The navigation path.</param>
    /// <returns>The selected structure node.</returns>
    IStructureNode Select(IExecutionContext context, string path);
}
