namespace ButterMorph.Core;

using ButterMorph.Abstractions;

/// <summary>
/// Represents a parsed validation document.
/// </summary>
public sealed class ValidationDocument : IValidationDocument
{
    // Stores executable validation statements.
    private IReadOnlyCollection<IValidationStatement> _statements = [];

    // Stores the compatibility assertion view.
    private IReadOnlyCollection<IValidationAssertion> _assertions = [];

    /// <summary>
    /// Gets or sets the source DSL definition.
    /// </summary>
    public IDslDefinition Definition { get; set; }

    /// <summary>
    /// Gets or sets executable validation statements preserving document order.
    /// </summary>
    public IReadOnlyCollection<IValidationStatement> Statements
    {
        get
        {
            if (_statements.Count > 0)
            {
                return _statements;
            }

            return _assertions.Cast<IValidationStatement>().ToArray();
        }

        set
        {
            _statements = value ?? [];
        }
    }

    /// <summary>
    /// Gets or sets boolean validation assertions.
    /// </summary>
    public IReadOnlyCollection<IValidationAssertion> Assertions
    {
        get
        {
            if (_assertions.Count > 0)
            {
                return _assertions;
            }

            return _statements.OfType<IValidationAssertion>().ToArray();
        }

        set
        {
            _assertions = value ?? [];
            _statements = _assertions.Cast<IValidationStatement>().ToArray();
        }
    }
}
