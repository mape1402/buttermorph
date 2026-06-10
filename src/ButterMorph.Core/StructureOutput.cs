using ButterMorph.Abstractions;

namespace ButterMorph.Core;

/// <summary>
/// Represents serialized structure output from writer adapters.
/// </summary>
public sealed class StructureOutput : IStructureOutput
{
    /// <summary>
    /// Gets or sets the output format name.
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the serialized output content.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
