namespace ButterMorph.Web.Razor;

/// <summary>
/// Represents an editable validation assertion row in the designer.
/// </summary>
public sealed class ValidationAssertionDisplayModel
{
    /// <summary>
    /// Gets or sets the validation function key.
    /// </summary>
    public string FunctionKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the left expression.
    /// </summary>
    public string LeftExpression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the right expression.
    /// </summary>
    public string RightExpression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the assertion failure message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
