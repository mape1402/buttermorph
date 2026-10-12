namespace ButterMorph.Web.Razor;

using System.Reflection;

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

    /// <summary>
    /// Discovers bundle metadata from an already loaded assembly.
    /// </summary>
    /// <param name="assembly">The loaded assembly.</param>
    /// <returns>The discovery result.</returns>
    ButterMorphLanguageTypeBundleDiscoveryResult Discover(Assembly assembly);
}
