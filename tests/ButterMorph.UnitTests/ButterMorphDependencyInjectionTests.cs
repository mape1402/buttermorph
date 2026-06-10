namespace ButterMorph.UnitTests;

using System;
using ButterMorph.Abstractions;
using ButterMorph.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Verifies ButterMorph dependency injection registration behavior.
/// </summary>
public sealed class ButterMorphDependencyInjectionTests
{
    /// <summary>
    /// Confirms that dependency injection resolves the public engine interface.
    /// </summary>
    [Fact]
    public void AddButterMorphResolvesEngineWhenRequiredEnginesAreRegistered()
    {
        ServiceCollection services = new();
        services.AddSingleton<ITransformationEngine, FakeTransformationEngine>();
        services.AddSingleton<IValidationEngine, FakeValidationEngine>();
        services.AddButterMorph();

        using ServiceProvider provider = services.BuildServiceProvider();

        IButterMorphEngine engine = provider.GetRequiredService<IButterMorphEngine>();

        Assert.NotNull(engine);
    }

    /// <summary>
    /// Confirms that dependency injection fails when the transformation engine is missing.
    /// </summary>
    [Fact]
    public void AddButterMorphRequiresTransformationEngine()
    {
        ServiceCollection services = new();
        services.AddSingleton<IValidationEngine, FakeValidationEngine>();
        services.AddButterMorph();

        using ServiceProvider provider = services.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<IButterMorphEngine>());
    }

    /// <summary>
    /// Confirms that dependency injection fails when the validation engine is missing.
    /// </summary>
    [Fact]
    public void AddButterMorphRequiresValidationEngine()
    {
        ServiceCollection services = new();
        services.AddSingleton<ITransformationEngine, FakeTransformationEngine>();
        services.AddButterMorph();

        using ServiceProvider provider = services.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<IButterMorphEngine>());
    }

    /// <summary>
    /// Confirms that dependency injection resolves navigation services.
    /// </summary>
    [Fact]
    public void AddButterMorphResolvesNavigationServices()
    {
        ServiceCollection services = new();
        services.AddSingleton<ITransformationEngine, FakeTransformationEngine>();
        services.AddSingleton<IValidationEngine, FakeValidationEngine>();
        services.AddButterMorph();

        using ServiceProvider provider = services.BuildServiceProvider();

        INavigationEngine navigationEngine = provider.GetRequiredService<INavigationEngine>();
        IPathResolver pathResolver = provider.GetRequiredService<IPathResolver>();

        Assert.NotNull(navigationEngine);
        Assert.NotNull(pathResolver);
    }
}
