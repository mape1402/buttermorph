namespace ButterMorph.Language;

/// <summary>
/// Maps one ButterMorph schema field path to one generated language type name.
/// </summary>
public sealed class ButterMorphLanguageTypeNameMapping
{
    /// <summary>
    /// Gets or sets the ButterMorph schema field path.
    /// </summary>
    public string FieldPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the generated language type name.
    /// </summary>
    public string TypeName { get; set; } = string.Empty;
}
