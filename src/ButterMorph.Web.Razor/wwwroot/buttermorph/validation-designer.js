window.ButterMorphValidationDesigner = {
  version: "0.1.0"
};
document.addEventListener("DOMContentLoaded", function () {
  let activeExpressionInput = null;
  let visualTimer = 0;
  let dslTimer = 0;
  let dslCodeEditor = null;
  let dslSelectionStart = 0;
  let dslSelectionEnd = 0;
  const workbench = document.querySelector(".bm-workbench");
  const visualForm = document.querySelector("[data-validation-form='true']");
  const dslEditor = document.querySelector("[data-dsl-editor='true']");
  const leftDock = document.querySelector("[data-left-dock='true']");
  const leftDockModeKey = "ButterMorphValidationDesigner.LeftDockMode";
  const leftDockPanelKey = "ButterMorphValidationDesigner.LeftDockPanel";

  function readValue(source, key) {
    if (!source) {
      return null;
    }
    if (Object.prototype.hasOwnProperty.call(source, key)) {
      return source[key];
    }
    const pascalKey = key.charAt(0).toUpperCase() + key.slice(1);
    if (Object.prototype.hasOwnProperty.call(source, pascalKey)) {
      return source[pascalKey];
    }
    return null;
  }

  function queryMarker() {
    return String.fromCharCode(63);
  }

  function createHandlerUrl(handler) {
    const parameters = new URLSearchParams(window.location.search);
    parameters.set("handler", handler);
    return window.location.pathname + queryMarker() + parameters.toString();
  }

  function getToken() {
    const token = document.querySelector("input[name='__RequestVerificationToken']");
    return token ? token.value : "";
  }

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
        type: "ButterMorphValidationDesignerSaved",
        contextKey: contextKey
      }, window.location.origin);
      window.close();
      return;
    }
    if (window.parent && window.parent !== window) {
      window.parent.postMessage({
        type: "ButterMorphValidationDesignerSaved",
        contextKey: contextKey
      }, window.location.origin);
      return;
    }
    if (returnUrl.length > 0) {
      const destination = new URL(returnUrl, window.location.origin);
      if (contextKey.length > 0) {
        destination.searchParams.set("buttermorphValidationSavedContext", contextKey);
      }
      window.location.assign(destination.pathname + destination.search + destination.hash);
    }
  }

  function configureDslMode() {
    if (!window.CodeMirror || window.CodeMirror.modes.buttermorphValidationDsl) {
      return;
    }
    window.CodeMirror.defineMode("buttermorphValidationDsl", function () {
      const keywords = /^(validate|assert|project|as|when|true|false|null)\b/;
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
          description: description,
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
      if (path.length > 0) {
        suggestions.push({
          text: path,
          displayText: path,
          description: path,
          render: function (element) {
            element.appendChild(createCompletionElement(name ? name.textContent : path, path, meta ? meta.textContent : "Source"));
          }
        });
      }
    });
    return suggestions;
  }

  function createKeywordSuggestions() {
    return [
      { text: "validate {\n  assert gt($source.quantity, 10): \"Quantity must be greater than 10\"\n}", displayText: "validate block", description: "Creates validation assertions." },
      { text: "assert gt($source.quantity, 10): \"Quantity must be greater than 10\"", displayText: "assert", description: "Creates a validation assertion." },
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
    return match ? match[0] : "";
  }

  function createDslHintProvider(editor) {
    const prefix = getCompletionPrefix(editor);
    const lowerPrefix = prefix.toLowerCase();
    const cursor = editor.getCursor();
    const from = window.CodeMirror.Pos(cursor.line, cursor.ch - prefix.length);
    const suggestions = createKeywordSuggestions().concat(createFunctionSuggestions()).concat(createSourceSuggestions());
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

  function rememberDslSelection() {
    if (!dslEditor) {
      return;
    }
    if (dslCodeEditor) {
      dslSelectionStart = dslCodeEditor.indexFromPos(dslCodeEditor.getCursor("from"));
      dslSelectionEnd = dslCodeEditor.indexFromPos(dslCodeEditor.getCursor("to"));
      return;
    }
    dslSelectionStart = dslEditor.selectionStart >= 0 ? dslEditor.selectionStart : dslEditor.value.length;
    dslSelectionEnd = dslEditor.selectionEnd >= 0 ? dslEditor.selectionEnd : dslSelectionStart;
  }

  function initializeDslCodeEditor() {
    if (!dslEditor || !window.CodeMirror) {
      return;
    }
    configureDslMode();
    dslCodeEditor = window.CodeMirror.fromTextArea(dslEditor, {
      mode: "buttermorphValidationDsl",
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
    rememberDslSelection();
  }

  function setDslValue(value) {
    if (!dslEditor || value === null || value === undefined) {
      return;
    }
    dslEditor.value = value;
    if (dslCodeEditor && dslCodeEditor.getValue() !== value) {
      dslCodeEditor.setValue(value);
    }
  }

  function readDslValue() {
    if (dslCodeEditor) {
      return dslCodeEditor.getValue();
    }
    return dslEditor ? dslEditor.value : "";
  }

  function postForm(handler, formData) {
    if (!formData) {
      formData = new FormData();
    }
    if (!formData.has("__RequestVerificationToken")) {
      formData.append("__RequestVerificationToken", getToken());
    }
    return fetch(createHandlerUrl(handler), {
      method: "POST",
      body: formData,
      headers: {
        "X-Requested-With": "XMLHttpRequest"
      }
    }).then(function (response) {
      if (!response.ok) {
        throw new Error("Request failed.");
      }
      return response.json();
    });
  }

  function collectVisualForm() {
    const data = new FormData(visualForm);
    data.set("ActiveView", "Visual");
    return data;
  }

  function collectDslForm() {
    const data = new FormData();
    data.append("__RequestVerificationToken", getToken());
    data.append("ActiveView", "Dsl");
    data.append("DslContent", readDslValue());
    return data;
  }

  function scheduleVisualSync() {
    window.clearTimeout(visualTimer);
    visualTimer = window.setTimeout(syncVisual, 450);
  }

  function scheduleDslSync() {
    window.clearTimeout(dslTimer);
    dslTimer = window.setTimeout(syncDsl, 650);
  }

  function syncVisual() {
    if (!visualForm) {
      return;
    }
    postForm("SyncVisual", collectVisualForm()).then(function (response) {
      applyDslDiagnostics(response);
      if (readValue(response, "succeeded")) {
        setDslValue(readValue(response, "dslContent"));
      }
      updateMessage(response, !readValue(response, "succeeded"));
    }).catch(function (error) {
      updateErrorMessage(error.message);
    });
  }

  function syncDsl() {
    if (!dslEditor) {
      return;
    }
    postForm("SyncDsl", collectDslForm()).then(function (response) {
      applyDslDiagnostics(response);
      updateMessage(response, !readValue(response, "succeeded"));
    }).catch(function (error) {
      updateErrorMessage(error.message);
    });
  }

  function saveValidationDocument(event) {
    if (event) {
      event.preventDefault();
    }
    postForm("SaveValidationDocument", collectVisualForm()).then(function (response) {
      applyDslDiagnostics(response);
      if (readValue(response, "succeeded")) {
        setDslValue(readValue(response, "dslContent"));
        completeHostPopupFlow(response);
        updateMessage(response);
        return;
      }
      updateMessage(response, true);
    }).catch(function (error) {
      updateErrorMessage(error.message);
    });
  }

  function updateMessage(response, showDslAction) {
    const box = document.querySelector("[data-message-box='true']");
    const text = document.querySelector("[data-message-text='true']");
    const count = document.querySelector("[data-diagnostics-count='true']");
    const action = document.querySelector("[data-message-dsl='true']");
    const message = readValue(response, "message") || "";
    const diagnosticsCount = readValue(response, "diagnosticsCount") || 0;
    const shouldShowDslAction = !!showDslAction && diagnosticsCount > 0;
    if (text) {
      text.textContent = message || (diagnosticsCount > 0 ? "Validation document has errors." : "Ready.");
    }
    if (count) {
      count.textContent = diagnosticsCount > 0 ? diagnosticsCount + " diagnostics" : "Ready";
    }
    if (action) {
      action.hidden = !shouldShowDslAction;
    }
    if (box) {
      box.classList.toggle("bm-message-hidden", !message && diagnosticsCount === 0);
    }
  }

  function updateErrorMessage(message) {
    updateMessage({
      succeeded: false,
      message: message || "Operation failed.",
      diagnosticsCount: 0
    });
  }

  function hideMessage() {
    const box = document.querySelector("[data-message-box='true']");
    if (box) {
      box.classList.add("bm-message-hidden");
    }
  }

  function applyDslDiagnostics(response) {
    const diagnostics = readValue(response, "editorDiagnostics") || [];
    const count = document.querySelector("[data-dsl-diagnostics-count='true']");
    const empty = document.querySelector("[data-dsl-diagnostics-empty='true']");
    const list = document.querySelector("[data-dsl-diagnostics-list='true']");
    if (count) {
      count.textContent = diagnostics.length.toString();
    }
    if (empty) {
      empty.hidden = diagnostics.length > 0;
    }
    if (!list) {
      return;
    }
    list.innerHTML = "";
    diagnostics.forEach(function (diagnostic) {
      const item = document.createElement("button");
      item.type = "button";
      item.className = "bm-dsl-diagnostic-item";
      item.textContent = (readValue(diagnostic, "code") || "") + " " + (readValue(diagnostic, "message") || "");
      item.addEventListener("click", function () {
        const line = Math.max(0, (readValue(diagnostic, "line") || 1) - 1);
        const column = Math.max(0, (readValue(diagnostic, "column") || 1) - 1);
        if (dslCodeEditor) {
          dslCodeEditor.focus();
          dslCodeEditor.setCursor({ line: line, ch: column });
        }
      });
      list.appendChild(item);
    });
  }

  function isDslViewActive() {
    return workbench && workbench.getAttribute("data-active-view") === "Dsl";
  }

  function setActiveDesignerView(view) {
    if (!workbench || !view) {
      return;
    }
    workbench.setAttribute("data-active-view", view);
    if (view === "Dsl" && dslCodeEditor) {
      dslCodeEditor.refresh();
    }
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

  function insertIntoDslEditor(expressionText, selectFirstArgument) {
    if (!dslEditor || expressionText.length === 0) {
      return;
    }
    if (dslCodeEditor) {
      const startPosition = dslCodeEditor.posFromIndex(dslSelectionStart);
      const endPosition = dslCodeEditor.posFromIndex(dslSelectionEnd);
      dslCodeEditor.replaceRange(expressionText, startPosition, endPosition);
      const insertedEnd = dslSelectionStart + expressionText.length;
      dslCodeEditor.focus();
      dslCodeEditor.setCursor(dslCodeEditor.posFromIndex(insertedEnd));
      rememberDslSelection();
      scheduleDslSync();
      return;
    }
    const start = dslSelectionStart >= 0 ? dslSelectionStart : dslEditor.value.length;
    const end = dslSelectionEnd >= 0 ? dslSelectionEnd : start;
    dslEditor.value = dslEditor.value.substring(0, start) + expressionText + dslEditor.value.substring(end);
    dslEditor.focus();
    rememberDslSelection();
    scheduleDslSync();
  }

  function cloneTemplate(selector, targetSelector) {
    const template = document.querySelector(selector);
    const target = document.querySelector(targetSelector);
    if (!template || !target) {
      return;
    }
    const fragment = template.content.cloneNode(true);
    target.appendChild(fragment);
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
      pinButton.setAttribute("data-pin-state", isPinned ? "pinned" : "auto");
    });
    closeDockFlyout();
    window.localStorage.setItem(leftDockModeKey, normalizedMode);
  }

  function loadLeftDockMode() {
    setLeftDockMode(window.localStorage.getItem(leftDockModeKey));
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
      } else {
        panel.setAttribute("hidden", "hidden");
      }
    });
    window.localStorage.setItem(leftDockPanelKey, normalizedPanel);
  }

  function loadActiveDockPanel() {
    const savedPanel = window.localStorage.getItem(leftDockPanelKey) || "sources";
    const panel = document.querySelector("[data-dock-panel='" + savedPanel + "']");
    setActiveDockPanel(panel ? savedPanel : "sources");
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
  document.addEventListener("input", function (event) {
    if (event.target.matches("[data-validation-input='true']")) {
      scheduleVisualSync();
    }
  });
  document.addEventListener("focusin", function (event) {
    if (event.target.matches(".bm-expression-input")) {
      activeExpressionInput = event.target;
    }
  });
  document.addEventListener("click", function (event) {
    if (event.target.matches("[data-add-assertion='true']")) {
      cloneTemplate("[data-assertion-template='true']", "[data-assertion-list='true']");
      return;
    }
    if (event.target.matches("[data-remove-row='true']")) {
      const row = event.target.closest(".bm-validation-row");
      if (row) {
        row.remove();
        scheduleVisualSync();
      }
      return;
    }
    if (event.target.matches("[data-message-close='true']")) {
      hideMessage();
      return;
    }
    if (event.target.matches("[data-message-dsl='true']")) {
      setActiveDesignerView("Dsl");
      hideMessage();
    }
  });
  document.querySelectorAll(".bm-source-field, .bm-source-branch[data-path]").forEach(function (field) {
    field.addEventListener("click", function () {
      const path = field.getAttribute("data-path") || "";
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
      const path = field.getAttribute("data-path") || "";
      if (!isDslViewActive() || !path) {
        return;
      }
      event.preventDefault();
      insertIntoDslEditor(path, false);
    });
  });
  document.querySelectorAll(".bm-function-item").forEach(function (functionItem) {
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
  document.querySelectorAll("[data-dsl-diagnostics-toggle='true']").forEach(function (button) {
    button.addEventListener("click", function () {
      const panel = button.closest("[data-dsl-diagnostics-panel='true']");
      if (!panel) {
        return;
      }
      const collapsed = panel.classList.toggle("bm-dsl-diagnostics-collapsed");
      button.setAttribute("aria-expanded", collapsed ? "false" : "true");
      if (dslCodeEditor) {
        dslCodeEditor.refresh();
      }
    });
  });
  if (visualForm) {
    visualForm.addEventListener("submit", saveValidationDocument);
  }
  if (dslEditor) {
    initializeDslCodeEditor();
    dslEditor.addEventListener("input", scheduleDslSync);
    dslEditor.addEventListener("click", rememberDslSelection);
    dslEditor.addEventListener("keyup", rememberDslSelection);
    dslEditor.addEventListener("select", rememberDslSelection);
    dslEditor.addEventListener("focus", rememberDslSelection);
  }
  document.addEventListener("keydown", function (event) {
    if (event.key === "Escape") {
      closeDockFlyout();
    }
  });
  loadActiveDockPanel();
  loadLeftDockMode();
  completeHostPopupFlow();
});
