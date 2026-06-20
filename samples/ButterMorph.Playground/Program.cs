using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.DependencyInjection;
using ButterMorph.Design;
using ButterMorph.Json;
using ButterMorph.Json.Schema;
using ButterMorph.SchemaDesign;
using ButterMorph.Web.Razor;
using System.Text.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddButterMorph();
builder.Services.AddButterMorphDesign();
builder.Services.AddButterMorphJsonSchema();
builder.Services.AddButterMorphSchemaDesign();
builder.Services.AddButterMorphRazorDesigner();
builder.Services.AddSingleton<PlaygroundMappingStore>();
builder.Services.AddSingleton<PlaygroundSchemaStore>();
builder.Services.AddSingleton<PlaygroundDesignerHost>();
builder.Services.AddSingleton<PlaygroundSchemaDesignerHost>();
builder.Services.AddSingleton<IButterMorphDesignerHost>(provider => provider.GetRequiredService<PlaygroundDesignerHost>());
builder.Services.AddSingleton<IButterMorphSchemaDesignerHost>(provider => provider.GetRequiredService<PlaygroundSchemaDesignerHost>());
builder.Services.AddSingleton<IButterMorphSchemaTypeDesignerHost>(provider => provider.GetRequiredService<PlaygroundSchemaDesignerHost>());
builder.Services.AddSingleton<IButterMorphFieldMetadataDesignerHost>(provider => provider.GetRequiredService<PlaygroundSchemaDesignerHost>());
builder.Services.AddSingleton<IButterMorphPayloadSchemaDesignerHost>(provider => provider.GetRequiredService<PlaygroundSchemaDesignerHost>());

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

app.MapGet("/", () => Results.Content(CreatePlaygroundHtml(), "text/html"));
app.MapGet("/playground/scenarios", (PlaygroundDesignerHost host) => Results.Json(host.ListScenarios()));
app.MapGet("/playground/schema-scenarios", (PlaygroundSchemaDesignerHost host) => Results.Json(host.ListScenarios()));
app.MapGet("/playground/schemas/{contextKey}", (string contextKey, PlaygroundSchemaDesignerHost host) => Results.Json(host.CreateView(contextKey)));
app.MapPost("/playground/schema-items/{contextKey}", async (string contextKey, PlaygroundSchemaDesignerHost host, HttpRequest request) =>
{
    PlaygroundSchemaClientItem item = await JsonSerializer.DeserializeAsync<PlaygroundSchemaClientItem>(
        request.Body,
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    if (item == null)
    {
        item = new PlaygroundSchemaClientItem();
    }

    if (string.IsNullOrWhiteSpace(item.ContextKey))
    {
        item.ContextKey = contextKey;
    }

    host.SaveClientItem(item);
    return Results.Json(host.CreateView(item.ContextKey));
});
app.MapGet("/playground/mappings/{contextKey}", (
    string contextKey,
    PlaygroundDesignerHost host,
    PlaygroundMappingStore store,
    IDslExporter dslExporter) =>
{
    if (!host.TryCreateLoadResult(contextKey, out ButterMorphDesignerLoadResult loadResult))
    {
        return Results.BadRequest(CreateUnknownMappingView(contextKey));
    }

    if (store.TryGet(contextKey, out PlaygroundMappingSave save))
    {
        return Results.Json(CreateMappingView(contextKey, save.DslContent, save.SavedAt, save.MappingCount, host));
    }

    string dslContent = dslExporter.Export(loadResult.InitialDocument);
    return Results.Json(CreateMappingView(contextKey, dslContent, string.Empty, loadResult.InitialDocument.Mappings.Count, host));
});
app.MapGet("/playground/saves/{contextKey}", (string contextKey, PlaygroundMappingStore store, PlaygroundDesignerHost host) =>
{
    if (store.TryGet(contextKey, out PlaygroundMappingSave save))
    {
        return Results.Json(CreateMappingView(contextKey, save.DslContent, save.SavedAt, save.MappingCount, host));
    }

    return Results.Json(CreateMappingView(contextKey, string.Empty, string.Empty, 0, host));
});
app.MapPost("/playground/execute/{contextKey}", async (
    string contextKey,
    PlaygroundDesignerHost host,
    PlaygroundMappingStore store,
    IButterMorphEngine engine,
    HttpRequest request) =>
{
    if (!host.TryCreateLoadResult(contextKey, out ButterMorphDesignerLoadResult loadResult))
    {
        return Results.BadRequest(new PlaygroundExecutionResult
        {
            ContextKey = contextKey,
            Succeeded = false,
            ExecutedAt = DateTimeOffset.UtcNow.ToString("O"),
            Diagnostics = ["Unknown playground scenario '" + contextKey + "'."]
        });
    }

    if (!host.TryGetSourceJson(contextKey, out IReadOnlyDictionary<string, string> sourceJson))
    {
        return Results.BadRequest(new PlaygroundExecutionResult
        {
            ContextKey = contextKey,
            Succeeded = false,
            ExecutedAt = DateTimeOffset.UtcNow.ToString("O"),
            Diagnostics = ["Source data is not available for '" + contextKey + "'."]
        });
    }

    sourceJson = await ResolvePostedSources(request, sourceJson);

    ITransformationDocument document = loadResult.InitialDocument;
    int mappingCount = document.Mappings.Count;

    if (store.TryGet(contextKey, out PlaygroundMappingSave save))
    {
        document = save.Document;
        mappingCount = save.MappingCount;
    }

    PlaygroundExecutionResult executionResult = ExecuteScenario(contextKey, sourceJson, document, mappingCount, engine);
    return Results.Json(executionResult);
});
app.MapButterMorphDesigner("/buttermorph");

app.Run();

/// <summary>
/// Exposes the playground entry point for integration tests.
/// </summary>
public partial class Program
{
    // Creates the playground shell used to simulate host integration.
    private static string CreatePlaygroundHtml()
    {
        return $$"""
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <title>ButterMorph Playground Host</title>
  <style>
    body { background:#eef2f7; color:#111827; font-family:Segoe UI,Arial,sans-serif; margin:0; }
    main { display:grid; gap:1rem; margin:0 auto; max-width:1360px; padding:1.4rem; }
    header { background:#111827; border-radius:10px; color:#fff; padding:1rem 1.2rem; }
    h1 { font-size:1.5rem; margin:0; }
    p { color:#5b6478; margin:.35rem 0 0; }
    .layout { display:grid; gap:1rem; grid-template-columns:320px minmax(0,1fr); }
    .scenarios { display:grid; gap:.7rem; }
    .scenario { background:#fff; border:1px solid #cfd7e6; border-radius:9px; color:#111827; cursor:pointer; padding:.75rem; text-align:left; }
    .scenario strong { display:block; }
    .scenario span { color:#5b6478; display:block; font-size:.8rem; line-height:1.35; margin-top:.25rem; }
    .scenario[aria-pressed="true"] { border-color:#4f46e5; box-shadow:inset 3px 0 0 #4f46e5; }
    button { background:#4f46e5; border:0; border-radius:7px; color:#fff; cursor:pointer; font-weight:700; min-height:36px; padding:.55rem .85rem; }
    button:hover { background:#4338ca; }
    button.secondary { background:#e5e7eb; color:#111827; }
    button.secondary:hover { background:#d1d5db; }
    button:disabled { background:#cbd5e1; cursor:not-allowed; }
    section { background:#fff; border:1px solid #d8deed; border-radius:10px; box-shadow:0 8px 20px rgba(15,23,42,.06); padding:1rem; }
    .actions { display:flex; gap:.6rem; margin:.75rem 0; }
    .execution-actions { display:flex; gap:.6rem; justify-content:flex-end; margin-bottom:.75rem; }
    .meta { display:flex; flex-wrap:wrap; gap:.55rem; margin-bottom:.75rem; }
    .meta span { background:#eef2ff; border-radius:999px; color:#3730a3; font-size:.78rem; font-weight:700; padding:.2rem .55rem; }
    textarea { border:1px solid #ccd4e5; border-radius:7px; box-sizing:border-box; font-family:Consolas,Cascadia Code,monospace; min-height:300px; padding:.65rem; resize:vertical; width:100%; }
    .execution { display:grid; gap:1rem; grid-template-columns:minmax(0,1fr) minmax(0,1fr); }
    .sources { display:grid; gap:.65rem; }
    .source-label { color:#374151; font-size:.78rem; font-weight:800; margin-bottom:.2rem; text-transform:uppercase; }
    .source-box, .output-box { min-height:220px; }
    .source-box { background:#fff; }
    .diagnostics { color:#b91c1c; font-size:.86rem; margin-top:.6rem; white-space:pre-wrap; }
    .schema-grid { display:grid; gap:1rem; grid-template-columns:360px minmax(0,1fr); }
    .schema-json { min-height:360px; }
    .schema-tabs { display:flex; gap:.35rem; margin:.75rem 0; }
    .schema-tab { background:#e5e7eb; color:#111827; flex:1; min-height:32px; padding:.35rem .5rem; }
    .schema-tab[aria-pressed="true"] { background:#4f46e5; color:#fff; }
    .schema-list { display:grid; gap:.6rem; margin-top:.65rem; }
    .schema-toolbar { display:flex; gap:.5rem; margin:.7rem 0; }
    .schema-empty { border:1px dashed #cbd5e1; border-radius:8px; color:#64748b; padding:.8rem; text-align:center; }
  </style>
</head>
<body>
  <main>
    <header>
      <h1>ButterMorph host playground</h1>
      <p>Simulates a real application opening ButterMorph as a temporary mapping editor.</p>
    </header>
    <div class="layout">
      <section>
        <h2>Scenarios</h2>
        <div class="scenarios" data-scenarios></div>
      </section>
      <section>
        <h2>Mapping</h2>
        <div class="meta">
          <span data-result-context>No context selected</span>
          <span data-result-time>Not saved yet</span>
          <span data-result-count>0 mappings</span>
        </div>
        <div class="actions">
          <button type="button" data-edit disabled>Edit</button>
        </div>
        <textarea readonly data-result-dsl placeholder="Select a scenario to load its mapping."></textarea>
      </section>
    </div>
    <section data-execution-panel hidden>
      <h2>Execution</h2>
      <div class="execution-actions">
        <button type="button" data-execute disabled>Execute</button>
      </div>
      <div class="meta">
        <span data-execution-context>No execution</span>
        <span data-execution-time>Not executed yet</span>
        <span data-execution-status>Waiting</span>
      </div>
      <div class="execution">
        <div>
          <h3>Sources</h3>
          <div class="sources" data-source-output></div>
        </div>
        <div>
          <h3>Generated output</h3>
          <textarea readonly class="output-box" data-output-json></textarea>
          <div class="diagnostics" data-execution-diagnostics></div>
        </div>
      </div>
    </section>
    <section data-schema-workbench>
      <h2>Schema designer</h2>
      <p>Create and maintain schemas, metadata fields and payload definitions in ButterMorph, then persist the full host result.</p>
      <div class="schema-grid">
        <div>
          <div class="schema-tabs">
            <button type="button" class="schema-tab" data-schema-tab="type" aria-pressed="true">Custom types</button>
            <button type="button" class="schema-tab" data-schema-tab="field" aria-pressed="false">Custom fields</button>
            <button type="button" class="schema-tab" data-schema-tab="payload" aria-pressed="false">Payload schemas</button>
          </div>
          <div class="schema-toolbar">
            <button type="button" data-create-schema>Create</button>
            <button type="button" data-edit-schema disabled>Edit</button>
            <button type="button" class="secondary" data-delete-schema disabled>Delete</button>
          </div>
          <div class="schema-list" data-schema-list></div>
        </div>
        <div>
          <div class="meta">
            <span data-schema-context>No schema selected</span>
            <span data-schema-time>Not saved yet</span>
          </div>
          <textarea readonly class="schema-json" data-schema-json placeholder="Select an item to view the saved host result."></textarea>
        </div>
      </div>
    </section>
  </main>
  <script>
    const popupOptions = "popup=yes,toolbar=no,location=no,menubar=no,status=no,resizable=yes,scrollbars=yes";
    let selectedContext = "";
    const queryMarker = "{{QueryMarker()}}";
    function openDesigner(contextKey) {
      const width = Math.min(1480, screen.availWidth - 80);
      const height = Math.min(900, screen.availHeight - 80);
      const left = Math.max(0, Math.round((screen.availWidth - width) / 2));
      const top = Math.max(0, Math.round((screen.availHeight - height) / 2));
      const url = "/buttermorph/designer" + queryMarker + "context=" + encodeURIComponent(contextKey) + "&popup=true&returnUrl=/";
      window.open(url, "buttermorph-" + contextKey, popupOptions + ",width=" + width + ",height=" + height + ",left=" + left + ",top=" + top);
    }
    function openSchemaDesigner(contextKey) {
      const width = Math.min(1280, screen.availWidth - 80);
      const height = Math.min(820, screen.availHeight - 80);
      const left = Math.max(0, Math.round((screen.availWidth - width) / 2));
      const top = Math.max(0, Math.round((screen.availHeight - height) / 2));
      const schemaButton = document.querySelector("[data-schema-context-button='" + contextKey + "']");
      let path = "/buttermorph/payload-schema/designer";
      if (schemaButton) {
        path = schemaButton.getAttribute("data-designer-path") || path;
      }
      const url = path + queryMarker + "context=" + encodeURIComponent(contextKey) + "&popup=true";
      window.open(url, "buttermorph-schema-" + contextKey, popupOptions + ",width=" + width + ",height=" + height + ",left=" + left + ",top=" + top);
    }
    async function loadScenarios() {
      const response = await fetch("/playground/scenarios", { credentials: "same-origin" });
      const scenarios = await response.json();
      const container = document.querySelector("[data-scenarios]");
      container.innerHTML = "";
      scenarios.forEach(scenario => {
        const button = document.createElement("button");
        button.type = "button";
        button.className = "scenario";
        button.setAttribute("data-context", scenario.contextKey);
        button.setAttribute("aria-pressed", "false");
        button.innerHTML = "<strong>" + scenario.displayName + "</strong><span>" + scenario.description + "</span>";
        button.addEventListener("click", () => loadMapping(scenario.contextKey));
        container.appendChild(button);
      });
    }
    async function loadSchemaScenarios() {
      const response = await fetch("/playground/schema-scenarios", { credentials: "same-origin" });
      const scenarios = await response.json();
      const container = document.querySelector("[data-schema-scenarios]");
      container.innerHTML = "";
      scenarios.forEach(scenario => {
        const button = document.createElement("button");
        button.type = "button";
        button.className = "scenario";
        button.setAttribute("data-schema-context-button", scenario.contextKey);
        button.setAttribute("data-designer-path", scenario.designerPath || "/buttermorph/payload-schema/designer");
        button.setAttribute("aria-pressed", "false");
        button.innerHTML = "<strong>" + scenario.displayName + "</strong><span>" + scenario.description + "</span>";
        button.addEventListener("click", () => loadSchema(scenario.contextKey));
        container.appendChild(button);
      });
    }
    async function loadSave(contextKey) {
      await loadMapping(contextKey);
    }
    async function loadSchema(contextKey) {
      const response = await fetch("/playground/schemas/" + encodeURIComponent(contextKey), { credentials: "same-origin" });
      const schema = await response.json();
      document.querySelectorAll("[data-schema-context-button]").forEach(button => {
        let pressed = "false";
        if (button.getAttribute("data-schema-context-button") === contextKey) {
          pressed = "true";
        }
        button.setAttribute("aria-pressed", pressed);
      });
      document.querySelector("[data-schema-context]").textContent = schema.displayName || contextKey;
      document.querySelector("[data-schema-time]").textContent = schema.savedAt || "Initial schema";
      document.querySelector("[data-schema-json]").value = schema.jsonSchema || "";
      document.querySelector("[data-edit-schema]").disabled = false;
      document.querySelector("[data-edit-schema]").setAttribute("data-schema-context", contextKey);
    }
    async function loadMapping(contextKey) {
      const response = await fetch("/playground/saves/" + encodeURIComponent(contextKey), { credentials: "same-origin" });
      const saved = await response.json();
      const mappingResponse = await fetch("/playground/mappings/" + encodeURIComponent(contextKey), { credentials: "same-origin" });
      const mapping = await mappingResponse.json();
      selectedContext = contextKey;
      document.querySelectorAll("[data-context]").forEach(button => {
        let pressed = "false";
        if (button.getAttribute("data-context") === contextKey) {
          pressed = "true";
        }
        button.setAttribute("aria-pressed", pressed);
      });
      document.querySelector("[data-result-context]").textContent = mapping.displayName || contextKey;
      document.querySelector("[data-result-time]").textContent = saved.savedAt || mapping.savedAt || "Initial mapping";
      document.querySelector("[data-result-count]").textContent = (mapping.mappingCount || 0) + " mappings";
      document.querySelector("[data-result-dsl]").value = mapping.dslContent || "";
      document.querySelector("[data-edit]").disabled = false;
      document.querySelector("[data-execute]").disabled = false;
      document.querySelector("[data-execution-panel]").hidden = false;
      document.querySelector("[data-execution-context]").textContent = mapping.displayName || contextKey;
      document.querySelector("[data-execution-time]").textContent = "Ready to execute";
      document.querySelector("[data-execution-status]").textContent = "Waiting";
      document.querySelector("[data-output-json]").value = "";
      document.querySelector("[data-execution-diagnostics]").textContent = "";
      await loadExecutionSources(contextKey);
    }
    async function loadExecutionSources(contextKey) {
      const response = await fetch("/playground/execute/" + encodeURIComponent(contextKey), {
        method: "POST",
        credentials: "same-origin"
      });
      const result = await response.json();
      renderSources(result.sources || {});
    }
    async function executeMapping() {
      if (!selectedContext) {
        return;
      }
      const formData = new FormData();
      document.querySelectorAll("[data-source-json]").forEach(sourceBox => {
        formData.append("SourceKeys", sourceBox.getAttribute("data-source-key") || "");
        formData.append("SourceJsonValues", sourceBox.value || "");
      });
      const response = await fetch("/playground/execute/" + encodeURIComponent(selectedContext), {
        method: "POST",
        credentials: "same-origin",
        body: formData
      });
      const result = await response.json();
      document.querySelector("[data-execution-panel]").hidden = false;
      document.querySelector("[data-execution-context]").textContent = result.contextKey || selectedContext;
      document.querySelector("[data-execution-time]").textContent = result.executedAt || "Not executed";
      let statusText = "Failed";
      if (result.succeeded) {
        statusText = "Succeeded";
      }
      document.querySelector("[data-execution-status]").textContent = statusText;
      document.querySelector("[data-output-json]").value = result.outputJson || "";
      renderSources(result.sources || {});
      document.querySelector("[data-execution-diagnostics]").textContent = (result.diagnostics || []).join("\\n");
    }
    function renderSources(sources) {
      const sourceContainer = document.querySelector("[data-source-output]");
      sourceContainer.innerHTML = "";
      for (const key in sources) {
        const wrapper = document.createElement("div");
        const label = document.createElement("div");
        const textarea = document.createElement("textarea");
        label.className = "source-label";
        label.textContent = key;
        textarea.className = "source-box";
        textarea.setAttribute("data-source-json", "true");
        textarea.setAttribute("data-source-key", key);
        textarea.value = sources[key];
        wrapper.appendChild(label);
        wrapper.appendChild(textarea);
        sourceContainer.appendChild(wrapper);
      }
    }
    document.querySelector("[data-edit]").addEventListener("click", () => openDesigner(selectedContext));
    document.querySelector("[data-execute]").addEventListener("click", executeMapping);
    window.addEventListener("message", event => {
      if (event.origin !== window.location.origin || !event.data) {
        return;
      }
      if (event.data.type === "ButterMorphDesignerSaved") {
        loadSave(event.data.contextKey);
      }
    });
    const savedContext = new URLSearchParams(window.location.search).get("buttermorphSavedContext");
    loadScenarios().then(() => {
      if (savedContext) {
        loadSave(savedContext);
      }
    });
  </script>
  <script src="/playground-schema.js{{QueryMarker()}}v=2"></script>
</body>
</html>
""";
    }

    // Creates the mapping view returned by the host shell.
    private static PlaygroundMappingView CreateMappingView(
        string contextKey,
        string dslContent,
        string savedAt,
        int mappingCount,
        PlaygroundDesignerHost host)
    {
        return new PlaygroundMappingView
        {
            ContextKey = contextKey,
            DisplayName = ResolveDisplayName(contextKey, host),
            DslContent = dslContent,
            SavedAt = savedAt,
            MappingCount = mappingCount
        };
    }

    // Creates an error mapping view for unknown scenarios.
    private static PlaygroundMappingView CreateUnknownMappingView(string contextKey)
    {
        return new PlaygroundMappingView
        {
            ContextKey = contextKey,
            DisplayName = "Unknown scenario",
            DslContent = string.Empty,
            SavedAt = string.Empty,
            MappingCount = 0
        };
    }

    // Resolves the display name for a scenario.
    private static string ResolveDisplayName(string contextKey, PlaygroundDesignerHost host)
    {
        foreach (PlaygroundScenarioSummary scenario in host.ListScenarios())
        {
            if (string.Equals(scenario.ContextKey, contextKey, StringComparison.OrdinalIgnoreCase))
            {
                return scenario.DisplayName;
            }
        }

        return contextKey;
    }

    // Executes a playground scenario using the real ButterMorph engine.
    private static PlaygroundExecutionResult ExecuteScenario(
        string contextKey,
        IReadOnlyDictionary<string, string> sourceJson,
        ITransformationDocument document,
        int mappingCount,
        IButterMorphEngine engine)
    {
        JsonReader reader = new();
        JsonWriter writer = new();
        Dictionary<string, IStructureGraph> sources = [];

        foreach (KeyValuePair<string, string> source in sourceJson)
        {
            sources[source.Key] = reader.Read(new StructureInput
            {
                Format = "json",
                Content = source.Value
            });
        }

        TransformationResult result = engine.Transform(new TransformationRequest
        {
            Sources = sources,
            Definition = document
        });

        string outputJson = string.Empty;

        if (result.ResultGraph != null)
        {
            outputJson = PrettyPrintJson(writer.Write(result.ResultGraph).Content);
        }

        return new PlaygroundExecutionResult
        {
            ContextKey = contextKey,
            Succeeded = result.Succeeded,
            ExecutedAt = DateTimeOffset.UtcNow.ToString("O"),
            MappingCount = mappingCount,
            Sources = sourceJson,
            OutputJson = outputJson,
            Diagnostics = CreateDiagnosticMessages(result.Diagnostics)
        };
    }

    // Resolves source JSON values posted from the playground source editors.
    private static async Task<IReadOnlyDictionary<string, string>> ResolvePostedSources(
        HttpRequest request,
        IReadOnlyDictionary<string, string> fallbackSources)
    {
        if (!request.HasFormContentType)
        {
            return fallbackSources;
        }

        IFormCollection form = await request.ReadFormAsync();
        Dictionary<string, string> sources = new(fallbackSources, StringComparer.OrdinalIgnoreCase);
        int valueCount = form["SourceJsonValues"].Count;
        int keyCount = form["SourceKeys"].Count;

        for (int index = 0; index < keyCount; index++)
        {
            string key = form["SourceKeys"][index];
            string value = string.Empty;

            if (index < valueCount)
            {
                value = form["SourceJsonValues"][index];
            }

            if (!string.IsNullOrWhiteSpace(key))
            {
                sources[key] = value;
            }
        }

        return sources;
    }

    // Formats JSON output for playground readability.
    private static string PrettyPrintJson(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    // Creates readable diagnostic messages for the playground shell.
    private static IReadOnlyCollection<string> CreateDiagnosticMessages(IReadOnlyCollection<DiagnosticEntry> diagnostics)
    {
        List<string> messages = [];

        foreach (DiagnosticEntry diagnostic in diagnostics)
        {
            messages.Add(diagnostic.Code + ": " + diagnostic.Message);
        }

        return messages;
    }

    // Creates the query separator without using forbidden nullable syntax characters.
    private static string QueryMarker()
    {
        return Convert.ToChar(63).ToString();
    }
}
