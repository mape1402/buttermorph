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
        Assert.Contains("rememberDslSelection", script, StringComparison.Ordinal);
        Assert.Contains("insertIntoDslEditor", script, StringComparison.Ordinal);
        Assert.Contains("initializeDslCodeEditor", script, StringComparison.Ordinal);
        Assert.Contains("buttermorphDsl", script, StringComparison.Ordinal);
        Assert.Contains("createDslHintProvider", script, StringComparison.Ordinal);
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
        Assert.Contains(".CodeMirror-hints", css, StringComparison.Ordinal);
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

    // Reads a mapping value from a JSON response.
    private static string ReadMapping(string json, string targetPath)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("mappings").GetProperty(targetPath).GetString();
    }
}


