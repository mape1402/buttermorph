namespace ButterMorph.Contracts;

/// <summary>
/// Marks a language-native type as available for ButterMorph type mappings.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
public sealed class ButterMorphLanguageTypeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ButterMorphLanguageTypeAttribute"/> class.
    /// </summary>
    /// <param name="key">The stable type key exposed to ButterMorph.</param>
    public ButterMorphLanguageTypeAttribute(string key)
    {
        Key = key;
    }

    /// <summary>
    /// Gets the stable type key exposed to ButterMorph.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets or sets the friendly display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type version.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets a short type description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
