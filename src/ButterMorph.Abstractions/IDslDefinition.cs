namespace ButterMorph.Abstractions;

/// <summary>
/// Encapsulates DSL definition content before parsing.
/// </summary>
public interface IDslDefinition
{
    /// <summary>
    /// Gets the DSL definition content.
    /// </summary>
    string Content { get; }
}
