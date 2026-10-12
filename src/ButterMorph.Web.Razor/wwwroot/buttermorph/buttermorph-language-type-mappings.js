(function () {
  const dataNode = document.getElementById("language-type-mapping-data");
  let data = null;
  data = normalizeData(readData(dataNode));
  let activeDocument = cloneDocument(resolveInitialDocument());
  let searchQuery = "";

  function readData(node) {
    if (!node) {
      return { languages: [], butterMorphTypes: [], documents: [], bundles: [] };
    }

    try {
      return JSON.parse(node.textContent || "{}");
    } catch {
      return { languages: [], butterMorphTypes: [], documents: [], bundles: [] };
    }
  }

  function normalizeData(source) {
    const normalized = source || {};
    normalized.languages = normalized.languages || [];
    normalized.butterMorphTypes = normalized.butterMorphTypes || [];
    normalized.bundles = normalized.bundles || [];
    normalized.documents = Array.isArray(normalized.documents)
      ? normalized.documents.map(cloneDocument)
      : [];

    if (!normalized.documents.length && Array.isArray(normalized.profiles)) {
      normalized.documents = normalized.profiles.map(documentFromProfile);
    }

    if (!normalized.documents.length) {
      normalized.documents = [createEmptyDocument(normalized.languages && normalized.languages[0] ? normalized.languages[0].key : "")];
    }

    normalized.activeDocumentKey = normalized.activeDocumentKey || normalized.activeProfileId || "";
    return normalized;
  }

  function resolveInitialDocument() {
    const activeKey = data.activeDocumentKey || "";
    return (data.documents || []).find(function (document) {
      return document.key === activeKey;
    }) || data.documents[0] || createEmptyDocument();
  }

  function createEmptyDocument(languageKey) {
    const sourceData = typeof data === "undefined" ? {} : data;
    const language = languageKey || (sourceData.languages && sourceData.languages[0] ? sourceData.languages[0].key : "");
    return {
      key: "",
      name: "",
      description: "",
      language,
      bundles: [],
      mappings: []
    };
  }

  function documentFromProfile(profile) {
    const language = profile.language || profile.languageKey || "";
    const key = profile.key || profile.id || (language ? language + "-type-mappings" : "type-mapping-document");
    return {
      key,
      name: profile.name || key,
      description: profile.description || "",
      language,
      bundles: [],
      mappings: (profile.typeMappings || profile.TypeMappings || []).map(function (mapping) {
        return {
          butterMorph: {
            key: mapping.butterMorphType || mapping.ButterMorphType || "",
            version: mapping.butterMorphTypeVersion || mapping.ButterMorphTypeVersion || "",
            reference: butterMorphReference(mapping.butterMorphType || mapping.ButterMorphType || "", mapping.butterMorphTypeVersion || mapping.ButterMorphTypeVersion || "")
          },
          targetLanguage: {
            key: mapping.languageType || mapping.LanguageType || "",
            version: mapping.languageTypeVersion || mapping.LanguageTypeVersion || "",
            reference: mapping.languageType || mapping.LanguageType || ""
          }
        };
      })
    };
  }

  function cloneDocument(document) {
    const clone = JSON.parse(JSON.stringify(document || createEmptyDocument()));
    clone.key = clone.key || "";
    clone.name = clone.name || "";
    clone.description = clone.description || "";
    clone.language = clone.language || clone.languageKey || "";
    clone.bundles = clone.bundles || [];
    clone.mappings = dedupeMappings((clone.mappings || clone.typeMappings || clone.TypeMappings || []).map(normalizeMapping));
    delete clone.typeMappings;
    delete clone.TypeMappings;
    return clone;
  }

  function normalizeMapping(mapping) {
    if (mapping.butterMorph || mapping.targetLanguage) {
      const normalized = {
        butterMorph: normalizeReference(mapping.butterMorph),
        targetLanguage: normalizeReference(mapping.targetLanguage)
      };
      normalized.butterMorph.reference = normalized.butterMorph.reference || butterMorphReference(normalized.butterMorph.key, normalized.butterMorph.version);
      normalized.targetLanguage.reference = normalized.targetLanguage.reference || normalized.targetLanguage.key;
      return normalized;
    }

    return {
      butterMorph: {
        key: mapping.butterMorphType || "",
        version: mapping.butterMorphTypeVersion || "",
        reference: butterMorphReference(mapping.butterMorphType || "", mapping.butterMorphTypeVersion || "")
      },
      targetLanguage: {
        key: mapping.languageType || "",
        version: mapping.languageTypeVersion || "",
        reference: mapping.languageType || ""
      }
    };
  }

  function normalizeReference(reference) {
    const source = reference || {};
    const key = source.key || "";
    const version = source.version || (isBuiltInButterMorphType(key) ? "builtin" : "");
    return {
      key,
      displayName: source.displayName || "",
      version,
      reference: cleanReference(source.reference || ""),
      source: source.source || "",
      bundleKey: source.bundleKey || ""
    };
  }

  function render() {
    activeDocument = enrichDocumentReferences(cloneDocument(activeDocument));
    renderDocumentSummary();
    renderPackageFields();
    renderLanguageSelector();
    renderBundleScope();
    wireControls();
    renderRows();
    renderSummary();
  }

  function renderPackageFields() {
    bindText("[data-type-mapping-key]", activeDocument.key || "", function (value) {
      activeDocument.key = value;
      upsertDocument(activeDocument);
      renderDocumentSummary();
    });
    bindText("[data-type-mapping-name]", activeDocument.name || "", function (value) {
      activeDocument.name = value;
      upsertDocument(activeDocument);
      renderDocumentSummary();
    });
    bindText("[data-type-mapping-description]", activeDocument.description || "", function (value) {
      activeDocument.description = value;
      upsertDocument(activeDocument);
    });
  }

  function renderDocumentSummary() {
    const title = document.querySelector("[data-type-mapping-title]");
    const subtitle = document.querySelector("[data-type-mapping-subtitle]");
    if (title) {
      title.textContent = activeDocument.name || activeDocument.key || "Type mapping package";
    }
    if (subtitle) {
      subtitle.textContent = activeDocument.key || "";
      subtitle.hidden = !subtitle.textContent;
    }
  }

  function bindText(selector, value, onInput) {
    const node = document.querySelector(selector);
    if (!node) {
      return;
    }

    if (node.value !== value) {
      node.value = value;
    }

    if (node.getAttribute("data-wired") === "true") {
      return;
    }

    node.setAttribute("data-wired", "true");
    node.addEventListener("input", function () {
      onInput(node.value);
      showMessage("Unsaved changes.");
    });
  }

  function renderLanguageSelector() {
    const select = document.querySelector("[data-type-mapping-language]");
    if (!select) {
      return;
    }

    select.innerHTML = "";
    (data.languages || []).forEach(function (language) {
      const option = document.createElement("option");
      option.value = language.key;
      option.textContent = language.displayName || language.key;
      option.selected = language.key === activeDocument.language;
      select.appendChild(option);
    });

    if (select.getAttribute("data-wired") !== "true") {
      select.setAttribute("data-wired", "true");
      select.addEventListener("change", function () {
        const previousLanguage = activeDocument.language || "";
        const nextLanguage = select.value;
        if (nextLanguage === previousLanguage) {
          return;
        }

        if (documentHasLanguageContent(activeDocument)) {
          const confirmed = window.confirm("Changing the language will clear all type mappings and bundle references for this document.\n\nThis avoids mixing references from different language adapters.\n\nDo you want to continue?");
          if (!confirmed) {
            select.value = previousLanguage;
            return;
          }
        }

        activeDocument.language = nextLanguage;
        activeDocument.bundles = [];
        activeDocument.mappings = [];
        activeDocument = enrichDocumentReferences(activeDocument);
        upsertDocument(activeDocument);
        renderBundleScope();
        renderRows();
        renderSummary();
        renderDocumentSummary();
        showMessage("Language changed. Mappings were cleared.");
      });
    }
  }

  function renderBundleScope() {
    const summary = document.querySelector("[data-type-mapping-bundle-summary]");
    const host = document.querySelector("[data-type-mapping-bundles]");
    if (!host) {
      return;
    }

    const bundles = resolveLanguageBundles(activeDocument.language);
    host.innerHTML = "";
    if (summary) {
      summary.textContent = bundles.length ? bundles.length + " loaded for " + resolveLanguageName(activeDocument.language) : "No bundles loaded for this language";
    }

    if (!bundles.length) {
      const empty = document.createElement("p");
      empty.className = "bm-type-mapping-empty";
      empty.textContent = "Only primitive language types are available.";
      host.appendChild(empty);
      return;
    }

    bundles.forEach(function (bundle) {
      const item = document.createElement("span");
      item.className = "bm-type-mapping-bundle";
      const types = bundle.types || bundle.Types || [];
      item.textContent = (bundle.assemblyKey || bundle.assemblyId || "Bundle") + " · " + types.length + " types";
      host.appendChild(item);
    });
  }

  function wireControls() {
    const search = document.querySelector("[data-type-mapping-search]");
    if (search && search.getAttribute("data-wired") !== "true") {
      search.setAttribute("data-wired", "true");
      search.addEventListener("input", function () {
        searchQuery = search.value.trim().toLowerCase();
        renderRows();
        renderSummary();
      });
    }

    const add = document.querySelector("[data-type-mapping-add]");
    if (add && add.getAttribute("data-wired") !== "true") {
      add.setAttribute("data-wired", "true");
      add.addEventListener("click", addMapping);
    }

  }

  function renderRows() {
    const list = document.querySelector("[data-type-mapping-list]");
    if (!list) {
      return;
    }

    list.innerHTML = "";
    const mappings = (activeDocument.mappings || []).filter(matchesSearch);
    mappings.forEach(function (mapping, index) {
      list.appendChild(createRow(mapping, index));
    });

    if (!mappings.length) {
      const empty = document.createElement("p");
      empty.className = "bm-type-mapping-empty";
      empty.textContent = searchQuery ? "No mappings match the search." : "No mappings yet. Add one row to start this package.";
      list.appendChild(empty);
    }
  }

  function createRow(mapping, index) {
    const row = document.createElement("div");
    row.className = "bm-type-mapping-row";

    const bmSelect = document.createElement("select");
    bmSelect.setAttribute("aria-label", "ButterMorph type");
    appendButterMorphOptions(bmSelect, mapping);
    bmSelect.addEventListener("change", function () {
      mapping.butterMorph = readReferenceValue(bmSelect.value);
      mapping.butterMorph.reference = mapping.butterMorph.reference || butterMorphReference(mapping.butterMorph.key, mapping.butterMorph.version);
      activeDocument.mappings = dedupeMappings(activeDocument.mappings || []);
      upsertDocument(activeDocument);
      renderRows();
      renderSummary();
      showMessage("Unsaved changes.");
    });
    row.appendChild(createMappingCell(bmSelect));

    const languageSelect = document.createElement("select");
    languageSelect.setAttribute("aria-label", "Language type");
    appendLanguageOptions(languageSelect, mapping);
    languageSelect.addEventListener("change", function () {
      mapping.targetLanguage = readReferenceValue(languageSelect.value);
      enrichTargetReference(mapping, activeDocument.language);
      activeDocument.bundles = resolveUsedBundles(activeDocument);
      activeDocument.mappings = dedupeMappings(activeDocument.mappings || []);
      upsertDocument(activeDocument);
      renderRows();
      renderSummary();
      showMessage("Unsaved changes.");
    });
    row.appendChild(createMappingCell(languageSelect));

    const remove = document.createElement("button");
    remove.type = "button";
    remove.className = "bm-type-mapping-remove";
    remove.textContent = "Remove";
    remove.addEventListener("click", function () {
      const originalIndex = (activeDocument.mappings || []).indexOf(mapping);
      activeDocument.mappings.splice(originalIndex >= 0 ? originalIndex : index, 1);
      activeDocument.bundles = resolveUsedBundles(activeDocument);
      upsertDocument(activeDocument);
      renderRows();
      renderSummary();
      showMessage("Unsaved changes.");
    });
    row.appendChild(remove);

    return row;
  }

  function createMappingCell(node) {
    const wrapper = document.createElement("span");
    wrapper.className = "bm-type-mapping-cell";
    wrapper.appendChild(node);
    return wrapper;
  }

  function appendButterMorphOptions(select, mapping) {
    const placeholder = document.createElement("option");
    placeholder.value = "{}";
    placeholder.textContent = "Select ButterMorph type";
    placeholder.selected = !mapping.butterMorph || !mapping.butterMorph.key;
    placeholder.disabled = true;
    select.appendChild(placeholder);

    (data.butterMorphTypes || []).forEach(function (type) {
      const reference = butterMorphCatalogReference(type);
      const option = document.createElement("option");
      option.value = writeReferenceValue(reference);
      option.textContent = formatButterMorphOptionLabel(type);
      option.title = option.textContent;
      option.selected = sameReference(mapping.butterMorph, reference);
      select.appendChild(option);
    });
  }

  function appendLanguageOptions(select, mapping) {
    const types = resolveLanguageTypes(activeDocument.language);
    let hasSelected = false;
    const placeholder = document.createElement("option");
    placeholder.value = "{}";
    placeholder.textContent = "Select language type";
    placeholder.selected = !mapping.targetLanguage || (!mapping.targetLanguage.reference && !mapping.targetLanguage.key);
    placeholder.disabled = true;
    select.appendChild(placeholder);

    types.forEach(function (type) {
      const reference = languageCatalogReference(type);
      const option = document.createElement("option");
      option.value = writeReferenceValue(reference);
      option.textContent = formatLanguageOptionLabel(type);
      option.title = option.textContent;
      option.selected = sameReference(mapping.targetLanguage, reference);
      hasSelected = hasSelected || option.selected;
      select.appendChild(option);
    });

    if (mapping.targetLanguage && (mapping.targetLanguage.reference || mapping.targetLanguage.key) && !hasSelected) {
      const option = document.createElement("option");
      option.value = writeReferenceValue(mapping.targetLanguage);
      option.textContent = formatReferenceOptionLabel(mapping.targetLanguage);
      option.title = option.textContent;
      option.selected = true;
      select.appendChild(option);
    }
  }

  function formatButterMorphOptionLabel(type) {
    return formatVersionedLabel(type.displayName || type.key || "Unnamed type", type.version || "");
  }

  function formatLanguageOptionLabel(type) {
    const name = type.displayName || type.key || type.qualifiedName || "Unnamed type";
    const version = type.version || (type.source === "primitive" ? "primitive" : "");
    const reference = type.qualifiedName && type.qualifiedName !== name ? type.qualifiedName : "";
    return [formatVersionedLabel(name, version), reference].filter(Boolean).join(" - ");
  }

  function formatReferenceOptionLabel(reference) {
    const value = normalizeReference(reference);
    const name = value.displayName || value.key || value.reference || "Unknown type";
    const resolvedReference = value.reference && value.reference !== name && value.reference !== value.key ? value.reference : "";
    return [formatVersionedLabel(name, value.version || ""), resolvedReference].filter(Boolean).join(" - ");
  }

  function formatVersionedLabel(name, version) {
    return [name, version].filter(Boolean).join(" - ");
  }

  function addMapping() {
    activeDocument.mappings = activeDocument.mappings || [];
    activeDocument.mappings.push({
      butterMorph: normalizeReference({}),
      targetLanguage: normalizeReference({})
    });
    activeDocument.mappings = dedupeMappings(activeDocument.mappings);
    activeDocument.bundles = resolveUsedBundles(activeDocument);
    upsertDocument(activeDocument);
    renderRows();
    renderSummary();
    showMessage("Unsaved changes.");
  }

  function resolveNextButterMorphType() {
    const used = new Set((activeDocument.mappings || []).map(function (mapping) {
      return referenceIdentity(mapping.butterMorph);
    }));
    return (data.butterMorphTypes || []).find(function (type) {
      return !used.has(referenceIdentity(butterMorphCatalogReference(type)));
    }) || (data.butterMorphTypes || [])[0] || {};
  }

  function describeButterMorphType(mapping) {
    if (!mapping.butterMorph || !mapping.butterMorph.key) {
      return "";
    }

    const type = findButterMorphType(mapping.butterMorph) || {};
    const version = mapping.butterMorph.version || type.version || "unversioned";
    const description = type.description || "";
    return [version, description].filter(Boolean).join(" · ");
  }

  function describeLanguageType(mapping) {
    if (!mapping.targetLanguage || (!mapping.targetLanguage.reference && !mapping.targetLanguage.key)) {
      return "";
    }

    const type = findLanguageType(mapping.targetLanguage, activeDocument.language) || {};
    const reference = mapping.targetLanguage.reference || type.qualifiedName || type.key || "";
    const version = mapping.targetLanguage.version || type.version || "unversioned";
    const source = mapping.targetLanguage.source || type.source || "";
    return [reference, version, source].filter(Boolean).join(" · ");
  }

  function renderSummary() {
    const summary = document.querySelector("[data-type-mapping-summary]");
    if (!summary) {
      return;
    }

    const mappings = activeDocument.mappings || [];
    const visible = mappings.filter(matchesSearch).length;
    summary.textContent = searchQuery
      ? visible + " of " + mappings.length + " visible"
      : mappings.length + (mappings.length === 1 ? " mapping" : " mappings");
  }

  function save() {
    activeDocument.key = activeDocument.key || activeDocument.language + "-type-mappings";
    activeDocument.name = activeDocument.name || activeDocument.key;
    activeDocument = enrichDocumentReferences(activeDocument);
    activeDocument.bundles = resolveUsedBundles(activeDocument);
    activeDocument.mappings = dedupeMappings((activeDocument.mappings || []).filter(function (mapping) {
      return mapping.butterMorph && mapping.butterMorph.key && mapping.targetLanguage && (mapping.targetLanguage.reference || mapping.targetLanguage.key);
    }));

    fetch(handlerUrl("SaveDocument"), {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(activeDocument)
    })
      .then(function (response) { return response.json(); })
      .then(function (payload) {
        if (!payload.succeeded) {
          showMessage(payload.message || "Mappings could not be published.");
          return;
        }

        if (payload.document) {
          activeDocument = cloneDocument(payload.document);
          upsertDocument(activeDocument);
          notifyHost(activeDocument);
        }
        showMessage(payload.message || "Mappings sent to host.");
        render();
      })
      .catch(function (error) {
        showMessage(error.message || "Could not save mappings.");
      });
  }

  function resolveLanguageTypes(languageKey) {
    const language = (data.languages || []).find(function (item) {
      return item.key === languageKey;
    });
    return language ? (language.types || []) : [];
  }

  function resolveLanguageBundles(languageKey) {
    return (data.bundles || []).filter(function (bundle) {
      return String(bundle.languageKey || "").toLowerCase() === String(languageKey || "").toLowerCase();
    });
  }

  function resolveLanguageName(languageKey) {
    const language = (data.languages || []).find(function (item) {
      return item.key === languageKey;
    });
    return language ? (language.displayName || language.key) : "language";
  }

  function resolveUsedBundles(document) {
    const languageBundles = resolveLanguageBundles(document.language);
    const byKey = new Map(languageBundles.map(function (bundle) {
      return [String(bundle.assemblyKey || bundle.assemblyId || "").toLowerCase(), bundle];
    }));
    const usedKeys = new Map();
    (document.mappings || []).forEach(function (mapping) {
      const bundleKey = mapping.targetLanguage && mapping.targetLanguage.bundleKey;
      if (bundleKey) {
        usedKeys.set(String(bundleKey).toLowerCase(), bundleKey);
      }
    });

    return Array.from(usedKeys).map(function (entry) {
      const key = entry[0];
      const originalKey = entry[1];
      const bundle = byKey.get(key) || {};
      return {
        key: bundle.assemblyKey || originalKey,
        version: bundle.assemblyVersion || "",
        reference: bundle.assemblyId || bundle.assemblyKey || originalKey
      };
    });
  }

  function documentHasLanguageContent(document) {
    return (document.mappings && document.mappings.length > 0) || (document.bundles && document.bundles.length > 0);
  }

  function matchesSearch(mapping) {
    if (!searchQuery) {
      return true;
    }

    const text = [
      mapping.butterMorph?.key,
      mapping.butterMorph?.displayName,
      mapping.butterMorph?.version,
      mapping.targetLanguage?.key,
      mapping.targetLanguage?.displayName,
      mapping.targetLanguage?.reference,
      mapping.targetLanguage?.version,
      mapping.targetLanguage?.bundleKey
    ].join(" ").toLowerCase();
    return text.indexOf(searchQuery) >= 0;
  }

  function butterMorphCatalogReference(type) {
    return {
      key: type.key || "",
      displayName: type.displayName || "",
      version: type.version || "",
      reference: butterMorphReference(type.key || "", type.version || ""),
      source: "buttermorph",
      bundleKey: ""
    };
  }

  function languageCatalogReference(type) {
    const key = type.key || type.qualifiedName || "";
    const reference = type.qualifiedName || type.reference || key;
    return {
      key,
      displayName: type.displayName || key,
      version: type.version || "",
      reference,
      source: type.source || "primitive",
      bundleKey: type.bundleKey || ""
    };
  }

  function butterMorphReference(key, version) {
    if (!key) {
      return "";
    }

    return version ? "#/dataTypes/" + key + "@" + version : "#/dataTypes/" + key;
  }

  function sameReference(left, right) {
    const leftValue = normalizeReference(left);
    const rightValue = normalizeReference(right);
    const leftReference = (leftValue.reference || "").toLowerCase();
    const rightReference = (rightValue.reference || "").toLowerCase();
    if (leftReference && rightReference && leftReference === rightReference) {
      return true;
    }

    if (String(leftValue.key || "").toLowerCase() !== String(rightValue.key || "").toLowerCase()) {
      return false;
    }

    if (leftValue.version && rightValue.version && String(leftValue.version).toLowerCase() !== String(rightValue.version).toLowerCase()) {
      return false;
    }

    if (leftValue.bundleKey && rightValue.bundleKey && String(leftValue.bundleKey).toLowerCase() !== String(rightValue.bundleKey).toLowerCase()) {
      return false;
    }

    return true;
  }

  function referenceIdentity(reference) {
    const value = normalizeReference(reference);
    return [
      value.reference || value.key,
      value.version,
      value.bundleKey
    ].join("@@").toLowerCase();
  }

  function writeReferenceValue(reference) {
    return JSON.stringify(normalizeReference(reference));
  }

  function readReferenceValue(value) {
    try {
      return normalizeReference(JSON.parse(value || "{}"));
    } catch {
      return normalizeReference({});
    }
  }

  function enrichDocumentReferences(document) {
    document.mappings = dedupeMappings((document.mappings || []).map(function (mapping) {
      enrichButterMorphReference(mapping);
      enrichTargetReference(mapping, document.language);
      return mapping;
    }));
    document.bundles = resolveUsedBundles(document);
    return document;
  }

  function enrichButterMorphReference(mapping) {
    mapping.butterMorph = normalizeReference(mapping.butterMorph);
    const type = findButterMorphType(mapping.butterMorph);
    if (!type) {
      mapping.butterMorph.reference = mapping.butterMorph.reference || butterMorphReference(mapping.butterMorph.key, mapping.butterMorph.version);
      return;
    }

    const catalog = butterMorphCatalogReference(type);
    mapping.butterMorph.key = catalog.key;
    mapping.butterMorph.displayName = catalog.displayName;
    mapping.butterMorph.version = catalog.version;
    mapping.butterMorph.reference = catalog.reference;
    mapping.butterMorph.source = catalog.source;
  }

  function enrichTargetReference(mapping, languageKey) {
    mapping.targetLanguage = normalizeReference(mapping.targetLanguage);
    const type = findLanguageType(mapping.targetLanguage, languageKey);
    if (!type) {
      mapping.targetLanguage.reference = mapping.targetLanguage.reference || mapping.targetLanguage.key;
      return;
    }

    mapping.targetLanguage = languageCatalogReference(type);
  }

  function findButterMorphType(reference) {
    const normalized = normalizeReference(reference);
    return (data.butterMorphTypes || []).find(function (item) {
      return sameReference(normalized, butterMorphCatalogReference(item));
    });
  }

  function findLanguageType(reference, languageKey) {
    const normalized = normalizeReference(reference);
    return resolveLanguageTypes(languageKey).find(function (item) {
      return sameReference(normalized, languageCatalogReference(item));
    });
  }

  function dedupeMappings(mappings) {
    const seen = new Set();
    const result = [];
    (mappings || []).forEach(function (mapping) {
      const butterMorph = normalizeReference(mapping.butterMorph);
      if (!butterMorph.key) {
        mapping.butterMorph = butterMorph;
        mapping.targetLanguage = normalizeReference(mapping.targetLanguage);
        result.push(mapping);
        return;
      }

      const identity = [butterMorph.key, butterMorph.version].join("@@").toLowerCase();
      if (seen.has(identity)) {
        return;
      }

      seen.add(identity);
      mapping.butterMorph = butterMorph;
      mapping.targetLanguage = normalizeReference(mapping.targetLanguage);
      result.push(mapping);
    });
    return result;
  }

  function isBuiltInButterMorphType(key) {
    return ["string", "number", "boolean", "date", "datetime", "time", "timespan", "object"].indexOf(String(key || "").toLowerCase()) >= 0;
  }

  function cleanReference(reference) {
    const value = String(reference || "");
    return value.endsWith("@") ? "" : value;
  }

  function upsertDocument(document) {
    const documents = data.documents || [];
    const key = document.key || "";
    const index = documents.findIndex(function (item) {
      return (item.key || "") === key;
    });
    if (index >= 0) {
      documents[index] = cloneDocument(document);
    } else {
      documents.push(cloneDocument(document));
    }
    data.documents = documents;
    data.activeDocumentKey = key;
  }

  function showMessage(message, hide) {
    const node = document.querySelector("[data-type-mapping-message]");
    if (!node) {
      return;
    }

    node.hidden = hide === true || !message;
    node.textContent = message;
  }

  function handlerUrl(handler) {
    const url = new URL(window.location.href);
    url.searchParams.set("handler", handler);
    return url.toString();
  }

  function notifyHost(document) {
    const message = {
      type: "ButterMorphLanguageTypeMappingsSaved",
      contextKey: document.key || "",
      document
    };

    if (window.parent && window.parent !== window) {
      window.parent.postMessage(message, window.location.origin);
    }

    if (window.opener) {
      window.opener.postMessage(message, window.location.origin);
      window.close();
    }
  }

  const saveButton = document.querySelector("[data-type-mapping-save]");
  if (saveButton) {
    saveButton.addEventListener("click", save);
  }

  render();
}());
