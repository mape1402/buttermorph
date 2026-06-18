namespace ButterMorph.UnitTests;

using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

/// <summary>
/// Verifies the reusable Razor designer in the playground host.
/// </summary>
public sealed class RazorDesignerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    // Creates playground test clients.
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RazorDesignerIntegrationTests"/> class.
    /// </summary>
    /// <param name="factory">The playground web application factory.</param>
    public RazorDesignerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Confirms that reusable designer routes respond successfully.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerRoutesRespondSuccessfully()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage home = await client.GetAsync("/buttermorph");
        HttpResponseMessage schemas = await client.GetAsync("/buttermorph/schemas");
        HttpResponseMessage designer = await client.GetAsync("/buttermorph/designer");
        HttpResponseMessage dsl = await client.GetAsync("/buttermorph/dsl");

        Assert.Equal(HttpStatusCode.OK, home.StatusCode);
        Assert.Equal(HttpStatusCode.OK, schemas.StatusCode);
        Assert.Equal(HttpStatusCode.OK, designer.StatusCode);
        Assert.Equal(HttpStatusCode.OK, dsl.StatusCode);
    }

    /// <summary>
    /// Confirms that reusable designer static assets respond successfully.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerStaticAssetsRespondSuccessfully()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/_content/ButterMorph.Web.Razor/buttermorph/designer.css");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Confirms that target-schema mapping rows can be saved without bad requests.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerSavesTargetFieldMappingsSuccessfully()
    {
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage demoResponse = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "handler=LoadDemo",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", ExtractToken(await client.GetStringAsync("/buttermorph/designer")))
            ]));

        string designerHtml = await client.GetStringAsync("/buttermorph/designer");
        string designerToken = ExtractToken(designerHtml);
        HttpResponseMessage saveResponse = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "handler=SaveTargetMappings",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", designerToken),
                new KeyValuePair<string, string>("TargetPaths", "Customer.Name"),
                new KeyValuePair<string, string>("Expressions", "$source.Customer.Name"),
                new KeyValuePair<string, string>("TargetPaths", "Customer.Email"),
                new KeyValuePair<string, string>("Expressions", "$source.Customer.Email")
            ]));
        string savedHtml = await saveResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, demoResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);
        Assert.Contains("Output schema", designerHtml, StringComparison.Ordinal);
        Assert.Contains("Target mappings saved", savedHtml, StringComparison.Ordinal);
        Assert.Contains("$source.Customer.Name", savedHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("$source.Customer.Email  }", savedHtml, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that report-style designer markup is rendered.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerRendersReportStyleWorkbench()
    {
        HttpClient client = _factory.CreateClient();

        string html = await client.GetStringAsync("/buttermorph/designer");

        Assert.Contains("bm-toolbox", html, StringComparison.Ordinal);
        Assert.Contains("bm-designer-surface", html, StringComparison.Ordinal);
        Assert.Contains("data-view=\"Dsl\"", html, StringComparison.Ordinal);
        Assert.Contains("Add source schema", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Current mappings", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Analyzer", html, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that DSL import and export are available from the designer page.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerImportsAndExportsDslSuccessfully()
    {
        HttpClient client = _factory.CreateClient();
        string designerHtml = await client.GetStringAsync("/buttermorph/designer");
        string token = ExtractToken(designerHtml);
        HttpResponseMessage importResponse = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "handler=ImportDsl",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
                new KeyValuePair<string, string>("DslContent", "target { Customer { Name: $source.Customer.Name } }"),
                new KeyValuePair<string, string>("ActiveView", "Dsl")
            ]));
        string importedHtml = await importResponse.Content.ReadAsStringAsync();
        string exportToken = ExtractToken(importedHtml);
        HttpResponseMessage exportResponse = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "handler=ExportDsl",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", exportToken),
                new KeyValuePair<string, string>("ActiveView", "Dsl")
            ]));
        string exportedHtml = await exportResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, importResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, exportResponse.StatusCode);
        Assert.Contains("DSL imported", importedHtml, StringComparison.Ordinal);
        Assert.Contains("$source.Customer.Name", exportedHtml, StringComparison.Ordinal);
    }

    // Extracts an antiforgery token from rendered Razor markup.
    private static string ExtractToken(string html)
    {
        string marker = "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"";
        int markerIndex = html.IndexOf(marker, StringComparison.Ordinal);

        if (markerIndex < 0)
        {
            return string.Empty;
        }

        int valueStart = markerIndex + marker.Length;
        int valueEnd = html.IndexOf("\"", valueStart, StringComparison.Ordinal);

        if (valueEnd < valueStart)
        {
            return string.Empty;
        }

        return html[valueStart..valueEnd];
    }

    // Creates the query separator without using forbidden nullable syntax characters.
    private static string QueryMarker()
    {
        return Convert.ToChar(63).ToString();
    }
}
