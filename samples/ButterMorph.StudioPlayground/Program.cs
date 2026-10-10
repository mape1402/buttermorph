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
        builder.Services.AddButterMorphRazorDesigner(UseDarkPinkTheme);
        builder.Services.AddSingleton<StudioStore>();
        builder.Services.AddSingleton<StudioButterMorphHost>();
        builder.Services.AddSingleton<IButterMorphDesignerHost>(provider => provider.GetRequiredService<StudioButterMorphHost>());
        builder.Services.AddSingleton<IButterMorphValidationDesignerHost>(provider => provider.GetRequiredService<StudioButterMorphHost>());
        builder.Services.AddSingleton<IButterMorphSchemaTypeDesignerHost>(provider => provider.GetRequiredService<StudioButterMorphHost>());
        builder.Services.AddSingleton<IButterMorphFieldMetadataDesignerHost>(provider => provider.GetRequiredService<StudioButterMorphHost>());
        builder.Services.AddSingleton<IButterMorphPayloadSchemaDesignerHost>(provider => provider.GetRequiredService<StudioButterMorphHost>());
        builder.Services.AddSingleton<IButterMorphPayloadSchemaViewerHost>(provider => provider.GetRequiredService<StudioButterMorphHost>());

        WebApplication app = builder.Build();

        StudioSeedData.Seed(app.Services.GetRequiredService<StudioStore>());

        app.UseStaticFiles();
        app.MapGet("/", () => Results.Content(StudioHtml.Render(), "text/html"));
        app.MapStudioEndpoints();
        app.MapButterMorphDesigner("/buttermorph");

        app.Run();
    }

    private static void UseDarkPinkTheme(ButterMorphRazorDesignerOptions options)
    {
        options.Theme.UseDarkMode();

        ConfigureLightPinkPalette(options.Theme.Light);
        ConfigureDarkPinkPalette(options.Theme.Dark);
    }

    private static void ConfigureLightPinkPalette(ButterMorphDesignerThemePaletteOptions palette)
    {
        palette.PrimaryColor = "#db2777";
        palette.PrimaryHoverColor = "#be185d";
        palette.PrimaryDarkColor = "#9d174d";
        palette.BackgroundColor = "#fff7fb";
        palette.SurfaceColor = "#ffffff";
        palette.SurfaceSoftColor = "#fff1f7";
        palette.TextColor = "#27111d";
        palette.MutedTextColor = "#7a5266";
        palette.BorderColor = "#f3d3e3";
        palette.StrongBorderColor = "#e8b8cf";
        palette.DangerColor = "#be123c";
        palette.SidebarBackgroundColor = "#21101a";
        palette.SidebarBrandBackgroundColor = "#160a12";
        palette.SidebarBorderColor = "#432238";
        palette.SidebarTextColor = "#f7d7e8";
        palette.SidebarMutedTextColor = "#d5a8bf";
        palette.SidebarActiveTextColor = "#fbcfe8";
    }

    private static void ConfigureDarkPinkPalette(ButterMorphDesignerThemePaletteOptions palette)
    {
        palette.PrimaryColor = "#f472b6";
        palette.PrimaryHoverColor = "#ec4899";
        palette.PrimaryDarkColor = "#f9a8d4";
        palette.BackgroundColor = "#100b12";
        palette.SurfaceColor = "#17121a";
        palette.SurfaceSoftColor = "#211925";
        palette.TextColor = "#fff7fb";
        palette.MutedTextColor = "#d8bdcb";
        palette.BorderColor = "#392634";
        palette.StrongBorderColor = "#57394e";
        palette.DangerColor = "#fb7185";
        palette.SidebarBackgroundColor = "#140b13";
        palette.SidebarBrandBackgroundColor = "#0d070d";
        palette.SidebarBorderColor = "#352131";
        palette.SidebarTextColor = "#f4d7e6";
        palette.SidebarMutedTextColor = "#caa2b8";
        palette.SidebarActiveTextColor = "#f9a8d4";
    }
}
