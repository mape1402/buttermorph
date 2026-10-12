namespace ButterMorph.Language;

/// <summary>
/// Maps one ButterMorph schema field path to one language member name.
/// </summary>
public sealed class ButterMorphLanguageMemberMapping
{
    /// <summary>
    /// Gets or sets the ButterMorph schema field path.
    /// </summary>
    public string FieldPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the generated language member name.
    /// </summary>
    public string MemberName { get; set; } = string.Empty;
}
