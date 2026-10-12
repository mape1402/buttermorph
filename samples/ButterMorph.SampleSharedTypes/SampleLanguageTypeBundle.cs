namespace ButterMorph.SampleSharedTypes;

using ButterMorph.Contracts;

/// <summary>
/// Exposes this sample assembly as a ButterMorph language type bundle.
/// </summary>
public sealed class SampleLanguageTypeBundle : IButterMorphLanguageTypeBundle
{
    /// <inheritdoc />
    public string Key => "sample-shared-types";

    /// <inheritdoc />
    public string Language => "csharp";

    /// <inheritdoc />
    public string Version => "1.0.0";

    /// <inheritdoc />
    public string Description => "Sample C# shared business types for bundle discovery.";
}
