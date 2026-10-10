(function () {
  const dataNode = document.getElementById("document-viewer-data");
  const data = readViewerData(dataNode);
  const body = document.body;
  const root = data.root || { id: "root", title: "Root", children: data.items || [] };
  const nodesById = new Map();
  const parentById = new Map();
  let currentNode = root;
  let dslEditorStarted = false;
  let dslCodeEditor = null;

  function readViewerData(node) {
    if (!node) {
      return { root: { children: [] }, items: [] };
    }

    try {
      return JSON.parse(node.textContent || "{}");
    } catch {
      return { root: { children: [] }, items: [] };
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
    const normalized = view === "dsl" ? "dsl" : "ux";
    body.setAttribute("data-document-active-view", normalized);
    document.querySelectorAll("[data-document-view-button]").forEach(function (button) {
      const isActive = button.getAttribute("data-document-view-button") === normalized;
      button.setAttribute("aria-selected", isActive ? "true" : "false");
    });
    document.querySelectorAll("[data-document-view-panel]").forEach(function (panel) {
      panel.hidden = panel.getAttribute("data-document-view-panel") !== normalized;
    });

    if (normalized === "dsl") {
      initializeDslEditor();
      refreshDslEditor();
    }
  }

  function wireViewSwitch() {
    document.querySelectorAll("[data-document-view-button]").forEach(function (button) {
      button.addEventListener("click", function () {
        setActiveView(button.getAttribute("data-document-view-button"));
      });
    });
  }

  function wireSearch() {
    const search = document.querySelector("[data-document-search]");
    if (!search) {
      return;
    }

    search.addEventListener("input", function () {
      const query = search.value.trim().toLowerCase();
      const results = document.querySelector("[data-document-search-results]");
      const list = document.querySelector("[data-document-node-list]");
      if (!results || !list) {
        return;
      }

      if (!query) {
        results.hidden = true;
        list.hidden = false;
        renderCurrentNode();
        return;
      }

      const matches = (data.items || []).filter(function (node) {
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
    const breadcrumb = document.querySelector("[data-document-breadcrumb]");
    if (!breadcrumb) {
      return;
    }

    breadcrumb.innerHTML = "";
    buildTrail(currentNode).forEach(function (node, index, trail) {
      if (index > 0) {
        const separator = document.createElement("span");
        separator.className = "bm-document-breadcrumb-separator";
        separator.textContent = "/";
        breadcrumb.appendChild(separator);
      }

      if (index === trail.length - 1) {
        const current = document.createElement("span");
        current.className = "bm-document-breadcrumb-current";
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
    return node && node.id === "root" ? "Root" : (node.title || "Item");
  }

  function renderCurrentNode() {
    const list = document.querySelector("[data-document-node-list]");
    if (!list) {
      return;
    }

    list.innerHTML = "";
    const children = currentNode.children || [];
    if (!children.length) {
      list.appendChild(createEmpty("No nested items."));
      return;
    }

    children.forEach(function (node) {
      list.appendChild(createNodeRow(node, false));
    });
  }

  function renderSearchResults(matches, query) {
    const results = document.querySelector("[data-document-search-results]");
    if (!results) {
      return;
    }

    results.innerHTML = "";
    const header = document.createElement("div");
    header.className = "bm-document-search-header";
    header.textContent = matches.length + " result" + (matches.length === 1 ? "" : "s") + " for \"" + query + "\"";
    results.appendChild(header);

    if (!matches.length) {
      results.appendChild(createEmpty("No matching items."));
      return;
    }

    matches.forEach(function (node) {
      results.appendChild(createNodeRow(node, true));
    });
  }

  function createNodeRow(node, isSearchResult) {
    const row = document.createElement("div");
    row.className = "bm-document-node-row";
    row.setAttribute("data-document-kind", node.kind || "");

    const info = document.createElement("button");
    info.type = "button";
    info.className = "bm-document-node-info";
    info.addEventListener("click", function () {
      showDetails(node);
    });

    const main = document.createElement("span");
    main.className = "bm-document-node-main";

    const title = document.createElement("span");
    title.className = "bm-document-node-title";

    const name = document.createElement("strong");
    name.className = "bm-document-node-name";
    name.textContent = node.title || "(unnamed)";
    title.appendChild(name);

    if (node.dataType) {
      const type = document.createElement("span");
      type.className = "bm-document-node-type";
      type.textContent = node.dataType;
      title.appendChild(type);
    }

    if (node.badge) {
      const badge = document.createElement("span");
      badge.className = "bm-document-node-badge";
      badge.textContent = node.badge;
      title.appendChild(badge);
    }

    main.appendChild(title);

    const summary = document.createElement("small");
    summary.textContent = isSearchResult ? (node.path || node.subtitle || "") : (node.subtitle || node.path || "");
    main.appendChild(summary);
    info.appendChild(main);
    row.appendChild(info);

    if ((node.children || []).length > 0) {
      const actions = document.createElement("span");
      actions.className = "bm-document-node-actions";
      const open = document.createElement("button");
      open.type = "button";
      open.className = "bm-document-node-open";
      open.textContent = "Open";
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
    const dialog = document.querySelector("[data-document-detail-dialog]");
    const title = document.querySelector("[data-document-detail-title]");
    const path = document.querySelector("[data-document-detail-path]");
    const bodyNode = document.querySelector("[data-document-detail-body]");
    if (!dialog || !title || !path || !bodyNode) {
      return;
    }

    title.textContent = node.title || "(unnamed)";
    path.textContent = node.path || node.subtitle || "";
    bodyNode.innerHTML = renderDetails(node);
    dialog.hidden = false;
  }

  function renderDetails(node) {
    const entries = node.details || [];
    if (!entries.length) {
      return "<p class='bm-empty'>No additional details.</p>";
    }

    return [
      "<section class='bm-document-detail-section'>",
      "<dl class='bm-document-detail-list'>",
      entries.map(function (entry) {
        return "<div><dt>" + escapeHtml(entry.label || "") + "</dt><dd>" + escapeHtml(entry.value || "-") + "</dd></div>";
      }).join(""),
      "</dl>",
      "</section>"
    ].join("");
  }

  function closeDetails() {
    const dialog = document.querySelector("[data-document-detail-dialog]");
    if (dialog) {
      dialog.hidden = true;
    }
  }

  function wireDetailsDialog() {
    document.querySelectorAll("[data-document-detail-close]").forEach(function (button) {
      button.addEventListener("click", closeDetails);
    });
    document.addEventListener("keydown", function (event) {
      if (event.key === "Escape") {
        closeDetails();
      }
    });
  }

  function clearSearch() {
    const search = document.querySelector("[data-document-search]");
    const results = document.querySelector("[data-document-search-results]");
    const list = document.querySelector("[data-document-node-list]");
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

  function createSearchText(node) {
    return [
      node.title,
      node.path,
      node.dataType,
      node.kind,
      node.subtitle,
      node.expression,
      node.badge,
      (node.details || []).map(function (entry) { return entry.value; }).join(" ")
    ].join(" ").toLowerCase();
  }

  function createEmpty(message) {
    const empty = document.createElement("p");
    empty.className = "bm-empty";
    empty.textContent = message;
    return empty;
  }

  function configureDslMode() {
    if (!window.CodeMirror || window.CodeMirror.modes.buttermorphViewerDsl) {
      return;
    }

    window.CodeMirror.defineMode("buttermorphViewerDsl", function () {
      const keywords = /^(metadata|target|validate|assert|foreach|project|as|when|true|false|null)\b/;
      return {
        token: function (stream) {
          if (stream.eatSpace()) {
            return null;
          }
          if (stream.match(/"(?:[^"\\]|\\.)*"/)) {
            return "string";
          }
          if (stream.match(/\$[A-Za-z_][A-Za-z0-9_]*(?:\[[0-9]+\])?(?:\.[A-Za-z_][A-Za-z0-9_]*(?:\[[0-9]+\])?)*/)) {
            return "variable-2";
          }
          if (stream.match(/[A-Za-z_][A-Za-z0-9_]*(?:\.[A-Za-z_][A-Za-z0-9_]*)+/)) {
            return "variable-3";
          }
          if (stream.match(/[A-Za-z_][A-Za-z0-9_]*(?=\s*:)/)) {
            return "property";
          }
          if (stream.match(/[A-Za-z_][A-Za-z0-9_]*(?=\()/)) {
            return "builtin";
          }
          if (stream.match(keywords)) {
            return "keyword";
          }
          if (stream.match(/[0-9]+(?:\.[0-9]+)?/)) {
            return "number";
          }
          if (stream.match(/=>|[{}[\]():,.]/)) {
            return "operator";
          }
          stream.next();
          return null;
        }
      };
    });
  }

  function initializeDslEditor() {
    const source = document.querySelector("[data-document-dsl-source]");
    if (dslEditorStarted || !source) {
      return;
    }

    dslEditorStarted = true;
    if (!window.CodeMirror) {
      return;
    }

    configureDslMode();
    dslCodeEditor = window.CodeMirror.fromTextArea(source, {
      mode: "buttermorphViewerDsl",
      theme: "buttermorph-document",
      lineNumbers: true,
      indentUnit: 2,
      tabSize: 2,
      lineWrapping: true,
      readOnly: true,
      viewportMargin: Infinity
    });
    refreshDslEditor();
  }

  function refreshDslEditor() {
    if (!dslCodeEditor) {
      return;
    }

    window.requestAnimationFrame(function () {
      dslCodeEditor.refresh();
    });
  }

  function preloadDslEditor() {
    if (!document.querySelector("[data-document-dsl-editor]")) {
      return;
    }

    window.setTimeout(function () {
      initializeDslEditor();
      refreshDslEditor();
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
  setActiveView(body.getAttribute("data-document-active-view"));
  preloadDslEditor();

  window.addEventListener("message", function () {
    refreshDslEditor();
  });
}());
