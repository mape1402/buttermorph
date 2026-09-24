namespace ButterMorph.Design;

using ButterMorph.Abstractions;

/// <summary>
/// Creates validation design sessions.
/// </summary>
public sealed class ValidationDesignSessionFactory : IValidationDesignSessionFactory
{
    // Parses DSL imports.
    private readonly IDslParser _dslParser;

    // Exports validation DSL content.
    private readonly IValidationDslExporter _dslExporter;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationDesignSessionFactory"/> class.
    /// </summary>
    /// <param name="dslParser">The DSL parser.</param>
    /// <param name="dslExporter">The validation DSL exporter.</param>
    public ValidationDesignSessionFactory(IDslParser dslParser, IValidationDslExporter dslExporter)
    {
        _dslParser = dslParser;
        _dslExporter = dslExporter;
    }

    /// <summary>
    /// Creates a validation design session.
    /// </summary>
    /// <returns>The validation design session.</returns>
    public IValidationDesignSession Create()
    {
        return new ValidationDesignSession(_dslParser, _dslExporter);
    }
}
