namespace ButterMorph.Abstractions;

using System.Collections.Generic;

/// <summary>
/// Represents a parsed validation document.
/// </summary>
public interface IValidationDocument : IDslDocument
{
    /// <summary>
    /// Gets executable validation statements preserving document order.
    /// </summary>
    IReadOnlyCollection<IValidationStatement> Statements { get; }

    /// <summary>
    /// Gets boolean validation assertions.
    /// </summary>
    IReadOnlyCollection<IValidationAssertion> Assertions { get; }
}
