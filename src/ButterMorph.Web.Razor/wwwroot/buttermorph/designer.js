window.ButterMorphDesigner = {
  version: "0.3.0"
};
document.addEventListener("DOMContentLoaded", function () {
  let activeExpressionInput = null;
  let visualTimer = 0;
  let dslTimer = 0;
  let dslSelectionStart = 0;
  let dslSelectionEnd = 0;
  let dslCodeEditor = null;
  let dslDiagnosticMarkers = [];
  let dslDiagnosticLineClasses = [];
  let functionTooltip = null;
  const workbench = document.querySelector(".bm-workbench");
  const dslEditor = document.querySelector("[data-dsl-editor='true']");
  const leftDock = document.querySelector("[data-left-dock='true']");
  const leftDockModeKey = "ButterMorphDesigner.LeftDockMode";
  const leftDockPanelKey = "ButterMorphDesigner.LeftDockPanel";
  const legacyToolboxModeKey = "ButterMorphDesigner.ToolboxMode";
  function configureDslMode() {
    if (!window.CodeMirror || window.CodeMirror.modes.buttermorphDsl) {
      return;
    }
    window.CodeMirror.defineMode("buttermorphDsl", function () {
      const keywords = /^(metadata|target|validate|project|as|when|true|false|null)\b/;
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
  function createCompletionElement(title, description, badge) {
    const wrapper = document.createElement("div");
    wrapper.className = "bm-dsl-hint";
    wrapper.title = description;
    const name = document.createElement("span");
    name.className = "bm-dsl-hint-name";
    name.textContent = title;
    const kind = document.createElement("span");
    kind.className = "bm-dsl-hint-kind";
    kind.textContent = badge;
    wrapper.appendChild(name);
    wrapper.appendChild(kind);
    if (description.length > 0) {
      const text = document.createElement("span");
      text.className = "bm-dsl-hint-description";
      text.textContent = description;
      wrapper.appendChild(text);
    }
    return wrapper;
  }
  function createFunctionSuggestions() {
    const suggestions = [];
    document.querySelectorAll(".bm-function-item").forEach(function (item) {
      const key = item.querySelector(".bm-function-key");
      const kind = item.querySelector(".bm-function-kind");
      const template = item.getAttribute("data-function-template") || "";
      const description = item.getAttribute("title") || "";
      if (key && template.length > 0) {
        const functionKey = key.textContent;
        suggestions.push({
          text: template,
          displayText: functionKey,
          className: "bm-dsl-function-hint",
          description: description,
          isFunction: true,
          category: "function",
          key: functionKey,
          render: function (element) {
            element.appendChild(createCompletionElement(functionKey, description, kind ? kind.textContent : "Function"));
          }
        });
      }
    });
    return suggestions;
  }
  function createSourceSuggestions() {
    const suggestions = [];
    document.querySelectorAll(".bm-source-field, .bm-source-branch[data-path]").forEach(function (item) {
      const path = item.getAttribute("data-path") || "";
      const name = item.querySelector(".bm-node-name");
      const meta = item.querySelector(".bm-node-meta");
      const schemaKind = item.getAttribute("data-kind") || "";
      const dataType = item.getAttribute("data-data-type") || "";
      if (path.length > 0) {
        suggestions.push({
          text: path,
          displayText: path,
          className: "bm-dsl-source-hint",
          description: path,
          category: "source",
          schemaKind: schemaKind,
          dataType: dataType,
          render: function (element) {
            element.appendChild(createCompletionElement(name ? name.textContent : path, path, meta ? meta.textContent : "Source"));
          }
        });
      }
    });
    return suggestions;
  }
  function createTargetSuggestions() {
    const suggestions = [];
    document.querySelectorAll(".bm-expression-input[data-target-path]").forEach(function (input) {
      const targetPath = input.getAttribute("data-target-path") || "";
      if (targetPath.length === 0 || targetPath.indexOf("::projection::") >= 0) {
        return;
      }
      suggestions.push({
        text: targetPath,
        displayText: targetPath,
        className: "bm-dsl-target-hint",
        description: "Target path",
        category: "target",
        render: function (element) {
          element.appendChild(createCompletionElement(targetPath, "Target path", "Target"));
        }
      });
    });
    return suggestions;
  }
  function singularizeName(value) {
    const text = value || "item";
    if (text.length > 3 && text.toLowerCase().lastIndexOf("ies") === text.length - 3) {
      return text.substring(0, text.length - 3) + "y";
    }
    if (text.length > 1 && text.toLowerCase().lastIndexOf("s") === text.length - 1) {
      return text.substring(0, text.length - 1);
    }
    return text;
  }
  function toAliasName(path) {
    const cleanPath = path.replace(/\[[0-9]+\]/g, "");
    const parts = cleanPath.split(".");
    const last = parts.length > 0 ? parts[parts.length - 1] : "item";
    return singularizeName(last).replace(/[^A-Za-z0-9_]/g, "") || "item";
  }
  function collectArrayItemFields(arrayPath, alias) {
    const fields = [];
    document.querySelectorAll(".bm-source-field[data-path]").forEach(function (item) {
      const path = item.getAttribute("data-path") || "";
      const itemPrefix = arrayPath + ".$item.";
      const indexedPrefix = arrayPath + "[0].";
      let fieldPath = "";
      if (path.indexOf(itemPrefix) === 0) {
        fieldPath = path.substring(itemPrefix.length);
      } else if (path.indexOf(indexedPrefix) === 0) {
        fieldPath = path.substring(indexedPrefix.length);
      }
      if (fieldPath.length === 0 || fieldPath.indexOf(".") >= 0) {
        return;
      }
      fields.push({
        name: fieldPath,
        expression: alias + "." + fieldPath
      });
    });
    return fields;
  }
  function createProjectBody(fields, alias) {
    if (fields.length === 0) {
      return alias;
    }
    const parts = [];
    fields.forEach(function (field) {
      parts.push(field.name + ": " + field.expression);
    });
    return "{ " + parts.join(", ") + " }";
  }
  function createProjectSuggestions() {
    const suggestions = [];
    createSourceSuggestions().forEach(function (source) {
      if (source.schemaKind !== "Array") {
        return;
      }
      const alias = toAliasName(source.text);
      const fields = collectArrayItemFields(source.text, alias);
      const snippet = "project " + source.text + " as " + alias + " => " + createProjectBody(fields, alias);
      suggestions.push({
        text: snippet,
        displayText: "project " + source.text,
        className: "bm-dsl-project-hint",
        description: "Projects " + source.text + " as " + alias + ".",
        category: "project",
        render: function (element) {
          element.appendChild(createCompletionElement("project " + source.text, "Projects " + source.text + " as " + alias + ".", "Project"));
        }
      });
    });
    suggestions.push({
      text: "project source as item => item",
      displayText: "project",
      description: "Projects a collection using an item alias.",
      category: "project"
    });
    return suggestions;
  }
  function createAliasSuggestions(editor) {
    const cursor = editor.getCursor();
    const line = editor.getLine(cursor.line);
    const match = line.match(/\bproject\s+[$A-Za-z0-9_.\[\]]+\s+as\s+([A-Za-z_][A-Za-z0-9_]*)\s*=>/);
    const suggestions = [];
    if (!match) {
      return suggestions;
    }
    const alias = match[1];
    suggestions.push({
      text: alias,
      displayText: alias,
      description: "Projection alias",
      category: "alias"
    });
    document.querySelectorAll(".bm-expression-input").forEach(function (input) {
      const value = input.value || "";
      if (value.indexOf(alias + ".") !== 0) {
        return;
      }
      suggestions.push({
        text: value,
        displayText: value,
        description: "Alias path",
        category: "alias"
      });
    });
    return suggestions;
  }
  function createKeywordSuggestions() {
    return [
      { text: "target {\n  \n}", displayText: "target block", description: "Creates target mappings." },
      { text: "validate {\n  \n}", displayText: "validate block", description: "Creates validation rules." },
      { text: "metadata {\n  key: \"value\"\n}", displayText: "metadata block", description: "Creates document metadata." },
      { text: "when(condition, thenExpression, elseExpression)", displayText: "when", description: "Creates a conditional expression.", isFunction: true },
      { text: "true", displayText: "true", description: "Boolean literal." },
      { text: "false", displayText: "false", description: "Boolean literal." },
      { text: "null", displayText: "null", description: "Null literal." }
    ];
  }
  function getCompletionPrefix(editor) {
    const cursor = editor.getCursor();
    const line = editor.getLine(cursor.line);
    const beforeCursor = line.substring(0, cursor.ch);
    const match = beforeCursor.match(/[$A-Za-z_][A-Za-z0-9_.$\[\]]*$/);
    if (match) {
      return match[0];
    }
    return "";
  }
  function getDslCompletionContext(editor, prefix) {
    const cursor = editor.getCursor();
    const line = editor.getLine(cursor.line);
    const beforeCursor = line.substring(0, cursor.ch);
    const fullTextBeforeCursor = editor.getRange(window.CodeMirror.Pos(0, 0), cursor);
    const metadataIndex = fullTextBeforeCursor.lastIndexOf("metadata");
    const targetIndex = fullTextBeforeCursor.lastIndexOf("target");
    const validateIndex = fullTextBeforeCursor.lastIndexOf("validate");
    if (prefix.indexOf("$") === 0) {
      return "source-path";
    }
    if (beforeCursor.indexOf("=>") >= 0) {
      return "projection-body";
    }
    if (validateIndex > metadataIndex && validateIndex > targetIndex) {
      return beforeCursor.indexOf(":") >= 0 ? "validation-expression" : "target-path";
    }
    if (metadataIndex > targetIndex && metadataIndex > validateIndex) {
      return "metadata";
    }
    if (beforeCursor.indexOf(":") >= 0) {
      return "expression";
    }
    return "general";
  }
  function getSuggestionsForContext(editor, context) {
    if (context === "source-path") {
      return createSourceSuggestions();
    }
    if (context === "target-path") {
      return createTargetSuggestions();
    }
    if (context === "metadata") {
      return [
        { text: "key: \"value\"", displayText: "metadata entry", description: "Adds document metadata." }
      ];
    }
    if (context === "projection-body") {
      return createAliasSuggestions(editor).concat(createFunctionSuggestions()).concat(createKeywordSuggestions());
    }
    if (context === "validation-expression") {
      return createFunctionSuggestions().concat(createKeywordSuggestions());
    }
    return createKeywordSuggestions()
      .concat(createProjectSuggestions())
      .concat(createFunctionSuggestions())
      .concat(createSourceSuggestions());
  }
  function createDslHintProvider(editor) {
    const prefix = getCompletionPrefix(editor);
    const lowerPrefix = prefix.toLowerCase();
    const cursor = editor.getCursor();
    const from = window.CodeMirror.Pos(cursor.line, cursor.ch - prefix.length);
    const context = getDslCompletionContext(editor, prefix);
    const suggestions = getSuggestionsForContext(editor, context);
    const filtered = [];
    suggestions.forEach(function (suggestion) {
      const displayText = suggestion.displayText || suggestion.text;
      if (lowerPrefix.length === 0 || displayText.toLowerCase().indexOf(lowerPrefix) >= 0 || suggestion.text.toLowerCase().indexOf(lowerPrefix) >= 0) {
        if (!suggestion.render) {
          suggestion.render = function (element) {
            element.appendChild(createCompletionElement(displayText, suggestion.description || "", "DSL"));
          };
        }
        filtered.push(suggestion);
      }
    });
    return {
      list: filtered,
      from: from,
      to: cursor
    };
  }
  function initializeDslCodeEditor() {
    if (!dslEditor || !window.CodeMirror) {
      return;
    }
    configureDslMode();
    dslCodeEditor = window.CodeMirror.fromTextArea(dslEditor, {
      mode: "buttermorphDsl",
      lineNumbers: true,
      indentUnit: 2,
      tabSize: 2,
      lineWrapping: true,
      extraKeys: {
        "Ctrl-Space": "autocomplete",
        "Alt-Space": "autocomplete"
      },
      hintOptions: {
        hint: createDslHintProvider,
        completeSingle: false
      }
    });
    dslCodeEditor.on("change", function (editor, change) {
      dslEditor.value = editor.getValue();
      rememberDslSelection();
      if (change.origin !== "setValue") {
        scheduleDslSync();
      }
      if (change.origin === "+input") {
        const inserted = change.text.join("");
        if (/[$A-Za-z_.]/.test(inserted)) {
          editor.showHint({ completeSingle: false });
        }
      }
    });
    dslCodeEditor.on("cursorActivity", rememberDslSelection);
    dslCodeEditor.on("focus", rememberDslSelection);
    dslCodeEditor.getWrapperElement().addEventListener("mousemove", handleDslFunctionHover);
    dslCodeEditor.getWrapperElement().addEventListener("mouseleave", hideFunctionTooltip);
    window.CodeMirror.on(dslCodeEditor, "endCompletion", rememberDslSelection);
    refreshDslEditor();
  }
  function getToken() {
    const token = document.querySelector("input[name='__RequestVerificationToken']");
    return token ? token.value : "";
  }
  function queryMarker() {
    return String.fromCharCode(63);
  }
  function createHandlerUrl(handler) {
    const parameters = new URLSearchParams(window.location.search);
    parameters.set("handler", handler);
    return window.location.pathname + queryMarker() + parameters.toString();
  }
  function setLeftDockMode(mode) {
    const normalizedMode = mode === "auto" ? "auto" : "pinned";
    if (workbench) {
      workbench.setAttribute("data-left-dock-mode", normalizedMode);
    }
    document.querySelectorAll("[data-dock-pin]").forEach(function (pinButton) {
      const isPinned = normalizedMode === "pinned";
      pinButton.setAttribute("aria-pressed", isPinned ? "true" : "false");
      pinButton.setAttribute("title", isPinned ? "Auto hide panel" : "Pin panel");
      pinButton.setAttribute("aria-label", isPinned ? "Auto hide panel" : "Pin panel");
      pinButton.setAttribute("data-pin-state", isPinned ? "pinned" : "auto");
    });
    closeDockFlyout();
    window.localStorage.setItem(leftDockModeKey, normalizedMode);
  }
  function loadLeftDockMode() {
    const savedMode = window.localStorage.getItem(leftDockModeKey)
      || window.localStorage.getItem(legacyToolboxModeKey);
    setLeftDockMode(savedMode);
  }
  function setActiveDockPanel(panelName) {
    const normalizedPanel = panelName || "sources";
    document.querySelectorAll("[data-dock-tab]").forEach(function (tab) {
      const isActive = tab.getAttribute("data-dock-tab") === normalizedPanel;
      tab.setAttribute("aria-selected", isActive ? "true" : "false");
    });
    document.querySelectorAll("[data-dock-panel]").forEach(function (panel) {
      const isActive = panel.getAttribute("data-dock-panel") === normalizedPanel;
      if (isActive) {
        panel.removeAttribute("hidden");
        return;
      }
      panel.setAttribute("hidden", "hidden");
    });
    window.localStorage.setItem(leftDockPanelKey, normalizedPanel);
  }
  function loadActiveDockPanel() {
    const savedPanel = window.localStorage.getItem(leftDockPanelKey) || "sources";
    const panel = document.querySelector("[data-dock-panel='" + savedPanel + "']");
    setActiveDockPanel(panel ? savedPanel : "sources");
  }
  function updateMessage(response) {
    const box = document.querySelector("[data-message-box='true']");
    const text = document.querySelector("[data-message-text='true']");
    const count = document.querySelector("[data-diagnostics-count='true']");
    const message = readValue(response, "message") || "";
    const diagnosticsCount = readValue(response, "diagnosticsCount") || 0;
    const succeeded = readValue(response, "succeeded");
    if (succeeded && message.length === 0 && diagnosticsCount === 0) {
      hideMessage();
      return;
    }
    if (box) {
      box.classList.remove("bm-message-hidden");
      box.classList.toggle("bm-message-error", !succeeded);
    }
    if (text) {
      text.textContent = message;
    }
    if (count) {
      count.textContent = diagnosticsCount > 0 ? diagnosticsCount + " diagnostics" : "Ready";
    }
  }
  function hideMessage() {
    const box = document.querySelector("[data-message-box='true']");
    if (box) {
      box.classList.add("bm-message-hidden");
    }
  }
  function readValue(source, key) {
    if (source[key] !== undefined) {
      return source[key];
    }
    const pascalKey = key.charAt(0).toUpperCase() + key.slice(1);
    return source[pascalKey];
  }
  function clearDslDiagnostics() {
    if (!dslCodeEditor) {
      return;
    }
    dslDiagnosticMarkers.forEach(function (marker) {
      marker.clear();
    });
    dslDiagnosticMarkers = [];
    dslDiagnosticLineClasses.forEach(function (lineClass) {
      dslCodeEditor.removeLineClass(lineClass.line, "background", lineClass.className);
      dslCodeEditor.setGutterMarker(lineClass.line, "CodeMirror-linenumbers", null);
    });
    dslDiagnosticLineClasses = [];
  }
  function createDslDiagnosticGutter(diagnostic) {
    const marker = document.createElement("span");
    marker.className = "bm-dsl-diagnostic-gutter";
    marker.title = diagnostic.message || diagnostic.Message || "";
    marker.textContent = "!";
    return marker;
  }
  function normalizeEditorDiagnostic(diagnostic) {
    return {
      code: readValue(diagnostic, "code") || "",
      message: readValue(diagnostic, "message") || "",
      severity: readValue(diagnostic, "severity") || "Error",
      path: readValue(diagnostic, "path") || "",
      line: readValue(diagnostic, "line") || 1,
      column: readValue(diagnostic, "column") || 1,
      length: readValue(diagnostic, "length") || 1
    };
  }
  function clearDslDiagnosticPanel() {
    const panel = document.querySelector("[data-dsl-diagnostics-panel='true']");
    const count = document.querySelector("[data-dsl-diagnostics-count='true']");
    const list = document.querySelector("[data-dsl-diagnostics-list='true']");
    const empty = document.querySelector("[data-dsl-diagnostics-empty='true']");
    if (count) {
      count.textContent = "0";
    }
    if (list) {
      list.innerHTML = "";
    }
    if (empty) {
      empty.removeAttribute("hidden");
    }
    if (panel) {
      panel.classList.remove("bm-dsl-diagnostics-has-items");
    }
  }
  function goToDslDiagnostic(line, column) {
    if (!dslCodeEditor) {
      return;
    }
    const position = window.CodeMirror.Pos(Math.max(0, line - 1), Math.max(0, column - 1));
    dslCodeEditor.focus();
    dslCodeEditor.setCursor(position);
    dslCodeEditor.scrollIntoView(position, 80);
  }
  function createDslDiagnosticRow(diagnostic) {
    const row = document.createElement("button");
    row.type = "button";
    row.className = "bm-dsl-diagnostic-row";
    row.setAttribute("data-dsl-diagnostic-row", "true");
    row.setAttribute("data-line", diagnostic.line);
    row.setAttribute("data-column", diagnostic.column);
    row.title = diagnostic.message;
    const code = document.createElement("span");
    code.className = "bm-dsl-diagnostic-code";
    code.textContent = diagnostic.code.length > 0 ? diagnostic.severity + " " + diagnostic.code : diagnostic.severity;
    const location = document.createElement("span");
    location.className = "bm-dsl-diagnostic-location";
    location.textContent = diagnostic.line + ":" + diagnostic.column;
    const path = document.createElement("span");
    path.className = "bm-dsl-diagnostic-path";
    path.textContent = diagnostic.path;
    const message = document.createElement("span");
    message.className = "bm-dsl-diagnostic-message";
    message.textContent = diagnostic.message;
    row.appendChild(code);
    row.appendChild(location);
    row.appendChild(path);
    row.appendChild(message);
    row.addEventListener("click", function () {
      goToDslDiagnostic(diagnostic.line, diagnostic.column);
    });
    return row;
  }
  function renderDslDiagnosticPanel(response) {
    const panel = document.querySelector("[data-dsl-diagnostics-panel='true']");
    const count = document.querySelector("[data-dsl-diagnostics-count='true']");
    const list = document.querySelector("[data-dsl-diagnostics-list='true']");
    const empty = document.querySelector("[data-dsl-diagnostics-empty='true']");
    if (!panel || !count || !list || !empty) {
      return [];
    }
    const diagnostics = (readValue(response, "editorDiagnostics") || []).map(normalizeEditorDiagnostic);
    count.textContent = diagnostics.length.toString();
    list.innerHTML = "";
    if (diagnostics.length === 0) {
      empty.removeAttribute("hidden");
      panel.classList.remove("bm-dsl-diagnostics-has-items");
      return diagnostics;
    }
    empty.setAttribute("hidden", "hidden");
    panel.classList.add("bm-dsl-diagnostics-has-items");
    diagnostics.forEach(function (diagnostic) {
      list.appendChild(createDslDiagnosticRow(diagnostic));
    });
    return diagnostics;
  }
  function applyDslDiagnostics(response) {
    clearDslDiagnostics();
    renderDslDiagnosticPanel(response);
    if (!dslCodeEditor) {
      return;
    }
    const diagnostics = readValue(response, "editorDiagnostics") || [];
    diagnostics.forEach(function (rawDiagnostic) {
      const diagnostic = normalizeEditorDiagnostic(rawDiagnostic);
      const lineIndex = Math.max(0, diagnostic.line - 1);
      const columnIndex = Math.max(0, diagnostic.column - 1);
      const length = Math.max(1, diagnostic.length);
      const from = window.CodeMirror.Pos(lineIndex, columnIndex);
      const to = window.CodeMirror.Pos(lineIndex, columnIndex + length);
      const marker = dslCodeEditor.markText(from, to, {
        className: "bm-dsl-diagnostic-underline",
        title: diagnostic.code.length > 0 ? diagnostic.code + ": " + diagnostic.message : diagnostic.message
      });
      dslDiagnosticMarkers.push(marker);
      dslCodeEditor.addLineClass(lineIndex, "background", "bm-dsl-diagnostic-line");
      dslCodeEditor.setGutterMarker(lineIndex, "CodeMirror-linenumbers", createDslDiagnosticGutter(diagnostic));
      dslDiagnosticLineClasses.push({
        line: lineIndex,
        className: "bm-dsl-diagnostic-line"
      });
    });
  }
  function createFunctionDescriptionMap() {
    const map = {};
    document.querySelectorAll(".bm-function-item").forEach(function (item) {
      const key = item.querySelector(".bm-function-key");
      if (!key) {
        return;
      }
      map[key.textContent] = item.getAttribute("title") || "";
    });
    return map;
  }
  function hideFunctionTooltip() {
    if (functionTooltip && functionTooltip.parentNode) {
      functionTooltip.parentNode.removeChild(functionTooltip);
    }
    functionTooltip = null;
  }
  function showFunctionTooltip(text, left, top) {
    hideFunctionTooltip();
    if (text.length === 0) {
      return;
    }
    functionTooltip = document.createElement("div");
    functionTooltip.className = "bm-dsl-function-tooltip";
    functionTooltip.textContent = text;
    functionTooltip.style.left = left + "px";
    functionTooltip.style.top = top + "px";
    document.body.appendChild(functionTooltip);
  }
  function handleDslFunctionHover(event) {
    if (!dslCodeEditor) {
      return;
    }
    const position = dslCodeEditor.coordsChar({ left: event.clientX, top: event.clientY }, "client");
    const token = dslCodeEditor.getTokenAt(position);
    const descriptions = createFunctionDescriptionMap();
    const tokenText = token.string || "";
    const line = dslCodeEditor.getLine(position.line);
    const nextCharacter = line.substring(token.end, token.end + 1);
    if (descriptions[tokenText] && nextCharacter === "(") {
      showFunctionTooltip(descriptions[tokenText], event.pageX + 12, event.pageY + 12);
      return;
    }
    hideFunctionTooltip();
  }
  function updateVisualMappings(mappings) {
    if (!mappings) {
      return;
    }
    document.querySelectorAll(".bm-expression-input").forEach(function (input) {
      const targetPath = input.getAttribute("data-target-path");
      if (!targetPath) {
        return;
      }
      if (mappings[targetPath] !== undefined) {
        input.value = mappings[targetPath];
      } else {
        input.value = "";
      }
    });
  }
  function postForm(handler, formData) {
    const token = getToken();
    formData.append("__RequestVerificationToken", token);
    return fetch(createHandlerUrl(handler), {
      method: "POST",
      body: formData,
      credentials: "same-origin",
      headers: {
        "RequestVerificationToken": token
      }
    }).then(function (response) {
      if (!response.ok) {
        return response.text().then(function (text) {
          throw new Error("Sync request failed with status " + response.status + ". " + text.substring(0, 160));
        });
      }
      return response.json();
    });
  }
  function updateErrorMessage(message) {
    clearDslDiagnosticPanel();
    updateMessage({
      succeeded: false,
      message: message,
      diagnosticsCount: 0
    });
  }
  function collectVisualMappings() {
    const form = document.querySelector(".bm-target-form");
    return form ? new FormData(form) : new FormData();
  }
  function getDslValue() {
    if (dslCodeEditor) {
      return dslCodeEditor.getValue();
    }
    if (dslEditor) {
      return dslEditor.value;
    }
    return "";
  }
  function setDslValue(value) {
    if (dslEditor) {
      dslEditor.value = value;
    }
    if (dslCodeEditor && dslCodeEditor.getValue() !== value) {
      dslCodeEditor.setValue(value);
    }
  }
  function refreshDslEditor() {
    if (dslCodeEditor) {
      window.setTimeout(function () {
        dslCodeEditor.refresh();
      }, 20);
    }
  }
  function syncVisual() {
    postForm("SyncVisual", collectVisualMappings()).then(function (response) {
      if (readValue(response, "succeeded") && dslEditor) {
        setDslValue(readValue(response, "dslContent"));
        applyDslDiagnostics(response);
        hideMessage();
        return;
      }
      applyDslDiagnostics(response);
      if ((readValue(response, "editorDiagnostics") || []).length > 0) {
        hideMessage();
      } else {
        updateMessage(response);
      }
    }).catch(function (error) {
      updateErrorMessage(error.message);
    });
  }
  function syncDsl() {
    if (!dslEditor) {
      return;
    }
    const formData = new FormData();
    formData.append("DslContent", getDslValue());
    formData.append("ActiveView", "Dsl");
    postForm("SyncDsl", formData).then(function (response) {
      applyDslDiagnostics(response);
      if (readValue(response, "succeeded")) {
        updateVisualMappings(readValue(response, "mappings"));
        hideMessage();
        return;
      }
      if ((readValue(response, "editorDiagnostics") || []).length > 0) {
        hideMessage();
      } else {
        updateMessage(response);
      }
    }).catch(function (error) {
      updateErrorMessage(error.message);
    });
  }
  function hasTextSelection(input) {
    return input && input.selectionStart >= 0 && input.selectionEnd > input.selectionStart;
  }
  function selectFirstFunctionArgument(input, expressionStart, expressionText) {
    const openIndex = expressionText.indexOf("(");
    const closeIndex = expressionText.indexOf(")", openIndex + 1);
    if (openIndex < 0 || closeIndex < 0 || closeIndex === openIndex + 1) {
      return;
    }
    const commaIndex = expressionText.indexOf(",", openIndex + 1);
    const argumentEnd = commaIndex >= 0 && commaIndex < closeIndex ? commaIndex : closeIndex;
    input.selectionStart = expressionStart + openIndex + 1;
    input.selectionEnd = expressionStart + argumentEnd;
  }
  function selectFirstFunctionArgumentInCodeEditor(expressionStart, expressionText) {
    if (!dslCodeEditor) {
      return;
    }
    const openIndex = expressionText.indexOf("(");
    const closeIndex = expressionText.indexOf(")", openIndex + 1);
    if (openIndex < 0 || closeIndex < 0 || closeIndex === openIndex + 1) {
      return;
    }
    const commaIndex = expressionText.indexOf(",", openIndex + 1);
    const argumentEnd = commaIndex >= 0 && commaIndex < closeIndex ? commaIndex : closeIndex;
    dslCodeEditor.setSelection(
      dslCodeEditor.posFromIndex(expressionStart + openIndex + 1),
      dslCodeEditor.posFromIndex(expressionStart + argumentEnd));
  }
  function rememberDslSelection() {
    if (!dslEditor) {
      return;
    }
    if (dslCodeEditor) {
      const selectionStart = dslCodeEditor.indexFromPos(dslCodeEditor.getCursor("from"));
      const selectionEnd = dslCodeEditor.indexFromPos(dslCodeEditor.getCursor("to"));
      dslSelectionStart = selectionStart;
      dslSelectionEnd = selectionEnd;
      return;
    }
    dslSelectionStart = dslEditor.selectionStart >= 0 ? dslEditor.selectionStart : dslEditor.value.length;
    dslSelectionEnd = dslEditor.selectionEnd >= 0 ? dslEditor.selectionEnd : dslSelectionStart;
  }
  function isDslViewActive() {
    return workbench && workbench.getAttribute("data-active-view") === "Dsl";
  }
  function insertIntoDslEditor(expressionText, selectFirstArgument) {
    if (!dslEditor || expressionText.length === 0) {
      return;
    }
    if (dslCodeEditor) {
      const startPosition = dslCodeEditor.posFromIndex(dslSelectionStart);
      const endPosition = dslCodeEditor.posFromIndex(dslSelectionEnd);
      dslCodeEditor.replaceRange(expressionText, startPosition, endPosition);
      const insertedStart = dslSelectionStart;
      const insertedEnd = dslSelectionStart + expressionText.length;
      dslCodeEditor.focus();
      dslCodeEditor.setCursor(dslCodeEditor.posFromIndex(insertedEnd));
      if (selectFirstArgument) {
        selectFirstFunctionArgumentInCodeEditor(insertedStart, expressionText);
      }
      rememberDslSelection();
      scheduleDslSync();
      return;
    }
    const start = dslSelectionStart >= 0 ? dslSelectionStart : dslEditor.value.length;
    const end = dslSelectionEnd >= 0 ? dslSelectionEnd : start;
    dslEditor.value = dslEditor.value.substring(0, start) + expressionText + dslEditor.value.substring(end);
    dslEditor.selectionStart = start + expressionText.length;
    dslEditor.selectionEnd = start + expressionText.length;
    if (selectFirstArgument) {
      selectFirstFunctionArgument(dslEditor, start, expressionText);
    }
    dslEditor.focus();
    rememberDslSelection();
    scheduleDslSync();
  }
  function insertIntoExpressionInput(input, expressionText, selectFirstArgument) {
    if (!input || expressionText.length === 0) {
      return;
    }
    const start = input.selectionStart;
    const end = input.selectionEnd;
    let insertionStart = input.value.length;
    if (start >= 0 && end >= 0) {
      input.value = input.value.substring(0, start) + expressionText + input.value.substring(end);
      insertionStart = start;
      input.selectionStart = start + expressionText.length;
      input.selectionEnd = start + expressionText.length;
    } else {
      input.value = expressionText;
      insertionStart = 0;
    }
    if (selectFirstArgument) {
      selectFirstFunctionArgument(input, insertionStart, expressionText);
    }
    input.focus();
    activeExpressionInput = input;
    scheduleVisualSync();
  }
  function replaceExpressionInput(input, expressionText, selectFirstArgument) {
    if (!input || expressionText.length === 0) {
      return;
    }
    input.value = expressionText;
    input.selectionStart = expressionText.length;
    input.selectionEnd = expressionText.length;
    if (selectFirstArgument) {
      selectFirstFunctionArgument(input, 0, expressionText);
    }
    input.focus();
    activeExpressionInput = input;
    scheduleVisualSync();
  }
  function scheduleVisualSync() {
    window.clearTimeout(visualTimer);
    visualTimer = window.setTimeout(syncVisual, 450);
  }
  function scheduleDslSync() {
    window.clearTimeout(dslTimer);
    dslTimer = window.setTimeout(syncDsl, 650);
  }
  function openModal(name) {
    const modal = document.querySelector("[data-modal='" + name + "']");
    if (modal) {
      modal.classList.add("bm-modal-open");
      modal.setAttribute("aria-hidden", "false");
    }
  }
  function closeModal(modal) {
    modal.classList.remove("bm-modal-open");
    modal.setAttribute("aria-hidden", "true");
  }
  function isLeftDockAuto() {
    return workbench && workbench.getAttribute("data-left-dock-mode") === "auto";
  }
  function openDockFlyout() {
    if (leftDock && isLeftDockAuto()) {
      leftDock.classList.add("bm-dock-flyout-open");
    }
  }
  function closeDockFlyout() {
    if (leftDock) {
      leftDock.classList.remove("bm-dock-flyout-open");
    }
  }
  document.querySelectorAll(".bm-view-button").forEach(function (button) {
    button.addEventListener("click", function () {
      const view = button.getAttribute("data-view");
      if (workbench && view) {
        workbench.setAttribute("data-active-view", view);
        if (view === "Dsl") {
          refreshDslEditor();
        }
      }
    });
  });
  document.querySelectorAll("[data-dock-pin]").forEach(function (button) {
    button.addEventListener("click", function () {
      const currentMode = workbench ? workbench.getAttribute("data-left-dock-mode") : "pinned";
      setLeftDockMode(currentMode === "auto" ? "pinned" : "auto");
    });
  });
  document.querySelectorAll("[data-dock-tab]").forEach(function (button) {
    button.addEventListener("click", function () {
      const panelName = button.getAttribute("data-dock-tab") || "sources";
      const alreadyActive = button.getAttribute("aria-selected") === "true";
      setActiveDockPanel(panelName);
      if (!isLeftDockAuto()) {
        return;
      }
      if (leftDock && alreadyActive && leftDock.classList.contains("bm-dock-flyout-open")) {
        closeDockFlyout();
      } else {
        openDockFlyout();
      }
    });
  });
  document.querySelectorAll("[data-open-modal]").forEach(function (button) {
    button.addEventListener("click", function () {
      openModal(button.getAttribute("data-open-modal"));
    });
  });
  document.querySelectorAll("[data-close-modal]").forEach(function (button) {
    button.addEventListener("click", function () {
      const modal = button.closest(".bm-modal-backdrop");
      if (modal) {
        closeModal(modal);
      }
    });
  });
  document.querySelectorAll(".bm-modal-backdrop").forEach(function (modal) {
    modal.addEventListener("click", function (event) {
      if (event.target === modal) {
        closeModal(modal);
      }
    });
  });
  document.addEventListener("keydown", function (event) {
    if (event.key !== "Escape") {
      return;
    }
    const openModals = document.querySelectorAll(".bm-modal-open");
    if (openModals.length > 0) {
      openModals.forEach(closeModal);
      return;
    }
    closeDockFlyout();
  });
  document.querySelectorAll(".bm-expression-input").forEach(function (input) {
    input.addEventListener("focus", function () {
      activeExpressionInput = input;
    });
    input.addEventListener("input", scheduleVisualSync);
  });
  if (dslEditor) {
    initializeDslCodeEditor();
    dslEditor.addEventListener("input", scheduleDslSync);
    dslEditor.addEventListener("click", rememberDslSelection);
    dslEditor.addEventListener("keyup", rememberDslSelection);
    dslEditor.addEventListener("select", rememberDslSelection);
    dslEditor.addEventListener("focus", rememberDslSelection);
  }
  document.querySelectorAll("[data-dsl-diagnostics-toggle='true']").forEach(function (button) {
    button.addEventListener("click", function () {
      const panel = button.closest("[data-dsl-diagnostics-panel='true']");
      if (!panel) {
        return;
      }
      const collapsed = panel.classList.toggle("bm-dsl-diagnostics-collapsed");
      button.setAttribute("aria-expanded", collapsed ? "false" : "true");
      refreshDslEditor();
    });
  });
  document.querySelectorAll("[data-message-close='true']").forEach(function (button) {
    button.addEventListener("click", function () {
      const box = document.querySelector("[data-message-box='true']");
      if (box) {
        box.classList.add("bm-message-hidden");
      }
    });
  });
  function readArraySource(target) {
    const input = target.closest(".bm-target-field");
    if (!input) {
      return "";
    }
    return input.getAttribute("data-array-source") || "";
  }
  function readArrayAlias(target) {
    const input = target.closest(".bm-target-field");
    if (!input) {
      return "item";
    }
    return input.getAttribute("data-array-alias") || "item";
  }
  function relativizePath(path, target) {
    const source = readArraySource(target);
    const alias = readArrayAlias(target);
    if (source.length === 0) {
      return path;
    }
    const indexedPrefix = source + "[0].";
    if (path.indexOf(indexedPrefix) === 0) {
      return alias + "." + path.substring(indexedPrefix.length);
    }
    const dottedPrefix = source + ".";
    if (path.indexOf(dottedPrefix) === 0) {
      return alias + "." + path.substring(dottedPrefix.length);
    }
    return path;
  }
  function updateTemplateSource(targetPath, sourceExpression, alias) {
    document.querySelectorAll(".bm-target-field[data-array-source]").forEach(function (field) {
      const hidden = field.querySelector("input[name='ProjectionFieldArrayPaths']");
      if (!hidden || hidden.value !== targetPath) {
        return;
      }
      field.setAttribute("data-array-source", sourceExpression);
      field.setAttribute("data-array-alias", alias || "item");
    });
  }
  document.querySelectorAll(".bm-source-field, .bm-source-branch[data-path]").forEach(function (field) {
    field.addEventListener("dragstart", function (event) {
      const path = field.getAttribute("data-path");
      field.classList.add("bm-dragging");
      if (event.dataTransfer && path) {
        event.dataTransfer.setData("application/x-buttermorph-source-path", path);
        event.dataTransfer.setData("text/plain", path);
        event.dataTransfer.effectAllowed = "copy";
      }
    });
    field.addEventListener("dragend", function () {
      field.classList.remove("bm-dragging");
    });
    field.addEventListener("click", function () {
      const path = field.getAttribute("data-path");
      if (activeExpressionInput && path) {
        if (hasTextSelection(activeExpressionInput)) {
          insertIntoExpressionInput(activeExpressionInput, path, false);
        } else {
          activeExpressionInput.value = path;
          activeExpressionInput.focus();
          scheduleVisualSync();
        }
      }
      if (navigator.clipboard && path) {
        navigator.clipboard.writeText(path);
      }
    });
    field.addEventListener("dblclick", function (event) {
      const path = field.getAttribute("data-path");
      if (!isDslViewActive() || !path) {
        return;
      }
      event.preventDefault();
      insertIntoDslEditor(path, false);
    });
  });
  document.querySelectorAll(".bm-function-item").forEach(function (functionItem) {
    functionItem.addEventListener("dragstart", function (event) {
      const template = functionItem.getAttribute("data-function-template");
      functionItem.classList.add("bm-dragging");
      if (event.dataTransfer && template) {
        event.dataTransfer.setData("application/x-buttermorph-function-template", template);
        event.dataTransfer.setData("text/plain", template);
        event.dataTransfer.effectAllowed = "copy";
      }
    });
    functionItem.addEventListener("dragend", function () {
      functionItem.classList.remove("bm-dragging");
    });
    functionItem.addEventListener("click", function () {
      const template = functionItem.getAttribute("data-function-template") || "";
      if (activeExpressionInput) {
        if (hasTextSelection(activeExpressionInput)) {
          insertIntoExpressionInput(activeExpressionInput, template, true);
        } else {
          replaceExpressionInput(activeExpressionInput, template, true);
        }
        return;
      }
      if (navigator.clipboard && template) {
        navigator.clipboard.writeText(template);
      }
    });
    functionItem.addEventListener("dblclick", function (event) {
      const template = functionItem.getAttribute("data-function-template") || "";
      if (!isDslViewActive() || template.length === 0) {
        return;
      }
      event.preventDefault();
      insertIntoDslEditor(template, true);
    });
  });
  document.querySelectorAll("[data-function-search='true']").forEach(function (input) {
    input.addEventListener("input", function () {
      const searchText = input.value.toLowerCase();
      document.querySelectorAll(".bm-function-item").forEach(function (functionItem) {
        const itemText = (functionItem.getAttribute("data-function-search-text") || "").toLowerCase();
        functionItem.hidden = searchText.length > 0 && itemText.indexOf(searchText) < 0;
      });
      document.querySelectorAll(".bm-function-group").forEach(function (group) {
        const visibleItems = group.querySelectorAll(".bm-function-item:not([hidden])");
        group.hidden = visibleItems.length === 0;
      });
    });
  });
  document.querySelectorAll(".bm-target-field, .bm-array-mapping").forEach(function (target) {
    target.addEventListener("dragover", function (event) {
      event.preventDefault();
      target.classList.add("bm-drop-hover");
      if (event.dataTransfer) {
        event.dataTransfer.dropEffect = "copy";
      }
    });
    target.addEventListener("dragleave", function () {
      target.classList.remove("bm-drop-hover");
    });
    target.addEventListener("drop", function (event) {
      event.preventDefault();
      target.classList.remove("bm-drop-hover");
      const functionTemplate = event.dataTransfer.getData("application/x-buttermorph-function-template");
      const sourcePath = event.dataTransfer.getData("application/x-buttermorph-source-path");
      const text = event.dataTransfer.getData("text/plain");
      const expression = functionTemplate || sourcePath || text;
      const input = target.hasAttribute("data-array-drop-target")
        ? target.querySelector(".bm-array-source-input")
        : target.querySelector(".bm-expression-input");
      if (input && expression) {
        if (functionTemplate && !target.hasAttribute("data-array-drop-target")) {
          if (hasTextSelection(input)) {
            insertIntoExpressionInput(input, functionTemplate, true);
          } else {
            replaceExpressionInput(input, functionTemplate, true);
          }
          return;
        }
        const value = target.hasAttribute("data-array-drop-target") ? expression : relativizePath(expression, target);
        if (!target.hasAttribute("data-array-drop-target") && hasTextSelection(input)) {
          insertIntoExpressionInput(input, value, false);
          return;
        }
        input.value = value;
        if (target.hasAttribute("data-array-drop-target")) {
          const aliasInput = target.querySelector(".bm-array-alias-input");
          updateTemplateSource(target.getAttribute("data-array-target-path"), expression, aliasInput ? aliasInput.value : "item");
        }
        input.focus();
        scheduleVisualSync();
      }
    });
  });
  document.querySelectorAll(".bm-clear-mapping").forEach(function (button) {
    button.addEventListener("click", function () {
      const shell = button.closest(".bm-expression-shell");
      const input = shell.querySelector(".bm-expression-input");
      if (input) {
        input.value = "";
        input.focus();
        scheduleVisualSync();
      }
    });
  });
  document.querySelectorAll(".bm-array-source-input, .bm-array-alias-input").forEach(function (input) {
    input.addEventListener("input", function () {
      const container = input.closest(".bm-array-mapping");
      if (!container) {
        return;
      }
      const sourceInput = container.querySelector(".bm-array-source-input");
      const aliasInput = container.querySelector(".bm-array-alias-input");
      updateTemplateSource(
        container.getAttribute("data-array-target-path"),
        sourceInput ? sourceInput.value : "",
        aliasInput ? aliasInput.value : "item");
    });
  });
  document.querySelectorAll(".bm-clear-array-mapping").forEach(function (button) {
    button.addEventListener("click", function () {
      const container = button.closest(".bm-array-mapping");
      if (!container) {
        return;
      }
      const targetPath = container.getAttribute("data-array-target-path");
      container.querySelectorAll(".bm-expression-input").forEach(function (input) {
        if (input.classList.contains("bm-array-alias-input")) {
          input.value = "item";
        } else {
          input.value = "";
        }
      });
      document.querySelectorAll("input[name='ProjectionFieldArrayPaths']").forEach(function (hidden) {
        if (hidden.value !== targetPath) {
          return;
        }
        const field = hidden.closest(".bm-target-field");
        const input = field ? field.querySelector(".bm-expression-input") : null;
        if (input) {
          input.value = "";
        }
      });
      updateTemplateSource(targetPath, "", "item");
      scheduleVisualSync();
    });
  });
  loadActiveDockPanel();
  loadLeftDockMode();
});
