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
    /// Gets or sets boolean validation assertions.
    /// </summary>
    public IReadOnlyCollection<IValidationAssertion> Assertions { get; set; } = [];
}
