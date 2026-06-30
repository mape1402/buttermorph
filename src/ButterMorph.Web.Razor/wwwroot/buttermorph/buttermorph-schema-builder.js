(function () {
    const form = document.getElementById("event-editor-form");
    const rootList = document.getElementById("schema-root-fields");
    const addRootButton = document.getElementById("add-root-field");
    const template = document.getElementById("schema-field-template");
    const hiddenSchemaInput = document.getElementById("payload-schema-json");
    const catalog = mergeCatalogs(defaultCatalog(), readCatalog("schema-type-catalog", []));
    const metadataCatalog = readCatalog("field-metadata-catalog", []);
    const modalStack = [];
    const modalBaseZIndex = 2000;
    const modalZIndexStep = 20;
    let activeMetadataField = null;
    let activeValidationField = null;
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

    const saveValidationButton = document.getElementById("save-field-validation-btn");
    if (saveValidationButton) {
        saveValidationButton.addEventListener("click", saveValidation);
    }

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
        Object.entries(schema.properties).forEach(function (entry) {
            rootList.appendChild(createPopulatedFieldNode(entry[0], entry[1], true, entry[1].required === true));
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
        const validationButton = field.querySelector(".field-validation-btn");
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
        validationButton?.addEventListener("click", function () { openValidation(field); });
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
            openObjectEditor(field.querySelector(".child-fields-list"), createFieldPath(field, "Object"), field);
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
        basic.label = "Basic";
        const custom = document.createElement("optgroup");
        custom.label = "Custom";
        catalog.forEach(function (item) {
            const normalized = normalizeCatalogItem(item);
            if (!isValidCatalogItem(normalized)) {
                return;
            }
            const option = document.createElement("option");
            option.value = normalized.isSystem && !normalized.typeVersionId ? normalized.baseType : normalized.typeVersionId;
            option.textContent = normalized.isSystem ? normalized.name : normalized.name + " (" + normalized.versionNumber + ")";
            option.dataset.baseType = normalized.baseType || normalized.name;
            option.dataset.isSystem = normalized.isSystem ? "true" : "false";
            option.dataset.typeId = normalized.typeId || "";
            option.dataset.typeVersionId = normalized.typeVersionId || "";
            option.dataset.definitionKey = normalized.definitionKey || "";
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
        const validationButton = field.querySelector(".field-validation-btn");
        arrayObjectBuilder?.classList.toggle("d-none", itemType !== "object");
        nestedArrayBuilder?.classList.toggle("d-none", itemType !== "array");
        editArrayButton?.classList.toggle("d-none", type !== "array" || itemType !== "object");
        validationButton?.classList.toggle("d-none", getValidationKeys(field).length === 0);
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
            Object.entries(definition.properties || {}).forEach(function (entry) {
                list.appendChild(createPopulatedFieldNode(entry[0], entry[1], true, entry[1].required === true));
            });
        }
        if (definition.type === "array") {
            const items = definition.items || { type: "string" };
            setSelectFromDefinition(field.querySelector(".array-item-type-select"), items);
            if (items.type === "object") {
                const list = field.querySelector(".array-object-fields-list");
                Object.entries(items.properties || {}).forEach(function (entry) {
                    list.appendChild(createPopulatedFieldNode(entry[0], entry[1], true, entry[1].required === true));
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
        Array.from(list.children).forEach(function (field) {
            const built = buildField(field, defs);
            if (!built.name) {
                return;
            }
            schema.properties[built.name] = built.definition;
        });
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
            const definitionKey = option.dataset.definitionKey || version;
            if (option.dataset.jsonSchema) {
                defs[definitionKey] = safeJson(option.dataset.jsonSchema);
            }
            return { $ref: "#/$defs/" + definitionKey, typeId: option.dataset.typeId || "", typeVersionId: version };
        }
        return { type: baseType };
    }

    function safeJson(json) {
        try { return JSON.parse(json || "{}"); } catch (error) { return {}; }
    }

    function normalizeCatalogItem(item) {
        const baseType = item.baseType || item.BaseType || item.name || item.Name || "string";
        const schema = item.jsonSchema || item.JsonSchema || "{\"type\":\"" + baseType + "\"}";
        return {
            typeId: item.typeId || item.TypeId || "",
            typeVersionId: item.typeVersionId || item.TypeVersionId || "",
            name: item.name || item.Name || "",
            versionNumber: item.versionNumber || item.VersionNumber || "",
            baseType: baseType,
            jsonSchema: schema,
            definitionKey: item.definitionKey || item.DefinitionKey || createDefinitionKey(item),
            isSystem: item.isSystem === true || item.IsSystem === true
        };
    }

    function mergeCatalogs(basicCatalog, customCatalog) {
        const result = [];
        const seen = new Set();
        basicCatalog.concat(customCatalog || []).forEach(function (item) {
            const normalized = normalizeCatalogItem(item);
            const key = normalized.isSystem ? "system:" + normalized.baseType : "custom:" + normalized.typeVersionId;
            if (seen.has(key) || !isValidCatalogItem(normalized)) {
                return;
            }

            seen.add(key);
            result.push(item);
        });
        return result;
    }

    function createDefinitionKey(item) {
        const name = item.name || item.Name || "";
        const version = item.versionNumber || item.VersionNumber || "";
        return name && version ? name + "@" + version : "";
    }

    function isValidCatalogItem(item) {
        if (item.isSystem) {
            return !!item.name && !!item.baseType;
        }

        return !!item.name &&
            !!item.baseType &&
            !!item.typeVersionId;
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
            title: title || "Object Properties",
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
            titleNode.textContent = context.title || "Object Properties";
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
            button.textContent = context.title || "Object";
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

    function openValidation(field) {
        activeValidationField = field;
        renderValidationModal(field);
        openModal(document.getElementById("field-validation-modal"));
    }

    function renderValidationModal(field) {
        const validationHost = document.getElementById("field-validation-fields");
        if (!validationHost) {
            return;
        }

        validationHost.innerHTML = "";
        const validation = safeJson(field.dataset.validation || "{}");
        getValidationKeys(field).forEach(function (key) {
            validationHost.appendChild(createValidationInput(key, validation[key] || ""));
        });
        if (validationHost.children.length === 0) {
            const empty = document.createElement("p");
            empty.className = "text-muted";
            empty.textContent = "No field validations are available for this data type.";
            validationHost.appendChild(empty);
        }
    }

    function getValidationKeys(field) {
        const type = getSelectedBaseType(field.querySelector(".field-type-select"));
        if (type === "string") {
            return ["minLength", "maxLength", "pattern"];
        }
        if (type === "number") {
            return ["minimum", "maximum", "precision", "scale"];
        }
        if (type === "integer") {
            return ["minimum", "maximum"];
        }
        if (type === "array") {
            return ["minItems", "maxItems"];
        }
        return [];
    }

    function openMetadata(field) {
        activeMetadataField = field;
        renderMetadataModal(field);
        openModal(document.getElementById("field-metadata-modal"));
    }

    function renderMetadataModal(field) {
        const metadataHost = document.getElementById("field-metadata-fields");
        if (!metadataHost) {
            return;
        }

        metadataHost.innerHTML = "";
        const metadata = safeJson(field.dataset.metadata || "{}");
        metadataCatalog.forEach(function (item) {
            if (!appliesToScope(readCatalogValue(item, "appliesToJson", "AppliesToJson"), "Field")) {
                return;
            }

            metadataHost.appendChild(createFieldMetadataInput(item, metadata));
        });
    }

    function createValidationInput(key, value) {
        const label = document.createElement("label");
        label.className = "col-md-4";
        label.innerHTML = "<span class='form-label'>" + formatValidationLabel(key) + "</span>";
        const input = document.createElement("input");
        input.className = "form-control";
        input.dataset.key = key;
        input.value = value;
        label.appendChild(input);
        return label;
    }

    function createFieldMetadataInput(item, metadata) {
        const key = readCatalogValue(item, "key", "Key");
        const dataType = (readCatalogValue(item, "dataType", "DataType") || "string").toLowerCase();
        const validation = safeJson(readCatalogValue(item, "validation", "Validation") || "{}");
        const definition = createMetadataDefinition(item);
        const wrapper = document.createElement("div");
        wrapper.className = "schema-metadata-field";
        wrapper.dataset.key = key;
        wrapper.dataset.type = dataType;

        const header = document.createElement("div");
        header.className = "schema-metadata-field-header";
        const title = document.createElement("strong");
        title.textContent = readCatalogValue(item, "name", "Name") || key;
        header.appendChild(title);
        wrapper.appendChild(header);

        const description = readCatalogValue(item, "description", "Description");
        const value = unwrapMetadataValue(metadata[key]);
        const allowedValues = Array.isArray(validation.allowedValues) ? validation.allowedValues :
            Array.isArray(validation.enum) ? validation.enum : [];
        let input = null;

        if (dataType === "object") {
            input = createMetadataObjectInput(definition, value);
        } else if (dataType === "array") {
            input = createMetadataArrayInput(definition, value);
        } else if (allowedValues.length > 0) {
            input = document.createElement("select");
            input.className = "form-control";
            const empty = document.createElement("option");
            empty.value = "";
            empty.textContent = "";
            input.appendChild(empty);
            allowedValues.forEach(function (allowedValue) {
                const option = document.createElement("option");
                option.value = String(allowedValue);
                option.textContent = String(allowedValue);
                option.selected = String(allowedValue) === String(value);
                input.appendChild(option);
            });
        } else {
            input = document.createElement("input");
            input.className = "form-control";
            input.value = value;
            if (dataType === "number") {
                input.type = "number";
                input.step = "any";
            } else if (dataType === "integer") {
                input.type = "number";
                input.step = "1";
            } else if (dataType === "boolean") {
                input.type = "checkbox";
                input.className = "form-check-input";
                input.checked = value === true || value === "true";
            } else if (dataType === "date") {
                input.type = "date";
            } else {
                input.type = "text";
            }
        }

        input.dataset.group = "metadata";
        input.dataset.key = key;
        input.dataset.type = dataType;
        wrapper.appendChild(input);
        if (description) {
            const help = document.createElement("small");
            help.className = "text-muted d-block mt-1";
            help.textContent = description;
            wrapper.appendChild(help);
        }

        return wrapper;
    }

    function createMetadataDefinition(item) {
        const key = readCatalogValue(item, "key", "Key");
        const dataType = (readCatalogValue(item, "dataType", "DataType") || "string").toLowerCase();
        return {
            key: key,
            name: readCatalogValue(item, "name", "Name") || key,
            dataType: dataType,
            children: readMetadataChildren(readCatalogValue(item, "childrenDefinitionJson", "ChildrenDefinitionJson")),
            arrayItem: readMetadataArrayItem(
                readCatalogValue(item, "arrayItemDataType", "ArrayItemDataType"),
                readCatalogValue(item, "arrayItemDefinitionJson", "ArrayItemDefinitionJson")),
            allowedValues: readAllowedValues(readCatalogValue(item, "validation", "Validation"))
        };
    }

    function readAllowedValues(validationJson) {
        const validation = safeJson(validationJson || "{}");
        if (Array.isArray(validation.allowedValues)) {
            return validation.allowedValues.map(function (value) { return String(value); });
        }
        if (Array.isArray(validation.enum)) {
            return validation.enum.map(function (value) { return String(value); });
        }
        return [];
    }

    function readMetadataChildren(json) {
        const schema = safeJson(json || "{}");
        const properties = schema.properties || {};
        return Object.keys(properties).map(function (name) {
            const property = properties[name] || {};
            return {
                key: name,
                name: name,
                description: property.description || "",
                dataType: (property.type || "string").toLowerCase(),
                isRequired: property.required === true,
                allowedValues: Array.isArray(property.allowedValues) ? property.allowedValues :
                    Array.isArray(property.enum) ? property.enum : [],
                children: readMetadataChildren(JSON.stringify(property)),
                arrayItem: property.items ? {
                    key: "item",
                    name: "Item",
                    dataType: (property.items.type || "string").toLowerCase(),
                    allowedValues: Array.isArray(property.items.allowedValues) ? property.items.allowedValues :
                        Array.isArray(property.items.enum) ? property.items.enum : [],
                    children: readMetadataChildren(JSON.stringify(property.items))
                } : null
            };
        });
    }

    function readMetadataArrayItem(itemType, itemJson) {
        const type = (itemType || "string").toLowerCase();
        const item = { key: "item", name: "Item", dataType: type, children: [] };
        if (type === "object") {
            item.children = readMetadataChildren(itemJson || "{}");
        }
        return item;
    }

    function createMetadataObjectInput(definition, value) {
        const container = document.createElement("div");
        container.className = "schema-metadata-object";
        const current = value && typeof value === "object" && !Array.isArray(value) ? value : {};
        definition.children.forEach(function (child) {
            container.appendChild(createNestedMetadataInput(child, current[child.key]));
        });
        return container;
    }

    function createMetadataArrayInput(definition, value) {
        const container = document.createElement("div");
        container.className = "schema-metadata-array";
        container.dataset.array = "true";
        const list = document.createElement("div");
        list.className = "schema-metadata-array-list";
        container.appendChild(list);
        const itemDefinition = definition.arrayItem || { key: "item", name: "Item", dataType: "string", children: [] };
        const values = Array.isArray(value) ? value : [];
        values.forEach(function (itemValue) {
            list.appendChild(createMetadataArrayItem(itemDefinition, itemValue));
        });
        const add = document.createElement("button");
        add.type = "button";
        add.className = "btn btn-secondary btn-sm";
        add.textContent = "Add Item";
        add.addEventListener("click", function () {
            list.appendChild(createMetadataArrayItem(itemDefinition, ""));
        });
        container.appendChild(add);
        return container;
    }

    function createMetadataArrayItem(definition, value) {
        const row = document.createElement("div");
        row.className = "schema-metadata-array-item";
        row.dataset.arrayItem = "true";
        row.appendChild(createNestedMetadataInput(definition, value));
        const remove = document.createElement("button");
        remove.type = "button";
        remove.className = "btn btn-outline-danger btn-sm";
        remove.textContent = "🗑";
        remove.addEventListener("click", function () { row.remove(); });
        row.appendChild(remove);
        return row;
    }

    function createNestedMetadataInput(definition, value) {
        const wrapper = document.createElement("div");
        wrapper.className = "schema-metadata-field";
        wrapper.dataset.key = definition.key;
        wrapper.dataset.type = definition.dataType || "string";
        const label = document.createElement("label");
        label.className = "form-label";
        label.textContent = definition.name || definition.key;
        wrapper.appendChild(label);
        let input;
        if (wrapper.dataset.type === "object") {
            input = createMetadataObjectInput(definition, value);
        } else if (wrapper.dataset.type === "array") {
            input = createMetadataArrayInput(definition, value);
        } else {
            const allowedValues = Array.isArray(definition.allowedValues) ? definition.allowedValues : [];
            if (allowedValues.length > 0) {
                input = document.createElement("select");
                input.className = "form-control";
                const empty = document.createElement("option");
                empty.value = "";
                empty.textContent = "";
                input.appendChild(empty);
                allowedValues.forEach(function (allowedValue) {
                    const option = document.createElement("option");
                    option.value = String(allowedValue);
                    option.textContent = String(allowedValue);
                    option.selected = String(allowedValue) === String(value);
                    input.appendChild(option);
                });
            } else {
                input = document.createElement("input");
                input.className = "form-control";
                input.value = value === undefined || value === null ? "" : String(value);
                input.type = wrapper.dataset.type === "number" || wrapper.dataset.type === "integer" ? "number" :
                    wrapper.dataset.type === "boolean" ? "checkbox" :
                    wrapper.dataset.type === "date" ? "date" :
                    wrapper.dataset.type === "datetime" ? "datetime-local" : "text";
                if (wrapper.dataset.type === "integer") {
                    input.step = "1";
                }
                if (wrapper.dataset.type === "number") {
                    input.step = "any";
                }
                if (wrapper.dataset.type === "boolean") {
                    input.className = "form-check-input";
                    input.checked = value === true || value === "true";
                }
            }
        }
        input.dataset.group = "metadata";
        input.dataset.key = definition.key;
        input.dataset.type = wrapper.dataset.type;
        wrapper.appendChild(input);
        return wrapper;
    }

    function readCatalogValue(item, camelName, pascalName) {
        return item[camelName] || item[pascalName] || "";
    }

    function appliesToScope(appliesToJson, scope) {
        if (!appliesToJson) {
            return false;
        }

        try {
            const values = JSON.parse(appliesToJson);
            return Array.isArray(values) && values.some(function (value) {
                return String(value).toLowerCase() === scope.toLowerCase();
            });
        } catch (error) {
            return false;
        }
    }

    function saveValidation() {
        if (!activeValidationField) {
            return;
        }
        const validation = {};
        document.querySelectorAll("#field-validation-fields input").forEach(function (input) {
            if (!input.value) {
                return;
            }
            validation[input.dataset.key] = input.value;
        });
        activeValidationField.dataset.validation = JSON.stringify(validation);
        closeModal("field-validation-modal");
    }

    function saveMetadata() {
        if (!activeMetadataField) {
            return;
        }
        const metadata = safeJson(activeMetadataField.dataset.metadata || "{}");
        document.querySelectorAll("#field-metadata-fields > .schema-metadata-field").forEach(function (field) {
            const key = field.dataset.key;
            const value = collectMetadataValue(field);
            if ((value === "" || value === null || value === undefined) && value !== false) {
                delete metadata[key];
                return;
            }
            metadata[key] = value;
        });
        activeMetadataField.dataset.metadata = JSON.stringify(metadata);
        closeModal("field-metadata-modal");
    }

    function unwrapMetadataValue(value) {
        if (value && typeof value === "object" && value.type !== undefined && value.value !== undefined) {
            return value.value;
        }

        return value === undefined || value === null ? "" : value;
    }

    function collectMetadataValue(field) {
        const type = field.dataset.type || field.querySelector("[data-type]")?.dataset.type || "string";
        if (type === "object") {
            const value = {};
            field.querySelectorAll(":scope > .schema-metadata-object > .schema-metadata-field").forEach(function (child) {
                const childValue = collectMetadataValue(child);
                if ((childValue === "" || childValue === null || childValue === undefined) && childValue !== false) {
                    return;
                }
                value[child.dataset.key] = childValue;
            });
            return value;
        }
        if (type === "array") {
            const values = [];
            field.querySelectorAll(":scope > .schema-metadata-array > .schema-metadata-array-list > .schema-metadata-array-item").forEach(function (item) {
                const child = item.querySelector(":scope > .schema-metadata-field");
                if (child) {
                    values.push(collectMetadataValue(child));
                }
            });
            return values;
        }
        const input = field.querySelector(":scope > input, :scope > select");
        if (!input) {
            return "";
        }
        if (type === "boolean") {
            return input.checked;
        }
        if (type === "number") {
            return input.value === "" ? "" : Number(input.value);
        }
        if (type === "integer") {
            return input.value === "" ? "" : parseInt(input.value, 10);
        }
        return input.value || "";
    }

    function openModal(modal) {
        if (modal) {
            const existingIndex = modalStack.indexOf(modal);
            if (existingIndex >= 0) {
                modalStack.splice(existingIndex, 1);
            }
            modalStack.push(modal);
            modal.style.zIndex = String(modalBaseZIndex + ((modalStack.length - 1) * modalZIndexStep));
            modal.classList.add("show");
        }
    }

    function closeModal(id) {
        const modal = document.getElementById(id);
        if (modal) {
            modal.classList.remove("show");
            modal.style.zIndex = "";
            removeModalFromStack(modal);
        }
        if (id === "object-schema-modal") {
            resetModalStackForObjectEditor();
            closeObjectEditor();
        }
    }

    function removeModalFromStack(modal) {
        const index = modalStack.indexOf(modal);
        if (index >= 0) {
            modalStack.splice(index, 1);
        }
        modalStack.forEach(function (stackedModal, stackedIndex) {
            stackedModal.style.zIndex = String(modalBaseZIndex + (stackedIndex * modalZIndexStep));
        });
    }

    function resetModalStackForObjectEditor() {
        for (let index = modalStack.length - 1; index >= 0; index -= 1) {
            if (modalStack[index].id === "object-schema-modal") {
                modalStack[index].style.zIndex = "";
                modalStack.splice(index, 1);
            }
        }
    }

    function updateSummaries() {
        document.querySelectorAll(".schema-field").forEach(function (field) {
            const objectSummary = field.querySelector(".object-summary");
            const arraySummary = field.querySelector(".array-object-summary");
            const childCount = field.querySelector(".child-fields-list")?.children.length || 0;
            const arrayCount = field.querySelector(".array-object-fields-list")?.children.length || 0;
            if (objectSummary) { objectSummary.textContent = childCount === 0 ? "No fields configured" : childCount + " fields configured"; }
            if (arraySummary) { arraySummary.textContent = arrayCount === 0 ? "No fields configured" : arrayCount + " fields configured"; }
        });
    }

    function formatValidationLabel(key) {
        const labels = {
            minLength: "Min Length",
            maxLength: "Max Length",
            pattern: "Pattern",
            minimum: "Minimum",
            maximum: "Maximum",
            precision: "Precision",
            scale: "Scale",
            minItems: "Min Items",
            maxItems: "Max Items"
        };
        return labels[key] || key;
    }
}());


