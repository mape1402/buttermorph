namespace ButterMorph.StudioPlayground;

using ButterMorph.DependencyInjection;
using ButterMorph.Json.Schema;
using ButterMorph.StudioPlayground.Services;
using ButterMorph.Web.Razor;

/// <summary>
/// Runs the structured ButterMorph Studio Playground host.
/// </summary>
public sealed partial class Program
{
    private static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddButterMorph();
        builder.Services.AddButterMorphJsonSchema();
        builder.Services.AddButterMorphRazorDesigner();
        builder.Services.AddSingleton<StudioStore>();
        builder.Services.AddSingleton<StudioButterMorphHost>();
        builder.Services.AddSingleton<IButterMorphDesignerHost>(provider => provider.GetRequiredService<StudioButterMorphHost>());
        builder.Services.AddSingleton<IButterMorphSchemaTypeDesignerHost>(provider => provider.GetRequiredService<StudioButterMorphHost>());
        builder.Services.AddSingleton<IButterMorphFieldMetadataDesignerHost>(provider => provider.GetRequiredService<StudioButterMorphHost>());
        builder.Services.AddSingleton<IButterMorphPayloadSchemaDesignerHost>(provider => provider.GetRequiredService<StudioButterMorphHost>());

        WebApplication app = builder.Build();

        StudioSeedData.Seed(app.Services.GetRequiredService<StudioStore>());

        app.UseStaticFiles();
        app.MapGet("/", () => Results.Content(StudioHtml.Render(), "text/html"));
        app.MapStudioEndpoints();
        app.MapButterMorphDesigner("/buttermorph");

        app.Run();
    }
}
