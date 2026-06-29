(() => {
  let state = { customTypes: [], customFields: [], schemas: [], mappings: [] };
  let selected = { customTypes: "", customFields: "", schemas: "", mappings: "" };

  const titles = {
    dashboard: "Dashboard",
    customTypes: "Custom Types",
    customFields: "Custom Fields",
    schemas: "Schemas",
    mappings: "Mappings",
    execution: "Execution"
  };

  document.querySelectorAll(".studio-sidebar button").forEach(button => {
    button.addEventListener("click", () => showSection(button.dataset.section));
  });
  document.getElementById("refresh-state").addEventListener("click", loadState);
  document.getElementById("execute-mapping").addEventListener("click", executeSelectedMapping);
  window.addEventListener("message", event => {
    if (event.origin !== window.location.origin || !event.data) {
      return;
    }
    if ((event.data.type || "").startsWith("ButterMorph")) {
      loadState();
    }
  });

  loadState();

  async function loadState() {
    const response = await fetch("/api/state", { cache: "no-store" });
    state = await response.json();
    renderAll();
  }

  function renderAll() {
    renderMetrics();
    renderCrud("customTypes", "Custom Type", item => item.name || item.key, item => item.baseType + " · " + item.version);
    renderCrud("customFields", "Custom Field", item => item.name || item.key, item => item.dataType + " · " + item.appliesToJson);
    renderCrud("schemas", "Schema", item => item.name || item.key, item => item.key + " · " + item.version);
    renderCrud("mappings", "Mapping", item => item.name || item.contextKey, item => item.targetSchemaKey || "No target selected");
    renderExecutionPicker();
  }

  function renderMetrics() {
    document.getElementById("metric-types").textContent = state.customTypes.length;
    document.getElementById("metric-fields").textContent = state.customFields.length;
    document.getElementById("metric-schemas").textContent = state.schemas.length;
    document.getElementById("metric-mappings").textContent = state.mappings.length;
  }

  function renderCrud(kind, label, titleSelector, subtitleSelector) {
    const section = document.getElementById(kind);
    if (!section) {
      return;
    }
    const items = state[kind] || [];
    if (!selected[kind] && items.length) {
      selected[kind] = items[0].contextKey;
    }
    const selectedItem = items.find(item => item.contextKey === selected[kind]);
    section.innerHTML = `
      <div class="work-card">
        <div class="card-header">
          <strong>${label}s</strong>
          <button class="primary-button" data-new="${kind}">New ${label}</button>
        </div>
        <div class="crud-layout">
          <div class="item-list">${items.map(item => `
            <button class="item-button ${item.contextKey === selected[kind] ? "active" : ""}" data-select="${kind}" data-key="${item.contextKey}">
              <strong>${escapeHtml(titleSelector(item))}</strong>
              <span>${escapeHtml(subtitleSelector(item) || item.contextKey)}</span>
            </button>`).join("") || `<p>No ${label.toLowerCase()}s yet.</p>`}</div>
          <div class="detail-pane">${selectedItem ? renderDetail(kind, selectedItem) : `<p>Select or create a ${label.toLowerCase()}.</p>`}</div>
        </div>
      </div>`;

    section.querySelector(`[data-new="${kind}"]`).addEventListener("click", () => createItem(kind));
    section.querySelectorAll("[data-select]").forEach(button => button.addEventListener("click", () => {
      selected[kind] = button.dataset.key;
      renderAll();
    }));
    section.querySelectorAll("[data-open-designer]").forEach(button => button.addEventListener("click", () => openDesigner(button.dataset.openDesigner, button.dataset.key)));
    section.querySelectorAll("[data-delete]").forEach(button => button.addEventListener("click", () => deleteItem(button.dataset.delete, button.dataset.key)));
    section.querySelectorAll("[data-save-injection]").forEach(button => button.addEventListener("click", () => saveInjection(button.dataset.key)));
    section.querySelectorAll("[data-save-mapping-settings]").forEach(button => button.addEventListener("click", () => saveMappingSettings(button.dataset.key)));
  }

  function renderDetail(kind, item) {
    if (kind === "schemas") {
      return renderSchemaDetail(item);
    }
    if (kind === "mappings") {
      return renderMappingDetail(item);
    }
    const route = kind === "customTypes" ? "/buttermorph/schema-types/designer" : "/buttermorph/metadata-fields/designer";
    return `
      <div class="action-row">
        <button class="primary-button" data-open-designer="${route}" data-key="${item.contextKey}">Open Designer</button>
        <button class="danger-button" data-delete="${kind}" data-key="${item.contextKey}">Delete</button>
      </div>
      <div class="detail-grid">
        <label>Name<input value="${escapeAttr(item.name || "")}" readonly></label>
        <label>Key<input value="${escapeAttr(item.key || "")}" readonly></label>
      </div>
      <pre>${escapeHtml(JSON.stringify(item, null, 2))}</pre>`;
  }

  function renderSchemaDetail(item) {
    return `
      <div class="action-row">
        <button class="primary-button" data-open-designer="/buttermorph/payload-schema/designer" data-key="${item.contextKey}">Open Designer</button>
        <button class="ghost-button" data-save-injection="${item.contextKey}" data-key="${item.contextKey}">Save Injection</button>
        <button class="danger-button" data-delete="schemas" data-key="${item.contextKey}">Delete</button>
      </div>
      <div class="detail-grid">
        <label>Name<input value="${escapeAttr(item.name || "")}" readonly></label>
        <label>Key<input value="${escapeAttr(item.key || "")}" readonly></label>
      </div>
      <div class="injection-grid">
        <div class="check-list"><strong>Inject Custom Types</strong>${state.customTypes.map(type => checkbox("type", item, type)).join("")}</div>
        <div class="check-list"><strong>Inject Custom Fields</strong>${state.customFields.map(field => checkbox("field", item, field)).join("")}</div>
      </div>
      <pre>${escapeHtml(pretty(item.jsonSchema || "{}"))}</pre>`;
  }

  function renderMappingDetail(item) {
    return `
      <div class="action-row">
        <button class="primary-button" data-open-designer="/buttermorph/designer" data-key="${item.contextKey}">Open Designer</button>
        <button class="ghost-button" data-save-mapping-settings="${item.contextKey}" data-key="${item.contextKey}">Save Setup</button>
        <button class="danger-button" data-delete="mappings" data-key="${item.contextKey}">Delete</button>
      </div>
      <div class="detail-grid">
        <label>Name<input id="mapping-name-${item.contextKey}" value="${escapeAttr(item.name || "")}"></label>
        <label>Target Schema<select id="mapping-target-${item.contextKey}">${schemaOptions(item.targetSchemaKey)}</select></label>
        <label>Source Alias<input id="mapping-source-alias-${item.contextKey}" value="${escapeAttr(Object.keys(item.sourceSchemaKeys || {})[0] || "source")}"></label>
        <label>Source Schema<select id="mapping-source-schema-${item.contextKey}">${schemaOptions(Object.values(item.sourceSchemaKeys || {})[0] || "")}</select></label>
      </div>
      <pre>${escapeHtml(item.dslContent || "No DSL saved yet.")}</pre>`;
  }

  function checkbox(kind, schema, item) {
    const list = kind === "type" ? schema.injectedCustomTypeKeys : schema.injectedCustomFieldKeys;
    const checked = (list || []).includes(item.contextKey) ? "checked" : "";
    return `<label><input type="checkbox" data-inject-${kind}="${schema.contextKey}" value="${item.contextKey}" ${checked}> ${escapeHtml(item.name || item.key)}</label>`;
  }

  function schemaOptions(selectedKey) {
    return state.schemas.map(schema => `<option value="${schema.contextKey}" ${schema.contextKey === selectedKey ? "selected" : ""}>${escapeHtml(schema.name || schema.key)}</option>`).join("");
  }

  async function createItem(kind) {
    const name = prompt("Name");
    if (!name) {
      return;
    }
    const response = await fetch("/api/" + kind, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ name })
    });
    const created = await response.json();
    selected[kind] = created.contextKey;
    await loadState();
    if (kind !== "mappings") {
      const route = kind === "customTypes" ? "/buttermorph/schema-types/designer" : kind === "customFields" ? "/buttermorph/metadata-fields/designer" : "/buttermorph/payload-schema/designer";
      openDesigner(route, created.contextKey);
    }
  }

  function openDesigner(route, contextKey) {
    const url = `${route}?context=${encodeURIComponent(contextKey)}&popup=true&returnUrl=/`;
    window.ButterMorphHost.openFrame(url, { title: "ButterMorph Designer", width: 1420, height: 900 });
  }

  async function deleteItem(kind, contextKey) {
    await fetch(`/api/${kind}/${encodeURIComponent(contextKey)}`, { method: "DELETE" });
    selected[kind] = "";
    await loadState();
  }

  async function saveInjection(contextKey) {
    const typeKeys = Array.from(document.querySelectorAll(`[data-inject-type="${contextKey}"]:checked`)).map(input => input.value);
    const fieldKeys = Array.from(document.querySelectorAll(`[data-inject-field="${contextKey}"]:checked`)).map(input => input.value);
    await fetch(`/api/schemas/${encodeURIComponent(contextKey)}/injection`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ customTypeKeys: typeKeys, customFieldKeys: fieldKeys })
    });
    await loadState();
  }

  async function saveMappingSettings(contextKey) {
    const name = document.getElementById(`mapping-name-${contextKey}`).value;
    const alias = document.getElementById(`mapping-source-alias-${contextKey}`).value || "source";
    const sourceSchema = document.getElementById(`mapping-source-schema-${contextKey}`).value;
    const targetSchema = document.getElementById(`mapping-target-${contextKey}`).value;
    await fetch(`/api/mappings/${encodeURIComponent(contextKey)}/settings`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ name, targetSchemaKey: targetSchema, sourceSchemaKeys: { [alias]: sourceSchema } })
    });
    await loadState();
  }

  function renderExecutionPicker() {
    const select = document.getElementById("execution-mapping");
    select.innerHTML = state.mappings.map(mapping => `<option value="${mapping.contextKey}">${escapeHtml(mapping.name || mapping.contextKey)}</option>`).join("");
    select.onchange = renderExecutionSources;
    renderExecutionSources();
  }

  function renderExecutionSources() {
    const mapping = state.mappings.find(item => item.contextKey === document.getElementById("execution-mapping").value);
    const host = document.getElementById("execution-sources");
    if (!mapping) {
      host.innerHTML = "";
      return;
    }
    host.innerHTML = Object.entries(mapping.sourceSchemaKeys || {}).map(([alias]) => `
      <label>${escapeHtml(alias)} JSON
        <textarea data-source-json="${alias}">${escapeHtml((mapping.sourceSamples || {})[alias] || "{}")}</textarea>
      </label>`).join("");
  }

  async function executeSelectedMapping() {
    const contextKey = document.getElementById("execution-mapping").value;
    const sources = {};
    document.querySelectorAll("[data-source-json]").forEach(textarea => {
      sources[textarea.dataset.sourceJson] = textarea.value;
    });
    const response = await fetch(`/api/mappings/${encodeURIComponent(contextKey)}/execute`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ sources })
    });
    const result = await response.json();
    document.getElementById("execution-output").value = result.outputJson || "";
    document.getElementById("execution-diagnostics").textContent = (result.diagnostics || []).join("\n") || (result.succeeded ? "Succeeded" : "No diagnostics");
  }

  function showSection(sectionName) {
    document.querySelectorAll(".studio-sidebar button").forEach(button => button.classList.toggle("active", button.dataset.section === sectionName));
    document.querySelectorAll(".studio-section").forEach(section => section.classList.toggle("active", section.id === sectionName));
    document.getElementById("section-title").textContent = titles[sectionName] || sectionName;
  }

  function pretty(json) {
    try {
      return JSON.stringify(JSON.parse(json), null, 2);
    } catch {
      return json;
    }
  }

  function escapeHtml(value) {
    return String(value).replace(/[&<>"']/g, character => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", "\"": "&quot;", "'": "&#39;" })[character]);
  }

  function escapeAttr(value) {
    return escapeHtml(value);
  }
})();
