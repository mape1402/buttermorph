namespace ButterMorph.Modeling;

using ButterMorph.Abstractions;

/// <summary>
/// Builds validation documents.
/// </summary>
public interface IValidationDocumentBuilder
{
    /// <summary>
    /// Sets the source DSL definition.
    /// </summary>
    /// <param name="definition">The DSL definition.</param>
    /// <returns>The current builder.</returns>
    IValidationDocumentBuilder WithDefinition(IDslDefinition definition);

    /// <summary>
    /// Adds a boolean validation assertion.
    /// </summary>
    /// <param name="assertion">The validation assertion.</param>
    /// <returns>The current builder.</returns>
    IValidationDocumentBuilder WithAssertion(IValidationAssertion assertion);

    /// <summary>
    /// Adds an executable validation statement.
    /// </summary>
    /// <param name="statement">The validation statement.</param>
    /// <returns>The current builder.</returns>
    IValidationDocumentBuilder WithStatement(IValidationStatement statement);

    /// <summary>
    /// Builds the validation document.
    /// </summary>
    /// <returns>The validation document.</returns>
    IValidationDocument Build();
}
