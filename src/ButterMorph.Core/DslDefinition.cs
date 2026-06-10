using ButterMorph.Abstractions;

namespace ButterMorph.Core;

/// <summary>
/// Represents DSL definition content before parsing.
/// </summary>
public sealed class DslDefinition : IDslDefinition
{
    /// <summary>
    /// Gets or sets the DSL definition content.
    /// </summary>
    public string Content { get; set; } = string.Empty;
}
