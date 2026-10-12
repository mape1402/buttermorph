namespace ButterMorph.Language.CSharp;

using ButterMorph.DependencyInjection;
using ButterMorph.Language;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides service registration helpers for the official C# language adapter.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the official C# language adapter to ButterMorph.
    /// </summary>
    /// <param name="builder">The ButterMorph builder.</param>
    /// <returns>The configured ButterMorph builder.</returns>
    public static IButterMorphBuilder AddCSharpLanguageAdapter(this IButterMorphBuilder builder)
    {
        builder.Services.AddSingleton<CSharpLanguageAdapter>();
        builder.Services.AddSingleton<IButterMorphLanguageAdapter>(provider => provider.GetRequiredService<CSharpLanguageAdapter>());
        builder.Services.AddSingleton<IButterMorphPrimitiveTypeResolver>(provider => provider.GetRequiredService<CSharpLanguageAdapter>());
        return builder;
    }
}
