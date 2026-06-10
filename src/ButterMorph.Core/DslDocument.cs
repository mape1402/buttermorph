using ButterMorph.Abstractions;

namespace ButterMorph.Core;

/// <summary>
/// Represents a parsed DSL document.
/// </summary>
public sealed class DslDocument : IDslDocument
{
    /// <summary>
    /// Gets or sets the source DSL definition.
    /// </summary>
    public IDslDefinition Definition { get; set; }
}
