namespace ButterMorph.Contracts;

/// <summary>
/// Identifies the ButterMorph schema field represented by a generated language member.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ButterMorphFieldAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ButterMorphFieldAttribute"/> class.
    /// </summary>
    /// <param name="key">The ButterMorph field key or path.</param>
    public ButterMorphFieldAttribute(string key)
    {
        Key = key;
    }

    /// <summary>
    /// Gets the ButterMorph field key or path.
    /// </summary>
    public string Key { get; }
}
