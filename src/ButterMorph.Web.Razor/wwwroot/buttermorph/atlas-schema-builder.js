(function () {
    const form = document.getElementById("event-editor-form");
    const rootList = document.getElementById("schema-root-fields");
    const addRootButton = document.getElementById("add-root-field");
    const template = document.getElementById("schema-field-template");
    const hiddenSchemaInput = document.getElementById("payload-schema-json");
    const catalog = readCatalog("schema-type-catalog", defaultCatalog());
    const metadataCatalog = readCatalog("field-metadata-catalog", []);
    let activeMetadataField = null;
    let activeObjectSchemaContext = null;
    let objectSchemaStack = [];

    if (!form || !rootList || !addRootButton || !template || !hiddenSchemaInput) {
        return;
    }

    addRootButton.addEventListener("click", function () {
        rootList.appendChild(createFieldNode());
    });

    window.ButterMorphPayloadSchemaSync = syncPayloadSchemaInput;

    form.addEventListener("submit", function () {
        syncPayloadSchemaInput();
    });

    function syncPayloadSchemaInput() {
        const defs = {};
        const schema = buildRootSchema(rootList, defs);
        if (Object.keys(defs).length > 0) {
            schema.$defs = defs;
        }
        hiddenSchemaInput.value = JSON.stringify(schema);
    }

    document.querySelectorAll("[data-modal-close]").forEach(function (button) {
        button.addEventListener("click", function () {
            closeModal(button.getAttribute("data-modal-close"));
        });
    });

    const saveMetadataButton = document.getElementById("save-field-metadata-btn");
    if (saveMetadataButton) {
        saveMetadataButton.addEventListener("click", saveMetadata);
    }

    const addModalFieldButton = document.getElementById("object-schema-add-field-btn");
    if (addModalFieldButton) {
        addModalFieldButton.addEventListener("click", function () {
            if (activeObjectSchemaContext && activeObjectSchemaContext.listNode) {
                activeObjectSchemaContext.listNode.appendChild(createFieldNode());
                updateSummaries();
            }
        });
    }

    const objectBackButton = document.getElementById("object-schema-back-btn");
    if (objectBackButton) {
        objectBackButton.addEventListener("click", navigateObjectBack);
    }

    hydrateFromHiddenSchema();

    function readCatalog(id, fallback) {
        const node = document.getElementById(id);
        if (!node) {
            return fallback;
        }
        try {
            const parsed = JSON.parse(node.textContent || "[]");
            return Array.isArray(parsed) ? parsed : fallback;
        } catch (error) {
            return fallback;
        }
    }

    function defaultCatalog() {
        return ["string", "number", "integer", "boolean", "object", "array"].map(function (type) {
            return { name: type, baseType: type, versionNumber: "1.0.0", isSystem: true };
        });
    }

    function hydrateFromHiddenSchema() {
        let schema = null;
        try {
            schema = JSON.parse(hiddenSchemaInput.value || "{}");
        } catch (error) {
            schema = null;
        }

        if (!schema || schema.type !== "object" || !schema.properties || Object.keys(schema.properties).length === 0) {
            rootList.appendChild(createFieldNode());
            return;
        }

        rootList.innerHTML = "";
        const required = Array.isArray(schema.required) ? schema.required : [];
        Object.entries(schema.properties).forEach(function (entry) {
            rootList.appendChild(createPopulatedFieldNode(entry[0], entry[1], true, required.includes(entry[0]) || entry[1].required === true));
        });
        updateSummaries();
    }

    function createFieldNode() {
        const fragment = template.content.cloneNode(true);
        const field = fragment.querySelector(".schema-field");
        populateField(field);
        return field;
    }

    function createFieldNodeWithoutName() {
        const node = createFieldNode();
        const nameContainer = node.querySelector(".field-name-input")?.closest(".col-md-3");
        const requiredContainer = node.querySelector(".field-required-input")?.closest(".col-md-2");
        if (nameContainer) {
            nameContainer.remove();
        }
        if (requiredContainer) {
            requiredContainer.remove();
        }
        node.dataset.noName = "1";
        return node;
    }

    function populateField(field) {
        const typeSelect = field.querySelector(".field-type-select");
        const itemTypeSelect = field.querySelector(".array-item-type-select");
        const removeButton = field.querySelector(".remove-field-btn");
        const metadataButton = field.querySelector(".field-metadata-btn");
        const addChildButton = field.querySelector(".add-child-field-btn");
        const addArrayButton = field.querySelector(".add-array-object-field-btn");
        const addNestedArrayButton = field.querySelector(".add-nested-array-item-btn");
        const editObjectButton = field.querySelector(".edit-object-fields-btn");
        const editArrayButton = field.querySelector(".edit-array-object-fields-btn");

        populateTypeSelect(typeSelect);
        populateTypeSelect(itemTypeSelect);

        removeButton?.addEventListener("click", function () {
            field.remove();
            updateSummaries();
        });
        metadataButton?.addEventListener("click", function () { openMetadata(field); });
        addChildButton?.addEventListener("click", function () {
            field.querySelector(".child-fields-list").appendChild(createFieldNode());
            updateSummaries();
        });
        addArrayButton?.addEventListener("click", function () {
            field.querySelector(".array-object-fields-list").appendChild(createFieldNode());
            updateSummaries();
        });
        addNestedArrayButton?.addEventListener("click", function () {
            field.querySelector(".nested-array-item-list").appendChild(createFieldNodeWithoutName());
            updateSummaries();
        });
        editObjectButton?.addEventListener("click", function () {
            openObjectEditor(field.querySelector(".child-fields-list"), createFieldPath(field, "Objeto"), field);
        });
        editArrayButton?.addEventListener("click", function () {
            openObjectEditor(field.querySelector(".array-object-fields-list"), createFieldPath(field, "Array") + "[]", field);
        });
        typeSelect?.addEventListener("change", function () { updateFieldUi(field); });
        itemTypeSelect?.addEventListener("change", function () { updateFieldUi(field); });
        field.querySelector(".field-name-input")?.addEventListener("input", updateSummaries);
        updateFieldUi(field);
    }

    function populateTypeSelect(select) {
        if (!select) {
            return;
        }
        select.innerHTML = "";
        const basic = document.createElement("optgroup");
        basic.label = "Basicos";
        const custom = document.createElement("optgroup");
        custom.label = "Personalizados";
        catalog.forEach(function (item) {
            const normalized = normalizeCatalogItem(item);
            const option = document.createElement("option");
            option.value = normalized.isSystem && !normalized.typeVersionId ? normalized.baseType : normalized.typeVersionId;
            option.textContent = normalized.isSystem ? normalized.name : normalized.name + " (" + normalized.versionNumber + ")";
            option.dataset.baseType = normalized.baseType || normalized.name;
            option.dataset.isSystem = normalized.isSystem ? "true" : "false";
            option.dataset.typeId = normalized.typeId || "";
            option.dataset.typeVersionId = normalized.typeVersionId || "";
            option.dataset.jsonSchema = normalized.jsonSchema || "";
            (normalized.isSystem ? basic : custom).appendChild(option);
        });
        select.appendChild(basic);
        if (custom.children.length > 0) {
            select.appendChild(custom);
        }
    }

    function updateFieldUi(field) {
        const type = getSelectedBaseType(field.querySelector(".field-type-select"));
        const objectBuilder = field.querySelector(".schema-object-builder");
        const arrayBuilder = field.querySelector(".schema-array-builder");
        const editObjectButton = field.querySelector(".edit-object-fields-btn");
        const editArrayButton = field.querySelector(".edit-array-object-fields-btn");
        objectBuilder?.classList.toggle("d-none", type !== "object");
        arrayBuilder?.classList.toggle("d-none", type !== "array");
        editObjectButton?.classList.toggle("d-none", type !== "object");
        const itemType = getSelectedBaseType(field.querySelector(".array-item-type-select"));
        const arrayObjectBuilder = field.querySelector(".schema-array-object-builder");
        const nestedArrayBuilder = field.querySelector(".schema-array-nested-builder");
        arrayObjectBuilder?.classList.toggle("d-none", itemType !== "object");
        nestedArrayBuilder?.classList.toggle("d-none", itemType !== "array");
        editArrayButton?.classList.toggle("d-none", type !== "array" || itemType !== "object");
        updateSummaries();
    }

    function getSelectedBaseType(select) {
        const selected = select?.selectedOptions?.[0];
        return selected?.dataset?.baseType || select?.value || "string";
    }

    function getFieldDisplayName(field, fallback) {
        const value = field.querySelector(".field-name-input")?.value || "";
        return value.trim() || fallback;
    }

    function createPopulatedFieldNode(name, definition, includeName, required) {
        const field = includeName ? createFieldNode() : createFieldNodeWithoutName();
        const nameInput = field.querySelector(".field-name-input");
        const descriptionInput = field.querySelector(".field-description-input");
        const requiredInput = field.querySelector(".field-required-input");
        const typeSelect = field.querySelector(".field-type-select");
        if (nameInput) {
            nameInput.value = name;
        }
        if (descriptionInput) {
            descriptionInput.value = definition.description || "";
        }
        if (requiredInput) {
            requiredInput.checked = !!required || definition.required === true;
        }
        setSelectFromDefinition(typeSelect, definition);
        field.dataset.metadata = JSON.stringify(definition.metadata || {});
        field.dataset.validation = JSON.stringify(readValidation(definition));
        if (definition.type === "object") {
            const list = field.querySelector(".child-fields-list");
            const requiredNames = Array.isArray(definition.required) ? definition.required : [];
            Object.entries(definition.properties || {}).forEach(function (entry) {
                list.appendChild(createPopulatedFieldNode(entry[0], entry[1], true, requiredNames.includes(entry[0]) || entry[1].required === true));
            });
        }
        if (definition.type === "array") {
            const items = definition.items || { type: "string" };
            setSelectFromDefinition(field.querySelector(".array-item-type-select"), items);
            if (items.type === "object") {
                const list = field.querySelector(".array-object-fields-list");
                const requiredNames = Array.isArray(items.required) ? items.required : [];
                Object.entries(items.properties || {}).forEach(function (entry) {
                    list.appendChild(createPopulatedFieldNode(entry[0], entry[1], true, requiredNames.includes(entry[0]) || entry[1].required === true));
                });
            }
            if (items.type === "array") {
                field.querySelector(".nested-array-item-list").appendChild(createPopulatedFieldNode("", items.items || { type: "string" }, false, false));
            }
        }
        updateFieldUi(field);
        return field;
    }

    function setSelectFromDefinition(select, definition) {
        if (!select) {
            return;
        }
        const version = definition.typeVersionId || "";
        const type = definition.type || "string";
        const match = Array.from(select.options).find(function (option) {
            return option.value === version || option.value === type || option.dataset.baseType === type;
        });
        if (match) {
            select.value = match.value;
        }
    }

    function readValidation(definition) {
        const result = {};
        ["minLength", "maxLength", "pattern", "minimum", "maximum", "precision", "scale", "minItems", "maxItems", "enum"].forEach(function (key) {
            if (definition[key] !== undefined) {
                result[key] = definition[key];
            }
        });
        return result;
    }

    function buildRootSchema(list, defs) {
        const schema = { type: "object", properties: {} };
        const required = [];
        Array.from(list.children).forEach(function (field) {
            const built = buildField(field, defs);
            if (!built.name) {
                return;
            }
            schema.properties[built.name] = built.definition;
            if (built.required) {
                required.push(built.name);
            }
        });
        if (required.length > 0) {
            schema.required = required;
        }
        return schema;
    }

    function buildField(field, defs) {
        const name = field.dataset.noName === "1" ? "" : (field.querySelector(".field-name-input")?.value || "").trim();
        const required = !!field.querySelector(".field-required-input")?.checked;
        const description = (field.querySelector(".field-description-input")?.value || "").trim();
        const selected = field.querySelector(".field-type-select")?.selectedOptions?.[0];
        const definition = createDefinitionFromOption(selected, defs);
        if (description) {
            definition.description = description;
        }
        if (required) {
            definition.required = true;
        }
        const metadata = safeJson(field.dataset.metadata || "{}");
        if (Object.keys(metadata).length > 0) {
            definition.metadata = metadata;
        }
        Object.assign(definition, safeJson(field.dataset.validation || "{}"));
        if (definition.type === "object") {
            const childSchema = buildRootSchema(field.querySelector(".child-fields-list"), defs);
            definition.properties = childSchema.properties;
            if (childSchema.required) {
                definition.required = childSchema.required;
            }
        }
        if (definition.type === "array") {
            definition.items = buildArrayItem(field, defs);
        }
        return { name: name, definition: definition, required: required };
    }

    function buildArrayItem(field, defs) {
        const selected = field.querySelector(".array-item-type-select")?.selectedOptions?.[0];
        const item = createDefinitionFromOption(selected, defs);
        if (item.type === "object") {
            const childSchema = buildRootSchema(field.querySelector(".array-object-fields-list"), defs);
            item.properties = childSchema.properties;
            if (childSchema.required) {
                item.required = childSchema.required;
            }
        }
        if (item.type === "array") {
            const nested = field.querySelector(".nested-array-item-list .schema-field");
            item.items = nested ? buildField(nested, defs).definition : { type: "string" };
        }
        return item;
    }

    function createDefinitionFromOption(option, defs) {
        if (!option) {
            return { type: "string" };
        }
        const baseType = option.dataset.baseType || option.value || "string";
        const version = option.dataset.typeVersionId || "";
        if (version) {
            if (option.dataset.jsonSchema) {
                defs[version] = safeJson(option.dataset.jsonSchema);
            }
            return { type: baseType, $ref: "#/$defs/" + version, typeId: option.dataset.typeId || "", typeVersionId: version };
        }
        return { type: baseType };
    }

    function safeJson(json) {
        try { return JSON.parse(json || "{}"); } catch (error) { return {}; }
    }

    function normalizeCatalogItem(item) {
        return {
            typeId: item.typeId || item.TypeId || "",
            typeVersionId: item.typeVersionId || item.TypeVersionId || "",
            name: item.name || item.Name || "",
            versionNumber: item.versionNumber || item.VersionNumber || "",
            baseType: item.baseType || item.BaseType || item.name || item.Name || "string",
            jsonSchema: item.jsonSchema || item.JsonSchema || "",
            isSystem: item.isSystem === true || item.IsSystem === true
        };
    }

    function openObjectEditor(list, title, ownerField) {
        if (!list) {
            return;
        }

        const context = createObjectContext(list, title, ownerField);
        objectSchemaStack.push(context);
        showObjectContext(context);
        const modal = document.getElementById("object-schema-modal");
        openModal(modal);
    }

    function createObjectContext(list, title, ownerField) {
        return {
            listNode: list,
            title: title || "Propiedades del objeto",
            ownerField: ownerField,
            homeParent: list.parentElement,
            homeNextSibling: list.nextSibling
        };
    }

    function showObjectContext(context) {
        const titleNode = document.getElementById("object-schema-modal-title");
        const host = document.getElementById("object-schema-fields-host");
        if (!context || !host) {
            return;
        }
        if (activeObjectSchemaContext && activeObjectSchemaContext !== context) {
            restoreObjectContext(activeObjectSchemaContext);
        }
        activeObjectSchemaContext = context;
        host.innerHTML = "";
        host.appendChild(context.listNode);
        context.listNode.classList.add("schema-fields-list");
        if (titleNode) {
            titleNode.textContent = context.title || "Propiedades del objeto";
        }
        renderObjectBreadcrumb();
        updateBackButton();
        updateSummaries();
    }

    function restoreObjectContext(context) {
        if (!context || !context.listNode || !context.homeParent) {
            return;
        }
        if (context.homeNextSibling && context.homeNextSibling.parentElement === context.homeParent) {
            context.homeParent.insertBefore(context.listNode, context.homeNextSibling);
            return;
        }
        context.homeParent.appendChild(context.listNode);
    }

    function navigateObjectBack() {
        if (objectSchemaStack.length <= 1) {
            return;
        }
        const current = objectSchemaStack.pop();
        restoreObjectContext(current);
        showObjectContext(objectSchemaStack[objectSchemaStack.length - 1]);
    }

    function closeObjectEditor() {
        if (activeObjectSchemaContext) {
            restoreObjectContext(activeObjectSchemaContext);
        }
        activeObjectSchemaContext = null;
        objectSchemaStack = [];
        const host = document.getElementById("object-schema-fields-host");
        if (host) {
            host.innerHTML = "";
        }
        updateBackButton();
        updateSummaries();
    }

    function renderObjectBreadcrumb() {
        const breadcrumb = document.getElementById("object-schema-breadcrumb");
        if (!breadcrumb) {
            return;
        }
        breadcrumb.innerHTML = "";
        objectSchemaStack.forEach(function (context, index) {
            const button = document.createElement("button");
            button.type = "button";
            button.textContent = context.title || "Objeto";
            button.className = index === objectSchemaStack.length - 1 ? "active" : "";
            button.disabled = index === objectSchemaStack.length - 1;
            button.addEventListener("click", function () {
                while (objectSchemaStack.length - 1 > index) {
                    restoreObjectContext(objectSchemaStack.pop());
                }
                showObjectContext(objectSchemaStack[index]);
            });
            breadcrumb.appendChild(button);
        });
    }

    function updateBackButton() {
        const button = document.getElementById("object-schema-back-btn");
        if (button) {
            button.disabled = objectSchemaStack.length <= 1;
        }
    }

    function createFieldPath(field, fallback) {
        const names = [];
        let current = field;
        while (current && current.classList && current.classList.contains("schema-field")) {
            names.unshift(getFieldDisplayName(current, fallback));
            const parentList = current.parentElement;
            const parentField = parentList ? parentList.closest(".schema-field") : null;
            current = parentField;
        }
        if (names.length === 0) {
            return fallback;
        }
        return names.join(" / ");
    }

    function openMetadata(field) {
        activeMetadataField = field;
        renderMetadataModal(field);
        openModal(document.getElementById("field-metadata-modal"));
    }

    function renderMetadataModal(field) {
        const validationHost = document.getElementById("field-validation-fields");
        const metadataHost = document.getElementById("field-metadata-fields");
        if (!validationHost || !metadataHost) {
            return;
        }
        validationHost.innerHTML = "";
        metadataHost.innerHTML = "";
        const validation = safeJson(field.dataset.validation || "{}");
        ["minLength", "maxLength", "pattern", "minimum", "maximum", "precision", "scale", "minItems", "maxItems"].forEach(function (key) {
            validationHost.appendChild(createMetadataInput(key, validation[key] || "", "validation"));
        });
        metadataCatalog.forEach(function (item) {
            const metadata = safeJson(field.dataset.metadata || "{}");
            metadataHost.appendChild(createMetadataInput(item.key || item.Key, metadata[item.key || item.Key] || "", "metadata"));
        });
    }

    function createMetadataInput(key, value, group) {
        const label = document.createElement("label");
        label.className = "col-md-4";
        label.innerHTML = "<span class='form-label'>" + key + "</span>";
        const input = document.createElement("input");
        input.className = "form-control";
        input.dataset.group = group;
        input.dataset.key = key;
        input.value = value;
        label.appendChild(input);
        return label;
    }

    function saveMetadata() {
        if (!activeMetadataField) {
            return;
        }
        const validation = {};
        const metadata = safeJson(activeMetadataField.dataset.metadata || "{}");
        document.querySelectorAll("#field-validation-fields input, #field-metadata-fields input").forEach(function (input) {
            if (!input.value) {
                return;
            }
            if (input.dataset.group === "validation") {
                validation[input.dataset.key] = input.value;
            } else {
                metadata[input.dataset.key] = input.value;
            }
        });
        activeMetadataField.dataset.validation = JSON.stringify(validation);
        activeMetadataField.dataset.metadata = JSON.stringify(metadata);
        closeModal("field-metadata-modal");
    }

    function openModal(modal) {
        if (modal) {
            modal.classList.add("show");
        }
    }

    function closeModal(id) {
        const modal = document.getElementById(id);
        if (modal) {
            modal.classList.remove("show");
        }
        if (id === "object-schema-modal") {
            closeObjectEditor();
        }
    }

    function updateSummaries() {
        document.querySelectorAll(".schema-field").forEach(function (field) {
            const objectSummary = field.querySelector(".object-summary");
            const arraySummary = field.querySelector(".array-object-summary");
            const childCount = field.querySelector(".child-fields-list")?.children.length || 0;
            const arrayCount = field.querySelector(".array-object-fields-list")?.children.length || 0;
            if (objectSummary) { objectSummary.textContent = childCount === 0 ? "Sin propiedades configuradas" : childCount + " propiedades configuradas"; }
            if (arraySummary) { arraySummary.textContent = arrayCount === 0 ? "Sin propiedades configuradas" : arrayCount + " propiedades configuradas"; }
        });
    }
}());
