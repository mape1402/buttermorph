namespace ButterMorph.Abstractions;

/// <summary>
/// Defines a writer that converts an internal structure graph into external output.
/// </summary>
public interface IStructureWriter
{
    /// <summary>
    /// Writes a structure graph into external output.
    /// </summary>
    /// <param name="graph">The internal structure graph.</param>
    /// <returns>The external output representation.</returns>
    IStructureOutput Write(IStructureGraph graph);
}
