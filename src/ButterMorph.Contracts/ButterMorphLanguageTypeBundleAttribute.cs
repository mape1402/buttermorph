namespace ButterMorph.Contracts;

/// <summary>
/// Marks an assembly as a ButterMorph language type bundle.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class ButterMorphLanguageTypeBundleAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ButterMorphLanguageTypeBundleAttribute"/> class.
    /// </summary>
    /// <param name="key">The stable bundle key.</param>
    /// <param name="language">The target language key.</param>
    public ButterMorphLanguageTypeBundleAttribute(string key, string language)
    {
        Key = key;
        Language = language;
    }

    /// <summary>
    /// Gets the stable bundle key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the target language key.
    /// </summary>
    public string Language { get; }

    /// <summary>
    /// Gets or sets the bundle version.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets a short bundle description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
