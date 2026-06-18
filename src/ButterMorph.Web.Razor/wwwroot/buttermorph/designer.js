window.ButterMorphDesigner = {
  version: "0.3.0"
};

document.addEventListener("DOMContentLoaded", function () {
  let activeExpressionInput = null;
  let visualTimer = 0;
  let dslTimer = 0;

  const workbench = document.querySelector(".bm-workbench");
  const dslEditor = document.querySelector("[data-dsl-editor='true']");

  function getToken() {
    const token = document.querySelector("input[name='__RequestVerificationToken']");
    return token ? token.value : "";
  }

  function queryMarker() {
    return String.fromCharCode(63);
  }

  function updateMessage(response) {
    const box = document.querySelector("[data-message-box='true']");
    const text = document.querySelector("[data-message-text='true']");
    const count = document.querySelector("[data-diagnostics-count='true']");

    if (box) {
      box.classList.remove("bm-message-hidden");
      box.classList.toggle("bm-message-error", !readValue(response, "succeeded"));
    }

    if (text) {
      text.textContent = readValue(response, "message");
    }

    if (count) {
      const diagnosticsCount = readValue(response, "diagnosticsCount");
      count.textContent = diagnosticsCount > 0 ? diagnosticsCount + " diagnostics" : "Ready";
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
    formData.append("__RequestVerificationToken", getToken());

    return fetch(window.location.pathname + queryMarker() + "handler=" + handler, {
      method: "POST",
      body: formData,
      credentials: "same-origin"
    }).then(function (response) {
      return response.json();
    });
  }

  function collectVisualMappings() {
    const formData = new FormData();
    const targetPaths = document.querySelectorAll("input[name='TargetPaths']");
    const expressions = document.querySelectorAll("input[name='Expressions']");

    targetPaths.forEach(function (input) {
      formData.append("TargetPaths", input.value);
    });

    expressions.forEach(function (input) {
      formData.append("Expressions", input.value);
    });

    return formData;
  }

  function syncVisual() {
    postForm("SyncVisual", collectVisualMappings()).then(function (response) {
      updateMessage(response);

      if (readValue(response, "succeeded") && dslEditor) {
        dslEditor.value = readValue(response, "dslContent");
      }
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
        return;
      }

      updateMessage(response);
    });
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

  document.querySelectorAll(".bm-view-button").forEach(function (button) {
    button.addEventListener("click", function () {
      const view = button.getAttribute("data-view");

      if (workbench && view) {
        workbench.setAttribute("data-active-view", view);
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

    document.querySelectorAll(".bm-modal-open").forEach(closeModal);
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

  document.querySelectorAll(".bm-source-field").forEach(function (field) {
    field.addEventListener("dragstart", function (event) {
      const path = field.getAttribute("data-path");
      field.classList.add("bm-dragging");

      if (event.dataTransfer && path) {
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
        activeExpressionInput.value = path;
        activeExpressionInput.focus();
        scheduleVisualSync();
      }

      if (navigator.clipboard && path) {
        navigator.clipboard.writeText(path);
      }
    });
  });

  document.querySelectorAll(".bm-target-field").forEach(function (target) {
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

      const path = event.dataTransfer.getData("text/plain");
      const input = target.querySelector(".bm-expression-input");

      if (input && path) {
        input.value = path;
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
});
