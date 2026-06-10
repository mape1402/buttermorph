namespace ButterMorph.Abstractions;

/// <summary>
/// Defines a parser for ButterMorph DSL definitions.
/// </summary>
public interface IDslParser
{
    /// <summary>
    /// Parses a DSL definition into a DSL document.
    /// </summary>
    /// <param name="definition">The DSL definition.</param>
    /// <returns>The parsed DSL document.</returns>
    IDslDocument Parse(IDslDefinition definition);
}
