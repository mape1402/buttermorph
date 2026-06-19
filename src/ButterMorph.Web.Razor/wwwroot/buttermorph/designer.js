window.ButterMorphDesigner = {
  version: "0.3.0"
};
document.addEventListener("DOMContentLoaded", function () {
  let activeExpressionInput = null;
  let visualTimer = 0;
  let dslTimer = 0;
  const workbench = document.querySelector(".bm-workbench");
  const dslEditor = document.querySelector("[data-dsl-editor='true']");
  const leftDock = document.querySelector("[data-left-dock='true']");
  const leftDockModeKey = "ButterMorphDesigner.LeftDockMode";
  const leftDockPanelKey = "ButterMorphDesigner.LeftDockPanel";
  const legacyToolboxModeKey = "ButterMorphDesigner.ToolboxMode";
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
      pinButton.textContent = isPinned ? "\uD83D\uDCCC" : "\u25C0";
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
  function syncVisual() {
    postForm("SyncVisual", collectVisualMappings()).then(function (response) {
      if (readValue(response, "succeeded") && dslEditor) {
        dslEditor.value = readValue(response, "dslContent");
        hideMessage();
        return;
      }
      updateMessage(response);
    }).catch(function (error) {
      updateErrorMessage(error.message);
    });
  }
  function syncDsl() {
    if (!dslEditor) {
      return;
    }
    const formData = new FormData();
    formData.append("DslContent", dslEditor.value);
    formData.append("ActiveView", "Dsl");
    postForm("SyncDsl", formData).then(function (response) {
      if (readValue(response, "succeeded")) {
        updateVisualMappings(readValue(response, "mappings"));
        hideMessage();
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
    dslEditor.addEventListener("input", scheduleDslSync);
  }
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
