using ButterMorph.DependencyInjection;
using ButterMorph.Design;
using ButterMorph.Json.Schema;
using ButterMorph.Web.Razor;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddButterMorph();
builder.Services.AddButterMorphDesign();
builder.Services.AddButterMorphJsonSchema();
builder.Services.AddButterMorphRazorDesigner();
builder.Services.AddSingleton<PlaygroundMappingStore>();
builder.Services.AddSingleton<IButterMorphDesignerHost, PlaygroundDesignerHost>();

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
app.MapGet("/playground/saves/{contextKey}", (string contextKey, PlaygroundMappingStore store) =>
{
    if (store.TryGet(contextKey, out PlaygroundMappingSave save))
    {
        return Results.Json(save);
    }

    return Results.Json(new PlaygroundMappingSave
    {
        ContextKey = contextKey
    });
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
    body { background:#f4f6fb; color:#111827; font-family:Segoe UI,Arial,sans-serif; margin:0; }
    main { display:grid; gap:1rem; margin:0 auto; max-width:1180px; padding:2rem; }
    header { background:#111827; border-radius:12px; color:#fff; padding:1.2rem 1.4rem; }
    h1 { font-size:1.5rem; margin:0; }
    p { color:#5b6478; margin:.35rem 0 0; }
    .scenarios { display:grid; gap:.75rem; grid-template-columns:repeat(3,minmax(0,1fr)); }
    button { background:#4f46e5; border:0; border-radius:8px; color:#fff; cursor:pointer; font-weight:700; min-height:44px; padding:.75rem 1rem; }
    button:hover { background:#4338ca; }
    section { background:#fff; border:1px solid #d8deed; border-radius:12px; box-shadow:0 10px 24px rgba(15,23,42,.08); padding:1rem; }
    .meta { display:flex; gap:1rem; margin-bottom:.75rem; }
    .meta span { background:#eef2ff; border-radius:999px; color:#3730a3; font-size:.8rem; font-weight:700; padding:.25rem .6rem; }
    textarea { border:1px solid #ccd4e5; border-radius:8px; box-sizing:border-box; font-family:Consolas,Cascadia Code,monospace; height:430px; padding:.75rem; resize:vertical; width:100%; }
  </style>
</head>
<body>
  <main>
    <header>
      <h1>ButterMorph host playground</h1>
      <p>Simulates a real application opening ButterMorph as a temporary mapping editor.</p>
    </header>
    <section>
      <h2>Open mapping designer</h2>
      <div class="scenarios">
        <button type="button" data-context="complex">Customer order mapping</button>
        <button type="button" data-context="invoice">Invoice accounting mapping</button>
        <button type="button" data-context="support">Support case mapping</button>
      </div>
    </section>
    <section>
      <h2>Returned mapping</h2>
      <div class="meta">
        <span data-result-context>No context saved</span>
        <span data-result-time>Not saved yet</span>
        <span data-result-count>0 mappings</span>
      </div>
      <textarea readonly data-result-dsl placeholder="Save a mapping from ButterMorph to see the returned DSL here."></textarea>
    </section>
  </main>
  <script>
    const popupOptions = "popup=yes,width=1480,height=900,resizable=yes,scrollbars=yes";
    function openDesigner(contextKey) {
      const queryMarker = "{{QueryMarker()}}";
      const width = Math.min(1480, screen.availWidth - 80);
      const height = Math.min(900, screen.availHeight - 80);
      const left = Math.max(0, Math.round((screen.availWidth - width) / 2));
      const top = Math.max(0, Math.round((screen.availHeight - height) / 2));
      const url = "/buttermorph/designer" + queryMarker + "context=" + encodeURIComponent(contextKey) + "&popup=true&returnUrl=/";
      window.open(url, "buttermorph-" + contextKey, popupOptions + ",width=" + width + ",height=" + height + ",left=" + left + ",top=" + top);
    }
    async function loadSave(contextKey) {
      const response = await fetch("/playground/saves/" + encodeURIComponent(contextKey), { credentials: "same-origin" });
      const save = await response.json();
      document.querySelector("[data-result-context]").textContent = save.contextKey || contextKey;
      document.querySelector("[data-result-time]").textContent = save.savedAt || "Not saved yet";
      document.querySelector("[data-result-count]").textContent = (save.mappingCount || 0) + " mappings";
      document.querySelector("[data-result-dsl]").value = save.dslContent || "";
    }
    document.querySelectorAll("[data-context]").forEach(button => {
      button.addEventListener("click", () => openDesigner(button.getAttribute("data-context")));
    });
    window.addEventListener("message", event => {
      if (event.origin !== window.location.origin || !event.data || event.data.type !== "ButterMorphDesignerSaved") {
        return;
      }
      loadSave(event.data.contextKey);
    });
    const savedContext = new URLSearchParams(window.location.search).get("buttermorphSavedContext");
    if (savedContext) {
      loadSave(savedContext);
    }
  </script>
</body>
</html>
""";
    }

    // Creates the query separator without using forbidden nullable syntax characters.
    private static string QueryMarker()
    {
        return Convert.ToChar(63).ToString();
    }
}
