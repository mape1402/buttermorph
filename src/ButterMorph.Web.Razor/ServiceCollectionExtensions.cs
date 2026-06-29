namespace ButterMorph.Web.Razor;

using ButterMorph.Design;
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
        services.AddRazorPages();
        services.AddButterMorphDesign();
        services.AddButterMorphSchemaDesign();
        services.Configure(configure);

        return services;
    }
}
