namespace ButterMorph.Web.Razor;

/// <summary>
/// Represents an editable validation assertion row.
/// </summary>
public sealed class ValidationAssertionDisplayModel
{
    /// <summary>
    /// Gets or sets the assertion editing kind.
    /// </summary>
    public string Kind { get; set; } = "Complex";

    /// <summary>
    /// Gets or sets the assertion expression as DSL text.
    /// </summary>
    public string Expression { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the field path used by simple field rules.
    /// </summary>
    public string FieldPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the operator used by simple field rules.
    /// </summary>
    public string Operator { get; set; } = "exists";

    /// <summary>
    /// Gets or sets the right-hand value used by simple field rules.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the selected simple operator needs a right-hand value.
    /// </summary>
    public bool RequiresValue =>
        Operator != "exists" &&
        Operator != "isEmpty" &&
        Operator != "notEmpty";

    /// <summary>
    /// Gets or sets the user-facing validation message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
