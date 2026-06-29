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
        Assert.Contains("Customer Profile", json, StringComparison.Ordinal);
        Assert.Contains("Customer Profile to Summary", json, StringComparison.Ordinal);
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
}
