(function () {
  const dataNode = document.getElementById("schema-viewer-data");
  const data = readViewerData(dataNode);
  const body = document.body;
  const root = data.root || { id: "root", name: data.name || "Schema", kind: "object", dataType: "object", children: data.fields || [] };
  const nodesById = new Map();
  const parentById = new Map();
  let currentNode = root;
  let jsonEditorStarted = false;
  let jsonCodeEditor = null;

  function readViewerData(node) {
    if (!node) {
      return { root: { children: [] }, fields: [] };
    }

    try {
      return JSON.parse(node.textContent || "{}");
    } catch {
      return { root: { children: [] }, fields: [] };
    }
  }

  function indexNode(node, parent) {
    if (!node || !node.id) {
      return;
    }

    nodesById.set(node.id, node);
    if (parent && parent.id) {
      parentById.set(node.id, parent.id);
    }

    (node.children || []).forEach(function (child) {
      indexNode(child, node);
    });
  }

  function setActiveView(view) {
    const normalized = view === "json" ? "json" : "ux";
    body.setAttribute("data-schema-active-view", normalized);
    document.querySelectorAll("[data-schema-view-button]").forEach(function (button) {
      const isActive = button.getAttribute("data-schema-view-button") === normalized;
      button.setAttribute("aria-selected", isActive ? "true" : "false");
    });
    document.querySelectorAll("[data-schema-view-panel]").forEach(function (panel) {
      panel.hidden = panel.getAttribute("data-schema-view-panel") !== normalized;
    });

    if (normalized === "json") {
      initializeJsonEditor();
      refreshJsonEditor();
    }
  }

  function wireViewSwitch() {
    document.querySelectorAll("[data-schema-view-button]").forEach(function (button) {
      button.addEventListener("click", function () {
        setActiveView(button.getAttribute("data-schema-view-button"));
      });
    });
  }

  function wireSearch() {
    const search = document.querySelector("[data-schema-search]");
    if (!search) {
      return;
    }

    search.addEventListener("input", function () {
      const query = search.value.trim().toLowerCase();
      const results = document.querySelector("[data-schema-search-results]");
      const list = document.querySelector("[data-schema-node-list]");
      if (!results || !list) {
        return;
      }

      if (!query) {
        results.hidden = true;
        list.hidden = false;
        renderCurrentNode();
        return;
      }

      const matches = (data.fields || []).filter(function (node) {
        return createSearchText(node).indexOf(query) >= 0;
      });
      list.hidden = true;
      results.hidden = false;
      renderSearchResults(matches, query);
    });
  }

  function setCurrentNode(node) {
    currentNode = node || root;
    renderBreadcrumb();
    renderCurrentNode();
  }

  function buildTrail(node) {
    const trail = [];
    let cursor = node;
    while (cursor) {
      trail.unshift(cursor);
      const parentId = parentById.get(cursor.id);
      cursor = parentId ? nodesById.get(parentId) : null;
    }

    return trail.length ? trail : [root];
  }

  function renderBreadcrumb() {
    const breadcrumb = document.querySelector("[data-schema-breadcrumb]");
    if (!breadcrumb) {
      return;
    }

    breadcrumb.innerHTML = "";
    buildTrail(currentNode).forEach(function (node, index, trail) {
      if (index > 0) {
        const separator = document.createElement("span");
        separator.className = "bm-schema-breadcrumb-separator";
        separator.textContent = "/";
        breadcrumb.appendChild(separator);
      }

      if (index === trail.length - 1) {
        const current = document.createElement("span");
        current.className = "bm-schema-breadcrumb-current";
        current.textContent = formatBreadcrumbName(node);
        breadcrumb.appendChild(current);
        return;
      }

      const button = document.createElement("button");
      button.type = "button";
      button.textContent = formatBreadcrumbName(node);
      button.addEventListener("click", function () {
        setCurrentNode(node);
      });
      breadcrumb.appendChild(button);
    });
  }

  function formatBreadcrumbName(node) {
    return node && node.id === "root" ? "Root" : (node.name || "Schema");
  }

  function renderCurrentNode() {
    const list = document.querySelector("[data-schema-node-list]");
    if (!list) {
      return;
    }

    list.innerHTML = "";
    const children = currentNode.children || [];
    if (!children.length) {
      list.appendChild(createEmpty("No nested properties."));
      return;
    }

    children.forEach(function (node) {
      list.appendChild(createNodeRow(node, false));
    });
  }

  function renderSearchResults(matches, query) {
    const results = document.querySelector("[data-schema-search-results]");
    if (!results) {
      return;
    }

    results.innerHTML = "";
    const header = document.createElement("div");
    header.className = "bm-schema-search-header";
    header.textContent = matches.length + " result" + (matches.length === 1 ? "" : "s") + " for \"" + query + "\"";
    results.appendChild(header);

    if (!matches.length) {
      results.appendChild(createEmpty("No matching properties."));
      return;
    }

    matches.forEach(function (node) {
      results.appendChild(createNodeRow(node, true));
    });
  }

  function createNodeRow(node, isSearchResult) {
    const row = document.createElement("div");
    row.className = "bm-schema-node-row";

    const info = document.createElement("button");
    info.type = "button";
    info.className = "bm-schema-node-info";
    info.addEventListener("click", function () {
      showDetails(node);
    });

    const main = document.createElement("span");
    main.className = "bm-schema-node-main";

    const title = document.createElement("span");
    title.className = "bm-schema-node-title";

    const name = document.createElement("strong");
    name.className = "bm-schema-node-name";
    name.textContent = node.name || "(unnamed)";
    title.appendChild(name);

    if (node.dataType) {
      const type = document.createElement("span");
      type.className = "bm-schema-node-type";
      type.textContent = node.dataType;
      title.appendChild(type);
    }

    main.appendChild(title);

    const path = document.createElement("small");
    path.textContent = isSearchResult ? (node.path || "") : (node.description || node.path || "");
    main.appendChild(path);
    info.appendChild(main);
    row.appendChild(info);

    if ((node.children || []).length > 0) {
      const actions = document.createElement("span");
      actions.className = "bm-schema-node-actions";
      const open = document.createElement("button");
      open.type = "button";
      open.className = "bm-schema-node-open";
      open.textContent = node.kind === "array" ? "Open items" : "Open";
      open.addEventListener("click", function () {
        setCurrentNode(node);
        clearSearch();
      });
      actions.appendChild(open);
      row.appendChild(actions);
    }

    return row;
  }

  function showDetails(node) {
    const dialog = document.querySelector("[data-schema-detail-dialog]");
    const title = document.querySelector("[data-schema-detail-title]");
    const path = document.querySelector("[data-schema-detail-path]");
    const bodyNode = document.querySelector("[data-schema-detail-body]");
    if (!dialog || !title || !path || !bodyNode) {
      return;
    }

    title.textContent = node.name || "(unnamed)";
    path.textContent = node.description || node.path || "";
    bodyNode.innerHTML = [
      renderCoreDetails(node),
      renderEntrySection("Restrictions", node.restrictions || []),
      renderEntrySection("Metadata", node.metadata || [])
    ].join("");
    dialog.hidden = false;
  }

  function renderCoreDetails(node) {
    const entries = [
      { label: "Type", value: node.dataType || node.kind || "" },
      { label: "Required", value: node.isRequired ? "Yes" : "No" },
      { label: "Kind", value: node.kind || "" }
    ];

    if (node.reference) {
      entries.push({ label: "Reference", value: node.reference });
    }

    return [
      "<section class='bm-schema-detail-summary'>",
      entries.map(function (entry) {
        return [
          "<article>",
          "<span>" + escapeHtml(entry.label) + "</span>",
          "<strong>" + escapeHtml(entry.value || "-") + "</strong>",
          "</article>"
        ].join("");
      }).join(""),
      "</section>"
    ].join("");
  }

  function closeDetails() {
    const dialog = document.querySelector("[data-schema-detail-dialog]");
    if (dialog) {
      dialog.hidden = true;
    }
  }

  function wireDetailsDialog() {
    document.querySelectorAll("[data-schema-detail-close]").forEach(function (button) {
      button.addEventListener("click", closeDetails);
    });
    document.addEventListener("keydown", function (event) {
      if (event.key === "Escape") {
        closeDetails();
      }
    });
  }

  function clearSearch() {
    const search = document.querySelector("[data-schema-search]");
    const results = document.querySelector("[data-schema-search-results]");
    const list = document.querySelector("[data-schema-node-list]");
    if (search) {
      search.value = "";
    }
    if (results) {
      results.hidden = true;
    }
    if (list) {
      list.hidden = false;
    }
  }

  function renderEntrySection(title, entries) {
    if (!entries.length) {
      return "";
    }

    return [
      "<section class='bm-schema-detail-section'>",
      "<h3>" + escapeHtml(title) + "</h3>",
      "<dl class='bm-schema-detail-list'>",
      entries.map(function (entry) {
        return "<div><dt>" + escapeHtml(entry.label || entry.key || "") + "</dt><dd>" + escapeHtml(entry.value || "") + "</dd></div>";
      }).join(""),
      "</dl>",
      "</section>"
    ].join("");
  }

  function createSearchText(node) {
    return [
      node.name,
      node.path,
      node.dataType,
      node.kind,
      node.description,
      (node.badges || []).join(" "),
      (node.metadata || []).map(function (entry) { return entry.value; }).join(" ")
    ].join(" ").toLowerCase();
  }

  function createEmpty(message) {
    const empty = document.createElement("p");
    empty.className = "bm-empty";
    empty.textContent = message;
    return empty;
  }

  function configureJsonMode() {
    if (!window.CodeMirror || window.CodeMirror.modes.buttermorphJson) {
      return;
    }

    window.CodeMirror.defineMode("buttermorphJson", function () {
      function readString(stream) {
        let escaped = false;
        let foundEnd = false;
        stream.next();

        while (!stream.eol()) {
          const character = stream.next();
          if (character === "\"" && !escaped) {
            foundEnd = true;
            break;
          }
          escaped = character === "\\" && !escaped;
          if (character !== "\\") {
            escaped = false;
          }
        }

        if (!foundEnd) {
          return "string";
        }

        const next = stream.string.slice(stream.pos).match(/^\s*:/);
        return next ? "property" : "string";
      }

      return {
        token: function (stream) {
          if (stream.eatSpace()) {
            return null;
          }

          const character = stream.peek();
          if (character === "\"") {
            return readString(stream);
          }

          if (/[{}\[\]]/.test(character)) {
            stream.next();
            return "bracket";
          }

          if (/[:,]/.test(character)) {
            stream.next();
            return "operator";
          }

          if (character === "-" || /\d/.test(character)) {
            stream.eat("-");
            stream.eatWhile(/\d/);
            if (stream.eat(".")) {
              stream.eatWhile(/\d/);
            }
            if (stream.eat(/[eE]/)) {
              stream.eat(/[+\-]/);
              stream.eatWhile(/\d/);
            }
            return "number";
          }

          if (/[A-Za-z]/.test(character)) {
            stream.eatWhile(/[A-Za-z]/);
            return "atom";
          }

          stream.next();
          return null;
        }
      };
    });
  }

  function initializeJsonEditor() {
    const host = document.querySelector("[data-schema-json-editor]");
    const source = document.querySelector("[data-schema-json-source]");
    if (jsonEditorStarted || !host || !source) {
      return;
    }

    jsonEditorStarted = true;
    if (!window.CodeMirror) {
      return;
    }

    configureJsonMode();
    jsonCodeEditor = window.CodeMirror.fromTextArea(source, {
      mode: "buttermorphJson",
      theme: "buttermorph-json",
      lineNumbers: true,
      indentUnit: 2,
      tabSize: 2,
      lineWrapping: false,
      readOnly: true,
      viewportMargin: Infinity
    });
    refreshJsonEditor();
  }

  function refreshJsonEditor() {
    if (!jsonCodeEditor) {
      return;
    }

    window.requestAnimationFrame(function () {
      jsonCodeEditor.refresh();
    });
  }

  function preloadJsonEditor() {
    if (!document.querySelector("[data-schema-json-editor]")) {
      return;
    }

    window.setTimeout(function () {
      initializeJsonEditor();
      refreshJsonEditor();
    }, 0);
  }

  function escapeHtml(value) {
    return String(value).replace(/[&<>"']/g, function (character) {
      return ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", "\"": "&quot;", "'": "&#39;" })[character];
    });
  }

  indexNode(root, null);
  wireViewSwitch();
  wireSearch();
  wireDetailsDialog();
  setCurrentNode(root);
  setActiveView(body.getAttribute("data-schema-active-view"));
  preloadJsonEditor();

  window.addEventListener("message", function () {
    refreshJsonEditor();
  });
}());
