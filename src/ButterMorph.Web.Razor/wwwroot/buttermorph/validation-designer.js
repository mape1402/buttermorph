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
  let activeBuilderInput = null;
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
      const keywords = /^(validate|assert|foreach|project|as|when|true|false|null)\b/;
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
      { text: "foreach $source.items as item {\n  assert gt($item.quantity, 0): \"Item quantity must be greater than zero\"\n}", displayText: "foreach", description: "Validates every item in a collection." },
      { text: "and(gt($source.quantity, 10), exists($source.id))", displayText: "and", description: "Requires every condition to be true." },
      { text: "or(eq($source.status, \"Paid\"), eq($source.status, \"Pending\"))", displayText: "or", description: "Requires at least one condition to be true." },
      { text: "and(gt($source.total, 0), or(eq($source.status, \"Paid\"), eq($source.status, \"Pending\")))", displayText: "and/or", description: "Combines AND and OR in one assertion." },
      { text: "when(eq($source.type, \"VIP\"), and(exists($source.id), gt($source.total, 0)), true)", displayText: "when", description: "Creates a conditional validation expression." },
      { text: "not(isEmpty($source.id))", displayText: "not", description: "Negates a validation condition." },
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
    syncAllAssertionRows();
    syncAllForEachRows();
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
      text.textContent = message || (diagnosticsCount > 0 ? "Validation rules have errors." : "Ready.");
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
    if (input.matches(".bm-expression-input")) {
      activeExpressionInput = input;
    } else {
      activeBuilderInput = input;
    }
    updateExpressionOwners(input);
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
    if (input.matches(".bm-expression-input")) {
      activeExpressionInput = input;
    } else {
      activeBuilderInput = input;
    }
    updateExpressionOwners(input);
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
      return null;
    }
    const fragment = template.content.cloneNode(true);
    const firstElement = fragment.firstElementChild;
    target.appendChild(fragment);
    return firstElement;
  }

  function cloneTemplateElement(selector) {
    const template = document.querySelector(selector);
    if (!template) {
      return null;
    }
    return template.content.firstElementChild.cloneNode(true);
  }

  function normalizeSimpleOperator(operatorKey) {
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

  function simpleOperatorNeedsValue(operatorKey) {
    const normalized = normalizeSimpleOperator(operatorKey);
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

  function buildSimpleExpression(fieldPath, operatorKey, value) {
    const operator = normalizeSimpleOperator(operatorKey);
    if (!fieldPath) {
      return "";
    }
    if (operator === "notEmpty") {
      return "not(isEmpty(" + fieldPath + "))";
    }
    if (!simpleOperatorNeedsValue(operator)) {
      return operator + "(" + fieldPath + ")";
    }
    if ((value || "").trim().length === 0) {
      return "";
    }
    return operator + "(" + fieldPath + ", " + formatDslValue(value) + ")";
  }

  function getExpressionOutput(row) {
    return row ? row.querySelector("[data-expression-output='true'], [data-foreach-child-expression-output='true']") : null;
  }

  function getComplexExpressionEditor(row) {
    return row ? row.querySelector("[data-complex-expression-editor='true'], [data-foreach-child-complex-expression-editor='true']") : null;
  }

  function updateSimpleValueVisibility(row) {
    const operator = row ? row.querySelector("[data-simple-operator='true']") : null;
    const wrapper = row ? row.querySelector("[data-simple-value-wrapper='true']") : null;
    const panel = row ? row.querySelector("[data-simple-rule-panel='true']") : null;
    if (!operator || !wrapper) {
      return;
    }
    const needsValue = simpleOperatorNeedsValue(operator.value);
    wrapper.hidden = !needsValue;
    if (panel) {
      panel.setAttribute("data-value-visible", needsValue ? "true" : "false");
    }
  }

  function updateSimpleExpression(row) {
    const field = row ? row.querySelector("[data-simple-field='true']") : null;
    const operator = row ? row.querySelector("[data-simple-operator='true']") : null;
    const value = row ? row.querySelector("[data-simple-value='true']") : null;
    const output = getExpressionOutput(row);
    if (!field || !operator || !value || !output) {
      return;
    }
    updateSimpleValueVisibility(row);
    output.value = buildSimpleExpression(field.value.trim(), operator.value, value.value.trim());
  }

  function syncComplexExpression(row) {
    const editor = getComplexExpressionEditor(row);
    const output = getExpressionOutput(row);
    if (!editor || !output) {
      return;
    }
    output.value = editor.value;
  }

  function setAssertionKind(row) {
    const kind = row ? row.querySelector("[data-assertion-kind='true'], [data-foreach-child-kind='true']") : null;
    const simplePanel = row ? row.querySelector("[data-simple-rule-panel='true']") : null;
    const complexPanel = row ? row.querySelector("[data-complex-assertion-panel='true']") : null;
    const kindLabel = row ? row.querySelector(".bm-validation-row-kind") : null;
    if (!kind || !simplePanel || !complexPanel) {
      return;
    }
    const isSimple = kind.value === "Simple";
    row.setAttribute("data-rule-kind", isSimple ? "simple" : "advanced");
    if (kindLabel) {
      const mode = row.querySelector("[data-builder-mode='true']");
      const editor = getComplexExpressionEditor(row);
      if (!isSimple && mode && editor && /^when\s*\(/.test(editor.value.trim())) {
        mode.value = "when";
      }
      kindLabel.textContent = isSimple ? "Field rule" : ((mode && mode.value === "when") ? "When assertion" : "Group assertion");
    }
    simplePanel.hidden = !isSimple;
    complexPanel.hidden = isSimple;
    if (isSimple) {
      updateSimpleExpression(row);
    } else {
      syncComplexExpression(row);
    }
  }

  function readConditionExpression(conditionRow) {
    const left = conditionRow.querySelector("[data-condition-left='true']");
    const operator = conditionRow.querySelector("[data-condition-operator='true']");
    const right = conditionRow.querySelector("[data-condition-right='true']");
    const fieldPath = left ? left.value.trim() : "";
    const operatorKey = operator ? normalizeSimpleOperator(operator.value) : "exists";
    updateConditionValueVisibility(conditionRow);
    if (!fieldPath) {
      return "";
    }
    return buildSimpleExpression(fieldPath, operatorKey, right ? right.value.trim() : "");
  }

  function updateConditionValueVisibility(conditionRow) {
    const operator = conditionRow ? conditionRow.querySelector("[data-condition-operator='true']") : null;
    const right = conditionRow ? conditionRow.querySelector("[data-condition-right='true']") : null;
    if (!operator || !right) {
      return;
    }
    const needsValue = simpleOperatorNeedsValue(operator.value);
    right.hidden = !needsValue;
    conditionRow.setAttribute("data-value-visible", needsValue ? "true" : "false");
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

  function readConditionGroupExpression(group) {
    const header = getDirectChild(group, ".bm-condition-group-header");
    const list = getDirectChild(group, "[data-condition-list='true']");
    const operator = header ? header.querySelector("[data-group-operator='true']") : null;
    const parts = [];
    if (!list) {
      return "";
    }
    Array.prototype.slice.call(list.children).forEach(function (child) {
      let expression = "";
      if (child.matches("[data-condition-row='true']")) {
        expression = readConditionExpression(child);
      } else if (child.matches("[data-condition-group='true']")) {
        expression = readConditionGroupExpression(child);
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

  function findPanelRootGroup(panel) {
    return panel ? getDirectChild(panel, "[data-condition-group='true']") : null;
  }

  function getWhenBranch(scope, branchName) {
    return scope ? getDirectChild(scope, "[data-when-branch='" + branchName + "']") : null;
  }

  function getWhenBranchGroup(scope, branchName) {
    const branch = getWhenBranch(scope, branchName);
    return branch ? getDirectChild(branch, "[data-condition-group='true']") : null;
  }

  function getWhenScopes(root) {
    const scopes = [];
    if (!root || !root.querySelectorAll) {
      return scopes;
    }
    if (root.matches && (root.matches("[data-condition-when='true']") || root.matches("[data-builder-panel='when']"))) {
      scopes.push(root);
    }
    root.querySelectorAll("[data-condition-when='true'], [data-builder-panel='when']").forEach(function (scope) {
      if (scopes.indexOf(scope) < 0) {
        scopes.push(scope);
      }
    });
    return scopes;
  }

  function getWhenScope(element) {
    return element ? element.closest("[data-condition-when='true'], [data-builder-panel='when']") : null;
  }

  function ensureConditionBuilderControls(root) {
    updateWhenBranchActions(root);
  }

  function updateBuilderPanels(builder) {
    const mode = builder.querySelector("[data-builder-mode='true']");
    const activeMode = mode ? mode.value : "group";
    builder.querySelectorAll("[data-builder-panel]").forEach(function (panel) {
      panel.hidden = panel.getAttribute("data-builder-panel") !== activeMode;
    });
    if (activeMode === "when") {
      const whenPanel = builder.querySelector("[data-builder-panel='when']");
      const conditionGroup = getWhenBranchGroup(whenPanel, "condition");
      ensureConditionListHasRow(getDirectChild(conditionGroup, "[data-condition-list='true']"));
      ensureValidationSlotHasNode(getWhenSlot(whenPanel, "then"), "field");
    }
    ensureConditionBuilderControls(builder);
    updateAssertionKindLabel(builder.closest("[data-assertion-row='true'], [data-foreach-child-row='true']"));
  }

  function ensureConditionListHasRow(list) {
    if (!list || getDirectChild(list, "[data-condition-row='true'], [data-condition-group='true']")) {
      return null;
    }
    return addConditionToList(list);
  }

  function updateWhenBranchActions(root) {
    if (!root) {
      return;
    }
    getWhenScopes(root).forEach(function (scope) {
      const actions = getDirectChild(scope, ".bm-condition-branch-actions");
      if (!actions) {
        return;
      }
      actions.querySelectorAll("[data-show-when-branch]").forEach(function (button) {
        const branchName = button.getAttribute("data-show-when-branch");
        const branch = branchName ? getWhenBranch(scope, branchName) : null;
        button.hidden = !branch || !branch.hidden || branchName !== "else";
      });
    });
  }

  function updateAssertionKindLabel(row) {
    if (!row) {
      return;
    }
    const kind = row.querySelector("[data-assertion-kind='true'], [data-foreach-child-kind='true']");
    const kindLabel = row.querySelector(".bm-validation-row-kind");
    if (!kind || !kindLabel || kind.value === "Simple") {
      return;
    }
    const mode = row.querySelector("[data-builder-mode='true']");
    kindLabel.textContent = mode && mode.value === "when" ? "When assertion" : "Group assertion";
  }

  function getWhenSlot(scope, branchName) {
    const branch = getWhenBranch(scope, branchName);
    return branch ? getDirectChild(branch, "[data-validation-slot='" + branchName + "']") : null;
  }

  function getValidationSlotBody(slot) {
    return slot ? getDirectChild(slot, "[data-validation-slot-body='true']") : null;
  }

  function getValidationSlotNode(slot) {
    const body = getValidationSlotBody(slot);
    return body ? getDirectChild(body, "[data-condition-row='true'], [data-condition-group='true'], [data-condition-when='true']") : null;
  }

  function setValidationSlotNode(slot, nodeType) {
    const body = getValidationSlotBody(slot);
    if (!body) {
      return null;
    }
    body.innerHTML = "";
    let node = null;
    if (nodeType === "group") {
      node = cloneTemplateElement("[data-condition-group-template='true']");
    } else if (nodeType === "when" && slot.getAttribute("data-allow-when") === "true") {
      node = cloneTemplateElement("[data-condition-when-template='true']");
    } else {
      node = cloneTemplateElement("[data-condition-row-template='true']");
    }
    if (!node) {
      return null;
    }
    body.appendChild(node);
    if (node.matches("[data-condition-row='true']")) {
      updateConditionValueVisibility(node);
    }
    if (node.matches("[data-condition-group='true']")) {
      ensureConditionListHasRow(getDirectChild(node, "[data-condition-list='true']"));
    }
    if (node.matches("[data-condition-when='true']")) {
      ensureConditionBuilderControls(node);
      ensureConditionListHasRow(getDirectChild(getWhenBranchGroup(node, "condition"), "[data-condition-list='true']"));
      ensureValidationSlotHasNode(getWhenSlot(node, "then"), "field");
    }
    return node;
  }

  function ensureValidationSlotHasNode(slot, defaultType) {
    if (!slot || getValidationSlotNode(slot)) {
      return null;
    }
    return setValidationSlotNode(slot, defaultType || "field");
  }

  function clearValidationSlot(slot) {
    const body = getValidationSlotBody(slot);
    if (body) {
      body.innerHTML = "";
    }
  }

  function readValidationSlotExpression(slot) {
    const node = getValidationSlotNode(slot);
    if (!node) {
      return "";
    }
    if (node.matches("[data-condition-row='true']")) {
      return readConditionExpression(node);
    }
    if (node.matches("[data-condition-group='true']")) {
      return readConditionGroupExpression(node);
    }
    if (node.matches("[data-condition-when='true']")) {
      return readConditionWhenExpression(node);
    }
    return "";
  }

  function readConditionWhenExpression(scope) {
    if (!scope || scope.hidden) {
      return "";
    }
    const conditionGroup = getWhenBranchGroup(scope, "condition");
    const thenSlot = getWhenSlot(scope, "then");
    const elseBranch = getWhenBranch(scope, "else");
    const elseSlot = getWhenSlot(scope, "else");
    const condition = readConditionGroupExpression(conditionGroup);
    const thenExpression = readValidationSlotExpression(thenSlot);
    const elseExpression = elseBranch && !elseBranch.hidden ? readValidationSlotExpression(elseSlot) : "";
    if (condition.length > 0 && thenExpression.length > 0) {
      if (elseExpression.length > 0) {
        return "when(" + condition + ", " + thenExpression + ", " + elseExpression + ")";
      }
      return "when(" + condition + ", " + thenExpression + ")";
    }
    return "";
  }

  function updateBuilderExpression(builder) {
    const row = builder.closest("[data-assertion-row='true'], [data-foreach-child-row='true']");
    const editor = getComplexExpressionEditor(row);
    const output = getExpressionOutput(row);
    const mode = builder.querySelector("[data-builder-mode='true']");
    let expression = "";
    updateBuilderPanels(builder);
    if (mode && mode.value === "when") {
      expression = readConditionWhenExpression(builder.querySelector("[data-builder-panel='when']"));
    } else {
      expression = readConditionGroupExpression(findPanelRootGroup(builder.querySelector("[data-builder-panel='group']")));
    }
    if (editor) {
      editor.value = expression;
      if (expression.length > 0) {
        activeExpressionInput = editor;
      }
    }
    if (output) {
      output.value = expression;
    }
  }

  function addConditionToList(list) {
    const condition = cloneTemplateElement("[data-condition-row-template='true']");
    if (condition) {
      list.appendChild(condition);
      updateConditionValueVisibility(condition);
    }
    return condition;
  }

  function addGroupToList(list) {
    const group = cloneTemplateElement("[data-condition-group-template='true']");
    if (group) {
      list.appendChild(group);
      ensureConditionBuilderControls(group);
    }
    return group;
  }

  function syncAssertionRow(row) {
    const kind = row.querySelector("[data-assertion-kind='true'], [data-foreach-child-kind='true']");
    if (kind && kind.value === "Simple") {
      updateSimpleExpression(row);
      return;
    }
    syncComplexExpression(row);
  }

  function syncAllAssertionRows() {
    document.querySelectorAll("[data-assertion-row='true']").forEach(syncAssertionRow);
  }

  function syncForEachRow(row) {
    if (!row) {
      return;
    }
    row.querySelectorAll("[data-foreach-child-row='true']").forEach(syncAssertionRow);
    updateForEachRuleCount(row);
  }

  function updateForEachRuleCount(row) {
    const count = row ? row.querySelector("[data-foreach-rule-count='true']") : null;
    const list = row ? row.querySelector("[data-foreach-child-list='true']") : null;
    if (!count || !list) {
      return;
    }
    count.value = list.querySelectorAll("[data-foreach-child-row='true']").length.toString();
  }

  function syncAllForEachRows() {
    document.querySelectorAll("[data-foreach-row='true']").forEach(syncForEachRow);
  }

  function initializeForEachRow(row) {
    if (!row) {
      return;
    }
    row.querySelectorAll("[data-foreach-child-row='true']").forEach(initializeAssertionRow);
    updateForEachRuleCount(row);
  }

  function initializeAssertionRow(row) {
    if (!row) {
      return;
    }
    setAssertionKind(row);
    row.querySelectorAll("[data-condition-operator='true']").forEach(function (operator) {
      const conditionRow = operator.closest("[data-condition-row='true']");
      updateConditionValueVisibility(conditionRow);
    });
    row.querySelectorAll("[data-condition-builder='true']").forEach(function (builder) {
      updateBuilderPanels(builder);
    });
  }

  function configureNewAssertionRow(kind, builderMode) {
    const row = cloneTemplate("[data-assertion-template='true']", "[data-assertion-list='true']");
    const kindInput = row ? row.querySelector("[data-assertion-kind='true']") : null;
    const modeInput = row ? row.querySelector("[data-builder-mode='true']") : null;
    if (kindInput) {
      kindInput.value = kind === "Complex" ? "Complex" : "Simple";
    }
    if (modeInput) {
      modeInput.value = builderMode === "when" ? "when" : "group";
    }
    initializeAssertionRow(row);
    return row;
  }

  function renameForEachChildFields(row) {
    if (!row) {
      return;
    }
    row.classList.add("bm-validation-row-foreach-child");
    row.removeAttribute("data-assertion-row");
    row.setAttribute("data-foreach-child-row", "true");

    const kind = row.querySelector("[data-assertion-kind='true']");
    if (kind) {
      kind.name = "ForEachChildAssertionKinds";
      kind.removeAttribute("data-assertion-kind");
      kind.setAttribute("data-foreach-child-kind", "true");
    }

    row.querySelectorAll("[name='AssertionMessages']").forEach(function (input) {
      input.name = "ForEachChildAssertionMessages";
    });
    row.querySelectorAll("[name='SimpleFieldPaths']").forEach(function (input) {
      input.name = "ForEachChildSimpleFieldPaths";
    });
    row.querySelectorAll("[name='SimpleOperators']").forEach(function (input) {
      input.name = "ForEachChildSimpleOperators";
    });
    row.querySelectorAll("[name='SimpleValues']").forEach(function (input) {
      input.name = "ForEachChildSimpleValues";
    });
    row.querySelectorAll("[name='AssertionExpressions']").forEach(function (input) {
      input.name = "ForEachChildAssertionExpressions";
    });

    const output = row.querySelector("[data-expression-output='true']");
    if (output) {
      output.removeAttribute("data-expression-output");
      output.setAttribute("data-foreach-child-expression-output", "true");
    }

    const editor = row.querySelector("[data-complex-expression-editor='true']");
    if (editor) {
      editor.removeAttribute("data-complex-expression-editor");
      editor.setAttribute("data-foreach-child-complex-expression-editor", "true");
    }
  }

  function configureNewForEachChildRow(foreachRow, kind, builderMode) {
    const list = foreachRow ? foreachRow.querySelector("[data-foreach-child-list='true']") : null;
    const row = cloneTemplateElement("[data-assertion-template='true']");
    if (!row || !list) {
      return null;
    }
    renameForEachChildFields(row);
    list.appendChild(row);
    const kindInput = row.querySelector("[data-foreach-child-kind='true']");
    const modeInput = row.querySelector("[data-builder-mode='true']");
    if (kindInput) {
      kindInput.value = kind === "Complex" ? "Complex" : "Simple";
    }
    if (modeInput) {
      modeInput.value = builderMode === "when" ? "when" : "group";
    }
    initializeAssertionRow(row);
    updateForEachRuleCount(foreachRow);
    return row;
  }

  function configureNewForEachRow() {
    const row = cloneTemplate("[data-foreach-template='true']", "[data-foreach-list='true']");
    initializeForEachRow(row);
    configureNewForEachChildRow(row, "Simple", "group");
    return row;
  }

  function updateExpressionOwners(input) {
    const row = input.closest("[data-assertion-row='true'], [data-foreach-child-row='true']");
    if (!row) {
      const foreachRow = input.closest("[data-foreach-row='true']");
      if (foreachRow) {
        updateForEachRuleCount(foreachRow);
      }
      return;
    }
    if (input.matches("[data-complex-expression-editor='true'], [data-foreach-child-complex-expression-editor='true']")) {
      syncComplexExpression(row);
    }
    if (input.matches("[data-simple-field='true'], [data-simple-operator='true'], [data-simple-value='true']")) {
      updateSimpleExpression(row);
    }
    if (input.matches("[data-condition-operator='true']")) {
      const conditionRow = input.closest("[data-condition-row='true']");
      updateConditionValueVisibility(conditionRow);
      const right = conditionRow ? conditionRow.querySelector("[data-condition-right='true']") : null;
      if (right && !right.hidden && right.value.length === 0) {
        right.focus();
      }
    }
    const builder = input.closest("[data-condition-builder='true']");
    if (builder) {
      updateBuilderExpression(builder);
    }
    updateForEachRuleCount(row.closest("[data-foreach-row='true']"));
  }

  function readDroppedExpression(event) {
    if (!event.dataTransfer) {
      return "";
    }
    return event.dataTransfer.getData("application/x-buttermorph-function-template") ||
      event.dataTransfer.getData("application/x-buttermorph-source-path") ||
      event.dataTransfer.getData("text/plain") ||
      "";
  }

  function findValidationDropTarget(target) {
    if (!target || !target.closest) {
      return null;
    }
    const input = target.closest("[data-simple-field='true'], [data-simple-value='true'], [data-condition-left='true'], [data-condition-right='true'], [data-complex-expression-editor='true'], [data-foreach-child-complex-expression-editor='true'], [data-foreach-source='true']");
    if (!input || !document.contains(input) || input.type === "hidden" || input.disabled) {
      return null;
    }
    return input;
  }

  function canWriteToInput(input) {
    return !!input &&
      document.contains(input) &&
      input.type !== "hidden" &&
      !input.disabled &&
      !input.closest("[hidden]");
  }

  function writeDropValue(input, expression) {
    if (!input || expression.length === 0) {
      return;
    }
    if (hasTextSelection(input)) {
      insertIntoExpressionInput(input, expression, false);
      return;
    }
    input.value = expression;
    input.focus();
    if (input.matches(".bm-expression-input")) {
      activeExpressionInput = input;
    } else {
      activeBuilderInput = input;
    }
    updateExpressionOwners(input);
    scheduleVisualSync();
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
      updateExpressionOwners(event.target);
      scheduleVisualSync();
    }
    if (event.target.matches("[data-condition-left='true'], [data-condition-right='true']")) {
      updateExpressionOwners(event.target);
      scheduleVisualSync();
    }
  });
  document.addEventListener("change", function (event) {
    if (event.target.matches("[data-assertion-kind='true'], [data-foreach-child-kind='true']")) {
      const row = event.target.closest("[data-assertion-row='true'], [data-foreach-child-row='true']");
      setAssertionKind(row);
      scheduleVisualSync();
      return;
    }
    if (event.target.matches("[data-simple-operator='true'], [data-builder-mode='true'], [data-group-operator='true'], [data-condition-operator='true']")) {
      updateExpressionOwners(event.target);
      if (event.target.matches("[data-simple-operator='true']") && simpleOperatorNeedsValue(event.target.value)) {
        const row = event.target.closest("[data-assertion-row='true'], [data-foreach-child-row='true']");
        const valueInput = row ? row.querySelector("[data-simple-value='true']") : null;
        if (valueInput && valueInput.value.length === 0) {
          valueInput.focus();
        }
      }
      scheduleVisualSync();
    }
  });
  document.addEventListener("dragover", function (event) {
    const target = findValidationDropTarget(event.target);
    if (!target) {
      return;
    }
    event.preventDefault();
    target.classList.add("bm-validation-drop-hover");
    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = "copy";
    }
  });
  document.addEventListener("dragleave", function (event) {
    const target = findValidationDropTarget(event.target);
    if (target) {
      target.classList.remove("bm-validation-drop-hover");
    }
  });
  document.addEventListener("drop", function (event) {
    const target = findValidationDropTarget(event.target);
    if (!target) {
      return;
    }
    event.preventDefault();
    target.classList.remove("bm-validation-drop-hover");
    writeDropValue(target, readDroppedExpression(event));
  });
  document.addEventListener("focusin", function (event) {
    if (event.target.matches(".bm-expression-input")) {
      activeExpressionInput = event.target;
    }
    if (event.target.matches("[data-simple-field='true'], [data-simple-value='true'], [data-condition-left='true'], [data-condition-right='true'], [data-foreach-source='true']")) {
      activeBuilderInput = event.target;
    }
  });
  document.addEventListener("click", function (event) {
    if (event.target.matches("[data-add-field-rule='true']")) {
      configureNewAssertionRow("Simple", "group");
      return;
    }
    if (event.target.matches("[data-add-group-assertion='true'], [data-add-assertion='true']")) {
      configureNewAssertionRow("Complex", "group");
      return;
    }
    if (event.target.matches("[data-add-when-assertion='true']")) {
      configureNewAssertionRow("Complex", "when");
      return;
    }
    if (event.target.matches("[data-add-foreach='true']")) {
      configureNewForEachRow();
      return;
    }
    if (event.target.matches("[data-add-foreach-child-field-rule='true']")) {
      const foreachRow = event.target.closest("[data-foreach-row='true']");
      configureNewForEachChildRow(foreachRow, "Simple", "group");
      scheduleVisualSync();
      return;
    }
    if (event.target.matches("[data-add-foreach-child-group-assertion='true']")) {
      const foreachRow = event.target.closest("[data-foreach-row='true']");
      configureNewForEachChildRow(foreachRow, "Complex", "group");
      scheduleVisualSync();
      return;
    }
    if (event.target.matches("[data-add-foreach-child-when-assertion='true']")) {
      const foreachRow = event.target.closest("[data-foreach-row='true']");
      configureNewForEachChildRow(foreachRow, "Complex", "when");
      scheduleVisualSync();
      return;
    }
    if (event.target.matches("[data-add-condition='true']")) {
      const group = event.target.closest("[data-condition-group='true']");
      const list = getDirectChild(group, "[data-condition-list='true']");
      const condition = list ? addConditionToList(list) : null;
      if (condition) {
        const firstInput = condition.querySelector("[data-condition-left='true']");
        if (firstInput) {
          firstInput.focus();
        }
      }
      updateBuilderExpression(event.target.closest("[data-condition-builder='true']"));
      scheduleVisualSync();
      return;
    }
    if (event.target.matches("[data-add-group='true']")) {
      const group = event.target.closest("[data-condition-group='true']");
      const list = getDirectChild(group, "[data-condition-list='true']");
      const nestedGroup = list ? addGroupToList(list) : null;
      if (nestedGroup) {
        const firstInput = nestedGroup.querySelector("[data-condition-left='true']");
        if (firstInput) {
          firstInput.focus();
        }
      }
      updateBuilderExpression(event.target.closest("[data-condition-builder='true']"));
      scheduleVisualSync();
      return;
    }
    if (event.target.matches("[data-show-when-branch]")) {
      const builder = event.target.closest("[data-condition-builder='true']");
      const whenScope = getWhenScope(event.target);
      const branchName = event.target.getAttribute("data-show-when-branch");
      const branch = branchName ? getWhenBranch(whenScope, branchName) : null;
      if (branch) {
        branch.hidden = false;
        ensureValidationSlotHasNode(getWhenSlot(whenScope, branchName), "field");
        const firstInput = branch.querySelector("[data-condition-left='true']");
        if (firstInput) {
          firstInput.focus();
        }
      }
      updateWhenBranchActions(whenScope);
      updateBuilderExpression(builder);
      scheduleVisualSync();
      return;
    }
    if (event.target.matches("[data-remove-when-branch]")) {
      const builder = event.target.closest("[data-condition-builder='true']");
      const branch = event.target.closest("[data-when-branch]");
      const whenScope = getWhenScope(event.target);
      clearValidationSlot(branch ? branch.querySelector("[data-validation-slot]") : null);
      if (branch) {
        branch.hidden = true;
      }
      updateWhenBranchActions(whenScope);
      updateBuilderExpression(builder);
      scheduleVisualSync();
      return;
    }
    if (event.target.matches("[data-set-slot-node]")) {
      const builder = event.target.closest("[data-condition-builder='true']");
      const slot = event.target.closest("[data-validation-slot]");
      const nodeType = event.target.getAttribute("data-set-slot-node") || "field";
      const node = setValidationSlotNode(slot, nodeType);
      const firstInput = node ? node.querySelector("[data-condition-left='true']") : null;
      if (firstInput) {
        firstInput.focus();
      }
      updateBuilderExpression(builder);
      scheduleVisualSync();
      return;
    }
    if (event.target.matches("[data-clear-slot='true']")) {
      const builder = event.target.closest("[data-condition-builder='true']");
      const slot = event.target.closest("[data-validation-slot]");
      const branch = slot ? slot.closest("[data-when-branch]") : null;
      clearValidationSlot(slot);
      if (branch && slot && slot.getAttribute("data-optional-slot") === "true") {
        branch.hidden = true;
        updateWhenBranchActions(getWhenScope(slot));
      }
      updateBuilderExpression(builder);
      scheduleVisualSync();
      return;
    }
    if (event.target.matches("[data-remove-when='true']")) {
      const builder = event.target.closest("[data-condition-builder='true']");
      const whenNode = event.target.closest("[data-condition-when='true']");
      if (whenNode) {
        whenNode.remove();
      }
      updateBuilderExpression(builder);
      scheduleVisualSync();
      return;
    }
    if (event.target.matches("[data-remove-condition='true']")) {
      const builder = event.target.closest("[data-condition-builder='true']");
      const condition = event.target.closest("[data-condition-row='true']");
      if (condition) {
        condition.remove();
      }
      updateBuilderExpression(builder);
      scheduleVisualSync();
      return;
    }
    if (event.target.matches("[data-remove-group='true']")) {
      const builder = event.target.closest("[data-condition-builder='true']");
      const group = event.target.closest("[data-condition-group='true']");
      if (group) {
        group.remove();
      }
      updateBuilderExpression(builder);
      scheduleVisualSync();
      return;
    }
    if (event.target.matches("[data-remove-row='true']")) {
      const row = event.target.closest(".bm-validation-row");
      if (row) {
        const foreachRow = row.closest("[data-foreach-row='true']");
        row.remove();
        updateForEachRuleCount(foreachRow);
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
  document.querySelectorAll("[data-assertion-row='true']").forEach(initializeAssertionRow);
  document.querySelectorAll("[data-foreach-row='true']").forEach(initializeForEachRow);
  document.querySelectorAll(".bm-source-field, .bm-source-branch[data-path]").forEach(function (field) {
    field.addEventListener("dragstart", function (event) {
      const path = field.getAttribute("data-path") || "";
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
      const path = field.getAttribute("data-path") || "";
      if (path && canWriteToInput(activeBuilderInput)) {
        if (hasTextSelection(activeBuilderInput)) {
          insertIntoExpressionInput(activeBuilderInput, path, false);
        } else {
          activeBuilderInput.value = path;
          activeBuilderInput.focus();
          updateExpressionOwners(activeBuilderInput);
          scheduleVisualSync();
        }
        return;
      }
      if (path && canWriteToInput(activeExpressionInput)) {
        if (hasTextSelection(activeExpressionInput)) {
          insertIntoExpressionInput(activeExpressionInput, path, false);
        } else {
          activeExpressionInput.value = path;
          activeExpressionInput.focus();
          updateExpressionOwners(activeExpressionInput);
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
    functionItem.addEventListener("dragstart", function (event) {
      const template = functionItem.getAttribute("data-function-template") || "";
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
      if (canWriteToInput(activeExpressionInput)) {
        if (hasTextSelection(activeExpressionInput)) {
          insertIntoExpressionInput(activeExpressionInput, template, true);
        } else {
          replaceExpressionInput(activeExpressionInput, template, true);
        }
        updateExpressionOwners(activeExpressionInput);
        return;
      }
      if (canWriteToInput(activeBuilderInput) && activeBuilderInput.matches("[data-simple-value='true'], [data-condition-right='true']")) {
        if (hasTextSelection(activeBuilderInput)) {
          insertIntoExpressionInput(activeBuilderInput, template, true);
        } else {
          activeBuilderInput.value = template;
          activeBuilderInput.focus();
          updateExpressionOwners(activeBuilderInput);
          scheduleVisualSync();
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
