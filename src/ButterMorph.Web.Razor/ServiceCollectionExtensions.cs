namespace ButterMorph.Web.Razor;

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
        services.AddRazorPages();

        return services;
    }
}
