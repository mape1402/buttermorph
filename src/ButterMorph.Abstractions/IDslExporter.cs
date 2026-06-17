namespace ButterMorph.Abstractions;

/// <summary>
/// Exports transformation documents into ButterMorph DSL text.
/// </summary>
public interface IDslExporter
{
    /// <summary>
    /// Exports a transformation document into DSL text.
    /// </summary>
    /// <param name="document">The transformation document.</param>
    /// <returns>The exported DSL text.</returns>
    string Export(ITransformationDocument document);
}
