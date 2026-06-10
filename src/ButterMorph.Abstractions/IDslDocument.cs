namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a parsed DSL document.
/// </summary>
public interface IDslDocument
{
    /// <summary>
    /// Gets the source DSL definition.
    /// </summary>
    IDslDefinition Definition { get; }
}
