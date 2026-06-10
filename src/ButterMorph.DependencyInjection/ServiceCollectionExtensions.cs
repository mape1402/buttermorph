using Microsoft.Extensions.DependencyInjection;

namespace ButterMorph.DependencyInjection;

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
        return services;
    }
}
