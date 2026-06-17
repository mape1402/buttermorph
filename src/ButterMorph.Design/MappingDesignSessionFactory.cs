namespace ButterMorph.Design;

using ButterMorph.Abstractions;

/// <summary>
/// Creates mapping design sessions.
/// </summary>
public sealed class MappingDesignSessionFactory : IMappingDesignSessionFactory
{
    // Parses DSL imports.
    private readonly IDslParser _dslParser;

    // Exports DSL content.
    private readonly IDslExporter _dslExporter;

    // Analyzes document semantics.
    private readonly ITransformationSemanticAnalyzer _semanticAnalyzer;

    /// <summary>
    /// Initializes a new instance of the <see cref="MappingDesignSessionFactory"/> class.
    /// </summary>
    /// <param name="dslParser">The DSL parser.</param>
    /// <param name="dslExporter">The DSL exporter.</param>
    /// <param name="semanticAnalyzer">The semantic analyzer.</param>
    public MappingDesignSessionFactory(IDslParser dslParser, IDslExporter dslExporter, ITransformationSemanticAnalyzer semanticAnalyzer)
    {
        _dslParser = dslParser;
        _dslExporter = dslExporter;
        _semanticAnalyzer = semanticAnalyzer;
    }

    /// <summary>
    /// Creates a mapping design session.
    /// </summary>
    /// <returns>The mapping design session.</returns>
    public IMappingDesignSession Create()
    {
        return new MappingDesignSession(_dslParser, _dslExporter, _semanticAnalyzer);
    }
}
