(function () {
  let discoveredBundle = null;

  function renderPreview() {
    const preview = document.querySelector("[data-type-bundle-preview]");
    const save = document.querySelector("[data-type-bundle-save]");
    if (!preview || !save) {
      return;
    }

    save.disabled = !discoveredBundle;
    if (!discoveredBundle) {
      preview.innerHTML = '<p class="bm-type-bundle-empty">Upload a .dll with a bundle provider and annotated language types.</p>';
      return;
    }

    const types = discoveredBundle.types || [];
    preview.innerHTML = `
      <div class="bm-type-bundle-assembly">
        <span>Assembly</span>
        <div>
          <strong>${escapeHtml(discoveredBundle.assemblyKey || discoveredBundle.assemblyId || "Bundle")}</strong>
          <small>${escapeHtml(discoveredBundle.languageKey || "")} · ${escapeHtml(discoveredBundle.assemblyVersion || "unversioned")}</small>
        </div>
        ${discoveredBundle.assemblyDescription ? `<p>${escapeHtml(discoveredBundle.assemblyDescription)}</p>` : ""}
      </div>
      <div class="bm-type-bundle-types-header">
        <strong>Types</strong>
        <span>${types.length}</span>
      </div>
      <div class="bm-type-bundle-types">
        ${types.map(type => `
          <div>
            <strong>${escapeHtml(type.displayName || type.key)}</strong>
            <span>${escapeHtml(type.qualifiedName || type.key)}</span>
            <small>${escapeHtml(type.version || "unversioned")}</small>
            ${type.description ? `<p>${escapeHtml(type.description)}</p>` : ""}
          </div>`).join("")}
      </div>`;
  }

  function discover(event) {
    event.preventDefault();
    const form = event.currentTarget;
    const input = form.querySelector("input[type='file']");
    if (!input || !input.files.length) {
      showMessage("Choose an assembly first.");
      return;
    }

    const body = new FormData();
    body.append("assembly", input.files[0]);
    fetch(handlerUrl("DiscoverBundle"), {
      method: "POST",
      body
    })
      .then(function (response) { return response.json(); })
      .then(function (payload) {
        if (!payload.succeeded) {
          discoveredBundle = null;
          showMessage((payload.diagnostics || []).join("\n") || "Bundle discovery failed.");
          renderPreview();
          return;
        }

        discoveredBundle = payload.bundle;
        showMessage("Bundle discovered.");
        renderPreview();
      })
      .catch(function (error) {
        discoveredBundle = null;
        showMessage(error.message || "Bundle discovery failed.");
        renderPreview();
      });
  }

  function saveBundle() {
    if (!discoveredBundle) {
      return;
    }

    fetch(handlerUrl("SaveBundle"), {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(discoveredBundle)
    })
      .then(function (response) { return response.json(); })
      .then(function (payload) {
        if (!payload.succeeded) {
          showMessage(payload.message || "Bundle could not be published.");
          return;
        }

        const bundle = payload.bundle || discoveredBundle;
        notifyHost(bundle);
        showMessage("Bundle sent to host.");
      })
      .catch(function (error) {
        showMessage(error.message || "Could not publish bundle.");
      });
  }

  function handlerUrl(handler) {
    const url = new URL(window.location.href);
    url.searchParams.set("handler", handler);
    return url.toString();
  }

  function showMessage(message) {
    const node = document.querySelector("[data-type-bundle-message]");
    if (!node) {
      return;
    }

    node.hidden = !message;
    node.textContent = message;
  }

  function notifyHost(bundle) {
    const message = {
      type: "ButterMorphLanguageTypeBundleSaved",
      contextKey: bundle.assemblyId || bundle.assemblyKey || "",
      bundle
    };

    if (window.parent && window.parent !== window) {
      window.parent.postMessage(message, window.location.origin);
    }

    if (window.opener) {
      window.opener.postMessage(message, window.location.origin);
      window.close();
    }
  }

  function escapeHtml(value) {
    return String(value || "").replace(/[&<>"']/g, character => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", "\"": "&quot;", "'": "&#39;" })[character]);
  }

  const upload = document.querySelector("[data-type-bundle-upload]");
  if (upload) {
    upload.addEventListener("submit", discover);
  }

  const fileInput = document.querySelector("[data-type-bundle-file]");
  const fileName = document.querySelector("[data-type-bundle-file-name]");
  if (fileInput && fileName) {
    fileInput.addEventListener("change", function () {
      const file = fileInput.files && fileInput.files.length ? fileInput.files[0] : null;
      fileName.textContent = file ? file.name : "No file selected";
    });
  }

  const save = document.querySelector("[data-type-bundle-save]");
  if (save) {
    save.addEventListener("click", saveBundle);
  }

  renderPreview();
}());
