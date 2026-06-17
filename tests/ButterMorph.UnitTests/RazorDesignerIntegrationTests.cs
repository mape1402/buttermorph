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
        string schemasHtml = await client.GetStringAsync("/buttermorph/schemas");
        string schemasToken = ExtractToken(schemasHtml);
        HttpResponseMessage demoResponse = await client.PostAsync(
            "/buttermorph/schemas" + QueryMarker() + "handler=Demo",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", schemasToken)
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
        Assert.Contains("Output field mappings", designerHtml, StringComparison.Ordinal);
        Assert.Contains("Target mappings saved", savedHtml, StringComparison.Ordinal);
        Assert.Contains("$source.Customer.Name", savedHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("$source.Customer.Email  }", savedHtml, StringComparison.Ordinal);
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
