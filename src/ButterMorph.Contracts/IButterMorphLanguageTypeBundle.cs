namespace ButterMorph.Contracts;

/// <summary>
/// Describes an assembly-level bundle of language-native types discoverable by ButterMorph.
/// </summary>
public interface IButterMorphLanguageTypeBundle
{
    /// <summary>
    /// Gets the stable bundle key.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Gets the target language key.
    /// </summary>
    string Language { get; }

    /// <summary>
    /// Gets the bundle version.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets a short bundle description.
    /// </summary>
    string Description { get; }
}
