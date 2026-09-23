namespace ButterMorph.Abstractions;

using System.Collections.Generic;

/// <summary>
/// Represents a parsed validation document.
/// </summary>
public interface IValidationDocument : IDslDocument
{
    /// <summary>
    /// Gets the validation rules.
    /// </summary>
    IReadOnlyCollection<IValidationRule> Rules { get; }

    /// <summary>
    /// Gets the payload alias used by validation assertions.
    /// </summary>
    string PayloadAlias { get; }

    /// <summary>
    /// Gets the schema key used by validation assertions.
    /// </summary>
    string SchemaKey { get; }

    /// <summary>
    /// Gets boolean validation assertions.
    /// </summary>
    IReadOnlyCollection<IValidationAssertion> Assertions { get; }
}
