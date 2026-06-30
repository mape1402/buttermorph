using ButterMorph.Abstractions;

namespace ButterMorph.Core;

/// <summary>
/// Represents serialized structure input for reader adapters.
/// </summary>
public sealed class StructureInput : IStructureInput
{
    /// <summary>
    /// Gets or sets the input format name.
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the serialized input content.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
