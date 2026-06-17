namespace ButterMorph.UnitTests;

using ButterMorph.Abstractions;

/// <summary>
/// Test semantic analyzer that always succeeds.
/// </summary>
internal sealed class PassingSemanticAnalyzer : ITransformationSemanticAnalyzer
{
    /// <summary>
    /// Analyzes a transformation document.
    /// </summary>
    /// <param name="document">The transformation document.</param>
    /// <returns>The semantic analysis result.</returns>
    public SemanticAnalysisResult Analyze(ITransformationDocument document)
    {
        return new SemanticAnalysisResult
        {
            Succeeded = true,
            Diagnostics = []
        };
    }
}
