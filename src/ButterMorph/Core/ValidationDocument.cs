namespace ButterMorph.Core;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a parsed validation document.
/// </summary>
public sealed class ValidationDocument : IValidationDocument
{
    /// <summary>
    /// Gets or sets the source DSL definition.
    /// </summary>
    public IDslDefinition Definition { get; set; }

    /// <summary>
    /// Gets or sets the validation rules.
    /// </summary>
    public IReadOnlyCollection<IValidationRule> Rules { get; set; } = [];

    /// <summary>
    /// Gets or sets the payload alias used by validation assertions.
    /// </summary>
    public string PayloadAlias { get; set; } = "source";

    /// <summary>
    /// Gets or sets the schema key used by validation assertions.
    /// </summary>
    public string SchemaKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets boolean validation assertions.
    /// </summary>
    public IReadOnlyCollection<IValidationAssertion> Assertions { get; set; } = [];
}
