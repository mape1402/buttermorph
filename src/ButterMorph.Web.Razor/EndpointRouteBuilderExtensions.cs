namespace ButterMorph.Web.Razor;

using System.Reflection;
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

        MapDesignerAsset(endpoints, "designer.css", "text/css; charset=utf-8");
        MapDesignerAsset(endpoints, "designer.js", "text/javascript; charset=utf-8");
        MapDesignerAsset(endpoints, "vendor/codemirror/codemirror.min.css", "text/css; charset=utf-8");
        MapDesignerAsset(endpoints, "vendor/codemirror/codemirror.min.js", "text/javascript; charset=utf-8");
        MapDesignerAsset(endpoints, "vendor/codemirror/show-hint.min.css", "text/css; charset=utf-8");
        MapDesignerAsset(endpoints, "vendor/codemirror/show-hint.min.js", "text/javascript; charset=utf-8");
        MapDesignerAsset(endpoints, "atlas-schema.css", "text/css; charset=utf-8");
        MapDesignerAsset(endpoints, "atlas-schema-builder.js", "text/javascript; charset=utf-8");
        MapDesignerAsset(endpoints, "atlas-schema-metadata-editor.js", "text/javascript; charset=utf-8");
        MapDesignerAsset(endpoints, "atlas-type-version-editor.js", "text/javascript; charset=utf-8");
        MapDesignerAsset(endpoints, "atlas-metadata-field-editor.js", "text/javascript; charset=utf-8");
        endpoints.MapRazorPages();

        return endpoints;
    }

    /// <summary>
    /// Maps an embedded designer asset.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="fileName">The asset file name.</param>
    /// <param name="contentType">The response content type.</param>
    private static void MapDesignerAsset(IEndpointRouteBuilder endpoints, string fileName, string contentType)
    {
        endpoints.MapGet("/_content/ButterMorph.Web.Razor/buttermorph/" + fileName, () =>
        {
            Assembly assembly = typeof(EndpointRouteBuilderExtensions).Assembly;
            string resourceName = "ButterMorph.Web.Razor.wwwroot.buttermorph." + fileName.Replace("/", ".", StringComparison.Ordinal);
            Stream stream = assembly.GetManifestResourceStream(resourceName);

            if (stream == null)
            {
                return Results.NotFound();
            }

            return Results.Stream(stream, contentType);
        });
    }
}
