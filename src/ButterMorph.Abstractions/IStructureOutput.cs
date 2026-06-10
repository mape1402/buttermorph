namespace ButterMorph.Abstractions;

/// <summary>
/// Encapsulates external structure output produced by writer adapters.
/// </summary>
public interface IStructureOutput
{
    /// <summary>
    /// Gets the output format name.
    /// </summary>
    string Format { get; }

    /// <summary>
    /// Gets the serialized output content.
    /// </summary>
    string Content { get; }
}
