(function () {
    const messageType = "ButterMorphThemeChanged";
    const variables = {
        primaryColor: {
            palette: "primary-color",
            targets: ["--bm-primary", "--bm-schema-primary", "--bm-sidebar-accent"]
        },
        primaryHoverColor: {
            palette: "primary-hover-color",
            targets: ["--bm-primary-hover"]
        },
        primaryDarkColor: {
            palette: "primary-dark-color",
            targets: ["--bm-schema-primary-dark"]
        },
        backgroundColor: {
            palette: "background-color",
            targets: ["--bm-content-bg", "--bm-schema-bg"]
        },
        surfaceColor: {
            palette: "surface-color",
            targets: ["--bm-surface", "--bm-schema-panel"]
        },
        surfaceSoftColor: {
            palette: "surface-soft-color",
            targets: ["--bm-schema-panel-soft"]
        },
        textColor: {
            palette: "text-color",
            targets: ["--bm-text", "--bm-schema-text"]
        },
        mutedTextColor: {
            palette: "muted-text-color",
            targets: ["--bm-muted", "--bm-schema-muted"]
        },
        borderColor: {
            palette: "border-color",
            targets: ["--bm-border", "--bm-schema-border"]
        },
        strongBorderColor: {
            palette: "strong-border-color",
            targets: ["--bm-border-strong", "--bm-schema-border-strong"]
        },
        dangerColor: {
            palette: "danger-color",
            targets: ["--bm-danger", "--bm-schema-danger"]
        },
        sidebarBackgroundColor: {
            palette: "sidebar-background-color",
            targets: ["--bm-sidebar-bg"]
        },
        sidebarBrandBackgroundColor: {
            palette: "sidebar-brand-background-color",
            targets: ["--bm-sidebar-brand-bg"]
        },
        sidebarBorderColor: {
            palette: "sidebar-border-color",
            targets: ["--bm-sidebar-border"]
        },
        sidebarTextColor: {
            palette: "sidebar-text-color",
            targets: ["--bm-sidebar-text"]
        },
        sidebarMutedTextColor: {
            palette: "sidebar-muted-text-color",
            targets: ["--bm-sidebar-muted"]
        },
        sidebarActiveTextColor: {
            palette: "sidebar-active-text-color",
            targets: ["--bm-sidebar-active-text"]
        }
    };

    function normalizeMode(mode) {
        return String(mode || "").toLowerCase() === "dark" ? "dark" : "light";
    }

    function normalizeTheme(theme) {
        return theme && typeof theme === "object" ? theme : {};
    }

    function readCssVariable(styles, name) {
        return (styles.getPropertyValue(name) || "").trim();
    }

    function readPalette(mode) {
        const normalizedMode = normalizeMode(mode);
        const styles = getComputedStyle(document.body || document.documentElement);
        const colors = {};

        Object.keys(variables).forEach(function (key) {
            colors[key] = readCssVariable(styles, "--bm-theme-" + normalizedMode + "-" + variables[key].palette);
        });

        return colors;
    }

    function applyThemeMode(mode, theme) {
        const normalizedMode = normalizeMode(mode);
        const colors = Object.assign({}, readPalette(normalizedMode), normalizeTheme(theme));
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

            variables[key].targets.forEach(function (variableName) {
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
