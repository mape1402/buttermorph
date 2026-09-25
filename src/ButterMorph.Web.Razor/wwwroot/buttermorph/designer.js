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
  function completeHostPopupFlow(response) {
    const completed = response
      ? readValue(response, "hostSaveCompleted")
      : workbench && workbench.getAttribute("data-host-save-completed") === "true";
    if (!completed) {
      return;
    }
    const contextKey = response
      ? readValue(response, "savedContextKey") || ""
      : workbench.getAttribute("data-host-context-key") || "";
    const returnUrl = response
      ? readValue(response, "safeReturnUrl") || ""
      : workbench.getAttribute("data-host-return-url") || "";
    if (window.opener && !window.opener.closed) {
      window.opener.postMessage({
        type: "ButterMorphDesignerSaved",
        contextKey: contextKey
      }, window.location.origin);
      window.close();
      return;
    }
    if (window.parent && window.parent !== window) {
      window.parent.postMessage({
        type: "ButterMorphDesignerSaved",
        contextKey: contextKey
      }, window.location.origin);
      return;
    }
    if (returnUrl.length > 0) {
      const destination = new URL(returnUrl, window.location.origin);
      if (contextKey.length > 0) {
        destination.searchParams.set("buttermorphSavedContext", contextKey);
      }
      window.location.assign(destination.pathname + destination.search + destination.hash);
    }
  }
  function configureDslMode() {
    if (!window.CodeMirror || window.CodeMirror.modes.buttermorphDsl) {
      return;
    }
    window.CodeMirror.defineMode("buttermorphDsl", function () {
      const keywords = /^(metadata|target|project|as|when|true|false|null)\b/;
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
          hint: function (editor, data, completion) {
            const insertionStart = editor.indexFromPos(data.from);
            editor.replaceRange(completion.text, data.from, data.to, "complete");
            editor.focus();
            selectFirstFunctionArgumentInCodeEditor(insertionStart, completion.text);
            rememberDslSelection();
          },
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
    if (prefix.indexOf("$") === 0) {
      return "source-path";
    }
    if (beforeCursor.indexOf("=>") >= 0) {
      return "projection-body";
    }
    if (metadataIndex > targetIndex) {
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
  function updateMessage(response, showDslAction) {
    const box = document.querySelector("[data-message-box='true']");
    const text = document.querySelector("[data-message-text='true']");
    const count = document.querySelector("[data-diagnostics-count='true']");
    const action = document.querySelector("[data-message-dsl='true']");
    const message = readValue(response, "message") || "";
    const diagnosticsCount = readValue(response, "diagnosticsCount") || 0;
    const succeeded = readValue(response, "succeeded");
    const shouldShowDslAction = !!showDslAction && diagnosticsCount > 0;
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
    if (action) {
      action.hidden = !shouldShowDslAction;
    }
  }
  function hideMessage() {
    const box = document.querySelector("[data-message-box='true']");
    const action = document.querySelector("[data-message-dsl='true']");
    if (box) {
      box.classList.add("bm-message-hidden");
    }
    if (action) {
      action.hidden = true;
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
    document.querySelectorAll(".bm-expression-input, [data-mapping-mode-hidden='true'], [data-conditional-output]").forEach(function (input) {
      const targetPath = input.getAttribute("data-target-path");
      if (!targetPath) {
        return;
      }
      if (mappings[targetPath] !== undefined) {
        input.value = mappings[targetPath];
      } else if (input.hasAttribute("data-mapping-mode-hidden")) {
        input.value = "basic";
      } else {
        input.value = "";
      }
    });
    refreshAllMappingEditors();
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
    syncAllMappingEditors();
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
  function saveTargetMappings(event) {
    if (event) {
      event.preventDefault();
    }
    postForm("SaveTargetMappings", collectVisualMappings()).then(function (response) {
      applyDslDiagnostics(response);
      if (readValue(response, "succeeded")) {
        setDslValue(readValue(response, "dslContent"));
        updateVisualMappings(readValue(response, "mappings"));
        completeHostPopupFlow(response);
        updateMessage(response);
        return;
      }
      if ((readValue(response, "editorDiagnostics") || []).length > 0 || (readValue(response, "diagnosticsCount") || 0) > 0) {
        updateMessage(response, true);
        return;
      }
      updateMessage(response);
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
  function setActiveDesignerView(view) {
    if (workbench && view) {
      workbench.setAttribute("data-active-view", view);
      if (view === "Dsl") {
        refreshDslEditor();
      }
    }
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
    if (input.closest("[data-mapping-condition-row='true']")) {
      handleMappingConditionEdited(input);
    } else {
      syncMappingEditorFromChild(input);
    }
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
    if (input.closest("[data-mapping-condition-row='true']")) {
      handleMappingConditionEdited(input);
    } else {
      syncMappingEditorFromChild(input);
    }
    scheduleVisualSync();
  }
  function cloneTemplateElement(selector) {
    const template = document.querySelector(selector);
    if (!template) {
      return null;
    }
    return template.content.firstElementChild.cloneNode(true);
  }
  function getMappingEditorMode(editor) {
    const hidden = editor ? editor.querySelector("[data-mapping-mode-hidden='true']") : null;
    return hidden && hidden.value === "conditional" ? "conditional" : "basic";
  }
  function normalizeMappingOperator(operatorKey) {
    if (operatorKey === "required") {
      return "exists";
    }
    const operators = [
      "exists",
      "notEmpty",
      "isEmpty",
      "eq",
      "neq",
      "gt",
      "gte",
      "lt",
      "lte",
      "contains",
      "startsWith",
      "endsWith",
      "regexMatch"
    ];
    return operators.indexOf(operatorKey) >= 0 ? operatorKey : "exists";
  }
  function mappingOperatorNeedsValue(operatorKey) {
    const normalized = normalizeMappingOperator(operatorKey);
    return normalized !== "exists" && normalized !== "isEmpty" && normalized !== "notEmpty";
  }
  function isDslValueExpression(value) {
    const text = (value || "").trim();
    if (text.length === 0) {
      return false;
    }
    if (text.charAt(0) === "$" ||
      (text.charAt(0) === "\"" && text.charAt(text.length - 1) === "\"") ||
      (text.charAt(0) === "[" && text.charAt(text.length - 1) === "]") ||
      (text.charAt(0) === "{" && text.charAt(text.length - 1) === "}") ||
      /^(true|false|null)$/i.test(text) ||
      /^-?\d+(?:\.\d+)?$/.test(text)) {
      return true;
    }
    if (/^[A-Za-z_][A-Za-z0-9_]*(?:\[[^\]]+\])?(?:\.[A-Za-z_][A-Za-z0-9_]*(?:\[[^\]]+\])?)+$/.test(text)) {
      return true;
    }
    return /^[A-Za-z_][A-Za-z0-9_]*\(.*\)$/.test(text);
  }
  function writeDslString(value) {
    return "\"" + String(value)
      .replace(/\\/g, "\\\\")
      .replace(/"/g, "\\\"")
      .replace(/\n/g, "\\n")
      .replace(/\r/g, "\\r")
      .replace(/\t/g, "\\t") + "\"";
  }
  function formatDslValue(value) {
    const text = (value || "").trim();
    return isDslValueExpression(text) ? text : writeDslString(text);
  }
  function buildMappingConditionExpression(fieldPath, operatorKey, value) {
    const operator = normalizeMappingOperator(operatorKey);
    if (!fieldPath) {
      return "";
    }
    if (operator === "notEmpty") {
      return "not(isEmpty(" + fieldPath + "))";
    }
    if (!mappingOperatorNeedsValue(operator)) {
      return operator + "(" + fieldPath + ")";
    }
    if ((value || "").trim().length === 0) {
      return "";
    }
    return operator + "(" + fieldPath + ", " + formatDslValue(value) + ")";
  }
  function splitTopLevelArguments(text) {
    const args = [];
    let start = 0;
    let depth = 0;
    let quote = "";
    let escaping = false;
    for (let index = 0; index < text.length; index++) {
      const character = text.charAt(index);
      if (quote) {
        if (escaping) {
          escaping = false;
        } else if (character === "\\") {
          escaping = true;
        } else if (character === quote) {
          quote = "";
        }
        continue;
      }
      if (character === "\"" || character === "'") {
        quote = character;
        continue;
      }
      if (character === "(" || character === "[" || character === "{") {
        depth++;
        continue;
      }
      if (character === ")" || character === "]" || character === "}") {
        depth = Math.max(0, depth - 1);
        continue;
      }
      if (character === "," && depth === 0) {
        args.push(text.substring(start, index).trim());
        start = index + 1;
      }
    }
    const tail = text.substring(start).trim();
    if (tail.length > 0) {
      args.push(tail);
    }
    return args;
  }
  function parseMappingExpression(text) {
    const raw = (text || "").trim();
    const callMatch = raw.match(/^([A-Za-z_][A-Za-z0-9_]*)\(([\s\S]*)\)$/);
    if (!callMatch) {
      return {
        type: "raw",
        raw: raw
      };
    }
    let depth = 0;
    let quote = "";
    let escaping = false;
    for (let index = callMatch[1].length; index < raw.length; index++) {
      const character = raw.charAt(index);
      if (quote) {
        if (escaping) {
          escaping = false;
        } else if (character === "\\") {
          escaping = true;
        } else if (character === quote) {
          quote = "";
        }
        continue;
      }
      if (character === "\"" || character === "'") {
        quote = character;
        continue;
      }
      if (character === "(") {
        depth++;
      } else if (character === ")") {
        depth--;
        if (depth === 0 && index < raw.length - 1) {
          return {
            type: "raw",
            raw: raw
          };
        }
      }
    }
    return {
      type: "call",
      name: callMatch[1],
      raw: raw,
      args: splitTopLevelArguments(callMatch[2]).map(parseMappingExpression)
    };
  }
  function expressionText(node) {
    return node ? node.raw || "" : "";
  }
  function getDirectChild(element, selector) {
    if (!element) {
      return null;
    }
    const children = Array.prototype.slice.call(element.children);
    for (let index = 0; index < children.length; index++) {
      if (children[index].matches(selector)) {
        return children[index];
      }
    }
    return null;
  }
  function updateMappingConditionValueVisibility(row) {
    const operator = row ? row.querySelector("[data-mapping-condition-operator='true']") : null;
    const right = row ? row.querySelector("[data-mapping-condition-right='true']") : null;
    if (!operator || !right) {
      return;
    }
    const needsValue = mappingOperatorNeedsValue(operator.value);
    right.hidden = !needsValue;
    row.setAttribute("data-value-visible", needsValue ? "true" : "false");
  }
  function readMappingConditionRowExpression(row) {
    const customExpression = row.getAttribute("data-custom-condition-expression") || "";
    const left = row.querySelector("[data-mapping-condition-left='true']");
    const operator = row.querySelector("[data-mapping-condition-operator='true']");
    const right = row.querySelector("[data-mapping-condition-right='true']");
    const leftValue = left ? left.value.trim() : "";
    if (customExpression.length > 0 && leftValue === customExpression) {
      return customExpression;
    }
    updateMappingConditionValueVisibility(row);
    return buildMappingConditionExpression(leftValue, operator ? operator.value : "exists", right ? right.value.trim() : "");
  }
  function readMappingConditionGroupExpression(group) {
    const header = getDirectChild(group, ".bm-mapping-condition-group-header");
    const list = getDirectChild(group, "[data-mapping-condition-list='true']");
    const operator = header ? header.querySelector("[data-mapping-group-operator='true']") : null;
    const parts = [];
    if (!list) {
      return "";
    }
    Array.prototype.slice.call(list.children).forEach(function (child) {
      let expression = "";
      if (child.matches("[data-mapping-condition-row='true']")) {
        expression = readMappingConditionRowExpression(child);
      } else if (child.matches("[data-mapping-condition-group='true']")) {
        expression = readMappingConditionGroupExpression(child);
      }
      if (expression.length > 0) {
        parts.push(expression);
      }
    });
    if (parts.length === 0) {
      return "";
    }
    if (parts.length === 1) {
      return parts[0];
    }
    return (operator && operator.value === "or" ? "or" : "and") + "(" + parts.join(", ") + ")";
  }
  function createMappingConditionRow(expression) {
    const row = cloneTemplateElement("[data-mapping-condition-row-template='true']");
    if (!row) {
      return null;
    }
    hydrateMappingConditionRow(row, expression || "");
    return row;
  }
  function createMappingConditionGroup(expression) {
    const group = cloneTemplateElement("[data-mapping-condition-group-template='true']");
    if (!group) {
      return null;
    }
    hydrateMappingConditionGroup(group, expression || "");
    return group;
  }
  function hydrateMappingConditionRow(row, expression) {
    const ast = typeof expression === "string" ? parseMappingExpression(expression) : expression;
    const left = row.querySelector("[data-mapping-condition-left='true']");
    const operator = row.querySelector("[data-mapping-condition-operator='true']");
    const right = row.querySelector("[data-mapping-condition-right='true']");
    row.removeAttribute("data-custom-condition-expression");
    if (ast && ast.type === "call") {
      if (ast.name === "not" && ast.args.length === 1 && ast.args[0].type === "call" && ast.args[0].name === "isEmpty" && ast.args[0].args.length === 1) {
        if (left) {
          left.value = expressionText(ast.args[0].args[0]);
        }
        if (operator) {
          operator.value = "notEmpty";
        }
        if (right) {
          right.value = "";
        }
        updateMappingConditionValueVisibility(row);
        return;
      }
      if ((ast.name === "exists" || ast.name === "isEmpty") && ast.args.length === 1) {
        if (left) {
          left.value = expressionText(ast.args[0]);
        }
        if (operator) {
          operator.value = ast.name;
        }
        if (right) {
          right.value = "";
        }
        updateMappingConditionValueVisibility(row);
        return;
      }
      if (["eq", "neq", "gt", "gte", "lt", "lte", "contains", "startsWith", "endsWith", "regexMatch"].indexOf(ast.name) >= 0 && ast.args.length === 2) {
        if (left) {
          left.value = expressionText(ast.args[0]);
        }
        if (operator) {
          operator.value = ast.name;
        }
        if (right) {
          right.value = expressionText(ast.args[1]);
        }
        updateMappingConditionValueVisibility(row);
        return;
      }
    }
    if (left) {
      left.value = expressionText(ast);
    }
    if (operator) {
      operator.value = "exists";
    }
    if (right) {
      right.value = "";
    }
    row.setAttribute("data-custom-condition-expression", expressionText(ast));
    updateMappingConditionValueVisibility(row);
  }
  function hydrateMappingConditionGroup(group, expression) {
    const ast = typeof expression === "string" ? parseMappingExpression(expression) : expression;
    const header = getDirectChild(group, ".bm-mapping-condition-group-header");
    const operator = header ? header.querySelector("[data-mapping-group-operator='true']") : null;
    const list = getDirectChild(group, "[data-mapping-condition-list='true']");
    if (!list) {
      return;
    }
    list.innerHTML = "";
    if (ast && ast.type === "call" && (ast.name === "and" || ast.name === "or") && ast.args.length > 0) {
      if (operator) {
        operator.value = ast.name;
      }
      ast.args.forEach(function (argument) {
        const child = argument.type === "call" && (argument.name === "and" || argument.name === "or")
          ? createMappingConditionGroup(argument)
          : createMappingConditionRow(argument);
        if (child) {
          list.appendChild(child);
        }
      });
    } else {
      const row = createMappingConditionRow(ast && ast.raw ? ast : "");
      if (row) {
        list.appendChild(row);
      }
    }
  }
  function getComposerOutput(composer, part) {
    if (!composer) {
      return null;
    }
    const directOutput = getDirectChild(composer, "[data-conditional-output='" + part + "']");
    if (directOutput) {
      return directOutput;
    }
    const panel = composer.closest("[data-mapping-panel='conditional']");
    return panel ? getDirectChild(panel, "[data-conditional-output='" + part + "']") : null;
  }
  function getComposerConditionGroup(composer) {
    const branch = composer ? getDirectChild(composer, "[data-mapping-branch='condition']") : null;
    return branch ? getDirectChild(branch, "[data-mapping-condition-group='true']") : null;
  }
  function getComposerSlot(composer, part) {
    const branch = composer ? getDirectChild(composer, "[data-mapping-branch='" + part + "']") : null;
    return branch ? getDirectChild(branch, "[data-mapping-value-slot='" + part + "']") : null;
  }
  function ensureNestedConditionalComposer(slot) {
    const panel = slot ? slot.querySelector("[data-mapping-slot-panel='conditional']") : null;
    if (!panel) {
      return null;
    }
    let composer = getDirectChild(panel, "[data-mapping-conditional-composer='true']");
    if (!composer) {
      composer = cloneTemplateElement("[data-mapping-conditional-composer-template='true']");
      if (composer) {
        panel.appendChild(composer);
        hydrateMappingConditionalComposer(composer, "", "", "null");
      }
    }
    return composer;
  }
  function setMappingValueSlotMode(slot, mode, expression) {
    if (!slot) {
      return;
    }
    const normalizedMode = mode === "conditional" || mode === "null" ? mode : "expression";
    slot.setAttribute("data-slot-mode", normalizedMode);
    slot.querySelectorAll("[data-mapping-slot-panel]").forEach(function (panel) {
      const active = panel.getAttribute("data-mapping-slot-panel") === normalizedMode;
      panel.hidden = !active;
    });
    slot.querySelectorAll("[data-set-mapping-slot-mode]").forEach(function (button) {
      button.setAttribute("aria-pressed", button.getAttribute("data-set-mapping-slot-mode") === normalizedMode ? "true" : "false");
    });
    if (normalizedMode === "conditional") {
      ensureNestedConditionalComposer(slot);
    }
    if (normalizedMode === "expression" && expression !== undefined) {
      const input = slot.querySelector("[data-mapping-slot-expression='true']");
      if (input) {
        input.value = expression;
      }
    }
  }
  function hydrateMappingValueSlot(slot, expression) {
    const text = (expression || "").trim();
    const ast = parseMappingExpression(text);
    if (ast.type === "call" && ast.name === "when" && ast.args.length >= 2) {
      setMappingValueSlotMode(slot, "conditional");
      hydrateMappingConditionalComposer(
        ensureNestedConditionalComposer(slot),
        expressionText(ast.args[0]),
        expressionText(ast.args[1]),
        ast.args.length > 2 ? expressionText(ast.args[2]) : "null");
      return;
    }
    if (text === "null") {
      setMappingValueSlotMode(slot, "null");
      return;
    }
    setMappingValueSlotMode(slot, "expression", text);
  }
  function readMappingValueSlotExpression(slot) {
    if (!slot) {
      return "";
    }
    const mode = slot.getAttribute("data-slot-mode") || "expression";
    if (mode === "null") {
      return "null";
    }
    if (mode === "conditional") {
      return buildMappingConditionalComposerExpression(ensureNestedConditionalComposer(slot));
    }
    const input = slot.querySelector("[data-mapping-slot-expression='true']");
    return input ? input.value.trim() : "";
  }
  function hydrateMappingConditionalComposer(composer, conditionExpression, thenExpression, elseExpression) {
    if (!composer) {
      return;
    }
    const conditionGroup = getComposerConditionGroup(composer);
    hydrateMappingConditionGroup(conditionGroup, conditionExpression || "");
    hydrateMappingValueSlot(getComposerSlot(composer, "then"), thenExpression || "");
    hydrateMappingValueSlot(getComposerSlot(composer, "else"), stringHasValue(elseExpression) ? elseExpression : "null");
    composer.setAttribute("data-composer-hydrated", "true");
    syncMappingConditionalComposer(composer);
  }
  function stringHasValue(value) {
    return (value || "").trim().length > 0;
  }
  function syncMappingConditionalComposer(composer) {
    if (!composer) {
      return "";
    }
    const condition = readMappingConditionGroupExpression(getComposerConditionGroup(composer));
    const thenExpression = readMappingValueSlotExpression(getComposerSlot(composer, "then"));
    const elseExpression = readMappingValueSlotExpression(getComposerSlot(composer, "else")) || "null";
    const conditionOutput = getComposerOutput(composer, "condition");
    const thenOutput = getComposerOutput(composer, "then");
    const elseOutput = getComposerOutput(composer, "else");
    if (conditionOutput) {
      conditionOutput.value = condition;
    }
    if (thenOutput) {
      thenOutput.value = thenExpression;
    }
    if (elseOutput) {
      elseOutput.value = elseExpression;
    }
    if (condition.length === 0 && thenExpression.length === 0 && elseExpression === "null") {
      return "";
    }
    if (condition.length === 0 || thenExpression.length === 0) {
      return "";
    }
    return "when(" + condition + ", " + thenExpression + ", " + elseExpression + ")";
  }
  function buildMappingConditionalComposerExpression(composer) {
    return syncMappingConditionalComposer(composer);
  }
  function hydrateMappingConditionalPanel(editor) {
    const panel = editor ? editor.querySelector("[data-mapping-panel='conditional']") : null;
    const composer = panel ? getDirectChild(panel, "[data-mapping-conditional-composer='true']") : null;
    if (!composer) {
      return;
    }
    hydrateMappingConditionalComposer(
      composer,
      getComposerOutput(composer, "condition") ? getComposerOutput(composer, "condition").value : "",
      getComposerOutput(composer, "then") ? getComposerOutput(composer, "then").value : "",
      getComposerOutput(composer, "else") ? getComposerOutput(composer, "else").value : "null");
  }
  function syncMappingEditor(editor) {
    if (!editor || getMappingEditorMode(editor) !== "conditional") {
      return;
    }
    const panel = editor.querySelector("[data-mapping-panel='conditional']");
    const composer = panel ? getDirectChild(panel, "[data-mapping-conditional-composer='true']") : null;
    syncMappingConditionalComposer(composer);
  }
  function syncAllMappingEditors() {
    document.querySelectorAll("[data-mapping-editor='true']").forEach(syncMappingEditor);
  }
  function refreshMappingEditor(editor) {
    if (!editor) {
      return;
    }
    const mode = getMappingEditorMode(editor);
    const hidden = editor.querySelector("[data-mapping-mode-hidden='true']");
    if (hidden) {
      hidden.value = mode;
    }
    editor.setAttribute("data-mapping-mode", mode);
    editor.querySelectorAll("[data-mapping-panel]").forEach(function (panel) {
      const isActive = panel.getAttribute("data-mapping-panel") === mode;
      if (isActive) {
        panel.removeAttribute("hidden");
      } else {
        panel.setAttribute("hidden", "hidden");
      }
    });
    editor.querySelectorAll("[data-enable-conditional-mapping], [data-use-basic-mapping]").forEach(function (button) {
      const enablesConditional = button.hasAttribute("data-enable-conditional-mapping");
      button.setAttribute("aria-pressed", (enablesConditional ? mode === "conditional" : mode !== "conditional") ? "true" : "false");
    });
    if (mode === "conditional") {
      hydrateMappingConditionalPanel(editor);
    }
  }
  function refreshAllMappingEditors() {
    document.querySelectorAll("[data-mapping-editor='true']").forEach(refreshMappingEditor);
  }
  function seedConditionalMapping(editor) {
    if (!editor) {
      return;
    }
    const basicInput = editor.querySelector("[data-basic-expression='true']");
    const panel = editor.querySelector("[data-mapping-panel='conditional']");
    const composer = panel ? getDirectChild(panel, "[data-mapping-conditional-composer='true']") : null;
    const thenOutput = composer ? getComposerOutput(composer, "then") : null;
    const elseOutput = composer ? getComposerOutput(composer, "else") : null;
    if (basicInput && thenOutput && thenOutput.value.length === 0 && basicInput.value.length > 0) {
      thenOutput.value = basicInput.value;
    }
    if (elseOutput && elseOutput.value.length === 0) {
      elseOutput.value = "null";
    }
  }
  function keepConditionalAsBasicExpression(editor) {
    const basicInput = editor ? editor.querySelector("[data-basic-expression='true']") : null;
    const panel = editor ? editor.querySelector("[data-mapping-panel='conditional']") : null;
    const composer = panel ? getDirectChild(panel, "[data-mapping-conditional-composer='true']") : null;
    const expression = buildMappingConditionalComposerExpression(composer);
    if (basicInput && expression.length > 0) {
      basicInput.value = expression;
    }
  }
  function isUsableExpressionInput(input) {
    return input &&
      input.matches(".bm-expression-input") &&
      input.type !== "hidden" &&
      !input.closest("[hidden]");
  }
  function getActiveDropInput(target) {
    if (isUsableExpressionInput(activeExpressionInput) && target.contains(activeExpressionInput)) {
      return activeExpressionInput;
    }
    const activePanel = target.querySelector("[data-mapping-panel]:not([hidden])");
    if (activePanel) {
      const activePanelInput = Array.prototype.slice.call(activePanel.querySelectorAll(".bm-expression-input")).find(isUsableExpressionInput);
      if (activePanelInput) {
        return activePanelInput;
      }
    }
    return Array.prototype.slice.call(target.querySelectorAll(".bm-expression-input")).find(isUsableExpressionInput) || null;
  }
  function syncMappingEditorFromChild(element) {
    const editor = element ? element.closest("[data-mapping-editor='true']") : null;
    if (editor) {
      syncMappingEditor(editor);
      return;
    }
    syncMappingConditionalComposer(element ? element.closest("[data-mapping-conditional-composer='true']") : null);
  }
  function handleMappingConditionEdited(element) {
    const row = element ? element.closest("[data-mapping-condition-row='true']") : null;
    if (row) {
      row.removeAttribute("data-custom-condition-expression");
      updateMappingConditionValueVisibility(row);
    }
    syncMappingEditorFromChild(element);
    scheduleVisualSync();
  }
  function addMappingCondition(group) {
    const list = group ? getDirectChild(group, "[data-mapping-condition-list='true']") : null;
    const row = createMappingConditionRow("");
    if (list && row) {
      list.appendChild(row);
      const input = row.querySelector("[data-mapping-condition-left='true']");
      if (input) {
        input.focus();
      }
    }
    syncMappingEditorFromChild(group);
    scheduleVisualSync();
  }
  function addMappingGroup(group) {
    const list = group ? getDirectChild(group, "[data-mapping-condition-list='true']") : null;
    const nestedGroup = createMappingConditionGroup("");
    if (list && nestedGroup) {
      list.appendChild(nestedGroup);
      const input = nestedGroup.querySelector("[data-mapping-condition-left='true']");
      if (input) {
        input.focus();
      }
    }
    syncMappingEditorFromChild(group);
    scheduleVisualSync();
  }
  function removeMappingCondition(row) {
    const list = row ? row.parentElement : null;
    if (!list) {
      return;
    }
    const rows = Array.prototype.slice.call(list.children)
      .filter(function (child) {
        return child.matches("[data-mapping-condition-row='true'], [data-mapping-condition-group='true']");
      });
    if (rows.length <= 1) {
      hydrateMappingConditionRow(row, "");
      const input = row.querySelector("[data-mapping-condition-left='true']");
      if (input) {
        input.focus();
      }
    } else {
      row.remove();
    }
    syncMappingEditorFromChild(list);
    scheduleVisualSync();
  }
  function removeMappingGroup(group) {
    const parent = group ? group.parentElement : null;
    if (parent) {
      group.remove();
      syncMappingEditorFromChild(parent);
      scheduleVisualSync();
    }
  }
  function changeMappingSlotMode(slot, mode) {
    if (!slot) {
      return;
    }
    const previousExpression = readMappingValueSlotExpression(slot);
    setMappingValueSlotMode(slot, mode, mode === "expression" ? previousExpression : undefined);
    if (mode === "conditional") {
      const nestedComposer = ensureNestedConditionalComposer(slot);
      const previousAst = parseMappingExpression(previousExpression);
      if (previousAst.type === "call" && previousAst.name === "when" && previousAst.args.length >= 2) {
        hydrateMappingConditionalComposer(
          nestedComposer,
          expressionText(previousAst.args[0]),
          expressionText(previousAst.args[1]),
          previousAst.args.length > 2 ? expressionText(previousAst.args[2]) : "null");
      } else if (previousExpression.length > 0 && previousExpression !== "null") {
        hydrateMappingConditionalComposer(nestedComposer, "", previousExpression, "null");
      }
    }
    syncMappingEditorFromChild(slot);
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
      setActiveDesignerView(button.getAttribute("data-view"));
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
  document.addEventListener("focusin", function (event) {
    if (event.target.matches(".bm-expression-input")) {
      activeExpressionInput = event.target;
    }
  });
  document.addEventListener("input", function (event) {
    if (!event.target.matches(".bm-expression-input")) {
      return;
    }
    if (event.target.closest("[data-mapping-condition-row='true']")) {
      handleMappingConditionEdited(event.target);
      return;
    }
    syncMappingEditorFromChild(event.target);
    scheduleVisualSync();
  });
  document.addEventListener("change", function (event) {
    if (event.target.matches("[data-mapping-condition-operator='true']")) {
      handleMappingConditionEdited(event.target);
      return;
    }
    if (event.target.matches("[data-mapping-group-operator='true']")) {
      syncMappingEditorFromChild(event.target);
      scheduleVisualSync();
    }
  });
  document.addEventListener("click", function (event) {
    const conditionalButton = event.target.closest("[data-enable-conditional-mapping='true']");
    if (conditionalButton) {
      event.preventDefault();
      const editor = conditionalButton.closest("[data-mapping-editor='true']");
      const hidden = editor ? editor.querySelector("[data-mapping-mode-hidden='true']") : null;
      if (hidden) {
        hidden.value = "conditional";
      }
      seedConditionalMapping(editor);
      refreshMappingEditor(editor);
      syncMappingEditor(editor);
      scheduleVisualSync();
      return;
    }
    const basicButton = event.target.closest("[data-use-basic-mapping='true']");
    if (basicButton) {
      event.preventDefault();
      const editor = basicButton.closest("[data-mapping-editor='true']");
      const hidden = editor ? editor.querySelector("[data-mapping-mode-hidden='true']") : null;
      keepConditionalAsBasicExpression(editor);
      if (hidden) {
        hidden.value = "basic";
      }
      refreshMappingEditor(editor);
      scheduleVisualSync();
      return;
    }
    const addConditionButton = event.target.closest("[data-add-mapping-condition='true']");
    if (addConditionButton) {
      event.preventDefault();
      addMappingCondition(addConditionButton.closest("[data-mapping-condition-group='true']"));
      return;
    }
    const addGroupButton = event.target.closest("[data-add-mapping-group='true']");
    if (addGroupButton) {
      event.preventDefault();
      addMappingGroup(addGroupButton.closest("[data-mapping-condition-group='true']"));
      return;
    }
    const removeConditionButton = event.target.closest("[data-remove-mapping-condition='true']");
    if (removeConditionButton) {
      event.preventDefault();
      removeMappingCondition(removeConditionButton.closest("[data-mapping-condition-row='true']"));
      return;
    }
    const removeGroupButton = event.target.closest("[data-remove-mapping-group='true']");
    if (removeGroupButton) {
      event.preventDefault();
      removeMappingGroup(removeGroupButton.closest("[data-mapping-condition-group='true']"));
      return;
    }
    const slotModeButton = event.target.closest("[data-set-mapping-slot-mode]");
    if (slotModeButton) {
      event.preventDefault();
      changeMappingSlotMode(
        slotModeButton.closest("[data-mapping-value-slot]"),
        slotModeButton.getAttribute("data-set-mapping-slot-mode") || "expression");
    }
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
      hideMessage();
    });
  });
  document.querySelectorAll("[data-message-dsl='true']").forEach(function (button) {
    button.addEventListener("click", function () {
      setActiveDesignerView("Dsl");
      hideMessage();
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
          if (activeExpressionInput.closest("[data-mapping-condition-row='true']")) {
            handleMappingConditionEdited(activeExpressionInput);
          } else {
            syncMappingEditorFromChild(activeExpressionInput);
          }
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
        : getActiveDropInput(target);
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
        if (!target.hasAttribute("data-array-drop-target")) {
          if (input.closest("[data-mapping-condition-row='true']")) {
            handleMappingConditionEdited(input);
          } else {
            syncMappingEditorFromChild(input);
          }
        }
        scheduleVisualSync();
      }
    });
  });
  document.querySelectorAll(".bm-clear-mapping").forEach(function (button) {
    button.addEventListener("click", function () {
      const editor = button.closest("[data-mapping-editor='true']");
      if (!editor) {
        return;
      }
      let focusTarget = null;
      editor.querySelectorAll(".bm-expression-input").forEach(function (input) {
        input.value = "";
        if (!focusTarget && !input.closest("[hidden]")) {
          focusTarget = input;
        }
      });
      editor.querySelectorAll("[data-conditional-output]").forEach(function (input) {
        input.value = input.getAttribute("data-conditional-output") === "else" ? "null" : "";
      });
      syncMappingEditor(editor);
      if (focusTarget) {
        focusTarget.focus();
        scheduleVisualSync();
      }
    });
  });
  refreshAllMappingEditors();
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
  document.querySelectorAll(".bm-target-form").forEach(function (form) {
    form.addEventListener("submit", saveTargetMappings);
  });
  loadActiveDockPanel();
  loadLeftDockMode();
  completeHostPopupFlow();
});
