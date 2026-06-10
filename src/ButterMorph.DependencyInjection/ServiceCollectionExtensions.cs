namespace ButterMorph.DependencyInjection;

using ButterMorph.Abstractions;
using ButterMorph.Execution;
using ButterMorph.Navigation;
using ButterMorph.Transformations;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides service registration helpers for ButterMorph.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the default ButterMorph service registrations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddButterMorph(this IServiceCollection services)
    {
        services.AddTransient<IButterMorphEngine, global::ButterMorph.ButterMorphEngine>();
        services.AddTransient<IExecutionPipeline, ExecutionPipeline>();
        services.AddTransient<IExecutionContextFactory, ExecutionContextFactory>();
        services.AddTransient<INavigationEngine, NavigationEngine>();
        services.AddTransient<IPathResolver, PathResolver>();
        services.AddTransient<ITransformationEngine, TransformationEngine>();

        return services;
    }
}
