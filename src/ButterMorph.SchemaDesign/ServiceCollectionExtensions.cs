namespace ButterMorph.SchemaDesign;

using ButterMorph.Json.Schema;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides schema designer service registration helpers.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds ButterMorph schema design services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddButterMorphSchemaDesign(this IServiceCollection services)
    {
        services.AddButterMorphJsonSchema();
        services.AddSingleton<ISchemaDesignSessionStore, SchemaDesignSessionStore>();
        services.AddSingleton<ISchemaTypeSchemaBuilder, SchemaTypeSchemaBuilder>();
        services.AddSingleton<IFieldMetadataDefinitionBuilder, FieldMetadataDefinitionBuilder>();
        services.AddSingleton<IPayloadSchemaBuilder, PayloadSchemaBuilder>();
        services.AddSingleton<IPayloadSchemaDefinitionBuilder, PayloadSchemaDefinitionBuilder>();

        return services;
    }
}
