window.ButterMorphDesigner = {
  version: "0.1.0"
};

document.addEventListener("DOMContentLoaded", function () {
  let activeExpressionInput = null;

  document.querySelectorAll(".bm-expression-input").forEach(function (input) {
    input.addEventListener("focus", function () {
      activeExpressionInput = input;
    });
  });

  document.querySelectorAll(".bm-copy-source").forEach(function (button) {
    button.addEventListener("click", function () {
      const path = button.getAttribute("data-path");

      if (activeExpressionInput && path) {
        activeExpressionInput.value = path;
        activeExpressionInput.focus();
      }

      if (navigator.clipboard && path) {
        navigator.clipboard.writeText(path);
      }
    });
  });
});
