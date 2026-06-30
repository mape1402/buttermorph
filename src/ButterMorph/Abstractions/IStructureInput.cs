namespace ButterMorph.Abstractions;

/// <summary>
/// Encapsulates external structure input consumed by reader adapters.
/// </summary>
public interface IStructureInput
{
    /// <summary>
    /// Gets the input format name.
    /// </summary>
    string Format { get; }

    /// <summary>
    /// Gets the serialized input content.
    /// </summary>
    string Content { get; }
}
