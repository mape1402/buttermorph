namespace ButterMorph.UnitTests;

using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

/// <summary>
/// Verifies the structured Studio Playground host.
/// </summary>
public sealed class StudioPlaygroundIntegrationTests : IClassFixture<WebApplicationFactory<ButterMorph.StudioPlayground.Program>>
{
    private readonly WebApplicationFactory<ButterMorph.StudioPlayground.Program> factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="StudioPlaygroundIntegrationTests"/> class.
    /// </summary>
    /// <param name="factory">The test host factory.</param>
    public StudioPlaygroundIntegrationTests(WebApplicationFactory<ButterMorph.StudioPlayground.Program> factory)
    {
        this.factory = factory;
    }

    /// <summary>
    /// Confirms the Studio home shell renders.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task StudioHomeRendersNavigation()
    {
        HttpClient client = factory.CreateClient();

        string html = await client.GetStringAsync("/");

        Assert.Contains("Studio Playground", html, StringComparison.Ordinal);
        Assert.Contains("Custom Types", html, StringComparison.Ordinal);
        Assert.Contains("Custom Fields", html, StringComparison.Ordinal);
        Assert.Contains("Schemas", html, StringComparison.Ordinal);
        Assert.Contains("Mappings", html, StringComparison.Ordinal);
        Assert.Contains("Execution", html, StringComparison.Ordinal);
        Assert.Contains("buttermorph-host.js", html, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms seeded state includes all host-owned catalogs.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task StudioStateIncludesSeededCatalogs()
    {
        HttpClient client = factory.CreateClient();

        string json = await client.GetStringAsync("/api/state");

        Assert.Contains("UniqueIdentifier", json, StringComparison.Ordinal);
        Assert.Contains("RFC", json, StringComparison.Ordinal);
        Assert.Contains("Topic", json, StringComparison.Ordinal);
        Assert.Contains("allowedValues", json, StringComparison.Ordinal);
        Assert.Contains("Customer Profile", json, StringComparison.Ordinal);
        Assert.Contains("Customer Profile to Summary", json, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms the visible Studio payload is the clean ButterMorph definition.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Theory]
    [InlineData("customTypes", "3d56346a-934c-414c-8659-8bc203e021c4")]
    [InlineData("customFields", "field-topic")]
    [InlineData("schemas", "schema-customer-profile")]
    public async Task StudioVisibleDefinitionDoesNotExposeOperationalOrHostFields(string kind, string id)
    {
        HttpClient client = factory.CreateClient();

        string json = await client.GetStringAsync("/api/" + kind + "/" + id);
        using JsonDocument document = JsonDocument.Parse(json);
        string visible = document.RootElement.GetProperty("butterMorphResultJson").GetString();
        using JsonDocument visibleDocument = JsonDocument.Parse(visible);

        Assert.Contains("\"key\"", visible, StringComparison.Ordinal);
        Assert.False(HasRootProperty(visibleDocument.RootElement, "id"));
        Assert.False(HasRootProperty(visibleDocument.RootElement, "savedAt"));
        Assert.False(HasRootProperty(visibleDocument.RootElement, "succeeded"));
        Assert.False(HasRootProperty(visibleDocument.RootElement, "diagnostics"));
    }

    /// <summary>
    /// Confirms seeded mapping execution returns output JSON.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task StudioSeededMappingExecutes()
    {
        HttpClient client = factory.CreateClient();
        string body = "{\"sources\":{}}";
        using StringContent content = new(body, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync("/api/mappings/mapping-customer-profile-to-summary/execute", content);
        string json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Northwind Trading", json, StringComparison.Ordinal);
        Assert.Contains("NTR990101ABC", json, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms schema injection settings are host-owned and editable.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task StudioSchemaInjectionCanBeUpdated()
    {
        HttpClient client = factory.CreateClient();
        string body = JsonSerializer.Serialize(new
        {
            customTypeKeys = new[] { "3d56346a-934c-414c-8659-8bc203e021c4" },
            customFieldKeys = new[] { "field-topic" }
        });
        using StringContent content = new(body, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync("/api/schemas/schema-customer-profile/injection", content);
        string json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("3d56346a-934c-414c-8659-8bc203e021c4", json, StringComparison.Ordinal);
        Assert.Contains("field-topic", json, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms schema tooling create requests only reserve host ids until ButterMorph saves.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Theory]
    [InlineData("customTypes")]
    [InlineData("customFields")]
    [InlineData("schemas")]
    public async Task StudioSchemaToolCreateDoesNotPersistBeforeDesignerSave(string kind)
    {
        HttpClient client = factory.CreateClient();
        string id = kind + "-unsaved-test";
        using StringContent content = new("{\"id\":\"" + id + "\",\"name\":\"Unsaved Item\"}", Encoding.UTF8, "application/json");

        HttpResponseMessage createResponse = await client.PostAsync("/api/" + kind, content);
        HttpResponseMessage itemResponse = await client.GetAsync("/api/" + kind + "/" + id);

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, itemResponse.StatusCode);
    }

    /// <summary>
    /// Confirms schema creation lets the host choose injection before opening ButterMorph.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task StudioSchemaCreateRendersPreDesignerInjectionSetup()
    {
        HttpClient client = factory.CreateClient();

        string script = await client.GetStringAsync("/studio.js");

        Assert.Contains("New Schema Setup", script, StringComparison.Ordinal);
        Assert.Contains("customTypes=", script, StringComparison.Ordinal);
        Assert.Contains("customFields=", script, StringComparison.Ordinal);
        Assert.Contains("data-setup-type", script, StringComparison.Ordinal);
        Assert.Contains("data-setup-field", script, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms mapping create persists because mappings are configured by the host.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task StudioMappingCreatePersistsHostItem()
    {
        HttpClient client = factory.CreateClient();
        using StringContent content = new("{\"id\":\"mapping-unsaved-test\",\"name\":\"Host Mapping\"}", Encoding.UTF8, "application/json");

        HttpResponseMessage createResponse = await client.PostAsync("/api/mappings", content);
        HttpResponseMessage itemResponse = await client.GetAsync("/api/mappings/mapping-unsaved-test");

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, itemResponse.StatusCode);
    }

    // Checks root property presence using case-insensitive comparison.
    private static bool HasRootProperty(JsonElement element, string propertyName)
    {
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
