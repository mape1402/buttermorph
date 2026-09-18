(function () {
    const messageType = "ButterMorphThemeChanged";
    const modes = {
        light: {
            primaryColor: "#4f46e5",
            primaryHoverColor: "#4338ca",
            primaryDarkColor: "#3730a3",
            backgroundColor: "#f4f5fb",
            surfaceColor: "#ffffff",
            surfaceSoftColor: "#fbfbff",
            textColor: "#1d2038",
            mutedTextColor: "#686d91",
            borderColor: "#e2e3ef",
            strongBorderColor: "#d4d7eb",
            dangerColor: "#b91c1c",
            sidebarBackgroundColor: "#1a1a2e",
            sidebarBrandBackgroundColor: "#141428",
            sidebarBorderColor: "#2c2c50",
            sidebarTextColor: "#d0d3e8",
            sidebarMutedTextColor: "#9094b3",
            sidebarActiveTextColor: "#a5b4fc"
        },
        dark: {
            primaryColor: "#818cf8",
            primaryHoverColor: "#6366f1",
            primaryDarkColor: "#a5b4fc",
            backgroundColor: "#0f172a",
            surfaceColor: "#111827",
            surfaceSoftColor: "#1f2937",
            textColor: "#f8fafc",
            mutedTextColor: "#cbd5e1",
            borderColor: "#334155",
            strongBorderColor: "#475569",
            dangerColor: "#f87171",
            sidebarBackgroundColor: "#020617",
            sidebarBrandBackgroundColor: "#020617",
            sidebarBorderColor: "#1e293b",
            sidebarTextColor: "#e2e8f0",
            sidebarMutedTextColor: "#94a3b8",
            sidebarActiveTextColor: "#c7d2fe"
        }
    };

    const variables = {
        primaryColor: ["--bm-primary", "--bm-schema-primary", "--bm-sidebar-accent"],
        primaryHoverColor: ["--bm-primary-hover"],
        primaryDarkColor: ["--bm-schema-primary-dark"],
        backgroundColor: ["--bm-content-bg", "--bm-schema-bg"],
        surfaceColor: ["--bm-surface", "--bm-schema-panel"],
        surfaceSoftColor: ["--bm-schema-panel-soft"],
        textColor: ["--bm-text", "--bm-schema-text"],
        mutedTextColor: ["--bm-muted", "--bm-schema-muted"],
        borderColor: ["--bm-border", "--bm-schema-border"],
        strongBorderColor: ["--bm-border-strong", "--bm-schema-border-strong"],
        dangerColor: ["--bm-danger", "--bm-schema-danger"],
        sidebarBackgroundColor: ["--bm-sidebar-bg"],
        sidebarBrandBackgroundColor: ["--bm-sidebar-brand-bg"],
        sidebarBorderColor: ["--bm-sidebar-border"],
        sidebarTextColor: ["--bm-sidebar-text"],
        sidebarMutedTextColor: ["--bm-sidebar-muted"],
        sidebarActiveTextColor: ["--bm-sidebar-active-text"]
    };

    function normalizeMode(mode) {
        return String(mode || "").toLowerCase() === "dark" ? "dark" : "light";
    }

    function normalizeTheme(theme) {
        return theme && typeof theme === "object" ? theme : {};
    }

    function applyThemeMode(mode, theme) {
        const normalizedMode = normalizeMode(mode);
        const colors = Object.assign({}, modes[normalizedMode], normalizeTheme(theme));
        const target = document.body || document.documentElement;

        document.documentElement.setAttribute("data-bm-theme-mode", normalizedMode);
        document.documentElement.style.colorScheme = normalizedMode;

        if (document.body) {
            document.body.setAttribute("data-bm-theme-mode", normalizedMode);
            document.body.style.colorScheme = normalizedMode;
        }

        Object.keys(variables).forEach(function (key) {
            if (!colors[key]) {
                return;
            }

            variables[key].forEach(function (variableName) {
                target.style.setProperty(variableName, colors[key]);
            });
        });
    }

    window.addEventListener("message", function (event) {
        if (event.origin !== window.location.origin || !event.data) {
            return;
        }

        if (event.data.type !== messageType) {
            return;
        }

        applyThemeMode(event.data.mode, event.data.theme || event.data.colors);
    });

    window.ButterMorphApplyThemeMode = applyThemeMode;

    if (document.body) {
        document.body.setAttribute(
            "data-bm-theme-mode",
            normalizeMode(document.body.getAttribute("data-bm-theme-mode")));
    }
}());
