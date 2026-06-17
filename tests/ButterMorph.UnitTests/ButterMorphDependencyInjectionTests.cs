namespace ButterMorph.UnitTests;

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
        services.AddButterMorph();

        using ServiceProvider provider = services.BuildServiceProvider();

        IButterMorphEngine engine = provider.GetRequiredService<IButterMorphEngine>();

        Assert.NotNull(engine);
    }

    /// <summary>
    /// Confirms that dependency injection resolves the transformation engine.
    /// </summary>
    [Fact]
    public void AddButterMorphResolvesTransformationEngine()
    {
        ServiceCollection services = new();
        services.AddButterMorph();

        using ServiceProvider provider = services.BuildServiceProvider();

        ITransformationEngine transformationEngine = provider.GetRequiredService<ITransformationEngine>();

        Assert.NotNull(transformationEngine);
    }

    /// <summary>
    /// Confirms that dependency injection resolves the transformation expression evaluator.
    /// </summary>
    [Fact]
    public void AddButterMorphResolvesTransformationExpressionEvaluator()
    {
        ServiceCollection services = new();
        services.AddButterMorph();

        using ServiceProvider provider = services.BuildServiceProvider();

        ITransformationExpressionEvaluator evaluator = provider.GetRequiredService<ITransformationExpressionEvaluator>();

        Assert.NotNull(evaluator);
    }

    /// <summary>
    /// Confirms that dependency injection resolves the function registry.
    /// </summary>
    [Fact]
    public void AddButterMorphResolvesFunctionRegistry()
    {
        ServiceCollection services = new();
        services.AddButterMorph();

        using ServiceProvider provider = services.BuildServiceProvider();

        IFunctionRegistry registry = provider.GetRequiredService<IFunctionRegistry>();

        Assert.NotNull(registry);
    }

    /// <summary>
    /// Confirms that dependency injection resolves the validation engine.
    /// </summary>
    [Fact]
    public void AddButterMorphResolvesValidationEngine()
    {
        ServiceCollection services = new();
        services.AddButterMorph();

        using ServiceProvider provider = services.BuildServiceProvider();

        IValidationEngine validationEngine = provider.GetRequiredService<IValidationEngine>();

        Assert.NotNull(validationEngine);
    }

    /// <summary>
    /// Confirms that dependency injection resolves the validation rule registry.
    /// </summary>
    [Fact]
    public void AddButterMorphResolvesValidationRuleRegistry()
    {
        ServiceCollection services = new();
        services.AddButterMorph();

        using ServiceProvider provider = services.BuildServiceProvider();

        IValidationRuleRegistry registry = provider.GetRequiredService<IValidationRuleRegistry>();

        Assert.NotNull(registry);
    }

    /// <summary>
    /// Confirms that dependency injection resolves navigation services.
    /// </summary>
    [Fact]
    public void AddButterMorphResolvesNavigationServices()
    {
        ServiceCollection services = new();
        services.AddButterMorph();

        using ServiceProvider provider = services.BuildServiceProvider();

        INavigationEngine navigationEngine = provider.GetRequiredService<INavigationEngine>();
        IPathResolver pathResolver = provider.GetRequiredService<IPathResolver>();

        Assert.NotNull(navigationEngine);
        Assert.NotNull(pathResolver);
    }
}
