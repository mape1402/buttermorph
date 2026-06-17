namespace ButterMorph.Abstractions;

/// <summary>
/// Analyzes transformation documents without executing runtime behavior.
/// </summary>
public interface ITransformationSemanticAnalyzer
{
    /// <summary>
    /// Analyzes a transformation document.
    /// </summary>
    /// <param name="document">The transformation document.</param>
    /// <returns>The semantic analysis result.</returns>
    SemanticAnalysisResult Analyze(ITransformationDocument document);
}
