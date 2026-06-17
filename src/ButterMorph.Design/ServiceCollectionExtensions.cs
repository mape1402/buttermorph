namespace ButterMorph.Design;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides service registration helpers for ButterMorph design services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds ButterMorph design-time services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddButterMorphDesign(this IServiceCollection services)
    {
        services.AddTransient<ISchemaExplorer, SchemaExplorer>();
        services.AddTransient<ICapabilityExplorer, CapabilityExplorer>();
        services.AddTransient<IMappingDesignSessionFactory, MappingDesignSessionFactory>();
        services.AddSingleton<IMappingDesignSessionStore, MappingDesignSessionStore>();

        return services;
    }
}
