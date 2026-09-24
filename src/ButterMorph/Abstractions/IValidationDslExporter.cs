namespace ButterMorph.Abstractions;

/// <summary>
/// Exports validation documents into ButterMorph validation DSL text.
/// </summary>
public interface IValidationDslExporter
{
    /// <summary>
    /// Exports a validation document into DSL text.
    /// </summary>
    /// <param name="document">The validation document.</param>
    /// <returns>The exported DSL text.</returns>
    string Export(IValidationDocument document);
}
