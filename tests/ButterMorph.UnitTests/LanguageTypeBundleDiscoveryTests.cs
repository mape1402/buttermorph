using ButterMorph.Contracts;

[assembly: ButterMorphLanguageTypeBundle("test-shared-types", "csharp", Version = "1.0.0", Description = "Test shared type bundle.")]

namespace ButterMorph.UnitTests;

using System.Runtime.Loader;
using ButterMorph.Web.Razor;

/// <summary>
/// Verifies language type bundle discovery.
/// </summary>
public sealed class LanguageTypeBundleDiscoveryTests
{
    /// <summary>
    /// Confirms assemblies expose bundle metadata and annotated custom types without runtime providers.
    /// </summary>
    [Fact]
    public void DiscoveryReadsBundleAttributeAndAnnotatedTypes()
    {
        MetadataButterMorphLanguageTypeBundleDiscovery discovery = new();

        ButterMorphLanguageTypeBundleDiscoveryResult result = Discover(discovery, typeof(LanguageTypeBundleDiscoveryTests).Assembly.Location);

        Assert.True(result.Succeeded, string.Join("; ", result.Diagnostics));
        Assert.Equal("test-shared-types", result.Bundle.AssemblyKey);
        Assert.Equal("csharp", result.Bundle.LanguageKey);
        Assert.Equal("1.0.0", result.Bundle.AssemblyVersion);
        Assert.Contains(result.Bundle.Types, type =>
            type.Key == "RFC" &&
            type.QualifiedName.Contains(nameof(TestRfc), StringComparison.Ordinal) &&
            type.Version == "2.1.0" &&
            type.Source == "bundle");
        Assert.Contains(result.Bundle.Types, type =>
            type.Key == "DefaultVersion" &&
            type.QualifiedName.Contains(nameof(TestDefaultVersion), StringComparison.Ordinal) &&
            type.Version == "1.0.0" &&
            type.Source == "bundle");
    }

    /// <summary>
    /// Confirms the sample shared-types DLL is a real discoverable bundle.
    /// </summary>
    [Fact]
    public void DiscoveryReadsSampleSharedTypesAssembly()
    {
        MetadataButterMorphLanguageTypeBundleDiscovery discovery = new();
        string assemblyPath = Path.Combine(AppContext.BaseDirectory, "ButterMorph.SampleSharedTypes.dll");

        ButterMorphLanguageTypeBundleDiscoveryResult result = Discover(discovery, assemblyPath);

        Assert.True(result.Succeeded, string.Join("; ", result.Diagnostics));
        Assert.Equal("sample-shared-types", result.Bundle.AssemblyKey);
        Assert.Equal("csharp", result.Bundle.LanguageKey);
        Assert.Contains(result.Bundle.Types, type =>
            type.Key == "RFC" &&
            type.QualifiedName == "ButterMorph.SampleSharedTypes.Rfc" &&
            type.Version == "1.0.0");
        Assert.Contains(result.Bundle.Types, type =>
            type.Key == "Money" &&
            type.QualifiedName == "ButterMorph.SampleSharedTypes.Money");
    }

    /// <summary>
    /// Confirms scanning an uploaded DLL does not load it into the default assembly context.
    /// </summary>
    [Fact]
    public void DiscoveryDoesNotLoadUploadedAssemblyIntoDefaultContext()
    {
        MetadataButterMorphLanguageTypeBundleDiscovery discovery = new();
        string assemblyPath = Path.Combine(AppContext.BaseDirectory, "ButterMorph.SampleSharedTypes.dll");
        int before = CountDefaultContextAssemblies("ButterMorph.SampleSharedTypes");

        ButterMorphLanguageTypeBundleDiscoveryResult result = Discover(discovery, assemblyPath);

        int after = CountDefaultContextAssemblies("ButterMorph.SampleSharedTypes");
        Assert.True(result.Succeeded, string.Join("; ", result.Diagnostics));
        Assert.Equal(before, after);
    }

    /// <summary>
    /// Confirms the scanner leaves uploaded files deletable after metadata inspection.
    /// </summary>
    [Fact]
    public void DiscoveryLeavesUploadedFileDeletable()
    {
        MetadataButterMorphLanguageTypeBundleDiscovery discovery = new();
        string sourcePath = Path.Combine(AppContext.BaseDirectory, "ButterMorph.SampleSharedTypes.dll");
        string temporaryPath = Path.Combine(Path.GetTempPath(), "buttermorph-test-" + Guid.NewGuid().ToString("N") + ".dll");
        File.Copy(sourcePath, temporaryPath);

        try
        {
            ButterMorphLanguageTypeBundleDiscoveryResult result = Discover(discovery, temporaryPath);

            Assert.True(result.Succeeded, string.Join("; ", result.Diagnostics));
        }
        finally
        {
            File.Delete(temporaryPath);
        }
    }

    /// <summary>
    /// Confirms assemblies must declare metadata attributes instead of runtime providers.
    /// </summary>
    [Fact]
    public void DiscoveryRejectsAssembliesWithoutBundleAttribute()
    {
        MetadataButterMorphLanguageTypeBundleDiscovery discovery = new();
        string assemblyPath = typeof(MetadataButterMorphLanguageTypeBundleDiscovery).Assembly.Location;

        ButterMorphLanguageTypeBundleDiscoveryResult result = Discover(discovery, assemblyPath);

        Assert.False(result.Succeeded);
        Assert.Contains("ButterMorphLanguageTypeBundleAttribute", result.Diagnostics[0], StringComparison.Ordinal);
        Assert.Contains("Runtime bundle providers are no longer supported", result.Diagnostics[0], StringComparison.Ordinal);
    }

    [ButterMorphLanguageType("RFC", Name = "RFC", Version = "2.1.0", Description = "RFC Mexico")]
    private sealed class TestRfc
    {
    }

    [ButterMorphLanguageType("DefaultVersion", Name = "Default Version", Description = "Uses the default version.")]
    private sealed class TestDefaultVersion
    {
    }

    private static ButterMorphLanguageTypeBundleDiscoveryResult Discover(
        MetadataButterMorphLanguageTypeBundleDiscovery discovery,
        string assemblyPath)
    {
        using FileStream stream = File.OpenRead(assemblyPath);
        return discovery.Discover(stream, Path.GetFileName(assemblyPath));
    }

    private static int CountDefaultContextAssemblies(string assemblyName)
    {
        return AssemblyLoadContext.Default.Assemblies.Count(assembly =>
            string.Equals(assembly.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase));
    }
}
