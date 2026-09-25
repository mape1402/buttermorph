namespace ButterMorph.Modeling;

using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Builds validation documents.
/// </summary>
public sealed class ValidationDocumentBuilder : IValidationDocumentBuilder
{
    // Stores the optional DSL definition associated with the document.
    private IDslDefinition _definition = new DslDefinition();

    // Stores validation statements in insertion order.
    private readonly List<IValidationStatement> _statements = [];

    /// <summary>
    /// Sets the source DSL definition.
    /// </summary>
    /// <param name="definition">The DSL definition.</param>
    /// <returns>The current builder.</returns>
    public IValidationDocumentBuilder WithDefinition(IDslDefinition definition)
    {
        _definition = definition;
        return this;
    }

    /// <summary>
    /// Adds a boolean validation assertion.
    /// </summary>
    /// <param name="assertion">The validation assertion.</param>
    /// <returns>The current builder.</returns>
    public IValidationDocumentBuilder WithAssertion(IValidationAssertion assertion)
    {
        _statements.Add(assertion);
        return this;
    }

    /// <summary>
    /// Adds an executable validation statement.
    /// </summary>
    /// <param name="statement">The validation statement.</param>
    /// <returns>The current builder.</returns>
    public IValidationDocumentBuilder WithStatement(IValidationStatement statement)
    {
        _statements.Add(statement);
        return this;
    }

    /// <summary>
    /// Builds the validation document.
    /// </summary>
    /// <returns>The validation document.</returns>
    public IValidationDocument Build()
    {
        return new ValidationDocument
        {
            Definition = _definition,
            Statements = [.. _statements]
        };
    }
}
