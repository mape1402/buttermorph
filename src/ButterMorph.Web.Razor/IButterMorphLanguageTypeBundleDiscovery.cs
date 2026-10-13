namespace ButterMorph.Web.Razor;

/// <summary>
/// Discovers ButterMorph language type bundle metadata from assemblies.
/// </summary>
public interface IButterMorphLanguageTypeBundleDiscovery
{
    /// <summary>
    /// Discovers bundle metadata from an assembly stream.
    /// </summary>
    /// <param name="assemblyStream">The assembly stream.</param>
    /// <param name="fileName">The original file name.</param>
    /// <returns>The discovery result.</returns>
    ButterMorphLanguageTypeBundleDiscoveryResult Discover(Stream assemblyStream, string fileName);
}
