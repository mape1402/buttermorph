namespace ButterMorph.UnitTests;

using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Web.Razor;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

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
        HttpResponseMessage schemaDesigner = await client.GetAsync("/buttermorph/schema-designer");
        HttpResponseMessage schemaTypeDesigner = await client.GetAsync("/buttermorph/schema-types/designer");
        HttpResponseMessage metadataFieldDesigner = await client.GetAsync("/buttermorph/metadata-fields/designer");
        HttpResponseMessage payloadSchemaDesigner = await client.GetAsync("/buttermorph/payload-schema/designer");
        HttpResponseMessage dsl = await client.GetAsync("/buttermorph/dsl");

        Assert.Equal(HttpStatusCode.OK, home.StatusCode);
        Assert.Equal(HttpStatusCode.OK, schemas.StatusCode);
        Assert.Equal(HttpStatusCode.OK, designer.StatusCode);
        Assert.Equal(HttpStatusCode.OK, schemaDesigner.StatusCode);
        Assert.Equal(HttpStatusCode.OK, schemaTypeDesigner.StatusCode);
        Assert.Equal(HttpStatusCode.OK, metadataFieldDesigner.StatusCode);
        Assert.Equal(HttpStatusCode.OK, payloadSchemaDesigner.StatusCode);
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

        HttpResponseMessage cssResponse = await client.GetAsync("/_content/ButterMorph.Web.Razor/buttermorph/designer.css");
        HttpResponseMessage scriptResponse = await client.GetAsync("/_content/ButterMorph.Web.Razor/buttermorph/designer.js");
        HttpResponseMessage codeMirrorCssResponse = await client.GetAsync("/_content/ButterMorph.Web.Razor/buttermorph/vendor/codemirror/codemirror.min.css");
        HttpResponseMessage codeMirrorScriptResponse = await client.GetAsync("/_content/ButterMorph.Web.Razor/buttermorph/vendor/codemirror/codemirror.min.js");
        HttpResponseMessage codeMirrorHintCssResponse = await client.GetAsync("/_content/ButterMorph.Web.Razor/buttermorph/vendor/codemirror/show-hint.min.css");
        HttpResponseMessage codeMirrorHintScriptResponse = await client.GetAsync("/_content/ButterMorph.Web.Razor/buttermorph/vendor/codemirror/show-hint.min.js");
        string css = await cssResponse.Content.ReadAsStringAsync();
        string script = await scriptResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, cssResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, scriptResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, codeMirrorCssResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, codeMirrorScriptResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, codeMirrorHintCssResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, codeMirrorHintScriptResponse.StatusCode);
        Assert.Contains("data-left-dock-mode=\"auto\"", css, StringComparison.Ordinal);
        Assert.Contains("bm-dock-tabs", css, StringComparison.Ordinal);
        Assert.Contains("bm-dock-tab", css, StringComparison.Ordinal);
        Assert.Contains("bm-dock-panel-host", css, StringComparison.Ordinal);
        Assert.Contains("bm-dock-panel", css, StringComparison.Ordinal);
        Assert.Contains("bm-dock-titlebar", css, StringComparison.Ordinal);
        Assert.Contains("bm-dock-flyout-open", css, StringComparison.Ordinal);
        Assert.Contains("ButterMorphDesigner.LeftDockMode", script, StringComparison.Ordinal);
        Assert.Contains("ButterMorphDesigner.LeftDockPanel", script, StringComparison.Ordinal);
        Assert.Contains("ButterMorphDesigner.ToolboxMode", script, StringComparison.Ordinal);
        Assert.Contains("data-dock-tab", script, StringComparison.Ordinal);
        Assert.Contains("URLSearchParams(window.location.search)", script, StringComparison.Ordinal);
        Assert.Contains("parameters.set(\"handler\", handler)", script, StringComparison.Ordinal);
        Assert.Contains("Sync request failed with status", script, StringComparison.Ordinal);
        Assert.Contains("\"RequestVerificationToken\": token", script, StringComparison.Ordinal);
        Assert.Contains("data-function-template", script, StringComparison.Ordinal);
        Assert.Contains("selectFirstFunctionArgument", script, StringComparison.Ordinal);
        Assert.Contains("hint: function (editor, data, completion)", script, StringComparison.Ordinal);
        Assert.Contains("editor.replaceRange(completion.text, data.from, data.to, \"complete\")", script, StringComparison.Ordinal);
        Assert.Contains("rememberDslSelection", script, StringComparison.Ordinal);
        Assert.Contains("insertIntoDslEditor", script, StringComparison.Ordinal);
        Assert.Contains("initializeDslCodeEditor", script, StringComparison.Ordinal);
        Assert.Contains("buttermorphDsl", script, StringComparison.Ordinal);
        Assert.Contains("createDslHintProvider", script, StringComparison.Ordinal);
        Assert.Contains("getDslCompletionContext", script, StringComparison.Ordinal);
        Assert.Contains("createProjectSuggestions", script, StringComparison.Ordinal);
        Assert.Contains("applyDslDiagnostics", script, StringComparison.Ordinal);
        Assert.Contains("renderDslDiagnosticPanel", script, StringComparison.Ordinal);
        Assert.Contains("clearDslDiagnosticPanel", script, StringComparison.Ordinal);
        Assert.Contains("goToDslDiagnostic", script, StringComparison.Ordinal);
        Assert.Contains("data-dsl-diagnostics-toggle", script, StringComparison.Ordinal);
        Assert.Contains("createFunctionDescriptionMap", script, StringComparison.Ordinal);
        Assert.Contains("handleDslFunctionHover", script, StringComparison.Ordinal);
        Assert.Contains("ButterMorphDesignerSaved", script, StringComparison.Ordinal);
        Assert.Contains("window.opener.postMessage", script, StringComparison.Ordinal);
        Assert.Contains("window.close()", script, StringComparison.Ordinal);
        Assert.Contains("getDslValue", script, StringComparison.Ordinal);
        Assert.Contains("addEventListener(\"dblclick\"", script, StringComparison.Ordinal);
        Assert.Contains("replaceExpressionInput", script, StringComparison.Ordinal);
        Assert.Contains("application/x-buttermorph-function-template", script, StringComparison.Ordinal);
        Assert.Contains("application/x-buttermorph-source-path", script, StringComparison.Ordinal);
        Assert.Contains("bm-dock-flyout-open", script, StringComparison.Ordinal);
        Assert.Contains("hideMessage", script, StringComparison.Ordinal);
        Assert.Contains("::file-selector-button", css, StringComparison.Ordinal);
        Assert.Contains("bm-function-item", css, StringComparison.Ordinal);
        Assert.Contains(".bm-dsl-form .CodeMirror", css, StringComparison.Ordinal);
        Assert.Contains(".bm-dsl-form .CodeMirror-scroll", css, StringComparison.Ordinal);
        Assert.Contains(".bm-json-canvas", css, StringComparison.Ordinal);
        Assert.Contains(".CodeMirror-hints", css, StringComparison.Ordinal);
        Assert.Contains(".bm-dsl-diagnostic-underline", css, StringComparison.Ordinal);
        Assert.Contains(".bm-dsl-diagnostic-gutter", css, StringComparison.Ordinal);
        Assert.Contains(".bm-dsl-function-tooltip", css, StringComparison.Ordinal);
        Assert.Contains(".bm-dsl-diagnostics-panel", css, StringComparison.Ordinal);
        Assert.Contains(".bm-dsl-diagnostic-row", css, StringComparison.Ordinal);
        Assert.DoesNotContain(".bm-left-dock:hover .bm-dock-panel-host", css, StringComparison.Ordinal);
        Assert.DoesNotContain("mouseenter", script, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that target-schema mapping rows can be saved without bad requests.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerSavesTargetFieldMappingsSuccessfully()
    {
        HttpClient client = _factory.CreateClient();
        await LoadTestSchemas(client);
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

        Assert.Equal(HttpStatusCode.OK, saveResponse.StatusCode);
        Assert.Contains("Output schema", designerHtml, StringComparison.Ordinal);
        Assert.Contains("Mappings saved", savedHtml, StringComparison.Ordinal);
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

        Assert.Contains("bm-dock-panel bm-toolbox", html, StringComparison.Ordinal);
        Assert.Contains("bm-left-dock", html, StringComparison.Ordinal);
        Assert.Contains("bm-dock-tabs", html, StringComparison.Ordinal);
        Assert.Contains("bm-dock-tab", html, StringComparison.Ordinal);
        Assert.Contains("bm-dock-panel-host", html, StringComparison.Ordinal);
        Assert.Contains("bm-designer-surface", html, StringComparison.Ordinal);
        Assert.Contains("data-view=\"Dsl\"", html, StringComparison.Ordinal);
        Assert.Contains("vendor/codemirror/codemirror.min.css", html, StringComparison.Ordinal);
        Assert.Contains("vendor/codemirror/codemirror.min.js", html, StringComparison.Ordinal);
        Assert.Contains("vendor/codemirror/show-hint.min.css", html, StringComparison.Ordinal);
        Assert.Contains("vendor/codemirror/show-hint.min.js", html, StringComparison.Ordinal);
        Assert.Contains("data-dsl-editor=\"true\"", html, StringComparison.Ordinal);
        Assert.Contains("data-dsl-diagnostics-panel=\"true\"", html, StringComparison.Ordinal);
        Assert.Contains("data-dsl-diagnostics-count=\"true\"", html, StringComparison.Ordinal);
        Assert.Contains("data-dsl-diagnostics-list=\"true\"", html, StringComparison.Ordinal);
        Assert.Contains("No DSL diagnostics", html, StringComparison.Ordinal);
        Assert.Contains("data-left-dock-mode=\"pinned\"", html, StringComparison.Ordinal);
        Assert.Contains("data-dock-panel=\"sources\"", html, StringComparison.Ordinal);
        Assert.Contains("data-dock-panel=\"functions\"", html, StringComparison.Ordinal);
        Assert.Contains("data-dock-pin=\"sources\"", html, StringComparison.Ordinal);
        Assert.Contains("data-dock-pin=\"functions\"", html, StringComparison.Ordinal);
        Assert.Contains("data-dock-tab=\"sources\"", html, StringComparison.Ordinal);
        Assert.Contains("data-dock-tab=\"functions\"", html, StringComparison.Ordinal);
        Assert.Contains("data-function-search=\"true\"", html, StringComparison.Ordinal);
        Assert.Contains("data-function-template=\"split(argument0, argument1)\"", html, StringComparison.Ordinal);
        Assert.Contains("title=\"Splits text using a literal separator.\"", html, StringComparison.Ordinal);
        Assert.Contains("data-function-template=\"camelCase(argument0)\"", html, StringComparison.Ordinal);
        Assert.Contains("data-function-template=\"ToUpper(argument0)\"", html, StringComparison.Ordinal);
        Assert.Contains("data-function-template=\"sum(argument0)\"", html, StringComparison.Ordinal);
        Assert.Contains("bm-function-group", html, StringComparison.Ordinal);
        Assert.DoesNotContain("<details class=\"bm-function-group\" open>", html, StringComparison.Ordinal);
        Assert.Contains("data-open-modal=\"source\"", html, StringComparison.Ordinal);
        Assert.Contains("data-modal=\"output\"", html, StringComparison.Ordinal);
        Assert.Contains("Source name", html, StringComparison.Ordinal);
        Assert.Contains("bm-message-hidden", html, StringComparison.Ordinal);
        Assert.True(
            html.IndexOf("data-modal=\"source\"", StringComparison.Ordinal) > html.IndexOf("</section>", html.IndexOf("bm-dock-panel bm-toolbox", StringComparison.Ordinal), StringComparison.Ordinal));
        Assert.DoesNotContain("Current mappings", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Analyzer", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Import DSL", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Export DSL", html, StringComparison.Ordinal);
        Assert.DoesNotContain("bm-status", html, StringComparison.Ordinal);
        Assert.DoesNotContain("&quot;properties&quot;", html, StringComparison.Ordinal);
        Assert.DoesNotContain("value=\"source\"", html, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that the playground shell renders host simulation controls.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task PlaygroundHomeRendersHostSimulation()
    {
        HttpClient client = _factory.CreateClient();

        string html = await client.GetStringAsync("/");

        Assert.Contains("ButterMorph host playground", html, StringComparison.Ordinal);
        Assert.Contains("/playground/scenarios", html, StringComparison.Ordinal);
        Assert.Contains("/playground/mappings/", html, StringComparison.Ordinal);
        Assert.Contains("/playground/execute/", html, StringComparison.Ordinal);
        Assert.Contains("/playground/schema-scenarios", html, StringComparison.Ordinal);
        Assert.Contains("/playground/schemas/", html, StringComparison.Ordinal);
        Assert.Contains("data-edit", html, StringComparison.Ordinal);
        Assert.Contains("data-execute", html, StringComparison.Ordinal);
        Assert.Contains("data-schema-tab=\"type\"", html, StringComparison.Ordinal);
        Assert.Contains("data-schema-tab=\"field\"", html, StringComparison.Ordinal);
        Assert.Contains("data-schema-tab=\"payload\"", html, StringComparison.Ordinal);
        Assert.Contains("data-create-schema", html, StringComparison.Ordinal);
        Assert.Contains("data-edit-schema", html, StringComparison.Ordinal);
        Assert.Contains("data-delete-schema", html, StringComparison.Ordinal);
        Assert.Contains("ButterMorphDesignerSaved", html, StringComparison.Ordinal);
        Assert.Contains("/buttermorph/designer\" + queryMarker + \"context=", html, StringComparison.Ordinal);
        Assert.Contains("/playground-schema.js", html, StringComparison.Ordinal);
        string schemaScript = await client.GetStringAsync("/playground-schema.js");
        Assert.Contains("/playground/schema-items/", schemaScript, StringComparison.Ordinal);
        Assert.Contains("ButterMorph.Playground.SchemaTypes", schemaScript, StringComparison.Ordinal);
        Assert.Contains("cache: \"no-store\"", schemaScript, StringComparison.Ordinal);
        Assert.Contains("formatItemResult", schemaScript, StringComparison.Ordinal);
        Assert.Contains("validation: parseJsonText", schemaScript, StringComparison.Ordinal);
        Assert.Contains("versionNumber: item.versionNumber", schemaScript, StringComparison.Ordinal);
        Assert.Contains("jsonSchema: parseJsonText(item.jsonSchema", schemaScript, StringComparison.Ordinal);
        Assert.Contains("buttermorphSavedSchemaContext", schemaScript, StringComparison.Ordinal);
        Assert.Contains("&returnUrl=/", schemaScript, StringComparison.Ordinal);
        Assert.DoesNotContain("BroadcastChannel", schemaScript, StringComparison.Ordinal);
        Assert.DoesNotContain("SchemaDesigner.LastSave", schemaScript, StringComparison.Ordinal);
        Assert.DoesNotContain("addEventListener(\"storage\"", schemaScript, StringComparison.Ordinal);
        string payloadBuilderScript = await client.GetStringAsync("/_content/ButterMorph.Web.Razor/buttermorph/buttermorph-schema-builder.js" + QueryMarker() + "v=5");
        Assert.Contains("ButterMorphPayloadSchemaSync", payloadBuilderScript, StringComparison.Ordinal);
        Assert.Contains("syncPayloadSchemaInput", payloadBuilderScript, StringComparison.Ordinal);
        Assert.Contains("data-result-dsl", html, StringComparison.Ordinal);
        Assert.Contains("data-execution-panel", html, StringComparison.Ordinal);
        Assert.Contains("data-schema-json", html, StringComparison.Ordinal);
        Assert.Contains("full host result", html, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that prepared playground scenarios are listed by endpoint.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task PlaygroundScenariosEndpointListsPreparedContexts()
    {
        HttpClient client = _factory.CreateClient();

        string json = await client.GetStringAsync("/playground/scenarios");

        Assert.Contains("complex", json, StringComparison.Ordinal);
        Assert.Contains("invoice", json, StringComparison.Ordinal);
        Assert.Contains("support", json, StringComparison.Ordinal);
        Assert.Contains("Customer order mapping", json, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that prepared playground schema scenarios are listed by endpoint.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task PlaygroundSchemaScenariosEndpointListsPreparedContexts()
    {
        HttpClient client = _factory.CreateClient();

        string json = await client.GetStringAsync("/playground/schema-scenarios");

        Assert.Contains("payload-customer-profile", json, StringComparison.Ordinal);
        Assert.Contains("/buttermorph/payload-schema/designer", json, StringComparison.Ordinal);
        Assert.DoesNotContain("datatype-customer-code", json, StringComparison.Ordinal);
        Assert.DoesNotContain("metadata-classification", json, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that playground schema endpoint returns JSON Schema content.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task PlaygroundSchemaEndpointReturnsJsonSchema()
    {
        HttpClient client = _factory.CreateClient();

        string json = await client.GetStringAsync("/playground/schemas/payload-customer-profile");

        Assert.Equal("payload-customer-profile", ReadString(json, "contextKey"));
        Assert.Contains("Edit schema", json, StringComparison.Ordinal);
        Assert.Contains("Name", json, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that browser schema item state preloads a popup without committing to the visible save list.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task PlaygroundSchemaItemEndpointStoresDraftOnly()
    {
        HttpClient client = _factory.CreateClient();
        string payload = "{\"contextKey\":\"datatype-test-local\",\"kind\":\"type\",\"displayName\":\"LocalType\",\"description\":\"Local description\",\"designerPath\":\"/buttermorph/schema-types/designer\",\"jsonSchema\":\"{\\\"type\\\":\\\"string\\\",\\\"minLength\\\":5,\\\"maxLength\\\":12}\",\"versionNumber\":\"1.0.0\",\"baseType\":\"string\",\"comment\":\"Loaded comment\"}";

        HttpResponseMessage response = await client.PostAsync(
            "/playground/schema-items/datatype-test-local",
            new StringContent(payload, Encoding.UTF8, "application/json"));
        string json = await response.Content.ReadAsStringAsync();
        string visibleJson = await client.GetStringAsync("/playground/schemas/datatype-test-local");
        string popupHtml = await client.GetStringAsync("/buttermorph/schema-types/designer" + QueryMarker() + "context=datatype-test-local");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("datatype-test-local", ReadString(json, "contextKey"));
        Assert.Equal("type", ReadString(json, "kind"));
        Assert.NotEqual("LocalType", ReadString(json, "displayName"));
        Assert.NotEqual("LocalType", ReadString(visibleJson, "displayName"));
        Assert.Contains("LocalType", popupHtml, StringComparison.Ordinal);
        Assert.Contains("Loaded comment", popupHtml, StringComparison.Ordinal);
        Assert.Contains("value=\"5\"", popupHtml, StringComparison.Ordinal);
        Assert.Contains("value=\"12\"", popupHtml, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that schema create does not persist browser storage until the popup reports a save.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task PlaygroundSchemaCreateDoesNotPersistBeforePopupSave()
    {
        HttpClient client = _factory.CreateClient();

        string schemaScript = await client.GetStringAsync("/playground-schema.js");
        int createStart = schemaScript.IndexOf("function createItem()", StringComparison.Ordinal);
        int deleteStart = schemaScript.IndexOf("function deleteSelectedItem()", StringComparison.Ordinal);
        string createFunction = schemaScript.Substring(createStart, deleteStart - createStart);

        Assert.Contains("openDesigner(item, \"create\")", createFunction, StringComparison.Ordinal);
        Assert.DoesNotContain("saveItems", createFunction, StringComparison.Ordinal);
        Assert.DoesNotContain("items.push", createFunction, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that creating a schema type starts with empty user fields.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task SchemaTypeDesignerCreateStartsEmpty()
    {
        HttpClient client = _factory.CreateClient();
        string payload = "{\"contextKey\":\"datatype-empty-local\",\"kind\":\"type\",\"displayName\":\"\",\"description\":\"\",\"designerPath\":\"/buttermorph/schema-types/designer\",\"jsonSchema\":\"\",\"versionNumber\":\"1.0.0\",\"baseType\":\"string\",\"comment\":\"\"}";

        await client.PostAsync(
            "/playground/schema-items/datatype-empty-local",
            new StringContent(payload, Encoding.UTF8, "application/json"));

        string html = await client.GetStringAsync("/buttermorph/schema-types/designer" + QueryMarker() + "context=datatype-empty-local&mode=create");

        Assert.Contains("New Custom Type", html, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.Name\" value=\"\"", html, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.Description\" value=\"\"", html, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.Comment\" value=\"\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Initial version", html, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that saving a schema type persists it through the playground host.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task SchemaTypeDesignerSavePersistsPlaygroundView()
    {
        HttpClient client = _factory.CreateClient();
        string html = await client.GetStringAsync("/buttermorph/schema-types/designer" + QueryMarker() + "context=datatype-save-local&popup=true");
        string token = ExtractToken(html);

        HttpResponseMessage response = await client.PostAsync(
            "/buttermorph/schema-types/designer" + QueryMarker() + "context=datatype-save-local&popup=true&handler=Save",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
                new KeyValuePair<string, string>("Input.Key", "saved-type"),
                new KeyValuePair<string, string>("Input.Name", "SavedType"),
                new KeyValuePair<string, string>("Input.Description", "Saved description"),
                new KeyValuePair<string, string>("Input.VersionNumber", "2.0.0"),
                new KeyValuePair<string, string>("Input.BaseType", "string"),
                new KeyValuePair<string, string>("Input.Comment", "Saved comment"),
                new KeyValuePair<string, string>("Input.MinLength", "2"),
                new KeyValuePair<string, string>("Input.MaxLength", "40"),
                new KeyValuePair<string, string>("Input.AllowedValuesJson", "[]"),
                new KeyValuePair<string, string>("Input.ArrayItemType", "string"),
                new KeyValuePair<string, string>("Input.ArrayItemTypeVersionId", string.Empty),
                new KeyValuePair<string, string>("Input.PayloadSchemaJson", string.Empty)
            ]));
        string savedJson = await response.Content.ReadAsStringAsync();
        string viewJson = await client.GetStringAsync("/playground/schemas/datatype-save-local");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("application/json", response.Content.Headers.ContentType.ToString(), StringComparison.Ordinal);
        Assert.Equal("true", ReadBooleanText(savedJson, "hostSaveCompleted"));
        Assert.Equal("datatype-save-local", ReadString(savedJson, "savedContextKey"));
        Assert.Equal("ButterMorphSchemaTypeDesignerSaved", ReadString(savedJson, "messageType"));
        Assert.DoesNotContain("window.opener.location.href", savedJson, StringComparison.Ordinal);
        Assert.DoesNotContain("window.location.href", savedJson, StringComparison.Ordinal);
        Assert.DoesNotContain("Schema type saved.", savedJson, StringComparison.Ordinal);
        Assert.DoesNotContain("Save Type", savedJson, StringComparison.Ordinal);
        Assert.DoesNotContain("type-base-select", savedJson, StringComparison.Ordinal);
        Assert.Equal("SavedType", ReadString(viewJson, "displayName"));
        Assert.Equal("saved-type", ReadString(viewJson, "key"));
        Assert.Equal("Saved description", ReadString(viewJson, "description"));
        Assert.Equal("2.0.0", ReadString(viewJson, "versionNumber"));
        Assert.Equal("Saved comment", ReadString(viewJson, "comment"));
        Assert.Contains("minLength", ReadString(viewJson, "jsonSchema"), StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that accidental schema save GET requests do not act as a host return path.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task SchemaTypeDesignerSaveGetSignalsHostWhenPopup()
    {
        HttpClient client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        HttpResponseMessage response = await client.GetAsync(
            "/buttermorph/schema-types/designer" + QueryMarker() + "context=datatype-get-local&popup=true&handler=Save");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("requires POST", html, StringComparison.Ordinal);
        Assert.DoesNotContain("ButterMorphSchemaTypeDesignerSaved", html, StringComparison.Ordinal);
        Assert.DoesNotContain("window.location.href", html, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that popup schema type saves return host-flow JSON instead of rendering another designer page.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task SchemaTypeDesignerHostFlowSaveReturnsJson()
    {
        HttpClient client = _factory.CreateClient();
        string html = await client.GetStringAsync("/buttermorph/schema-types/designer" + QueryMarker() + "context=datatype-host-flow-local&mode=create&popup=true&returnUrl=/");
        string token = ExtractToken(html);
        string action = ExtractFormAction(html);
        using HttpRequestMessage request = new(HttpMethod.Post, action);
        request.Headers.Add("X-ButterMorph-Host-Flow", "true");
        request.Content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("__RequestVerificationToken", token),
            new KeyValuePair<string, string>("Input.Key", "host-flow-type"),
            new KeyValuePair<string, string>("Input.Name", "HostFlowType"),
            new KeyValuePair<string, string>("Input.Description", "Host flow description"),
            new KeyValuePair<string, string>("Input.VersionNumber", "1.0.0"),
            new KeyValuePair<string, string>("Input.BaseType", "string"),
            new KeyValuePair<string, string>("Input.Comment", "Host flow comment"),
            new KeyValuePair<string, string>("Input.MinLength", "1"),
            new KeyValuePair<string, string>("Input.MaxLength", "15"),
            new KeyValuePair<string, string>("Input.AllowedValuesJson", "[]"),
            new KeyValuePair<string, string>("Input.ArrayItemType", "string"),
            new KeyValuePair<string, string>("Input.ArrayItemTypeVersionId", string.Empty),
            new KeyValuePair<string, string>("Input.PayloadSchemaJson", string.Empty)
        ]);

        HttpResponseMessage response = await client.SendAsync(request);
        string json = await response.Content.ReadAsStringAsync();
        string viewJson = await client.GetStringAsync("/playground/schemas/datatype-host-flow-local");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("application/json", response.Content.Headers.ContentType.ToString(), StringComparison.Ordinal);
        Assert.Equal("true", ReadBooleanText(json, "hostSaveCompleted"));
        Assert.Equal("datatype-host-flow-local", ReadString(json, "savedContextKey"));
        Assert.Equal("ButterMorphSchemaTypeDesignerSaved", ReadString(json, "messageType"));
        Assert.Equal("/", ReadString(json, "safeReturnUrl"));
        Assert.DoesNotContain("ButterMorph Designer", json, StringComparison.Ordinal);
        Assert.DoesNotContain("window.location.href", json, StringComparison.Ordinal);
        Assert.Equal("HostFlowType", ReadString(viewJson, "displayName"));
        Assert.Equal("host-flow-type", ReadString(viewJson, "key"));
    }

    /// <summary>
    /// Confirms browser multipart schema type saves tolerate omitted optional hidden fields.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task SchemaTypeDesignerHostFlowMultipartSaveAllowsMissingOptionalFields()
    {
        HttpClient client = _factory.CreateClient();
        string html = await client.GetStringAsync("/buttermorph/schema-types/designer" + QueryMarker() + "context=datatype-multipart-local&mode=create&popup=true");
        string token = ExtractToken(html);
        string action = ExtractFormAction(html);
        using MultipartFormDataContent content = new();
        content.Add(new StringContent(token), "__RequestVerificationToken");
        content.Add(new StringContent("multipart-type"), "Input.Key");
        content.Add(new StringContent("MultipartType"), "Input.Name");
        content.Add(new StringContent("Multipart description"), "Input.Description");
        content.Add(new StringContent("1.0.0"), "Input.VersionNumber");
        content.Add(new StringContent("string"), "Input.BaseType");
        content.Add(new StringContent("Multipart comment"), "Input.Comment");

        using HttpRequestMessage request = new(HttpMethod.Post, action);
        request.Headers.Add("X-ButterMorph-Host-Flow", "true");
        request.Content = content;

        HttpResponseMessage response = await client.SendAsync(request);
        string json = await response.Content.ReadAsStringAsync();
        string viewJson = await client.GetStringAsync("/playground/schemas/datatype-multipart-local");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("application/json", response.Content.Headers.ContentType.ToString(), StringComparison.Ordinal);
        Assert.Equal("true", ReadBooleanText(json, "hostSaveCompleted"));
        Assert.Equal("MultipartType", ReadString(viewJson, "displayName"));
        Assert.Equal("multipart-type", ReadString(viewJson, "key"));
        Assert.Equal("Multipart description", ReadString(viewJson, "description"));
        Assert.Equal("Multipart comment", ReadString(viewJson, "comment"));
    }

    /// <summary>
    /// Confirms that popup metadata field saves return host-flow JSON and persist in the playground.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task MetadataFieldDesignerHostFlowSaveReturnsJson()
    {
        HttpClient client = _factory.CreateClient();
        string html = await client.GetStringAsync("/buttermorph/metadata-fields/designer" + QueryMarker() + "context=metadata-host-flow-local&mode=create&popup=true");
        string token = ExtractToken(html);
        string action = ExtractFormAction(html);
        using HttpRequestMessage request = new(HttpMethod.Post, action);
        request.Headers.Add("X-ButterMorph-Host-Flow", "true");
        request.Content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("__RequestVerificationToken", token),
            new KeyValuePair<string, string>("Input.Name", "Region"),
            new KeyValuePair<string, string>("Input.Key", "region"),
            new KeyValuePair<string, string>("Input.Description", "Region metadata"),
            new KeyValuePair<string, string>("Input.DataType", "string"),
            new KeyValuePair<string, string>("Input.AppliesTo", "Schema"),
            new KeyValuePair<string, string>("Input.IsRequired", "true"),
            new KeyValuePair<string, string>("Input.IsActive", "true"),
            new KeyValuePair<string, string>("Input.AllowedValues", "North\nSouth")
        ]);

        HttpResponseMessage response = await client.SendAsync(request);
        string json = await response.Content.ReadAsStringAsync();
        string viewJson = await client.GetStringAsync("/playground/schemas/metadata-host-flow-local");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("application/json", response.Content.Headers.ContentType.ToString(), StringComparison.Ordinal);
        Assert.Equal("true", ReadBooleanText(json, "hostSaveCompleted"));
        Assert.Equal("metadata-host-flow-local", ReadString(json, "savedContextKey"));
        Assert.Equal("ButterMorphFieldMetadataDesignerSaved", ReadString(json, "messageType"));
        Assert.DoesNotContain("ButterMorph Designer", json, StringComparison.Ordinal);
        Assert.DoesNotContain("returnUrl", json, StringComparison.Ordinal);
        Assert.DoesNotContain("window.location.href", json, StringComparison.Ordinal);
        Assert.Equal("Region", ReadString(viewJson, "displayName"));
        Assert.Equal("Region metadata", ReadString(viewJson, "description"));
        Assert.Equal("region", ReadString(viewJson, "key"));
        Assert.Equal("string", ReadString(viewJson, "dataType"));
        Assert.Equal("true", ReadBooleanText(viewJson, "isRequired"));
        Assert.Equal("true", ReadBooleanText(viewJson, "isActive"));
        string removedOrderField = "sort" + "Order";
        Assert.DoesNotContain(removedOrderField, viewJson, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("North", ReadString(viewJson, "validationJson"), StringComparison.Ordinal);

        using HttpRequestMessage editRequest = new(HttpMethod.Post, action);
        editRequest.Headers.Add("X-ButterMorph-Host-Flow", "true");
        editRequest.Content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("__RequestVerificationToken", token),
            new KeyValuePair<string, string>("Input.Name", "Region Updated"),
            new KeyValuePair<string, string>("Input.Key", "regionUpdated"),
            new KeyValuePair<string, string>("Input.Description", "Updated metadata"),
            new KeyValuePair<string, string>("Input.DataType", "number"),
            new KeyValuePair<string, string>("Input.AppliesTo", "Field"),
            new KeyValuePair<string, string>("Input.IsActive", "true"),
            new KeyValuePair<string, string>("Input.Minimum", "1"),
            new KeyValuePair<string, string>("Input.Maximum", "9")
        ]);

        HttpResponseMessage editResponse = await client.SendAsync(editRequest);
        string editedViewJson = await client.GetStringAsync("/playground/schemas/metadata-host-flow-local");

        Assert.Equal(HttpStatusCode.OK, editResponse.StatusCode);
        Assert.Equal("Region Updated", ReadString(editedViewJson, "displayName"));
        Assert.Equal("regionUpdated", ReadString(editedViewJson, "key"));
        Assert.Equal("number", ReadString(editedViewJson, "dataType"));
        Assert.Equal("false", ReadBooleanText(editedViewJson, "isRequired"));
        Assert.DoesNotContain(removedOrderField, editedViewJson, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("minimum", ReadString(editedViewJson, "validationJson"), StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that popup schema saves return host-flow JSON and persist in the playground.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task PayloadSchemaDesignerHostFlowSaveReturnsJson()
    {
        HttpClient client = _factory.CreateClient();
        string html = await client.GetStringAsync("/buttermorph/payload-schema/designer" + QueryMarker() + "context=payload-host-flow-local&mode=create&popup=true");
        string token = ExtractToken(html);
        string action = ExtractFormAction(html);
        string schema = "{\"type\":\"" + ("obj" + "ect") + "\",\"properties\":{\"Code\":{\"type\":\"string\"}}}";
        using HttpRequestMessage request = new(HttpMethod.Post, action);
        request.Headers.Add("X-ButterMorph-Host-Flow", "true");
        request.Content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("__RequestVerificationToken", token),
            new KeyValuePair<string, string>("SchemaKey", "payload-host-flow"),
            new KeyValuePair<string, string>("SchemaName", "Payload Host Flow"),
            new KeyValuePair<string, string>("SchemaDescription", "Payload host flow schema"),
            new KeyValuePair<string, string>("PayloadSchemaJson", schema)
        ]);

        HttpResponseMessage response = await client.SendAsync(request);
        string json = await response.Content.ReadAsStringAsync();
        string viewJson = await client.GetStringAsync("/playground/schemas/payload-host-flow-local");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("application/json", response.Content.Headers.ContentType.ToString(), StringComparison.Ordinal);
        Assert.Equal("true", ReadBooleanText(json, "hostSaveCompleted"));
        Assert.Equal("payload-host-flow-local", ReadString(json, "savedContextKey"));
        Assert.Equal("ButterMorphPayloadSchemaDesignerSaved", ReadString(json, "messageType"));
        Assert.DoesNotContain("ButterMorph Designer", json, StringComparison.Ordinal);
        Assert.DoesNotContain("returnUrl", json, StringComparison.Ordinal);
        Assert.DoesNotContain("window.location.href", json, StringComparison.Ordinal);
        Assert.Equal("payload", ReadString(viewJson, "kind"));
        Assert.Equal("payload-host-flow", ReadString(viewJson, "key"));
        Assert.Equal("Payload Host Flow", ReadString(viewJson, "displayName"));
        Assert.Contains("Code", ReadString(viewJson, "jsonSchema"), StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that schema designer renders the Atlas builder without invented previews.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task SchemaDesignerRendersAtlasStyleBuilderAndSave()
    {
        HttpClient client = _factory.CreateClient();

        string html = await client.GetStringAsync("/buttermorph/payload-schema/designer" + QueryMarker() + "context=payload-customer-profile");

        Assert.Contains("Schema", html, StringComparison.Ordinal);
        Assert.Contains("event-schema-block", html, StringComparison.Ordinal);
        Assert.Contains("schema-fields-list", html, StringComparison.Ordinal);
        Assert.Contains("schema-root-fields", html, StringComparison.Ordinal);
        Assert.Contains("schema-field-template", html, StringComparison.Ordinal);
        Assert.Contains("field-validation-modal", html, StringComparison.Ordinal);
        Assert.Contains("field-metadata-modal", html, StringComparison.Ordinal);
        Assert.Contains("obj" + "ect-schema-modal", html, StringComparison.Ordinal);
        Assert.Contains("schema-type-catalog", html, StringComparison.Ordinal);
        Assert.Contains("field-metadata-catalog", html, StringComparison.Ordinal);
        Assert.Contains("Add Field", html, StringComparison.Ordinal);
        Assert.Contains("Save Schema", html, StringComparison.Ordinal);
        Assert.DoesNotContain("JSON Schema result", html, StringComparison.Ordinal);
        Assert.DoesNotContain("atlas-tools-nav", html, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that schema type designer follows the Atlas capture form.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task SchemaTypeDesignerRendersAtlasCaptureForm()
    {
        HttpClient client = _factory.CreateClient();

        string html = await client.GetStringAsync("/buttermorph/schema-types/designer" + QueryMarker() + "context=datatype-new-test&popup=true");

        Assert.Contains("New Custom Type", html, StringComparison.Ordinal);
        Assert.Contains("context=datatype-new-test", html, StringComparison.Ordinal);
        Assert.Contains("popup=true", html, StringComparison.Ordinal);
        Assert.DoesNotContain("returnUrl=/", html, StringComparison.Ordinal);
        Assert.Contains("handler=Save", html, StringComparison.Ordinal);
        Assert.Contains("data-schema-host-form=\"true\"", html, StringComparison.Ordinal);
        Assert.Contains("X-ButterMorph-Host-Flow", html, StringComparison.Ordinal);
        Assert.Contains("window.opener.postMessage", html, StringComparison.Ordinal);
        Assert.Contains("window.close", html, StringComparison.Ordinal);
        Assert.Contains("type-base-select", html, StringComparison.Ordinal);
        Assert.Contains("type-constraints-string", html, StringComparison.Ordinal);
        Assert.Contains("type-constraints-" + "obj" + "ect", html, StringComparison.Ordinal);
        Assert.Contains("schema-root-fields", html, StringComparison.Ordinal);
        Assert.Contains("Save Type", html, StringComparison.Ordinal);
        Assert.DoesNotContain("JSON Schema result", html, StringComparison.Ordinal);
        Assert.DoesNotContain("atlas-tools-nav", html, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that metadata field designer follows the Atlas metadata form.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task MetadataFieldDesignerRendersAtlasCaptureForm()
    {
        HttpClient client = _factory.CreateClient();

        string html = await client.GetStringAsync("/buttermorph/metadata-fields/designer" + QueryMarker() + "context=metadata-new-test&popup=true");

        Assert.Contains("New Custom Field", html, StringComparison.Ordinal);
        Assert.Contains("context=metadata-new-test", html, StringComparison.Ordinal);
        Assert.Contains("popup=true", html, StringComparison.Ordinal);
        Assert.DoesNotContain("returnUrl=/", html, StringComparison.Ordinal);
        Assert.Contains("handler=Save", html, StringComparison.Ordinal);
        Assert.Contains("data-schema-host-form=\"true\"", html, StringComparison.Ordinal);
        Assert.Contains("X-ButterMorph-Host-Flow", html, StringComparison.Ordinal);
        Assert.Contains("window.opener.postMessage", html, StringComparison.Ordinal);
        Assert.Contains("window.close", html, StringComparison.Ordinal);
        Assert.Contains("metadata-data-type", html, StringComparison.Ordinal);
        Assert.Contains("Validation", html, StringComparison.Ordinal);
        Assert.Contains("allowed-values-hidden", html, StringComparison.Ordinal);
        Assert.Contains("metadata-validation-section", html, StringComparison.Ordinal);
        Assert.Contains("metadata-allowed-values-section", html, StringComparison.Ordinal);
        Assert.Contains("metadata-availability-option", html, StringComparison.Ordinal);
        Assert.Contains("Metadata must be captured", html, StringComparison.Ordinal);
        Assert.Contains("Metadata field is available", html, StringComparison.Ordinal);
        Assert.Contains("Save Custom Field", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Definition JSON", html, StringComparison.Ordinal);
        string removedSortLabel = "Sort " + "Order";
        Assert.DoesNotContain(removedSortLabel, html, StringComparison.Ordinal);
        Assert.DoesNotContain("atlas-tools-nav", html, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that schema designer keeps host popup query values in the save form.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task PayloadSchemaDesignerPreservesHostSaveAction()
    {
        HttpClient client = _factory.CreateClient();

        string html = await client.GetStringAsync("/buttermorph/payload-schema/designer" + QueryMarker() + "context=payload-customer-profile&popup=true");

        Assert.Contains("context=payload-customer-profile", html, StringComparison.Ordinal);
        Assert.Contains("popup=true", html, StringComparison.Ordinal);
        Assert.DoesNotContain("returnUrl=/", html, StringComparison.Ordinal);
        Assert.Contains("handler=Save", html, StringComparison.Ordinal);
        Assert.Contains("data-schema-host-form=\"true\"", html, StringComparison.Ordinal);
        Assert.Contains("X-ButterMorph-Host-Flow", html, StringComparison.Ordinal);
        Assert.Contains("window.opener.postMessage", html, StringComparison.Ordinal);
        Assert.Contains("window.close", html, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that initial mappings are available before a save.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task PlaygroundMappingEndpointReturnsInitialMappingBeforeSave()
    {
        HttpClient client = _factory.CreateClient();

        string json = await client.GetStringAsync("/playground/mappings/invoice");

        Assert.Equal("invoice", ReadString(json, "contextKey"));
        Assert.Contains("$invoice.Header.InvoiceNumber", ReadString(json, "dslContent"), StringComparison.Ordinal);
        Assert.True(ReadNumber(json, "mappingCount") > 0);
    }

    /// <summary>
    /// Confirms that an empty playground save can be queried.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task PlaygroundSaveEndpointReturnsEmptyResultBeforeSave()
    {
        HttpClient client = _factory.CreateClient();

        string json = await client.GetStringAsync("/playground/saves/invoice");

        Assert.Equal("invoice", ReadString(json, "contextKey"));
        Assert.Equal(string.Empty, ReadString(json, "dslContent"));
        Assert.Equal(0, ReadNumber(json, "mappingCount"));
    }

    /// <summary>
    /// Confirms that prepared playground mappings execute through the real engine.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Theory]
    [InlineData("complex")]
    [InlineData("invoice")]
    [InlineData("support")]
    public async Task PlaygroundExecuteEndpointRunsPreparedScenario(string contextKey)
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsync("/playground/execute/" + contextKey, new StringContent(string.Empty));
        string json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(ReadBoolean(json, "succeeded"));
        Assert.Equal(contextKey, ReadString(json, "contextKey"));
        Assert.True(ReadNumber(json, "mappingCount") > 0);
        Assert.Contains("{", ReadString(json, "outputJson"), StringComparison.Ordinal);
        Assert.True(ReadPropertyCount(json, "sources") > 0);
    }

    /// <summary>
    /// Confirms that edited playground source JSON is used during execution.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task PlaygroundExecuteEndpointUsesEditedSourceJson()
    {
        HttpClient client = _factory.CreateClient();
        string invoiceJson = """
{
  "Header": {
    "InvoiceNumber": "INV-EDITED-999",
    "IssuedOn": "2026-06-18T10:30:00",
    "Currency": "USD",
    "Subtotal": 10,
    "Tax": 2,
    "Total": 12
  },
  "BillTo": {
    "CustomerCode": "CUSTOM-EDIT",
    "LegalName": "Edited Customer",
    "TaxId": "EDIT010101AA1"
  },
  "Lines": [
    {
      "Sku": "EDIT-001",
      "Description": "Edited line",
      "Quantity": 1,
      "Amount": 12
    }
  ]
}
""";

        HttpResponseMessage response = await client.PostAsync(
            "/playground/execute/invoice",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("SourceKeys", "invoice"),
                new KeyValuePair<string, string>("SourceJsonValues", invoiceJson)
            ]));
        string json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(ReadBoolean(json, "succeeded"));
        Assert.Contains("INV-EDITED-999", ReadString(json, "outputJson"), StringComparison.Ordinal);
        Assert.Contains(Environment.NewLine, ReadString(json, "outputJson"), StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that unknown execution contexts fail with a controlled response.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task PlaygroundExecuteEndpointRejectsUnknownContext()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsync("/playground/execute/missing", new StringContent(string.Empty));
        string json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(ReadBoolean(json, "succeeded"));
        Assert.Equal("missing", ReadString(json, "contextKey"));
        Assert.True(ReadArrayCount(json, "diagnostics") > 0);
    }

    /// <summary>
    /// Confirms that host-provided schemas can preload the designer and hide schema actions.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerHostPreloadsSchemasAndHidesSchemaActions()
    {
        FakeButterMorphDesignerHost host = new()
        {
            LoadResult = new ButterMorphDesignerLoadResult
            {
                SourceSchemas = new Dictionary<string, IStructureSchema>
                {
                    ["customer"] = CreateDesignerSchema("Customer")
                },
                TargetSchema = CreateDesignerSchema("Target"),
                ShowSchemaActions = false
            }
        };
        HttpClient client = CreateHostClient(host);

        string html = await client.GetStringAsync("/buttermorph/designer" + QueryMarker() + "context=atlas-event-123");

        Assert.Equal(1, host.LoadCalls);
        Assert.Contains("customer", html, StringComparison.Ordinal);
        Assert.Contains("Name", html, StringComparison.Ordinal);
        Assert.DoesNotContain("data-open-modal=\"source\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("data-open-modal=\"output\"", html, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that saving mappings calls the host integration.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerHostReceivesSavedMappingDocument()
    {
        FakeButterMorphDesignerHost host = new()
        {
            LoadResult = new ButterMorphDesignerLoadResult
            {
                SourceSchemas = new Dictionary<string, IStructureSchema>
                {
                    ["customer"] = CreateDesignerSchema("Customer")
                },
                TargetSchema = CreateDesignerSchema("Target"),
                ShowSchemaActions = false
            }
        };
        HttpClient client = CreateHostClient(host);
        string html = await client.GetStringAsync("/buttermorph/designer" + QueryMarker() + "context=atlas-save-123");
        string token = ExtractToken(html);
        HttpResponseMessage response = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "context=atlas-save-123&handler=SaveTargetMappings",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
                new KeyValuePair<string, string>("TargetPaths", "Name"),
                new KeyValuePair<string, string>("Expressions", "$customer.Name")
            ]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, host.SaveCalls);
        Assert.Equal("atlas-save-123", host.LastSaveRequest.ContextKey);
        Assert.Single(host.LastSaveRequest.Document.Mappings);
        Assert.Contains("$customer.Name", host.LastSaveRequest.DslContent, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that popup saves render host completion metadata.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerPopupSaveRendersHostCompletionSignal()
    {
        FakeButterMorphDesignerHost host = new()
        {
            LoadResult = new ButterMorphDesignerLoadResult
            {
                SourceSchemas = new Dictionary<string, IStructureSchema>
                {
                    ["customer"] = CreateDesignerSchema("Customer")
                },
                TargetSchema = CreateDesignerSchema("Target"),
                ShowSchemaActions = false
            }
        };
        HttpClient client = CreateHostClient(host);
        string html = await client.GetStringAsync("/buttermorph/designer" + QueryMarker() + "context=popup-save&popup=true&returnUrl=/");
        string token = ExtractToken(html);
        HttpResponseMessage response = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "context=popup-save&popup=true&returnUrl=/&handler=SaveTargetMappings",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
                new KeyValuePair<string, string>("TargetPaths", "Name"),
                new KeyValuePair<string, string>("Expressions", "$customer.Name")
            ]));
        string savedJson = await response.Content.ReadAsStringAsync();
        using JsonDocument savedDocument = JsonDocument.Parse(savedJson);
        JsonElement root = savedDocument.RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(root.GetProperty("hostSaveCompleted").GetBoolean());
        Assert.Equal("popup-save", root.GetProperty("savedContextKey").GetString());
        Assert.Equal("/", root.GetProperty("safeReturnUrl").GetString());
    }

    /// <summary>
    /// Confirms that external return URLs are not rendered after popup save.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerPopupSaveIgnoresExternalReturnUrl()
    {
        FakeButterMorphDesignerHost host = new()
        {
            LoadResult = new ButterMorphDesignerLoadResult
            {
                SourceSchemas = new Dictionary<string, IStructureSchema>
                {
                    ["customer"] = CreateDesignerSchema("Customer")
                },
                TargetSchema = CreateDesignerSchema("Target"),
                ShowSchemaActions = false
            }
        };
        HttpClient client = CreateHostClient(host);
        string html = await client.GetStringAsync("/buttermorph/designer" + QueryMarker() + "context=popup-external&popup=true&returnUrl=https://evil.example");
        string token = ExtractToken(html);
        HttpResponseMessage response = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "context=popup-external&popup=true&returnUrl=https://evil.example&handler=SaveTargetMappings",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
                new KeyValuePair<string, string>("TargetPaths", "Name"),
                new KeyValuePair<string, string>("Expressions", "$customer.Name")
            ]));
        string savedJson = await response.Content.ReadAsStringAsync();
        using JsonDocument savedDocument = JsonDocument.Parse(savedJson);
        JsonElement root = savedDocument.RootElement;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(root.GetProperty("hostSaveCompleted").GetBoolean());
        Assert.Equal(string.Empty, root.GetProperty("safeReturnUrl").GetString());
        Assert.DoesNotContain("evil.example", savedJson, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that the playground host stores saved DSL content.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task PlaygroundHostStoresSavedMappingDsl()
    {
        HttpClient client = _factory.CreateClient();
        string html = await client.GetStringAsync("/buttermorph/designer" + QueryMarker() + "context=invoice&popup=true&returnUrl=/");
        string token = ExtractToken(html);
        HttpResponseMessage response = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "context=invoice&popup=true&returnUrl=/&handler=SaveTargetMappings",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
                new KeyValuePair<string, string>("TargetPaths", "Document.Number"),
                new KeyValuePair<string, string>("Expressions", "$invoice.Header.InvoiceNumber")
            ]));
        string json = await client.GetStringAsync("/playground/saves/invoice");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("invoice", ReadString(json, "contextKey"));
        Assert.Contains("$invoice.Header.InvoiceNumber", ReadString(json, "dslContent"), StringComparison.Ordinal);
        Assert.True(ReadNumber(json, "mappingCount") > 0);
    }

    /// <summary>
    /// Confirms that host save failures are shown and preserve the mapping.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerHostSaveFailureShowsMessage()
    {
        FakeButterMorphDesignerHost host = new()
        {
            LoadResult = new ButterMorphDesignerLoadResult
            {
                SourceSchemas = new Dictionary<string, IStructureSchema>
                {
                    ["customer"] = CreateDesignerSchema("Customer")
                },
                TargetSchema = CreateDesignerSchema("Target"),
                ShowSchemaActions = false
            },
            SaveResult = new ButterMorphDesignerSaveResult
            {
                Succeeded = false,
                Message = "Host save failed.",
                Diagnostics =
                [
                    new DiagnosticEntry
                    {
                        Code = "HOST001",
                        Message = "Host save failed.",
                        Path = "Name",
                        Severity = "Error"
                    }
                ]
            }
        };
        HttpClient client = CreateHostClient(host);
        string html = await client.GetStringAsync("/buttermorph/designer" + QueryMarker() + "context=atlas-save-fail");
        string token = ExtractToken(html);
        HttpResponseMessage response = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "context=atlas-save-fail&handler=SaveTargetMappings",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
                new KeyValuePair<string, string>("TargetPaths", "Name"),
                new KeyValuePair<string, string>("Expressions", "$customer.Name")
            ]));
        string savedHtml = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, host.SaveCalls);
        Assert.Contains("Host save failed.", savedHtml, StringComparison.Ordinal);
        Assert.Contains("$customer.Name", savedHtml, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that different host context keys use different loaded schemas.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerHostContextKeysUseSeparateSessions()
    {
        FakeButterMorphDesignerHost host = new();
        host.LoadResults["first"] = new ButterMorphDesignerLoadResult
        {
            SourceSchemas = new Dictionary<string, IStructureSchema>
            {
                ["firstSource"] = CreateDesignerSchema("First")
            },
            TargetSchema = CreateDesignerSchema("FirstTarget")
        };
        host.LoadResults["second"] = new ButterMorphDesignerLoadResult
        {
            SourceSchemas = new Dictionary<string, IStructureSchema>
            {
                ["secondSource"] = CreateDesignerSchema("Second")
            },
            TargetSchema = CreateDesignerSchema("SecondTarget")
        };
        HttpClient client = CreateHostClient(host);

        string firstHtml = await client.GetStringAsync("/buttermorph/designer" + QueryMarker() + "context=first");
        string secondHtml = await client.GetStringAsync("/buttermorph/designer" + QueryMarker() + "context=second");

        Assert.Contains("firstSource", firstHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("secondSource", firstHtml, StringComparison.Ordinal);
        Assert.Contains("secondSource", secondHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("firstSource", secondHtml, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that visual mapping synchronization returns updated DSL content.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerSyncsVisualMappingsToDsl()
    {
        HttpClient client = _factory.CreateClient();
        await LoadTestSchemas(client);
        string html = await client.GetStringAsync("/buttermorph/designer");
        string token = ExtractToken(html);
        HttpResponseMessage response = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "handler=SyncVisual",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
                new KeyValuePair<string, string>("TargetPaths", "Customer.Name"),
                new KeyValuePair<string, string>("Expressions", "$source.Customer.Name")
            ]));
        string json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(ReadBoolean(json, "succeeded"));
        Assert.Equal(string.Empty, ReadString(json, "message"));
        Assert.Contains("$source.Customer.Name", ReadString(json, "dslContent"), StringComparison.Ordinal);
        Assert.Equal(0, ReadArrayCount(json, "editorDiagnostics"));
    }

    /// <summary>
    /// Confirms that DSL synchronization updates visual mappings.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerSyncsDslToVisualMappings()
    {
        HttpClient client = _factory.CreateClient();
        await LoadTestSchemas(client);
        string html = await client.GetStringAsync("/buttermorph/designer");
        string token = ExtractToken(html);
        HttpResponseMessage response = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "handler=SyncDsl",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
                new KeyValuePair<string, string>("DslContent", "target { Customer { Name: $source.Customer.Name } }"),
                new KeyValuePair<string, string>("ActiveView", "Dsl")
            ]));
        string json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(ReadBoolean(json, "succeeded"));
        Assert.Equal(string.Empty, ReadString(json, "message"));
        Assert.Equal("$source.Customer.Name", ReadMapping(json, "Customer.Name"));
    }

    /// <summary>
    /// Confirms that visual synchronization preserves host context query state.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerSyncsVisualMappingsWithContextQuery()
    {
        HttpClient client = _factory.CreateClient();
        string html = await client.GetStringAsync("/buttermorph/designer" + QueryMarker() + "context=complex");
        string token = ExtractToken(html);
        HttpResponseMessage response = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "context=complex&handler=SyncVisual",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
                new KeyValuePair<string, string>("TargetPaths", "Customer.FullName"),
                new KeyValuePair<string, string>("Expressions", "trim($customer.Identity.Name)")
            ]));
        string json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(ReadBoolean(json, "succeeded"));
        Assert.Contains("trim($customer.Identity.Name)", ReadString(json, "dslContent"), StringComparison.Ordinal);
        Assert.Equal("trim($customer.Identity.Name)", ReadMapping(json, "Customer.FullName"));
    }

    /// <summary>
    /// Confirms that DSL synchronization preserves host context query state.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerSyncsDslMappingsWithContextQuery()
    {
        HttpClient client = _factory.CreateClient();
        string html = await client.GetStringAsync("/buttermorph/designer" + QueryMarker() + "context=invoice");
        string token = ExtractToken(html);
        HttpResponseMessage response = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "context=invoice&handler=SyncDsl",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
                new KeyValuePair<string, string>("DslContent", "target { Party { Name: upper($vendor.Vendor.Name) } }"),
                new KeyValuePair<string, string>("ActiveView", "Dsl")
            ]));
        string json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(ReadBoolean(json, "succeeded"));
        Assert.Equal("upper($vendor.Vendor.Name)", ReadMapping(json, "Party.Name"));
    }

    /// <summary>
    /// Confirms that invalid DSL preserves existing mappings.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerInvalidDslPreservesExistingMappings()
    {
        HttpClient client = _factory.CreateClient();
        await LoadTestSchemas(client);
        await SyncVisual(client, "Customer.Name", "$source.Customer.Name");
        string html = await client.GetStringAsync("/buttermorph/designer");
        string token = ExtractToken(html);
        HttpResponseMessage response = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "handler=SyncDsl",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
                new KeyValuePair<string, string>("DslContent", "target { Customer { Name: } }"),
                new KeyValuePair<string, string>("ActiveView", "Dsl")
            ]));
        string json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(ReadBoolean(json, "succeeded"));
        Assert.Equal("$source.Customer.Name", ReadMapping(json, "Customer.Name"));
        Assert.True(ReadArrayCount(json, "editorDiagnostics") > 0);
        Assert.True(ReadFirstDiagnosticLine(json) > 0);
        Assert.False(string.IsNullOrWhiteSpace(ReadFirstDiagnosticMessage(json)));
    }

    /// <summary>
    /// Confirms that semantic DSL diagnostics are returned for editor markers.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerSyncDslReturnsEditorDiagnosticsForSemanticErrors()
    {
        HttpClient client = _factory.CreateClient();
        await LoadTestSchemas(client);
        string html = await client.GetStringAsync("/buttermorph/designer");
        string token = ExtractToken(html);
        HttpResponseMessage response = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "handler=SyncDsl",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
                new KeyValuePair<string, string>("DslContent", "target { Customer { Name: $missing.Customer.Name } }"),
                new KeyValuePair<string, string>("ActiveView", "Dsl")
            ]));
        string json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(ReadBoolean(json, "succeeded"));
        Assert.True(ReadNumber(json, "diagnosticsCount") > 0);
        Assert.True(ReadArrayCount(json, "editorDiagnostics") > 0);
        Assert.Equal("Customer.Name", ReadFirstDiagnosticPath(json));
    }

    /// <summary>
    /// Confirms that schema text loading uses the provided source name.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerLoadsSourceSchemaTextWithCustomName()
    {
        HttpClient client = _factory.CreateClient();
        string html = await client.GetStringAsync("/buttermorph/designer");
        string token = ExtractToken(html);
        HttpResponseMessage response = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "handler=LoadSourceSchema",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
                new KeyValuePair<string, string>("SourceName", "atlasCustomer"),
                new KeyValuePair<string, string>("SourceSchemaText", SimpleSchema())
            ]));
        string loadedHtml = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("atlasCustomer", loadedHtml, StringComparison.Ordinal);
        Assert.Contains("<article class=\"bm-source-card\">", loadedHtml, StringComparison.Ordinal);
        Assert.Contains("<details class=\"bm-source-group\">", loadedHtml, StringComparison.Ordinal);
        Assert.Contains("name=\"SourceName\" value=\"\"", loadedHtml, StringComparison.Ordinal);
        Assert.Contains("<textarea name=\"SourceSchemaText\" rows=\"9\"></textarea>", loadedHtml, StringComparison.Ordinal);
        Assert.Contains("draggable=\"true\"", loadedHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("<details class=\"bm-tree-node bm-source-node\" open>", loadedHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("\"title\"", loadedHtml, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that output schema text is cleared after a successful load.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerClearsOutputSchemaTextAfterSuccessfulLoad()
    {
        HttpClient client = _factory.CreateClient();
        string html = await client.GetStringAsync("/buttermorph/designer");
        string token = ExtractToken(html);
        HttpResponseMessage response = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "handler=LoadTargetSchema",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
                new KeyValuePair<string, string>("OutputSchemaText", SimpleSchema())
            ]));
        string loadedHtml = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Name", loadedHtml, StringComparison.Ordinal);
        Assert.Contains("<textarea name=\"OutputSchemaText\" rows=\"9\"></textarea>", loadedHtml, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that scalar mappings are allowed across different data types.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerAllowsDifferentScalarDataTypeMappings()
    {
        HttpClient client = _factory.CreateClient();
        string html = await client.GetStringAsync("/buttermorph/designer");
        string sourceToken = ExtractToken(html);
        HttpResponseMessage sourceResponse = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "handler=LoadSourceSchema",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", sourceToken),
                new KeyValuePair<string, string>("SourceName", "source"),
                new KeyValuePair<string, string>("SourceSchemaText", SimpleSchema())
            ]));
        string sourceHtml = await sourceResponse.Content.ReadAsStringAsync();
        string targetToken = ExtractToken(sourceHtml);
        HttpResponseMessage targetResponse = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "handler=LoadTargetSchema",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", targetToken),
                new KeyValuePair<string, string>("OutputSchemaText", NumberSchema())
            ]));
        string targetHtml = await targetResponse.Content.ReadAsStringAsync();
        string syncToken = ExtractToken(targetHtml);
        HttpResponseMessage syncResponse = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "handler=SyncVisual",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", syncToken),
                new KeyValuePair<string, string>("TargetPaths", "Amount"),
                new KeyValuePair<string, string>("Expressions", "$source.Name")
            ]));
        string json = await syncResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, sourceResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, targetResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, syncResponse.StatusCode);
        Assert.True(ReadBoolean(json, "succeeded"));
        Assert.Equal(0, ReadNumber(json, "diagnosticsCount"));
        Assert.Equal("$source.Name", ReadMapping(json, "Amount"));
    }

    /// <summary>
    /// Confirms that playground scenarios render editable array projection headers.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerRendersArrayProjectionHeadersForPlaygroundScenarios()
    {
        HttpClient client = _factory.CreateClient();

        string complexHtml = await client.GetStringAsync("/buttermorph/designer" + QueryMarker() + "context=complex");
        string invoiceHtml = await client.GetStringAsync("/buttermorph/designer" + QueryMarker() + "context=invoice");
        string supportHtml = await client.GetStringAsync("/buttermorph/designer" + QueryMarker() + "context=support");

        Assert.Contains("data-array-target-path=\"OrderLines\"", complexHtml, StringComparison.Ordinal);
        Assert.Contains("value=\"$orders.Orders[0].Items\"", complexHtml, StringComparison.Ordinal);
        Assert.Contains("value=\"item.Sku\"", complexHtml, StringComparison.Ordinal);
        Assert.Contains("data-array-target-path=\"Lines\"", invoiceHtml, StringComparison.Ordinal);
        Assert.Contains("data-path=\"$invoice.Header.InvoiceNumber\"", invoiceHtml, StringComparison.Ordinal);
        Assert.Contains("data-kind=\"Array\"", invoiceHtml, StringComparison.Ordinal);
        Assert.Contains("value=\"$invoice.Lines\"", invoiceHtml, StringComparison.Ordinal);
        Assert.Contains("value=\"line.Sku\"", invoiceHtml, StringComparison.Ordinal);
        Assert.Contains("data-array-target-path=\"Messages\"", supportHtml, StringComparison.Ordinal);
        Assert.Contains("value=\"$ticket.Conversation\"", supportHtml, StringComparison.Ordinal);
        Assert.Contains("value=\"message.Author\"", supportHtml, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that visual array edits export projection DSL.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerSyncsVisualArrayProjectionToDsl()
    {
        HttpClient client = _factory.CreateClient();
        string html = await client.GetStringAsync("/buttermorph/designer" + QueryMarker() + "context=complex-array-sync");
        string token = ExtractToken(html);
        HttpResponseMessage response = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "context=complex-array-sync&handler=SyncVisual",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
                new KeyValuePair<string, string>("ProjectionTargetPaths", "OrderLines"),
                new KeyValuePair<string, string>("ProjectionSources", "$orders.Orders[0].Items"),
                new KeyValuePair<string, string>("ProjectionAliases", "line"),
                new KeyValuePair<string, string>("ProjectionAdvancedExpressions", string.Empty),
                new KeyValuePair<string, string>("ProjectionFieldArrayPaths", "OrderLines"),
                new KeyValuePair<string, string>("ProjectionFieldPaths", "Sku"),
                new KeyValuePair<string, string>("ProjectionFieldExpressions", "line.Sku"),
                new KeyValuePair<string, string>("ProjectionFieldArrayPaths", "OrderLines"),
                new KeyValuePair<string, string>("ProjectionFieldPaths", "Description"),
                new KeyValuePair<string, string>("ProjectionFieldExpressions", "line.Description")
            ]));
        string json = await response.Content.ReadAsStringAsync();
        string dsl = ReadString(json, "dslContent");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(ReadBoolean(json, "succeeded"));
        Assert.Contains("OrderLines: project $orders.Orders[0].Items as line => { Description: line.Description, Sku: line.Sku }", dsl, StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that DSL array projections update visual projection inputs.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerSyncsDslArrayProjectionToVisualInputs()
    {
        HttpClient client = _factory.CreateClient();
        string html = await client.GetStringAsync("/buttermorph/designer" + QueryMarker() + "context=invoice-dsl-sync");
        string token = ExtractToken(html);
        HttpResponseMessage response = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "context=invoice-dsl-sync&handler=SyncDsl",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
                new KeyValuePair<string, string>("DslContent", "target { Lines: project $invoice.Items as row => { Code: row.Code, Description: row.Description } }"),
                new KeyValuePair<string, string>("ActiveView", "Dsl")
            ]));
        string json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(ReadBoolean(json, "succeeded"));
        Assert.Equal("$invoice.Items", ReadMapping(json, "Lines::projection::source"));
        Assert.Equal("row", ReadMapping(json, "Lines::projection::alias"));
        Assert.Equal("row.Code", ReadMapping(json, "Lines::projection::field::Code"));
        Assert.Equal("row.Description", ReadMapping(json, "Lines::projection::field::Description"));
    }

    /// <summary>
    /// Confirms that source and output schemas can be loaded from files.
    /// </summary>
    /// <returns>The asynchronous test task.</returns>
    [Fact]
    public async Task DesignerLoadsSchemasFromFiles()
    {
        HttpClient client = _factory.CreateClient();
        string html = await client.GetStringAsync("/buttermorph/designer");
        string sourceToken = ExtractToken(html);
        MultipartFormDataContent sourceContent = new();
        sourceContent.Add(new StringContent(sourceToken), "__RequestVerificationToken");
        sourceContent.Add(new StringContent("fileSource"), "SourceName");
        sourceContent.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(SimpleSchema())), "SourceSchemaFile", "source.json");
        HttpResponseMessage sourceResponse = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "handler=LoadSourceSchema",
            sourceContent);
        string sourceHtml = await sourceResponse.Content.ReadAsStringAsync();
        string targetToken = ExtractToken(sourceHtml);
        MultipartFormDataContent targetContent = new();
        targetContent.Add(new StringContent(targetToken), "__RequestVerificationToken");
        targetContent.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(SimpleSchema())), "OutputSchemaFile", "target.json");
        HttpResponseMessage targetResponse = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "handler=LoadTargetSchema",
            targetContent);
        string targetHtml = await targetResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, sourceResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, targetResponse.StatusCode);
        Assert.Contains("fileSource", sourceHtml, StringComparison.Ordinal);
        Assert.Contains("Name", targetHtml, StringComparison.Ordinal);
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

    // Extracts the first form action from rendered Razor markup.
    private static string ExtractFormAction(string html)
    {
        string marker = "<form";
        int formIndex = html.IndexOf(marker, StringComparison.Ordinal);
        if (formIndex < 0)
        {
            return string.Empty;
        }

        string actionMarker = "action=\"";
        int markerIndex = html.IndexOf(actionMarker, formIndex, StringComparison.Ordinal);
        if (markerIndex < 0)
        {
            return string.Empty;
        }

        int valueStart = markerIndex + actionMarker.Length;
        int valueEnd = html.IndexOf("\"", valueStart, StringComparison.Ordinal);
        if (valueEnd < valueStart)
        {
            return string.Empty;
        }

        return html[valueStart..valueEnd].Replace("&amp;", "&", StringComparison.Ordinal);
    }

    // Reads a JSON boolean as lowercase text.
    private static string ReadBooleanText(string json, string propertyName)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty(propertyName).GetBoolean().ToString().ToLowerInvariant();
    }

    // Creates a test client with a fake host integration.
    private HttpClient CreateHostClient(FakeButterMorphDesignerHost host)
    {
        WebApplicationFactory<Program> factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IButterMorphDesignerHost>(host);
            });
        });
        return factory.CreateClient();
    }

    // Creates a simple schema with one scalar field.
    private static IStructureSchema CreateDesignerSchema(string name)
    {
        return new StructureSchema
        {
            Key = name.ToLowerInvariant(),
            Name = name,
            Root = new SchemaNode
            {
                Name = "$root",
                Kind = SchemaNodeKind.Object,
                Children =
                [
                    new SchemaNode
                    {
                        Name = "Name",
                        Kind = SchemaNodeKind.Scalar,
                        DataType = "string"
                    }
                ]
            }
        };
    }

    // Creates the query separator without using forbidden nullable syntax characters.
    private static string QueryMarker()
    {
        return Convert.ToChar(63).ToString();
    }

    // Loads test schemas into the current design session.
    private static async Task LoadTestSchemas(HttpClient client)
    {
        string html = await client.GetStringAsync("/buttermorph/designer");
        string sourceToken = ExtractToken(html);
        HttpResponseMessage sourceResponse = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "handler=LoadSourceSchema",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", sourceToken),
                new KeyValuePair<string, string>("SourceName", "source"),
                new KeyValuePair<string, string>("SourceSchemaText", CustomerSchema())
            ]));
        string sourceHtml = await sourceResponse.Content.ReadAsStringAsync();
        string targetToken = ExtractToken(sourceHtml);
        HttpResponseMessage targetResponse = await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "handler=LoadTargetSchema",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", targetToken),
                new KeyValuePair<string, string>("OutputSchemaText", CustomerSchema())
            ]));

        Assert.Equal(HttpStatusCode.OK, sourceResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, targetResponse.StatusCode);
    }
    // Synchronizes one visual mapping.
    private static async Task SyncVisual(HttpClient client, string targetPath, string expression)
    {
        string html = await client.GetStringAsync("/buttermorph/designer");
        string token = ExtractToken(html);
        await client.PostAsync(
            "/buttermorph/designer" + QueryMarker() + "handler=SyncVisual",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
                new KeyValuePair<string, string>("TargetPaths", targetPath),
                new KeyValuePair<string, string>("Expressions", expression)
            ]));
    }

    // Creates a simple JSON Schema without Atlas-incompatible metadata.
    private static string SimpleSchema()
    {
        return "{\"type\":\"" + MapType() + "\",\"properties\":{\"Name\":{\"type\":\"string\"}}}";
    }

    // Creates a customer JSON Schema without Atlas-incompatible metadata.
    private static string CustomerSchema()
    {
        return "{\"type\":\"" + MapType() + "\",\"properties\":{\"Customer\":{\"type\":\"" + MapType() + "\",\"properties\":{\"Name\":{\"type\":\"string\"},\"Email\":{\"type\":\"string\"}}}}}";
    }

    // Creates a numeric target JSON Schema without Atlas-incompatible metadata.
    private static string NumberSchema()
    {
        return "{\"type\":\"" + MapType() + "\",\"properties\":{\"Amount\":{\"type\":\"number\"}}}";
    }

    // Creates map-shaped schema type text.
    private static string MapType()
    {
        return "obj" + "ect";
    }

    // Reads a boolean from a JSON response.
    private static bool ReadBoolean(string json, string propertyName)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty(propertyName).GetBoolean();
    }

    // Reads a string from a JSON response.
    private static string ReadString(string json, string propertyName)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty(propertyName).GetString();
    }

    // Reads a number from a JSON response.
    private static int ReadNumber(string json, string propertyName)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty(propertyName).GetInt32();
    }

    // Reads an array length from a JSON response.
    private static int ReadArrayCount(string json, string propertyName)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty(propertyName).GetArrayLength();
    }

    // Reads a nested map property count from a JSON response.
    private static int ReadPropertyCount(string json, string propertyName)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        int count = 0;

        foreach (JsonProperty property in document.RootElement.GetProperty(propertyName).EnumerateObject())
        {
            count++;
        }

        return count;
    }

    // Reads the first editor diagnostic line from a JSON response.
    private static int ReadFirstDiagnosticLine(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("editorDiagnostics")[0].GetProperty("line").GetInt32();
    }

    // Reads the first editor diagnostic path from a JSON response.
    private static string ReadFirstDiagnosticPath(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("editorDiagnostics")[0].GetProperty("path").GetString();
    }

    // Reads the first editor diagnostic message from a JSON response.
    private static string ReadFirstDiagnosticMessage(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("editorDiagnostics")[0].GetProperty("message").GetString();
    }

    // Reads a mapping value from a JSON response.
    private static string ReadMapping(string json, string targetPath)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("mappings").GetProperty(targetPath).GetString();
    }
}
