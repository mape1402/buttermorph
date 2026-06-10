namespace ButterMorph.Abstractions;

/// <summary>
/// Defines a reader that converts external input into an internal structure graph.
/// </summary>
public interface IStructureReader
{
    /// <summary>
    /// Reads source input into a structure graph.
    /// </summary>
    /// <param name="source">The external source input.</param>
    /// <returns>The internal structure graph.</returns>
    IStructureGraph Read(object source);
}
