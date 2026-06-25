(function () {
    const keys = {
        type: "ButterMorph.Playground.SchemaTypes",
        field: "ButterMorph.Playground.CustomFields",
        payload: "ButterMorph.Playground.PayloadSchemas"
    };
    const designerPaths = {
        type: "/buttermorph/schema-types/designer",
        field: "/buttermorph/metadata-fields/designer",
        payload: "/buttermorph/payload-schema/designer"
    };
    const labels = {
        type: "Custom type",
        field: "Custom field",
        payload: "Payload schema"
    };
    const queryMarker = window.ButterMorphPlaygroundQueryMarker || "?";
    let activeKind = "type";
    let selectedContext = "";

    document.querySelectorAll("[data-schema-tab]").forEach(function (button) {
        button.addEventListener("click", function () {
            activeKind = button.getAttribute("data-schema-tab") || "type";
            selectedContext = "";
            renderWorkbench();
        });
    });

    document.querySelector("[data-create-schema]")?.addEventListener("click", createItem);
    document.querySelector("[data-edit-schema]")?.addEventListener("click", function () {
        const item = findSelectedItem();
        if (item) {
            openDesigner(item, "edit");
        }
    });
    document.querySelector("[data-delete-schema]")?.addEventListener("click", deleteSelectedItem);

    window.addEventListener("message", function (event) {
        if (event.origin !== window.location.origin || !event.data) {
            return;
        }
        handleSchemaDesignerSave(event.data);
    });

    initialize();

    async function initialize() {
        removeLegacySeedItems();
        await seedStorage();
        await refreshReturnedSchema();
        renderWorkbench();
    }

    async function refreshReturnedSchema() {
        const parameters = new URLSearchParams(window.location.search);
        const queryContext = parameters.get("buttermorphSavedSchemaContext") || "";
        if (queryContext) {
            await refreshSavedItem(queryContext);
            return;
        }

        return;
    }

    async function seedStorage() {
        if (hasStoredItems()) {
            return;
        }

        const response = await fetch("/playground/schema-scenarios", { credentials: "same-origin" });
        const scenarios = await response.json();
        const grouped = { type: [], field: [], payload: [] };
        for (const scenario of scenarios) {
            const viewResponse = await fetch("/playground/schemas/" + encodeURIComponent(scenario.contextKey), { credentials: "same-origin" });
            const view = await viewResponse.json();
            const kind = normalizeKind(view.kind || scenario.kind || inferKind(scenario.designerPath));
            grouped[kind].push(normalizeItem({
                contextKey: scenario.contextKey,
                kind: kind,
                displayName: view.displayName || scenario.displayName,
                description: view.description || scenario.description,
                designerPath: scenario.designerPath || designerPaths[kind],
                jsonSchema: view.jsonSchema || "",
                savedAt: view.savedAt || "",
                versionNumber: view.versionNumber || "1.0.0",
                baseType: view.baseType || "string",
                comment: view.comment || "",
                versionComment: view.versionComment || "",
                metadataJson: view.metadataJson || "{}",
                key: view.key || "",
                dataType: view.dataType || "string",
                appliesToJson: view.appliesToJson || "",
                validationJson: view.validationJson || "",
                isRequired: view.isRequired,
                isActive: view.isActive,
                childrenDefinitionJson: view.childrenDefinitionJson || "",
                arrayItemDataType: view.arrayItemDataType || "",
                arrayItemDefinitionJson: view.arrayItemDefinitionJson || ""
            }));
        }

        saveItems("type", grouped.type);
        saveItems("field", grouped.field);
        saveItems("payload", grouped.payload);
    }

    function hasStoredItems() {
        return window.localStorage.getItem(keys.type) ||
            window.localStorage.getItem(keys.field) ||
            window.localStorage.getItem(keys.payload);
    }

    function removeLegacySeedItems() {
        removeStoredItems("type", ["datatype-customer-code"]);
        removeStoredItems("field", ["metadata-classification"]);
    }

    function removeStoredItems(kind, contextKeys) {
        const items = readItems(kind).filter(function (item) {
            return !contextKeys.includes(item.contextKey);
        });
        saveItems(kind, items);
    }

    function renderWorkbench() {
        document.querySelectorAll("[data-schema-tab]").forEach(function (button) {
            button.setAttribute("aria-pressed", button.getAttribute("data-schema-tab") === activeKind ? "true" : "false");
        });
        renderList();
        renderDetail(findSelectedItem());
    }

    function renderList() {
        const list = document.querySelector("[data-schema-list]");
        if (!list) {
            return;
        }
        const items = readItems(activeKind);
        list.innerHTML = "";
        if (items.length === 0) {
            const empty = document.createElement("div");
            empty.className = "schema-empty";
            empty.textContent = "No " + labels[activeKind].toLowerCase() + " items yet.";
            list.appendChild(empty);
            return;
        }
        for (const item of items) {
            const button = document.createElement("button");
            button.type = "button";
            button.className = "scenario";
            button.setAttribute("data-schema-context-button", item.contextKey);
            button.setAttribute("aria-pressed", item.contextKey === selectedContext ? "true" : "false");
            button.innerHTML = "<strong>" + escapeHtml(item.displayName || labels[activeKind]) + "</strong><span>" + escapeHtml(item.description || item.contextKey) + "</span>";
            button.addEventListener("click", function () {
                selectedContext = item.contextKey;
                renderWorkbench();
            });
            list.appendChild(button);
        }
    }

    function renderDetail(item) {
        const edit = document.querySelector("[data-edit-schema]");
        const remove = document.querySelector("[data-delete-schema]");
        if (!item) {
            document.querySelector("[data-schema-context]").textContent = "No schema selected";
            document.querySelector("[data-schema-time]").textContent = "Not saved yet";
            document.querySelector("[data-schema-json]").value = "";
            edit.disabled = true;
            remove.disabled = true;
            return;
        }
        document.querySelector("[data-schema-context]").textContent = item.displayName || item.contextKey;
        document.querySelector("[data-schema-time]").textContent = item.savedAt || "Local draft";
        document.querySelector("[data-schema-json]").value = formatItemResult(item);
        edit.disabled = false;
        remove.disabled = false;
    }

    function createItem() {
        const item = createDefaultItem(activeKind);
        openDesigner(item, "create");
    }

    function deleteSelectedItem() {
        if (!selectedContext) {
            return;
        }
        const items = readItems(activeKind).filter(function (item) {
            return item.contextKey !== selectedContext;
        });
        saveItems(activeKind, items);
        selectedContext = "";
        renderWorkbench();
    }

    async function openDesigner(item, mode) {
        const width = Math.min(1280, screen.availWidth - 80);
        const height = Math.min(820, screen.availHeight - 80);
        const left = Math.max(0, Math.round((screen.availWidth - width) / 2));
        const top = Math.max(0, Math.round((screen.availHeight - height) / 2));
        const path = item.designerPath || designerPaths[item.kind] || designerPaths.payload;
        const features = "toolbar=no,location=no,menubar=no,status=no,resizable=yes,scrollbars=yes,width=" + width + ",height=" + height + ",left=" + left + ",top=" + top;
        const url = path + queryMarker + "context=" + encodeURIComponent(item.contextKey) + "&mode=" + encodeURIComponent(mode) + "&popup=true&returnUrl=/";
        const popup = window.open("about:blank", "buttermorph-schema-" + item.contextKey, features);
        if (popup) {
            try {
                popup.opener = window;
            } catch (error) {
            }
            popup.focus();
        }

        await preloadDesignerCatalog(item);
        if (popup) {
            popup.location.href = url;
            return;
        }

        window.location.href = url;
    }

    async function preloadDesignerCatalog(item) {
        if (item.kind === "payload") {
            const catalogItems = readItems("type").concat(readItems("field"));
            for (const catalogItem of catalogItems) {
                await preloadItem(catalogItem);
            }
        }

        await preloadItem(item);
    }

    async function preloadItem(item) {
        await fetch("/playground/schema-items/" + encodeURIComponent(item.contextKey), {
            method: "POST",
            credentials: "same-origin",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(item)
        });
    }

    async function refreshSavedItem(contextKey) {
        if (!contextKey) {
            return;
        }
        const response = await fetch("/playground/schemas/" + encodeURIComponent(contextKey), { credentials: "same-origin", cache: "no-store" });
        if (!response.ok) {
            return;
        }
        const view = await response.json();
        const kind = normalizeKind(view.kind || inferKind(view.designerPath));
        const item = normalizeItem(view);
        const items = readItems(kind).filter(function (entry) {
            return entry.contextKey !== contextKey;
        });
        items.push(item);
        saveItems(kind, items);
        activeKind = kind;
        selectedContext = contextKey;
        renderWorkbench();
    }

    function handleSchemaDesignerSave(data) {
        if (!data) {
            return;
        }

        const type = data.type || "";
        if (type === "ButterMorphSchemaDesignerSaved" ||
            type === "ButterMorphSchemaTypeDesignerSaved" ||
            type === "ButterMorphFieldMetadataDesignerSaved" ||
            type === "ButterMorphPayloadSchemaDesignerSaved") {
            refreshSavedItem(data.contextKey || "");
        }
    }

    function createDefaultItem(kind) {
        const stamp = String(Date.now());
        if (kind === "type") {
            return normalizeItem({
                contextKey: "datatype-local-" + stamp,
                kind: "type",
                displayName: "",
                description: "",
                designerPath: designerPaths.type,
                jsonSchema: "",
                key: "",
                versionNumber: "1.0.0",
                baseType: "string",
                comment: "",
                versionComment: ""
            });
        }
        if (kind === "field") {
            return normalizeItem({
                contextKey: "metadata-local-" + stamp,
                kind: "field",
                displayName: "",
                description: "",
                designerPath: designerPaths.field,
                jsonSchema: "",
                key: "",
                dataType: "string",
                validationJson: "",
                appliesToJson: "",
                isRequired: false,
                isActive: true,
                childrenDefinitionJson: "",
                arrayItemDataType: "",
                arrayItemDefinitionJson: ""
            });
        }
        return normalizeItem({
            contextKey: "payload-local-" + stamp,
            kind: "payload",
            displayName: "",
            description: "",
            designerPath: designerPaths.payload,
            key: "",
            jsonSchema: "{\"type\":\"object\",\"properties\":{}}",
            versionNumber: "1.0.0",
            versionComment: "",
            metadataJson: "{}"
        });
    }

    function findSelectedItem() {
        return readItems(activeKind).find(function (item) {
            return item.contextKey === selectedContext;
        });
    }

    function readItems(kind) {
        try {
            const parsed = JSON.parse(window.localStorage.getItem(keys[kind]) || "[]");
            return Array.isArray(parsed) ? parsed.map(normalizeItem) : [];
        } catch (error) {
            return [];
        }
    }

    function saveItems(kind, items) {
        const sorted = items.slice().sort(function (left, right) {
            return String(left.displayName || "").localeCompare(String(right.displayName || ""));
        });
        window.localStorage.setItem(keys[kind], JSON.stringify(sorted));
    }

    function normalizeItem(item) {
        const kind = normalizeKind(item.kind || inferKind(item.designerPath));
        return {
            contextKey: item.contextKey || item.ContextKey || "",
            kind: kind,
            displayName: item.displayName || item.DisplayName || "",
            description: item.description || item.Description || "",
            designerPath: item.designerPath || item.DesignerPath || designerPaths[kind],
            jsonSchema: item.jsonSchema || item.JsonSchema || "",
            savedAt: item.savedAt || item.SavedAt || "",
            versionNumber: item.versionNumber || item.VersionNumber || "",
            baseType: item.baseType || item.BaseType || "",
            comment: item.comment || item.Comment || "",
            versionComment: item.versionComment || item.VersionComment || "",
            metadataJson: item.metadataJson || item.MetadataJson || "{}",
            key: item.key || item.Key || "",
            dataType: item.dataType || item.DataType || "",
            appliesToJson: item.appliesToJson || item.AppliesToJson || "",
            validationJson: item.validationJson || item.ValidationJson || "",
            isRequired: readBoolean(item, "isRequired", "IsRequired"),
            isActive: readBoolean(item, "isActive", "IsActive"),
            childrenDefinitionJson: item.childrenDefinitionJson || item.ChildrenDefinitionJson || "",
            arrayItemDataType: item.arrayItemDataType || item.ArrayItemDataType || "",
            arrayItemDefinitionJson: item.arrayItemDefinitionJson || item.ArrayItemDefinitionJson || ""
        };
    }

    function formatItemResult(item) {
        if (!item) {
            return "";
        }

        if (item.kind === "field") {
            return JSON.stringify({
                name: item.displayName || "",
                key: item.key || "",
                description: item.description || "",
                dataType: item.dataType || "",
                appliesTo: parseJsonText(item.appliesToJson, []),
                isRequired: item.isRequired,
                isActive: item.isActive,
                validation: parseJsonText(item.validationJson, {}),
                childrenDefinition: parseJsonText(item.childrenDefinitionJson, {}),
                arrayItemDataType: item.arrayItemDataType || "",
                arrayItemDefinition: parseJsonText(item.arrayItemDefinitionJson, {})
            }, null, 2);
        }

        if (item.kind === "type") {
            return JSON.stringify({
                key: item.key || "",
                name: item.displayName || "",
                description: item.description || "",
                versionNumber: item.versionNumber || "",
                baseType: item.baseType || "",
                comment: item.comment || "",
                versionComment: item.versionComment || item.comment || "",
                jsonSchema: parseJsonText(item.jsonSchema, {})
            }, null, 2);
        }

        return JSON.stringify({
            key: item.key || "",
            name: item.displayName || "",
            description: item.description || "",
            version: item.versionNumber || "",
            versionComment: item.versionComment || "",
            metadata: parseJsonText(item.metadataJson, {}),
            jsonSchema: parseJsonText(item.jsonSchema, {})
        }, null, 2);
    }

    function prettyJson(value) {
        const parsed = parseJsonText(value, null);
        if (parsed === null) {
            return value || "";
        }

        return JSON.stringify(parsed, null, 2);
    }

    function parseJsonText(value, fallback) {
        if (!value) {
            return fallback;
        }

        try {
            return JSON.parse(value);
        } catch (error) {
            return fallback;
        }
    }

    function readBoolean(item, camelName, pascalName) {
        const value = item[camelName] !== undefined ? item[camelName] : item[pascalName];
        return value === true || value === "true" || value === "True";
    }

    function readNumber(item, camelName, pascalName) {
        const value = item[camelName] !== undefined ? item[camelName] : item[pascalName];
        const number = Number(value);
        return Number.isFinite(number) ? number : 0;
    }

    function normalizeKind(kind) {
        if (kind === "type" || kind === "field" || kind === "payload") {
            return kind;
        }
        return "payload";
    }

    function inferKind(path) {
        const value = path || "";
        if (value.indexOf("schema-types") >= 0) {
            return "type";
        }
        if (value.indexOf("metadata-fields") >= 0) {
            return "field";
        }
        return "payload";
    }

    function escapeHtml(value) {
        return String(value || "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;");
    }
}());
