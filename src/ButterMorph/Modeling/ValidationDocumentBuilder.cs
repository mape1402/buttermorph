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

    // Stores validation rules in insertion order.
    private readonly List<IValidationRule> _rules = [];

    // Stores the validation payload alias.
    private string _payloadAlias = "source";

    // Stores the validation schema key.
    private string _schemaKey = string.Empty;

    // Stores validation assertions in insertion order.
    private readonly List<IValidationAssertion> _assertions = [];

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
    /// Adds a validation rule.
    /// </summary>
    /// <param name="rule">The validation rule.</param>
    /// <returns>The current builder.</returns>
    public IValidationDocumentBuilder WithRule(IValidationRule rule)
    {
        _rules.Add(rule);
        return this;
    }

    /// <summary>
    /// Sets the validation payload and schema scope.
    /// </summary>
    /// <param name="payloadAlias">The payload alias.</param>
    /// <param name="schemaKey">The schema key.</param>
    /// <returns>The current builder.</returns>
    public IValidationDocumentBuilder WithValidationScope(string payloadAlias, string schemaKey)
    {
        if (!string.IsNullOrWhiteSpace(payloadAlias))
        {
            _payloadAlias = payloadAlias.TrimStart('$');
        }

        _schemaKey = schemaKey ?? string.Empty;
        return this;
    }

    /// <summary>
    /// Adds a boolean validation assertion.
    /// </summary>
    /// <param name="assertion">The validation assertion.</param>
    /// <returns>The current builder.</returns>
    public IValidationDocumentBuilder WithAssertion(IValidationAssertion assertion)
    {
        _assertions.Add(assertion);
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
            Rules = [.. _rules],
            PayloadAlias = _payloadAlias,
            SchemaKey = _schemaKey,
            Assertions = [.. _assertions]
        };
    }
}
