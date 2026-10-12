namespace ButterMorph.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Exposes the service collection while composing ButterMorph packages.
/// </summary>
public interface IButterMorphBuilder
{
    /// <summary>
    /// Gets the application service collection.
    /// </summary>
    IServiceCollection Services { get; }
}

internal sealed class ButterMorphBuilder : IButterMorphBuilder
{
    public ButterMorphBuilder(IServiceCollection services)
    {
        Services = services;
    }

    public IServiceCollection Services { get; }
}
