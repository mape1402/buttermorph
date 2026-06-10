namespace ButterMorph.Abstractions;

/// <summary>
/// Defines a parser for ButterMorph DSL definitions.
/// </summary>
public interface IDslParser
{
    /// <summary>
    /// Parses a DSL definition into a syntax representation.
    /// </summary>
    /// <param name="definition">The DSL definition text.</param>
    /// <returns>The parsed syntax representation.</returns>
    object Parse(string definition);
}
