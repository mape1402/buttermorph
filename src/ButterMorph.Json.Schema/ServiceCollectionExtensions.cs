namespace ButterMorph.Json.Schema;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides service registration helpers for ButterMorph JSON Schema conversion.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds ButterMorph JSON Schema conversion services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddButterMorphJsonSchema(this IServiceCollection services)
    {
        services.AddTransient<IJsonSchemaImporter, JsonSchemaImporter>();
        services.AddTransient<IJsonSchemaExporter, JsonSchemaExporter>();
        return services;
    }
}
