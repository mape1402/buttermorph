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
}
