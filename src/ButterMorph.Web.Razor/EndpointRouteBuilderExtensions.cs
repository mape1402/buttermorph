namespace ButterMorph.Web.Razor;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Provides endpoint mapping helpers for the reusable Razor designer.
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the reusable ButterMorph Razor designer.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="routePrefix">The route prefix.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapButterMorphDesigner(this IEndpointRouteBuilder endpoints, string routePrefix)
    {
        string prefix = routePrefix.TrimEnd('/');

        if (!string.Equals(prefix, "/buttermorph", StringComparison.OrdinalIgnoreCase))
        {
            endpoints.MapGet(prefix, () => Results.Redirect("/buttermorph"));
        }

        endpoints.MapRazorPages();

        return endpoints;
    }
}
