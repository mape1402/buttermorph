using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.DependencyInjection;
using ButterMorph.Design;
using ButterMorph.Functions;
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

RegisterDemoCapabilities(app.Services);

app.MapGet("/", () => Results.Redirect("/buttermorph"));
app.MapButterMorphDesigner("/buttermorph");

app.Run();

// Registers demo descriptors for the playground toolbox.
static void RegisterDemoCapabilities(IServiceProvider services)
{
    IFunctionRegistry functionRegistry = services.GetRequiredService<IFunctionRegistry>();
    functionRegistry.Register("concat", new DemoFunction(), new FunctionDescriptor
    {
        Key = "concat",
        DisplayName = "Concat",
        Description = "Combines scalar values.",
        ValueKind = FunctionValueKind.Scalar,
        Parameters =
        [
            new FunctionParameterDescriptor
            {
                Key = "left",
                DisplayName = "Left",
                Description = "Left scalar.",
                ValueKind = FunctionValueKind.Scalar,
                IsRequired = true
            },
            new FunctionParameterDescriptor
            {
                Key = "right",
                DisplayName = "Right",
                Description = "Right scalar.",
                ValueKind = FunctionValueKind.Scalar,
                IsRequired = true
            }
        ]
    });

    IValidationRuleRegistry validationRuleRegistry = services.GetRequiredService<IValidationRuleRegistry>();
    validationRuleRegistry.Register("required", new DemoValidationRuleHandler(), new ValidationRuleDescriptor
    {
        Key = "required",
        DisplayName = "Required",
        Description = "Requires a value.",
        ValueKind = FunctionValueKind.Scalar,
        Parameters = []
    });
}

/// <summary>
/// Exposes the playground entry point for integration tests.
/// </summary>
public partial class Program
{
}
