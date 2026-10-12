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
        Assert.Contains("Type Bundles", html, StringComparison.Ordinal);
        Assert.Contains("Type Mappings", html, StringComparison.Ordinal);
        Assert.Contains("Schemas", html, StringComparison.Ordinal);
        Assert.Contains("Mappings", html, StringComparison.Ordinal);
        Assert.Contains("Validations", html, StringComparison.Ordinal);
        Assert.Contains("Execution", html, StringComparison.Ordinal);
        Assert.Contains("Validate", html, StringComparison.Ordinal);
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
        Assert.Contains("Customer Profile Validation", json, StringComparison.Ordinal);
        Assert.Contains("languageTypeProfiles", json, StringComparison.Ordinal);
        Assert.Contains("csharp-type-mappings", json, StringComparison.Ordinal);
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
        Assert.DoesNotContain("typeVersionId", visible, StringComparison.Ordinal);
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
    /// Confirms seeded mapping payloads validate without producing output.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task StudioSeededMappingValidates()
    {
        HttpClient client = factory.CreateClient();
        string body = "{\"sources\":{}}";
        using StringContent content = new(body, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync("/api/mappings/mapping-customer-profile-to-summary/validate", content);
        string json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"succeeded\":true", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"outputJson\":\"\"", json, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Confirms the Studio host exposes a host-owned bundle listing section.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task StudioScriptIncludesLanguageTypeBundleListing()
    {
        HttpClient client = factory.CreateClient();

        string script = await client.GetStringAsync("/studio.js");

        Assert.Contains("renderLanguageTypeBundles", script, StringComparison.Ordinal);
        Assert.Contains("Language type bundles", script, StringComparison.Ordinal);
        Assert.Contains("Create Bundle", script, StringComparison.Ordinal);
        Assert.Contains("bundle-type-list", script, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms seeded validation documents run through the real engine.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task StudioSeededValidationRuns()
    {
        HttpClient client = factory.CreateClient();
        string body = "{\"sources\":{}}";
        using StringContent content = new(body, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync("/api/validations/validation-customer-profile/run", content);
        string json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("\"succeeded\":true", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"outputJson\":\"\"", json, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Confirms the Studio sample exposes read-only mapping and validation viewer actions.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task StudioScriptIncludesDocumentViewerActions()
    {
        HttpClient client = factory.CreateClient();

        string script = await client.GetStringAsync("/studio.js");

        Assert.Contains("View Mapping", script, StringComparison.Ordinal);
        Assert.Contains("/buttermorph/mappings/viewer", script, StringComparison.Ordinal);
        Assert.Contains("View Validation", script, StringComparison.Ordinal);
        Assert.Contains("/buttermorph/validations/viewer", script, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms seeded mappings can be opened in the read-only mapping viewer.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task StudioSeededMappingViewerRenders()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/buttermorph/mappings/viewer?context=mapping-customer-profile-to-summary");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Customer Profile to Summary", html, StringComparison.Ordinal);
        Assert.Contains("data-document-view-button=\"dsl\"", html, StringComparison.Ordinal);
        Assert.Contains("CustomerId", html, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms seeded validations can be opened in the read-only validation viewer.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task StudioSeededValidationViewerRenders()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/buttermorph/validations/viewer?context=validation-customer-profile");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Customer Profile Validation", html, StringComparison.Ordinal);
        Assert.Contains("data-document-view-button=\"dsl\"", html, StringComparison.Ordinal);
        Assert.Contains("Customer id is required.", html, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms seeded schemas can be opened in the read-only code viewer.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task StudioSeededSchemaCodeViewerRenders()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/buttermorph/payload-schema/viewer?context=schema-customer-profile&view=code");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("data-schema-active-view=\"code\"", html, StringComparison.Ordinal);
        Assert.Contains("Company.SharedTypes.Rfc", html, StringComparison.Ordinal);
        Assert.Contains("Company.SharedTypes.Rfc RFC", html, StringComparison.Ordinal);
        Assert.Contains("ButterMorph.Generated", html, StringComparison.Ordinal);
        Assert.DoesNotContain("\"butterMorphType\":\"SecurityClasification\"", html, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms language type mappings can be opened in their own designer.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task StudioSeededLanguageTypeMappingsDesignerRenders()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/buttermorph/language-type-mappings/designer");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Type mapping packages", html, StringComparison.Ordinal);
        Assert.Contains("New package", html, StringComparison.Ordinal);
        Assert.Contains("Add mapping", html, StringComparison.Ordinal);
        Assert.Contains("DataType (ButterMorph)", html, StringComparison.Ordinal);
        Assert.Contains("DataType (Language)", html, StringComparison.Ordinal);
        Assert.Contains("Company.SharedTypes.Rfc", html, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms language type bundles are created as a single discovered document.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task StudioLanguageTypeBundlesDesignerRendersDocumentFlow()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/buttermorph/language-type-bundles/designer");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Type Bundles", html, StringComparison.Ordinal);
        Assert.Contains("Choose", html, StringComparison.Ordinal);
        Assert.Contains("Preview", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Registered bundles", html, StringComparison.Ordinal);
        Assert.DoesNotContain("company-shared-types", html, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms the Studio host persists bundle documents received from ButterMorph events.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task StudioLanguageTypeBundleEndpointPersistsBundleDocument()
    {
        HttpClient client = factory.CreateClient();
        string body = """
            {
              "assemblyId": "test-shared-types",
              "assemblyKey": "test-shared-types",
              "assemblyDescription": "Test shared types.",
              "assemblyVersion": "1.0.0",
              "languageKey": "csharp",
              "types": [
                {
                  "key": "TestRfc",
                  "displayName": "Test RFC",
                  "qualifiedName": "Company.SharedTypes.TestRfc",
                  "version": "1.0.0",
                  "description": "Test RFC type."
                }
              ]
            }
            """;
        using StringContent content = new(body, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync("/api/language-type-bundles", content);
        string json = await client.GetStringAsync("/api/state");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("test-shared-types", json, StringComparison.Ordinal);
        Assert.Contains("Company.SharedTypes.TestRfc", json, StringComparison.Ordinal);
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
    /// Confirms custom field allowed values are injected into schema designer metadata catalogs.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task StudioSchemaDesignerReceivesAllowedValuesForInjectedCustomFields()
    {
        HttpClient client = factory.CreateClient();

        string html = await client.GetStringAsync("/buttermorph/payload-schema/designer?context=schema-allowed-values-test&mode=create&customFields=field-security-classification");

        Assert.Contains("field-metadata-catalog", html, StringComparison.Ordinal);
        Assert.Contains("Security Clasification", html, StringComparison.Ordinal);
        Assert.Contains("allowedValues", html, StringComparison.Ordinal);
        Assert.Contains("Confidential", html, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms editing a custom field reloads its saved allowed values.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task StudioCustomFieldEditReloadsAllowedValues()
    {
        HttpClient client = factory.CreateClient();

        string html = await client.GetStringAsync("/buttermorph/metadata-fields/designer?context=field-security-classification&mode=edit");

        Assert.Contains("allowed-values-hidden", html, StringComparison.Ordinal);
        Assert.Contains("Public", html, StringComparison.Ordinal);
        Assert.Contains("Private", html, StringComparison.Ordinal);
        Assert.Contains("Confidential", html, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms editing a custom type reloads constraints through ButterMorph.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task StudioCustomTypeEditReloadsScalarConstraints()
    {
        HttpClient client = factory.CreateClient();

        string html = await client.GetStringAsync("/buttermorph/schema-types/designer?context=3d56346a-934c-414c-8659-8bc203e021c4&mode=edit");

        Assert.Contains("name=\"Input.MinLength\"", html, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.MaxLength\"", html, StringComparison.Ordinal);
        Assert.Contains("value=\"26\"", html, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms Studio does not own schema designer rehydration logic.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task StudioHostDoesNotContainSchemaDesignerHydrationHelpers()
    {
        string source = await File.ReadAllTextAsync(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "..",
            "samples",
            "ButterMorph.StudioPlayground",
            "Services",
            "StudioButterMorphHost.cs"));

        Assert.DoesNotContain("ApplySchemaTypeConstraints", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ReadSchemaTypeSchemaElement", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ReadSchemaMetadata", source, StringComparison.Ordinal);
        Assert.DoesNotContain("CreateSchemaTypeInput", source, StringComparison.Ordinal);
        Assert.DoesNotContain("CreateFieldInput", source, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms mapping setup stays temporary until ButterMorph saves.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task StudioMappingSetupDoesNotPersistHostItem()
    {
        HttpClient client = factory.CreateClient();
        using StringContent content = new("{\"name\":\"Host Mapping\",\"targetSchemaId\":\"schema-customer-summary\",\"sourceSchemaIds\":{\"customer\":\"schema-customer-profile\"},\"showSchemaActions\":true}", Encoding.UTF8, "application/json");

        HttpResponseMessage createResponse = await client.PostAsync("/api/mappings/mapping-unsaved-test/setup", content);
        HttpResponseMessage itemResponse = await client.GetAsync("/api/mappings/mapping-unsaved-test");

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, itemResponse.StatusCode);
    }

    /// <summary>
    /// Confirms validation setup stays temporary until ButterMorph saves.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task StudioValidationSetupDoesNotPersistHostItem()
    {
        HttpClient client = factory.CreateClient();
        using StringContent content = new("{\"name\":\"Host Validation\",\"sourceSchemaIds\":{\"customer\":\"schema-customer-profile\"}}", Encoding.UTF8, "application/json");

        HttpResponseMessage createResponse = await client.PostAsync("/api/validations/validation-unsaved-test/setup", content);
        HttpResponseMessage itemResponse = await client.GetAsync("/api/validations/validation-unsaved-test");

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, itemResponse.StatusCode);
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
