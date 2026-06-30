namespace ButterMorph.StudioPlayground.Services;

/// <summary>
/// Renders the Studio Playground host page.
/// </summary>
internal static class StudioHtml
{
    /// <summary>
    /// Renders the host HTML.
    /// </summary>
    /// <returns>The host HTML.</returns>
    public static string Render()
    {
        return """
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>ButterMorph Studio Playground</title>
              <link rel="stylesheet" href="/studio.css?v=2">
              <script src="/_content/ButterMorph.Web.Razor/buttermorph/buttermorph-host.js?v=1"></script>
            </head>
            <body>
              <div class="studio-shell">
                <aside class="studio-sidebar">
                  <div class="studio-brand">
                    <span>BUTTERMORPH</span>
                    <strong>Studio Playground</strong>
                  </div>
                  <nav>
                    <button class="active" data-section="dashboard">Dashboard</button>
                    <button data-section="customTypes">Custom Types</button>
                    <button data-section="customFields">Custom Fields</button>
                    <button data-section="schemas">Schemas</button>
                    <button data-section="mappings">Mappings</button>
                    <button data-section="execution">Execution</button>
                  </nav>
                </aside>
                <main class="studio-main">
                  <header class="studio-topbar">
                    <div>
                      <span class="kicker">Plug-and-play host simulation</span>
                      <h1 id="section-title">Dashboard</h1>
                    </div>
                    <button id="refresh-state" class="ghost-button">Refresh</button>
                  </header>
                  <section id="dashboard" class="studio-section active">
                    <div class="hero-card">
                      <h2>Host-owned catalogs, ButterMorph-owned designers.</h2>
                      <p>This sample keeps persistence in the host, injects only selected catalog items into designers, and executes mappings through the real engine.</p>
                    </div>
                    <div class="metric-grid">
                      <article><strong id="metric-types">0</strong><span>Custom types</span></article>
                      <article><strong id="metric-fields">0</strong><span>Custom fields</span></article>
                      <article><strong id="metric-schemas">0</strong><span>Schemas</span></article>
                      <article><strong id="metric-mappings">0</strong><span>Mappings</span></article>
                    </div>
                  </section>
                  <section id="customTypes" class="studio-section" data-kind="customTypes"></section>
                  <section id="customFields" class="studio-section" data-kind="customFields"></section>
                  <section id="schemas" class="studio-section" data-kind="schemas"></section>
                  <section id="mappings" class="studio-section" data-kind="mappings"></section>
                  <section id="execution" class="studio-section">
                    <div class="work-card">
                      <div class="card-header">
                        <strong>Mapping execution</strong>
                        <button id="execute-mapping" class="primary-button">Execute</button>
                      </div>
                      <div class="execution-layout">
                        <div>
                          <label>Mapping</label>
                          <select id="execution-mapping"></select>
                          <div id="execution-sources" class="source-editors"></div>
                        </div>
                        <div>
                          <label>Output</label>
                          <textarea id="execution-output" readonly></textarea>
                          <label>Diagnostics</label>
                          <pre id="execution-diagnostics"></pre>
                        </div>
                      </div>
                    </div>
                  </section>
                </main>
              </div>
              <script src="/studio.js?v=3"></script>
            </body>
            </html>
            """;
    }
}
