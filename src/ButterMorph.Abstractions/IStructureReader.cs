namespace ButterMorph.Abstractions;

/// <summary>
/// Defines a reader that converts external input into an internal structure graph.
/// </summary>
public interface IStructureReader
{
    /// <summary>
    /// Reads source input into a structure graph.
    /// </summary>
    /// <param name="input">The external structure input.</param>
    /// <returns>The internal structure graph.</returns>
    IStructureGraph Read(IStructureInput input);
}
