(function () {
    const defaultOptions = "popup=yes,toolbar=no,location=no,menubar=no,status=no,resizable=yes,scrollbars=yes";
    const frameId = "buttermorph-host-frame";
    const overlayId = "buttermorph-host-frame-overlay";
    const previousOverflowAttribute = "data-buttermorph-previous-overflow";
    const themeMessageType = "ButterMorphThemeChanged";
    const trackedPopups = [];
    let currentThemeMode = resolveInitialThemeMode();
    let currentTheme = null;

    function resolveCenteredPopup(width, height) {
        const hostLeft = window.screenLeft !== undefined ? window.screenLeft : window.screenX || 0;
        const hostTop = window.screenTop !== undefined ? window.screenTop : window.screenY || 0;
        const hostWidth = window.outerWidth || document.documentElement.clientWidth || screen.availWidth || width;
        const hostHeight = window.outerHeight || document.documentElement.clientHeight || screen.availHeight || height;
        const popupWidth = Math.min(width, Math.max(360, (screen.availWidth || width) - 80));
        const popupHeight = Math.min(height, Math.max(360, (screen.availHeight || height) - 80));

        return {
            width: popupWidth,
            height: popupHeight,
            left: Math.round(hostLeft + Math.max(0, (hostWidth - popupWidth) / 2)),
            top: Math.round(hostTop + Math.max(0, (hostHeight - popupHeight) / 2))
        };
    }

    function openPopup(url, name, width, height, options) {
        const bounds = resolveCenteredPopup(width || 1280, height || 820);
        const features = (options || defaultOptions) +
            ",width=" + bounds.width +
            ",height=" + bounds.height +
            ",left=" + bounds.left +
            ",top=" + bounds.top;
        const popup = window.open(url, name || "buttermorph", features);

        if (popup) {
            try {
                popup.moveTo(bounds.left, bounds.top);
                popup.resizeTo(bounds.width, bounds.height);
                popup.focus();
            } catch (error) {
                popup.focus();
            }
            trackPopup(popup);
        }

        return popup;
    }

    function openFrame(url, options) {
        const settings = options || {};
        closeFrame();
        ensureFrameStyles();

        const overlay = document.createElement("div");
        overlay.id = overlayId;
        overlay.className = "buttermorph-host-frame-overlay";
        overlay.setAttribute("role", "dialog");
        overlay.setAttribute("aria-modal", "true");
        if (settings.themeMode) {
            currentThemeMode = normalizeThemeMode(settings.themeMode);
            currentTheme = settings.theme || null;
        }
        overlay.setAttribute("data-theme-mode", currentThemeMode);

        const shell = document.createElement("div");
        shell.className = "buttermorph-host-frame-shell";
        shell.style.width = resolveFrameSize(settings.width || 1280, "96vw");
        shell.style.height = resolveFrameSize(settings.height || 820, "92vh");

        const header = document.createElement("div");
        header.className = "buttermorph-host-frame-header";

        const title = document.createElement("strong");
        title.textContent = settings.title || "ButterMorph";

        const closeButton = document.createElement("button");
        closeButton.type = "button";
        closeButton.className = "buttermorph-host-frame-close";
        closeButton.setAttribute("aria-label", "Close ButterMorph");
        closeButton.title = "Close";
        closeButton.textContent = "\u00d7";
        closeButton.addEventListener("click", closeFrame);

        const iframe = document.createElement("iframe");
        iframe.id = frameId;
        iframe.className = "buttermorph-host-frame";
        iframe.src = withEmbeddedParameters(url);
        iframe.title = settings.title || "ButterMorph";
        iframe.addEventListener("load", function () {
            syncFrameTheme(iframe);
        });

        header.appendChild(title);
        header.appendChild(closeButton);
        shell.appendChild(header);
        shell.appendChild(iframe);
        overlay.appendChild(shell);
        document.body.appendChild(overlay);
        lockBodyScroll();
        iframe.focus();
        syncFrameTheme(iframe);

        return iframe;
    }

    function closeFrame() {
        const overlay = document.getElementById(overlayId);
        if (overlay && overlay.parentElement) {
            overlay.parentElement.removeChild(overlay);
        }
        unlockBodyScroll();
    }

    function withEmbeddedParameters(url) {
        const destination = new URL(url, window.location.origin);
        destination.searchParams.set("embedded", "true");
        destination.searchParams.set("popup", "true");
        return destination.pathname + destination.search + destination.hash;
    }

    function resolveFrameSize(size, fallback) {
        return typeof size === "number" ? Math.round(size) + "px" : (size || fallback);
    }

    function lockBodyScroll() {
        if (!document.body.hasAttribute(previousOverflowAttribute)) {
            document.body.setAttribute(previousOverflowAttribute, document.body.style.overflow || "");
        }
        document.body.style.overflow = "hidden";
    }

    function unlockBodyScroll() {
        if (!document.body.hasAttribute(previousOverflowAttribute)) {
            return;
        }
        document.body.style.overflow = document.body.getAttribute(previousOverflowAttribute) || "";
        document.body.removeAttribute(previousOverflowAttribute);
    }

    function ensureFrameStyles() {
        if (document.getElementById("buttermorph-host-frame-styles")) {
            return;
        }

        const style = document.createElement("style");
        style.id = "buttermorph-host-frame-styles";
        style.textContent = [
            ".buttermorph-host-frame-overlay{position:fixed;inset:0;z-index:2147483000;display:flex;align-items:center;justify-content:center;background:rgba(12,16,32,.58);padding:24px;box-sizing:border-box;}",
            ".buttermorph-host-frame-shell{max-width:calc(100vw - 48px);max-height:calc(100vh - 48px);display:flex;flex-direction:column;background:#fff;border:1px solid rgba(74,83,122,.35);border-radius:10px;box-shadow:0 26px 80px rgba(10,14,28,.36);overflow:hidden;}",
            ".buttermorph-host-frame-header{height:38px;display:flex;align-items:center;justify-content:space-between;gap:12px;padding:0 10px 0 14px;background:#f7f8fc;border-bottom:1px solid rgba(74,83,122,.22);color:#151936;font:600 13px system-ui,-apple-system,Segoe UI,sans-serif;}",
            ".buttermorph-host-frame-close{width:28px;height:28px;display:inline-flex;align-items:center;justify-content:center;border:1px solid transparent;border-radius:5px;background:transparent;color:#5b617c;font:400 20px/1 system-ui,-apple-system,Segoe UI,sans-serif;cursor:pointer;transition:background .12s ease,border-color .12s ease,color .12s ease;}",
            ".buttermorph-host-frame-close:hover{background:#f1f3fb;border-color:#d8def4;color:#111827;}",
            ".buttermorph-host-frame-close:focus-visible{outline:2px solid #635bff;outline-offset:1px;}",
            ".buttermorph-host-frame-overlay[data-theme-mode='dark']{background:rgba(2,6,23,.72);}",
            ".buttermorph-host-frame-overlay[data-theme-mode='dark'] .buttermorph-host-frame-shell{background:#111827;border-color:#334155;box-shadow:0 26px 80px rgba(0,0,0,.54);}",
            ".buttermorph-host-frame-overlay[data-theme-mode='dark'] .buttermorph-host-frame-header{background:#0f172a;border-bottom-color:#334155;color:#f8fafc;}",
            ".buttermorph-host-frame-overlay[data-theme-mode='dark'] .buttermorph-host-frame-close{color:#cbd5e1;}",
            ".buttermorph-host-frame-overlay[data-theme-mode='dark'] .buttermorph-host-frame-close:hover{background:#1f2937;border-color:#475569;color:#ffffff;}",
            ".buttermorph-host-frame{width:100%;height:100%;min-height:0;border:0;display:block;}"
        ].join("");
        document.head.appendChild(style);
    }

    function normalizeThemeMode(mode) {
        return String(mode || "").toLowerCase() === "dark" ? "dark" : "light";
    }

    function resolveInitialThemeMode() {
        const candidates = [
            document.documentElement && document.documentElement.getAttribute("data-bm-theme-mode"),
            document.body && document.body.getAttribute("data-bm-theme-mode"),
            document.documentElement && document.documentElement.getAttribute("data-theme"),
            document.body && document.body.getAttribute("data-theme"),
            document.documentElement && document.documentElement.getAttribute("data-bs-theme"),
            document.body && document.body.getAttribute("data-bs-theme"),
            document.documentElement && document.documentElement.getAttribute("data-color-mode"),
            document.body && document.body.getAttribute("data-color-mode")
        ];

        for (let index = 0; index < candidates.length; index += 1) {
            const candidate = String(candidates[index] || "").toLowerCase();
            if (candidate === "dark" || candidate === "light") {
                return candidate;
            }
        }

        if (document.documentElement && document.documentElement.classList.contains("dark")) {
            return "dark";
        }

        if (document.body && document.body.classList.contains("dark")) {
            return "dark";
        }

        return "light";
    }

    function createThemeMessage(mode, theme) {
        return {
            type: themeMessageType,
            mode: normalizeThemeMode(mode),
            theme: theme || null
        };
    }

    function postThemeMode(targetWindow, mode, theme) {
        if (!targetWindow) {
            return;
        }

        try {
            targetWindow.postMessage(createThemeMessage(mode, theme), window.location.origin);
        } catch (error) {
            // The target can be closing or cross-origin. Ignore and let the host continue.
        }
    }

    function syncFrameTheme(iframe) {
        if (!iframe || !iframe.contentWindow) {
            return;
        }

        postThemeMode(iframe.contentWindow, currentThemeMode, currentTheme);
    }

    function trackPopup(popup) {
        trackedPopups.push(popup);
        postThemeMode(popup, currentThemeMode, currentTheme);
        window.setTimeout(function () { postThemeMode(popup, currentThemeMode, currentTheme); }, 250);
        window.setTimeout(function () { postThemeMode(popup, currentThemeMode, currentTheme); }, 1000);
    }

    function broadcastThemeMode() {
        const overlay = document.getElementById(overlayId);
        if (overlay) {
            overlay.setAttribute("data-theme-mode", currentThemeMode);
        }

        const iframe = document.getElementById(frameId);
        if (iframe) {
            syncFrameTheme(iframe);
        }

        for (let index = trackedPopups.length - 1; index >= 0; index -= 1) {
            const popup = trackedPopups[index];
            if (!popup || popup.closed) {
                trackedPopups.splice(index, 1);
                continue;
            }

            postThemeMode(popup, currentThemeMode, currentTheme);
        }
    }

    function setThemeMode(mode, theme) {
        currentThemeMode = normalizeThemeMode(mode);
        currentTheme = theme || null;
        broadcastThemeMode();
    }

    window.addEventListener("message", function (event) {
        if (event.origin !== window.location.origin || !event.data) {
            return;
        }

        const type = event.data.type || "";
        if (type === "ButterMorphDesignerSaved" ||
            type === "ButterMorphSchemaTypeDesignerSaved" ||
            type === "ButterMorphFieldMetadataDesignerSaved" ||
            type === "ButterMorphPayloadSchemaDesignerSaved" ||
            type === "ButterMorphSchemaDesignerSaved") {
            closeFrame();
        }
    });

    window.ButterMorphHost = window.ButterMorphHost || {};
    window.ButterMorphHost.resolveCenteredPopup = resolveCenteredPopup;
    window.ButterMorphHost.openPopup = openPopup;
    window.ButterMorphHost.openFrame = openFrame;
    window.ButterMorphHost.closeFrame = closeFrame;
    window.ButterMorphHost.setThemeMode = setThemeMode;
    window.ButterMorphHost.notifyThemeMode = postThemeMode;
    window.ButterMorphHost.getThemeMode = function () { return currentThemeMode; };
}());
