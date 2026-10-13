namespace ButterMorph.Web.Razor;

using ButterMorph.Design;
using ButterMorph.DependencyInjection;
using ButterMorph.Language;
using ButterMorph.SchemaDesign;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides service registration helpers for the reusable Razor designer.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds reusable ButterMorph Razor designer services and pages.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddButterMorphRazorDesigner(this IServiceCollection services)
    {
        return services.AddButterMorphRazorDesigner(_ => { });
    }

    /// <summary>
    /// Adds reusable ButterMorph Razor designer services and pages.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">The designer options configuration.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddButterMorphRazorDesigner(this IServiceCollection services, Action<ButterMorphRazorDesignerOptions> configure)
    {
        AddRazorDesignerServices(services, configure);
        return services;
    }

    /// <summary>
    /// Adds reusable ButterMorph Razor designer services and pages.
    /// </summary>
    /// <param name="builder">The ButterMorph builder.</param>
    /// <returns>The configured ButterMorph builder.</returns>
    public static IButterMorphBuilder AddRazorDesigner(this IButterMorphBuilder builder)
    {
        return builder.AddRazorDesigner(_ => { });
    }

    /// <summary>
    /// Adds reusable ButterMorph Razor designer services and pages.
    /// </summary>
    /// <param name="builder">The ButterMorph builder.</param>
    /// <param name="configure">The designer options configuration.</param>
    /// <returns>The configured ButterMorph builder.</returns>
    public static IButterMorphBuilder AddRazorDesigner(this IButterMorphBuilder builder, Action<ButterMorphRazorDesignerOptions> configure)
    {
        AddRazorDesignerServices(builder.Services, configure);
        return builder;
    }

    private static void AddRazorDesignerServices(IServiceCollection services, Action<ButterMorphRazorDesignerOptions> configure)
    {
        services.AddRazorPages();
        services.AddButterMorphDesign();
        services.AddButterMorphSchemaDesign();
        services.Configure(configure);
        services.AddSingleton<IButterMorphLanguageTypeResolver, ButterMorphLanguageTypeResolver>();
        services.AddSingleton<IButterMorphLanguageTypeBundleDiscovery, MetadataButterMorphLanguageTypeBundleDiscovery>();
    }
}
