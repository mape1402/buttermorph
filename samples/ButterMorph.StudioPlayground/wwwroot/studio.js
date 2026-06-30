(() => {
  const storageKey = "ButterMorph.StudioPlayground.State";
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
    const type = event.data.type || "";
    if (type.startsWith("ButterMorph")) {
      selectSavedItem(type, event.data.contextKey || "");
      reloadStateFromBackend();
    }
  });

  loadState();

  async function loadState() {
    const persisted = readPersistedState();
    if (persisted) {
      state = persisted;
      await syncStateToBackend();
      renderAll();
      return;
    }

    const response = await fetch("/api/state", { cache: "no-store" });
    state = await response.json();
    persistState();
    renderAll();
  }

  async function reloadStateFromBackend() {
    const response = await fetch("/api/state", { cache: "no-store" });
    state = await response.json();
    persistState();
    renderAll();
  }

  function readPersistedState() {
    try {
      const json = localStorage.getItem(storageKey);
      if (!json) {
        return null;
      }
      const parsed = JSON.parse(json);
      return parsed && Array.isArray(parsed.customTypes) ? parsed : null;
    } catch {
      return null;
    }
  }

  function persistState() {
    localStorage.setItem(storageKey, JSON.stringify(state));
  }

  async function syncStateToBackend() {
    await fetch("/api/state/hydrate", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(state)
    });
  }

  function renderAll() {
    renderMetrics();
    renderCrud("customTypes", "Custom Type", item => item.name || item.key, item => item.baseType + " · " + item.version);
    renderCrud("customFields", "Custom Field", item => item.name || item.key, item => item.dataType + " · " + item.appliesToJson);
    renderCrud("schemas", "Schema", item => item.name || item.key, item => item.key + " · " + item.version);
    renderCrud("mappings", "Mapping", item => item.name || item.id, item => item.targetSchemaId || "No target selected");
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
      selected[kind] = items[0].id;
    }
    const selectedItem = items.find(item => item.id === selected[kind]);
    section.innerHTML = `
      <div class="work-card">
        <div class="card-header">
          <strong>${label}s</strong>
          <button class="primary-button" data-new="${kind}">New ${label}</button>
        </div>
        <div class="crud-layout">
          <div class="item-list">${items.map(item => `
            <button class="item-button ${item.id === selected[kind] ? "active" : ""}" data-select="${kind}" data-key="${item.id}">
              <strong>${escapeHtml(titleSelector(item))}</strong>
              <span>${escapeHtml(subtitleSelector(item) || item.id)}</span>
            </button>`).join("") || `<p>No ${label.toLowerCase()}s yet.</p>`}</div>
          <div class="detail-pane">${selectedItem ? renderDetail(kind, selectedItem) : `<p>Select or create a ${label.toLowerCase()}.</p>`}</div>
        </div>
      </div>`;

    section.querySelector(`[data-new="${kind}"]`).addEventListener("click", () => createItem(kind));
    section.querySelectorAll("[data-select]").forEach(button => button.addEventListener("click", () => {
      selected[kind] = button.dataset.key;
      renderAll();
    }));
    section.querySelectorAll("[data-open-designer]").forEach(button => button.addEventListener("click", () => openDesigner(button.dataset.openDesigner, button.dataset.key, "", createEditQuery(kind, button.dataset.key))));
    section.querySelectorAll("[data-delete]").forEach(button => button.addEventListener("click", () => deleteItem(button.dataset.delete, button.dataset.key)));
    section.querySelectorAll("[data-save-injection]").forEach(button => button.addEventListener("click", () => saveInjection(button.dataset.key)));
    section.querySelectorAll("[data-save-mapping-settings]").forEach(button => button.addEventListener("click", () => saveMappingSettings(button.dataset.key)));
    section.querySelectorAll("[data-add-mapping-source]").forEach(button => button.addEventListener("click", () => addMappingSourceRow(button.dataset.addMappingSource)));
    section.querySelectorAll("[data-remove-mapping-source]").forEach(button => button.addEventListener("click", () => removeMappingSourceRow(button)));
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
        <button class="primary-button" data-open-designer="${route}" data-key="${item.id}">Open Designer</button>
        <button class="danger-button" data-delete="${kind}" data-key="${item.id}">Delete</button>
      </div>
      <div class="detail-grid">
        <label>Name<input value="${escapeAttr(item.name || "")}" readonly></label>
        <label>Key<input value="${escapeAttr(item.key || "")}" readonly></label>
      </div>
      <pre class="json-viewer">${escapeHtml(pretty(item.butterMorphResultJson || "{}"))}</pre>`;
  }

  function renderSchemaDetail(item) {
    return `
      <div class="action-row">
        <button class="primary-button" data-open-designer="/buttermorph/payload-schema/designer" data-key="${item.id}">Open Designer</button>
        <button class="ghost-button" data-save-injection="${item.id}" data-key="${item.id}">Save Injection</button>
        <button class="danger-button" data-delete="schemas" data-key="${item.id}">Delete</button>
      </div>
      <div class="detail-grid">
        <label>Name<input value="${escapeAttr(item.name || "")}" readonly></label>
        <label>Key<input value="${escapeAttr(item.key || "")}" readonly></label>
      </div>
      <div class="injection-grid">
        <div class="check-list"><strong>Inject Custom Types</strong>${state.customTypes.map(type => checkbox("type", item, type)).join("")}</div>
        <div class="check-list"><strong>Inject Custom Fields</strong>${state.customFields.map(field => checkbox("field", item, field)).join("")}</div>
      </div>
      <pre class="json-viewer schema-json-viewer">${escapeHtml(pretty(item.butterMorphResultJson || "{}"))}</pre>`;
  }

  function renderMappingDetail(item) {
    return `
      <div class="action-row">
        <button class="primary-button" data-open-designer="/buttermorph/designer" data-key="${item.id}">Open Designer</button>
        <button class="ghost-button" data-save-mapping-settings="${item.id}" data-key="${item.id}">Save Setup</button>
        <button class="danger-button" data-delete="mappings" data-key="${item.id}">Delete</button>
      </div>
      <div class="detail-grid">
        <label>Name<input id="mapping-name-${item.id}" value="${escapeAttr(item.name || "")}"></label>
        <label>Target Schema<select id="mapping-target-${item.id}">${schemaOptions(item.targetSchemaId)}</select></label>
        <label class="check-row"><input type="checkbox" id="mapping-schema-actions-${item.id}" ${item.showSchemaActions ? "checked" : ""}> Allow schema loading in ButterMorph Studio</label>
      </div>
      <div class="mapping-source-editor">
        <div class="mapping-source-header">
          <strong>Sources</strong>
          <button class="ghost-button" data-add-mapping-source="${item.id}" type="button">Add Source</button>
        </div>
        <div data-mapping-sources="${item.id}">
          ${mappingSourceRows(item.id, item.sourceSchemaIds || {})}
        </div>
      </div>
      <pre>${escapeHtml(item.dslContent || "No DSL saved yet.")}</pre>`;
  }

  function checkbox(kind, schema, item) {
    const list = kind === "type" ? schema.injectedCustomTypeKeys : schema.injectedCustomFieldKeys;
    const checked = (list || []).includes(item.id) ? "checked" : "";
    return `<label><input type="checkbox" data-inject-${kind}="${schema.id}" value="${item.id}" ${checked}> ${escapeHtml(item.name || item.key)}</label>`;
  }

  function schemaOptions(selectedKey) {
    return state.schemas.map(schema => `<option value="${schema.id}" ${schema.id === selectedKey ? "selected" : ""}>${escapeHtml(schema.name || schema.key)}</option>`).join("");
  }

  async function createItem(kind) {
    if (kind !== "mappings") {
      const id = createHostId(kind);
      selected[kind] = id;
      const route = kind === "customTypes" ? "/buttermorph/schema-types/designer" : kind === "customFields" ? "/buttermorph/metadata-fields/designer" : "/buttermorph/payload-schema/designer";
      if (kind === "schemas") {
        openSchemaSetup(id, route);
        return;
      }
      openDesigner(route, id, "create", "");
      return;
    }
    openMappingSetup(createHostId(kind), "/buttermorph/designer");
  }

  async function openDesigner(route, id, mode, extraQuery) {
    await syncStateToBackend();
    const modeQuery = mode ? `&mode=${encodeURIComponent(mode)}` : "";
    const injectionQuery = extraQuery || "";
    const url = `${route}?context=${encodeURIComponent(id)}${modeQuery}${injectionQuery}&popup=true&returnUrl=/`;
    window.ButterMorphHost.openFrame(url, { title: "ButterMorph Designer", width: 1420, height: 900 });
  }

  function createEditQuery(kind, id) {
    if (kind !== "schemas") {
      return "";
    }
    const schema = state.schemas.find(item => item.id === id);
    if (!schema) {
      return "";
    }
    const typeIds = schema.injectedCustomTypeKeys || [];
    const fieldIds = schema.injectedCustomFieldKeys || [];
    return `&customTypes=${encodeURIComponent(typeIds.join(","))}&customFields=${encodeURIComponent(fieldIds.join(","))}`;
  }

  function openSchemaSetup(id, route) {
    const overlay = document.createElement("div");
    overlay.className = "studio-modal-overlay";
    overlay.innerHTML = `
      <div class="studio-modal">
        <div class="studio-modal-header">
          <strong>New Schema Setup</strong>
          <button type="button" data-close-schema-setup>&times;</button>
        </div>
        <p class="studio-modal-help">Choose the custom types and custom fields that ButterMorph will receive for this schema designer session.</p>
        <div class="injection-grid setup-injection-grid">
          <div class="check-list"><strong>Inject Custom Types</strong>${state.customTypes.map(type => setupCheckbox("type", type)).join("") || "<span>No custom types available.</span>"}</div>
          <div class="check-list"><strong>Inject Custom Fields</strong>${state.customFields.map(field => setupCheckbox("field", field)).join("") || "<span>No custom fields available.</span>"}</div>
        </div>
        <div class="action-row studio-modal-actions">
          <button class="ghost-button" type="button" data-close-schema-setup>Cancel</button>
          <button class="primary-button" type="button" data-create-schema-with-injection>Create Schema</button>
        </div>
      </div>`;
    document.body.appendChild(overlay);
    overlay.querySelectorAll("[data-close-schema-setup]").forEach(button => button.addEventListener("click", () => overlay.remove()));
    overlay.querySelector("[data-create-schema-with-injection]").addEventListener("click", () => {
      const typeIds = Array.from(overlay.querySelectorAll("[data-setup-type]:checked")).map(input => input.value);
      const fieldIds = Array.from(overlay.querySelectorAll("[data-setup-field]:checked")).map(input => input.value);
      const query = `&customTypes=${encodeURIComponent(typeIds.join(","))}&customFields=${encodeURIComponent(fieldIds.join(","))}`;
      overlay.remove();
      openDesigner(route, id, "create", query);
    });
  }

  function openMappingSetup(id, route) {
    const overlay = document.createElement("div");
    overlay.className = "studio-modal-overlay";
    overlay.innerHTML = `
      <div class="studio-modal studio-modal-wide mapping-setup-modal">
        <div class="studio-modal-header">
          <strong>New Mapping Setup</strong>
          <button type="button" data-close-mapping-setup>&times;</button>
        </div>
        <p class="studio-modal-help">Choose the target schema and all source schemas that ButterMorph will receive for this mapping designer session.</p>
        <div class="mapping-setup-grid">
          <label>Name<input id="new-mapping-name" placeholder="Mapping display name"></label>
          <label>Target Schema<select id="new-mapping-target">${schemaOptions("")}</select></label>
          <label class="mapping-toggle"><input type="checkbox" id="new-mapping-schema-actions"><span>Allow schema loading in ButterMorph Studio</span></label>
        </div>
        <div class="mapping-source-editor">
          <div class="mapping-source-header">
            <div>
              <strong>Sources</strong>
              <span>Each source needs a unique alias used by mapping expressions.</span>
            </div>
            <button class="ghost-button" type="button" data-add-new-mapping-source>Add Source</button>
          </div>
          <div class="mapping-source-columns">
            <span>Alias</span>
            <span>Schema</span>
            <span></span>
          </div>
          <div data-new-mapping-sources>
            ${mappingSourceRow("new", "source", "")}
          </div>
        </div>
        <div class="action-row studio-modal-actions">
          <button class="ghost-button" type="button" data-close-mapping-setup>Cancel</button>
          <button class="primary-button" type="button" data-create-mapping-with-setup>Create Mapping</button>
        </div>
      </div>`;
    document.body.appendChild(overlay);
    overlay.querySelectorAll("[data-close-mapping-setup]").forEach(button => button.addEventListener("click", () => overlay.remove()));
    overlay.querySelector("[data-add-new-mapping-source]").addEventListener("click", () => {
      const host = overlay.querySelector("[data-new-mapping-sources]");
      host.insertAdjacentHTML("beforeend", mappingSourceRow("new", "source" + String(host.children.length + 1), ""));
      host.querySelectorAll("[data-remove-mapping-source]").forEach(button => button.onclick = () => removeMappingSourceRow(button));
    });
    overlay.querySelectorAll("[data-remove-mapping-source]").forEach(button => button.addEventListener("click", () => removeMappingSourceRow(button)));
    overlay.querySelector("[data-create-mapping-with-setup]").addEventListener("click", async () => {
      const setup = collectMappingSetup("new", {
        name: overlay.querySelector("#new-mapping-name").value,
        targetSchemaId: overlay.querySelector("#new-mapping-target").value,
        showSchemaActions: overlay.querySelector("#new-mapping-schema-actions").checked,
        sourceHost: overlay.querySelector("[data-new-mapping-sources]")
      });
      if (!setup.name || !setup.targetSchemaId || Object.keys(setup.sourceSchemaIds).length === 0) {
        alert("Mapping name, target schema and at least one source are required.");
        return;
      }

      await saveMappingSetup(id, setup);
      overlay.remove();
      openDesigner(route, id, "create", "");
    });
  }

  function setupCheckbox(kind, item) {
    const attr = kind === "type" ? "data-setup-type" : "data-setup-field";
    return `<label><input type="checkbox" ${attr} value="${escapeAttr(item.id)}"> ${escapeHtml(item.name || item.key)}</label>`;
  }

  function createHostId(kind) {
    return kind.replace(/s$/, "") + "-" + Date.now();
  }

  function selectSavedItem(messageType, id) {
    if (!id) {
      return;
    }
    if (messageType === "ButterMorphSchemaTypeDesignerSaved") {
      selected.customTypes = id;
      return;
    }
    if (messageType === "ButterMorphFieldMetadataDesignerSaved") {
      selected.customFields = id;
      return;
    }
    if (messageType === "ButterMorphPayloadSchemaDesignerSaved") {
      selected.schemas = id;
      return;
    }
    if (messageType === "ButterMorphDesignerSaved") {
      selected.mappings = id;
    }
  }

  async function deleteItem(kind, id) {
    await fetch(`/api/${kind}/${encodeURIComponent(id)}`, { method: "DELETE" });
    selected[kind] = "";
    await reloadStateFromBackend();
  }

  async function saveInjection(id) {
    const typeKeys = Array.from(document.querySelectorAll(`[data-inject-type="${id}"]:checked`)).map(input => input.value);
    const fieldKeys = Array.from(document.querySelectorAll(`[data-inject-field="${id}"]:checked`)).map(input => input.value);
    await fetch(`/api/schemas/${encodeURIComponent(id)}/injection`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ customTypeKeys: typeKeys, customFieldKeys: fieldKeys })
    });
    await reloadStateFromBackend();
  }

  async function saveMappingSettings(id) {
    const setup = collectMappingSetup(id, {
      name: document.getElementById(`mapping-name-${id}`).value,
      targetSchemaId: document.getElementById(`mapping-target-${id}`).value,
      showSchemaActions: document.getElementById(`mapping-schema-actions-${id}`).checked,
      sourceHost: document.querySelector(`[data-mapping-sources="${id}"]`)
    });
    await fetch(`/api/mappings/${encodeURIComponent(id)}/settings`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(setup)
    });
    await reloadStateFromBackend();
  }

  async function saveMappingSetup(id, setup) {
    await fetch(`/api/mappings/${encodeURIComponent(id)}/setup`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(setup)
    });
  }

  function collectMappingSetup(id, values) {
    const sourceSchemaIds = {};
    const selector = id === "new" ? "[data-mapping-source-row='new']" : `[data-mapping-source-row="${id}"]`;
    values.sourceHost.querySelectorAll(selector).forEach(row => {
      const alias = row.querySelector("[data-mapping-source-alias]")?.value.trim() || "";
      const schemaId = row.querySelector("[data-mapping-source-schema]")?.value || "";
      if (alias && schemaId) {
        sourceSchemaIds[alias] = schemaId;
      }
    });
    return {
      name: values.name || "",
      targetSchemaId: values.targetSchemaId || "",
      showSchemaActions: values.showSchemaActions === true,
      sourceSchemaIds
    };
  }

  function mappingSourceRows(id, sourceSchemaIds) {
    const entries = Object.entries(sourceSchemaIds || {});
    if (entries.length === 0) {
      return mappingSourceRow(id, "source", "");
    }
    return entries.map(([alias, schemaId]) => mappingSourceRow(id, alias, schemaId)).join("");
  }

  function mappingSourceRow(id, alias, schemaId) {
    return `
      <div class="mapping-source-row" data-mapping-source-row="${escapeAttr(id)}">
        <label>Alias<input data-mapping-source-alias value="${escapeAttr(alias)}"></label>
        <label>Schema<select data-mapping-source-schema>${schemaOptions(schemaId)}</select></label>
        <button class="danger-button" type="button" data-remove-mapping-source="${escapeAttr(id)}">Remove</button>
      </div>`;
  }

  function addMappingSourceRow(id) {
    const host = document.querySelector(`[data-mapping-sources="${id}"]`);
    if (!host) {
      return;
    }
    host.insertAdjacentHTML("beforeend", mappingSourceRow(id, "source" + String(host.children.length + 1), ""));
    host.querySelectorAll("[data-remove-mapping-source]").forEach(button => button.onclick = () => removeMappingSourceRow(button));
  }

  function removeMappingSourceRow(button) {
    const row = button.closest(".mapping-source-row");
    const host = row?.parentElement;
    if (!row || !host || host.querySelectorAll(".mapping-source-row").length <= 1) {
      return;
    }
    row.remove();
  }

  function renderExecutionPicker() {
    const select = document.getElementById("execution-mapping");
    select.innerHTML = state.mappings.map(mapping => `<option value="${mapping.id}">${escapeHtml(mapping.name || mapping.id)}</option>`).join("");
    select.onchange = renderExecutionSources;
    renderExecutionSources();
  }

  function renderExecutionSources() {
    const mapping = state.mappings.find(item => item.id === document.getElementById("execution-mapping").value);
    const host = document.getElementById("execution-sources");
    if (!mapping) {
      host.innerHTML = "";
      return;
    }
    host.innerHTML = Object.entries(mapping.sourceSchemaIds || {}).map(([alias]) => `
      <label>${escapeHtml(alias)} JSON
        <textarea data-source-json="${alias}">${escapeHtml((mapping.sourceSamples || {})[alias] || "{}")}</textarea>
      </label>`).join("");
  }

  async function executeSelectedMapping() {
    const id = document.getElementById("execution-mapping").value;
    const sources = {};
    document.querySelectorAll("[data-source-json]").forEach(textarea => {
      sources[textarea.dataset.sourceJson] = textarea.value;
    });
    const response = await fetch(`/api/mappings/${encodeURIComponent(id)}/execute`, {
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




