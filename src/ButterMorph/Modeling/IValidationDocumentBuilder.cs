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
    /// Adds a validation rule.
    /// </summary>
    /// <param name="rule">The validation rule.</param>
    /// <returns>The current builder.</returns>
    IValidationDocumentBuilder WithRule(IValidationRule rule);

    /// <summary>
    /// Sets the validation payload and schema scope.
    /// </summary>
    /// <param name="payloadAlias">The payload alias.</param>
    /// <param name="schemaKey">The schema key.</param>
    /// <returns>The current builder.</returns>
    IValidationDocumentBuilder WithValidationScope(string payloadAlias, string schemaKey);

    /// <summary>
    /// Adds a boolean validation assertion.
    /// </summary>
    /// <param name="assertion">The validation assertion.</param>
    /// <returns>The current builder.</returns>
    IValidationDocumentBuilder WithAssertion(IValidationAssertion assertion);

    /// <summary>
    /// Builds the validation document.
    /// </summary>
    /// <returns>The validation document.</returns>
    IValidationDocument Build();
}
