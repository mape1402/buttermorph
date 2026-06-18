window.ButterMorphDesigner = {
  version: "0.2.0"
};

document.addEventListener("DOMContentLoaded", function () {
  let activeExpressionInput = null;

  const workbench = document.querySelector(".bm-workbench");

  document.querySelectorAll(".bm-view-button").forEach(function (button) {
    button.addEventListener("click", function () {
      const view = button.getAttribute("data-view");

      if (workbench && view) {
        workbench.setAttribute("data-active-view", view);
      }
    });
  });

  document.querySelectorAll(".bm-expression-input").forEach(function (input) {
    input.addEventListener("focus", function () {
      activeExpressionInput = input;
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
      }
    });
  });
});
