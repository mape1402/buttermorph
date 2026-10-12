(function () {
  const dataNode = document.getElementById("language-type-mapping-data");
  const data = readData(dataNode);
  let activeProfile = cloneProfile(resolveInitialProfile());
  let searchQuery = "";

  function readData(node) {
    if (!node) {
      return { languages: [], butterMorphTypes: [], profiles: [] };
    }

    try {
      return JSON.parse(node.textContent || "{}");
    } catch {
      return { languages: [], butterMorphTypes: [], profiles: [] };
    }
  }

  function resolveInitialProfile() {
    const profiles = data.profiles || [];
    const activeId = data.activeProfileId || "";
    return profiles.find(function (profile) {
      return profile.id === activeId || profile.key === activeId;
    }) || profiles[0] || createEmptyProfile();
  }

  function createEmptyProfile(languageKey) {
    const language = languageKey || (data.languages && data.languages[0] ? data.languages[0].key : "");
    const key = language ? language + "-type-mappings" : "type-mapping-package";
    return {
      id: key,
      key,
      name: "New type mapping package",
      description: "",
      language,
      languageKey: language,
      collectionType: "List",
      typeMappings: [],
      memberMappings: []
    };
  }

  function cloneProfile(profile) {
    const clone = JSON.parse(JSON.stringify(profile || createEmptyProfile()));
    clone.key = clone.key || clone.id || "";
    clone.id = clone.id || clone.key || "";
    clone.language = clone.language || clone.languageKey || "";
    clone.languageKey = clone.languageKey || clone.language || "";
    clone.typeMappings = clone.typeMappings || clone.TypeMappings || [];
    clone.memberMappings = clone.memberMappings || [];
    return clone;
  }

  function render() {
    activeProfile = cloneProfile(activeProfile);
    renderPackageSelector();
    renderPackageFields();
    renderLanguageSelector();
    wireControls();
    renderRows();
    renderSummary();
  }

  function renderPackageSelector() {
    const select = document.querySelector("[data-type-mapping-package]");
    if (!select) {
      return;
    }

    const profiles = data.profiles || [];
    select.innerHTML = "";
    profiles.forEach(function (profile) {
      const option = document.createElement("option");
      option.value = profile.key || profile.id || "";
      option.textContent = profile.name || profile.key || profile.id || "Package";
      option.selected = option.value === (activeProfile.key || activeProfile.id);
      select.appendChild(option);
    });

    if (!profiles.length) {
      const option = document.createElement("option");
      option.value = activeProfile.key || activeProfile.id || "";
      option.textContent = activeProfile.name || "New package";
      option.selected = true;
      select.appendChild(option);
    }

    if (select.getAttribute("data-wired") !== "true") {
      select.setAttribute("data-wired", "true");
      select.addEventListener("change", function () {
        const nextProfile = (data.profiles || []).find(function (profile) {
          return (profile.key || profile.id || "") === select.value;
        });
        activeProfile = cloneProfile(nextProfile || activeProfile);
        render();
        showMessage("", true);
      });
    }
  }

  function renderPackageFields() {
    bindText("[data-type-mapping-key]", activeProfile.key || activeProfile.id || "", function (value) {
      activeProfile.key = value;
      activeProfile.id = value;
      upsertProfile(activeProfile);
      renderPackageSelector();
    });
    bindText("[data-type-mapping-name]", activeProfile.name || "", function (value) {
      activeProfile.name = value;
      upsertProfile(activeProfile);
      renderPackageSelector();
    });
    bindText("[data-type-mapping-description]", activeProfile.description || "", function (value) {
      activeProfile.description = value;
    });
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
      option.selected = language.key === resolveProfileLanguage(activeProfile);
      select.appendChild(option);
    });

    if (select.getAttribute("data-wired") !== "true") {
      select.setAttribute("data-wired", "true");
      select.addEventListener("change", function () {
        activeProfile.language = select.value;
        activeProfile.languageKey = select.value;
        renderRows();
        renderSummary();
        showMessage("Unsaved changes.");
      });
    }
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

    const create = document.querySelector("[data-type-mapping-new]");
    if (create && create.getAttribute("data-wired") !== "true") {
      create.setAttribute("data-wired", "true");
      create.addEventListener("click", function () {
        activeProfile = createEmptyProfile(resolveProfileLanguage(activeProfile));
        activeProfile.key = activeProfile.language + "-type-mappings-" + Date.now();
        activeProfile.id = activeProfile.key;
        activeProfile.name = "New " + resolveLanguageName(activeProfile.language) + " package";
        upsertProfile(activeProfile);
        render();
        showMessage("New package ready.");
      });
    }
  }

  function renderRows() {
    const list = document.querySelector("[data-type-mapping-list]");
    if (!list) {
      return;
    }

    list.innerHTML = "";
    const mappings = (activeProfile.typeMappings || []).filter(matchesSearch);
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
      const selected = readCompositeValue(bmSelect.value);
      mapping.butterMorphType = selected.key;
      mapping.butterMorphTypeVersion = selected.version;
      renderSummary();
      showMessage("Unsaved changes.");
    });
    row.appendChild(wrapEditor(bmSelect));

    const bmVersion = document.createElement("input");
    bmVersion.value = mapping.butterMorphTypeVersion || "";
    bmVersion.setAttribute("aria-label", "ButterMorph type version");
    bmVersion.addEventListener("input", function () {
      mapping.butterMorphTypeVersion = bmVersion.value;
      renderSummary();
      showMessage("Unsaved changes.");
    });
    row.appendChild(wrapEditor(bmVersion));

    const languageSelect = document.createElement("select");
    languageSelect.setAttribute("aria-label", "Language type");
    appendLanguageOptions(languageSelect, mapping);
    languageSelect.addEventListener("change", function () {
      const selected = readCompositeValue(languageSelect.value);
      mapping.languageType = selected.key;
      mapping.languageTypeVersion = selected.version;
      renderSummary();
      showMessage("Unsaved changes.");
    });
    row.appendChild(wrapEditor(languageSelect));

    const languageVersion = document.createElement("input");
    languageVersion.value = mapping.languageTypeVersion || "";
    languageVersion.setAttribute("aria-label", "Language type version");
    languageVersion.addEventListener("input", function () {
      mapping.languageTypeVersion = languageVersion.value;
      renderSummary();
      showMessage("Unsaved changes.");
    });
    row.appendChild(wrapEditor(languageVersion));

    const remove = document.createElement("button");
    remove.type = "button";
    remove.className = "bm-type-mapping-remove";
    remove.textContent = "Remove";
    remove.addEventListener("click", function () {
      const originalIndex = (activeProfile.typeMappings || []).indexOf(mapping);
      activeProfile.typeMappings.splice(originalIndex >= 0 ? originalIndex : index, 1);
      renderRows();
      renderSummary();
      showMessage("Unsaved changes.");
    });
    row.appendChild(remove);

    return row;
  }

  function wrapEditor(node) {
    const wrapper = document.createElement("span");
    wrapper.className = "bm-type-mapping-editor";
    wrapper.appendChild(node);
    return wrapper;
  }

  function appendButterMorphOptions(select, mapping) {
    (data.butterMorphTypes || []).forEach(function (type) {
      const option = document.createElement("option");
      option.value = compositeValue(type.key, type.version);
      option.textContent = (type.displayName || type.key) + " - " + (type.version || "unversioned");
      option.selected = sameType(mapping.butterMorphType, mapping.butterMorphTypeVersion, type.key, type.version);
      select.appendChild(option);
    });
  }

  function appendLanguageOptions(select, mapping) {
    const types = resolveLanguageTypes(resolveProfileLanguage(activeProfile));
    let hasSelected = false;
    types.forEach(function (type) {
      const key = type.qualifiedName || type.key;
      const version = type.version || "";
      const option = document.createElement("option");
      option.value = compositeValue(key, version);
      option.textContent = (type.displayName || key) + " - " + key + " - " + (version || "unversioned");
      option.selected = sameType(mapping.languageType, mapping.languageTypeVersion, key, version);
      hasSelected = hasSelected || option.selected;
      select.appendChild(option);
    });

    if (mapping.languageType && !hasSelected) {
      const option = document.createElement("option");
      option.value = compositeValue(mapping.languageType, mapping.languageTypeVersion);
      option.textContent = mapping.languageType + " - " + (mapping.languageTypeVersion || "unversioned");
      option.selected = true;
      select.appendChild(option);
    }
  }

  function addMapping() {
    const bmType = (data.butterMorphTypes || [])[0] || { key: "", version: "" };
    const languageType = resolveLanguageTypes(resolveProfileLanguage(activeProfile))[0] || { key: "", qualifiedName: "", version: "" };
    activeProfile.typeMappings = activeProfile.typeMappings || [];
    activeProfile.typeMappings.push({
      butterMorphType: bmType.key || "",
      butterMorphTypeVersion: bmType.version || "",
      languageType: languageType.qualifiedName || languageType.key || "",
      languageTypeVersion: languageType.version || ""
    });
    renderRows();
    renderSummary();
    showMessage("Unsaved changes.");
  }

  function renderSummary() {
    const summary = document.querySelector("[data-type-mapping-summary]");
    if (!summary) {
      return;
    }

    const mappings = activeProfile.typeMappings || [];
    const visible = mappings.filter(matchesSearch).length;
    summary.textContent = searchQuery
      ? visible + " of " + mappings.length + " visible"
      : mappings.length + (mappings.length === 1 ? " mapping" : " mappings");
  }

  function save() {
    activeProfile.id = activeProfile.key || activeProfile.id || "";
    activeProfile.key = activeProfile.key || activeProfile.id || "";
    activeProfile.language = resolveProfileLanguage(activeProfile);
    activeProfile.languageKey = activeProfile.language;
    activeProfile.typeMappings = (activeProfile.typeMappings || []).filter(function (mapping) {
      return mapping.butterMorphType && mapping.languageType;
    });
    activeProfile.memberMappings = [];

    fetch(handlerUrl("SaveProfile"), {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(activeProfile)
    })
      .then(function (response) { return response.json(); })
      .then(function (payload) {
        if (payload.profile) {
          activeProfile = cloneProfile(payload.profile);
          upsertProfile(activeProfile);
          notifyHost(activeProfile.id || activeProfile.key || activeProfile.language || "");
        }
        showMessage(payload.message || "Mappings saved.");
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

  function resolveLanguageName(languageKey) {
    const language = (data.languages || []).find(function (item) {
      return item.key === languageKey;
    });
    return language ? (language.displayName || language.key) : "language";
  }

  function resolveProfileLanguage(profile) {
    return profile.language || profile.languageKey || "";
  }

  function matchesSearch(mapping) {
    if (!searchQuery) {
      return true;
    }

    const text = [
      mapping.butterMorphType,
      mapping.butterMorphTypeVersion,
      mapping.languageType,
      mapping.languageTypeVersion
    ].join(" ").toLowerCase();
    return text.indexOf(searchQuery) >= 0;
  }

  function sameType(leftKey, leftVersion, rightKey, rightVersion) {
    return String(leftKey || "").toLowerCase() === String(rightKey || "").toLowerCase() &&
      String(leftVersion || "").toLowerCase() === String(rightVersion || "").toLowerCase();
  }

  function compositeValue(key, version) {
    return encodeURIComponent(key || "") + "@@" + encodeURIComponent(version || "");
  }

  function readCompositeValue(value) {
    const parts = String(value || "").split("@@");
    return {
      key: decodeURIComponent(parts[0] || ""),
      version: decodeURIComponent(parts[1] || "")
    };
  }

  function upsertProfile(profile) {
    const profiles = data.profiles || [];
    const key = profile.key || profile.id || "";
    const index = profiles.findIndex(function (item) {
      return (item.key || item.id || "") === key;
    });
    if (index >= 0) {
      profiles[index] = profile;
    } else {
      profiles.push(profile);
    }
    data.profiles = profiles;
    data.activeProfileId = key;
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

  function notifyHost(contextKey) {
    if (!window.parent || window.parent === window) {
      return;
    }

    window.parent.postMessage({
      type: "ButterMorphLanguageTypeMappingsSaved",
      contextKey
    }, window.location.origin);
  }

  const saveButton = document.querySelector("[data-type-mapping-save]");
  if (saveButton) {
    saveButton.addEventListener("click", save);
  }

  render();
}());
