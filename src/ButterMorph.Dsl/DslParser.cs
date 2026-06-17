namespace ButterMorph.Dsl;

using ButterMorph.Abstractions;

/// <summary>
/// Parses ButterMorph DSL definitions into typed transformation documents.
/// </summary>
public sealed class DslParser : IDslParser
{
    /// <summary>
    /// Parses a DSL definition into a DSL document.
    /// </summary>
    /// <param name="definition">The DSL definition.</param>
    /// <returns>The parsed DSL document.</returns>
    public IDslDocument Parse(IDslDefinition definition)
    {
        if (definition == null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        Tokenizer tokenizer = new(definition.Content);
        SyntaxAnalyzer analyzer = new(tokenizer.Tokenize());
        DocumentNode document = analyzer.Analyze();
        AstBuilder builder = new(definition);

        return builder.Build(document);
    }
}
