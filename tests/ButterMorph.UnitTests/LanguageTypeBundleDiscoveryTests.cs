namespace ButterMorph.UnitTests;

using ButterMorph.Contracts;
using ButterMorph.SampleSharedTypes;
using ButterMorph.Web.Razor;

/// <summary>
/// Verifies language type bundle discovery.
/// </summary>
public sealed class LanguageTypeBundleDiscoveryTests
{
    /// <summary>
    /// Confirms assemblies can expose a bundle provider and annotated custom types.
    /// </summary>
    [Fact]
    public void DiscoveryReadsBundleProviderAndAnnotatedTypes()
    {
        ReflectionButterMorphLanguageTypeBundleDiscovery discovery = new();

        ButterMorphLanguageTypeBundleDiscoveryResult result = discovery.Discover(typeof(TestBundle).Assembly);

        Assert.True(result.Succeeded, string.Join("; ", result.Diagnostics));
        Assert.Equal("test-shared-types", result.Bundle.AssemblyKey);
        Assert.Equal("csharp", result.Bundle.LanguageKey);
        Assert.Contains(result.Bundle.Types, type =>
            type.Key == "RFC" &&
            type.QualifiedName.Contains(nameof(TestRfc), StringComparison.Ordinal) &&
            type.Version == "2.1.0" &&
            type.Source == "bundle");
    }

    /// <summary>
    /// Confirms the sample shared-types DLL is a real discoverable bundle.
    /// </summary>
    [Fact]
    public void DiscoveryReadsSampleSharedTypesAssembly()
    {
        ReflectionButterMorphLanguageTypeBundleDiscovery discovery = new();

        ButterMorphLanguageTypeBundleDiscoveryResult result = discovery.Discover(typeof(SampleLanguageTypeBundle).Assembly);

        Assert.True(result.Succeeded, string.Join("; ", result.Diagnostics));
        Assert.Equal("sample-shared-types", result.Bundle.AssemblyKey);
        Assert.Equal("csharp", result.Bundle.LanguageKey);
        Assert.Contains(result.Bundle.Types, type =>
            type.Key == "RFC" &&
            type.QualifiedName == typeof(Rfc).FullName &&
            type.Version == "1.0.0");
        Assert.Contains(result.Bundle.Types, type =>
            type.Key == "Money" &&
            type.QualifiedName == typeof(Money).FullName);
    }

    private sealed class TestBundle : IButterMorphLanguageTypeBundle
    {
        public string Key => "test-shared-types";

        public string Language => "csharp";

        public string Version => "1.0.0";

        public string Description => "Test shared type bundle.";
    }

    [ButterMorphLanguageType("RFC", Name = "RFC", Version = "2.1.0", Description = "RFC Mexico")]
    private sealed class TestRfc
    {
    }
}
