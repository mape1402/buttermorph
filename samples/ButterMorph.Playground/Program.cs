using ButterMorph.DependencyInjection;
using ButterMorph.Design;
using ButterMorph.Json.Schema;
using ButterMorph.Web.Razor;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddButterMorph();
builder.Services.AddButterMorphDesign();
builder.Services.AddButterMorphJsonSchema();
builder.Services.AddButterMorphRazorDesigner();

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/buttermorph");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/buttermorph"));
app.MapButterMorphDesigner("/buttermorph");

app.Run();

/// <summary>
/// Exposes the playground entry point for integration tests.
/// </summary>
public partial class Program
{
}
