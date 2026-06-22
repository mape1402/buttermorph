(function () {
    const definitionNode = document.getElementById("schema-metadata-definition");
    const hiddenInput = document.getElementById("schema-metadata-json");
    const openButton = document.getElementById("open-schema-metadata-btn");
    const saveButton = document.getElementById("save-schema-metadata-btn");
    const fieldHost = document.getElementById("schema-metadata-fields");
    const validationBox = document.getElementById("schema-metadata-validation");
    const summaryNode = document.getElementById("schema-metadata-summary");
    const form = document.getElementById("event-editor-form");
    let currentMetadata = readCurrentMetadata();

    if (!definitionNode || !hiddenInput || !openButton || !saveButton || !fieldHost) {
        return;
    }

    const definition = normalizeDefinition(readJson(definitionNode.textContent || "{}", { fields: [] }));
    window.ButterMorphPayloadMetadataSync = validateAndSyncMetadata;
    renderFields();
    updateSummary();

    openButton.addEventListener("click", function () {
        currentMetadata = readCurrentMetadata();
        renderFields();
        clearValidation();
        openModal("schema-metadata-modal");
    });

    saveButton.addEventListener("click", function () {
        const result = collectMetadata();
        if (!result.succeeded) {
            showValidation(result.errors);
            return;
        }

        currentMetadata = result.metadata;
        hiddenInput.value = JSON.stringify(currentMetadata);
        updateSummary();
        closeModal("schema-metadata-modal");
    });

    form?.addEventListener("submit", function (event) {
        if (new URLSearchParams(window.location.search).get("popup") === "true") {
            return;
        }

        if (!validateAndSyncMetadata()) {
            event.preventDefault();
        }
    });

    function validateAndSyncMetadata() {
        const result = collectMetadata();
        if (!result.succeeded) {
            currentMetadata = readCurrentMetadata();
            renderFields();
            showValidation(result.errors);
            openModal("schema-metadata-modal");
            return false;
        }

        currentMetadata = result.metadata;
        hiddenInput.value = JSON.stringify(currentMetadata);
        updateSummary();
        return true;
    }

    function renderFields() {
        fieldHost.innerHTML = "";
        if (definition.fields.length === 0) {
            const empty = document.createElement("p");
            empty.className = "text-muted";
            empty.textContent = "No metadata fields configured.";
            fieldHost.appendChild(empty);
            return;
        }

        definition.fields.forEach(function (field) {
            fieldHost.appendChild(renderField(field, readValueNode(field.key), "schema"));
        });
    }

    function renderField(field, existing, path) {
        const wrapper = document.createElement("section");
        wrapper.className = "schema-metadata-field";
        wrapper.dataset.key = field.key;
        wrapper.dataset.type = field.dataType;
        wrapper.dataset.path = path + "." + field.key;
        wrapper.dataset.required = field.isRequired ? "true" : "false";

        const header = document.createElement("div");
        header.className = "schema-metadata-field-header";
        const label = document.createElement("label");
        label.className = "form-label";
        label.textContent = field.name || field.key;
        if (field.isRequired) {
            const mark = document.createElement("span");
            mark.className = "schema-metadata-required";
            mark.textContent = " requerido";
            label.appendChild(mark);
        }
        header.appendChild(label);

        if (field.description) {
            const help = document.createElement("small");
            help.className = "text-muted";
            help.textContent = field.description;
            header.appendChild(help);
        }

        wrapper.appendChild(header);
        wrapper.appendChild(renderValueInput(field, existing));
        return wrapper;
    }

    function renderValueInput(field, existing) {
        const type = normalizeType(field.dataType);
        if (type === "object") {
            return renderObjectInput(field, existing);
        }

        if (type === "array") {
            return renderArrayInput(field, existing);
        }

        return renderScalarInput(field, existing);
    }

    function renderObjectInput(field, existing) {
        const container = document.createElement("div");
        container.className = "schema-metadata-object";
        const value = existing && typeof existing.value === "object" && !Array.isArray(existing.value) ? existing.value : {};
        field.children.forEach(function (child) {
            const childExisting = value[child.key] || {};
            container.appendChild(renderField(child, childExisting, field.key));
        });
        return container;
    }

    function renderArrayInput(field, existing) {
        const container = document.createElement("div");
        container.className = "schema-metadata-array";
        const itemDefinition = normalizeField(field.arrayItem || { key: "item", name: "Item", dataType: "String" });
        const list = document.createElement("div");
        list.className = "schema-metadata-array-list";
        container.appendChild(list);

        const values = existing && Array.isArray(existing.value) ? existing.value : [];
        values.forEach(function (value) {
            list.appendChild(renderArrayItem(itemDefinition, value));
        });

        const addButton = document.createElement("button");
        addButton.type = "button";
        addButton.className = "btn btn-secondary btn-sm";
        addButton.textContent = "Agregar item";
        addButton.addEventListener("click", function () {
            list.appendChild(renderArrayItem(itemDefinition, createEmptyValue(itemDefinition)));
        });
        container.appendChild(addButton);
        return container;
    }

    function renderArrayItem(itemDefinition, value) {
        const row = document.createElement("div");
        row.className = "schema-metadata-array-item";
        row.dataset.arrayItem = "true";
        row.dataset.type = normalizeType(itemDefinition.dataType);
        row.appendChild(renderValueInput(itemDefinition, normalizeExistingValue(itemDefinition, value)));

        const remove = document.createElement("button");
        remove.type = "button";
        remove.className = "btn btn-outline-danger btn-sm";
        remove.textContent = "×";
        remove.addEventListener("click", function () {
            row.remove();
        });
        row.appendChild(remove);
        return row;
    }

    function renderScalarInput(field, existing) {
        const type = normalizeType(field.dataType);
        if (type === "boolean") {
            const label = document.createElement("label");
            label.className = "form-check";
            const input = document.createElement("input");
            input.type = "checkbox";
            input.className = "form-check-input schema-metadata-value";
            input.checked = existing && existing.value === true;
            label.appendChild(input);
            label.appendChild(document.createTextNode(" Activo"));
            return label;
        }

        const input = document.createElement("input");
        input.className = "form-control schema-metadata-value";
        input.value = existing && existing.value !== undefined ? String(existing.value) : readDefaultValue(field);

        if (type === "number") {
            input.type = "number";
            input.step = "any";
            return input;
        }

        if (type === "integer") {
            input.type = "number";
            input.step = "1";
            return input;
        }

        if (type === "date") {
            input.type = "date";
            return input;
        }

        if (type === "datetime") {
            input.type = "datetime-local";
            return input;
        }

        input.type = "text";
        return input;
    }

    function collectMetadata() {
        const errors = [];
        const metadata = {};
        definition.fields.forEach(function (field) {
            const element = findFieldElement(fieldHost, field.key);
            const value = collectFieldValue(field, element, errors);
            if (!isEmptyValue(value.value) || field.isRequired) {
                metadata[field.key] = value;
            }
        });

        return {
            succeeded: errors.length === 0,
            errors: errors,
            metadata: metadata
        };
    }

    function collectFieldValue(field, element, errors) {
        const type = normalizeType(field.dataType);
        let value;
        if (type === "object") {
            value = {};
            field.children.forEach(function (child) {
                const childElement = findFieldElement(element, child.key);
                const childValue = collectFieldValue(child, childElement, errors);
                if (!isEmptyValue(childValue.value) || child.isRequired) {
                    value[child.key] = childValue;
                }
            });
        } else if (type === "array") {
            value = collectArrayValue(field, element, errors);
        } else {
            value = readScalarValue(type, element);
        }

        if (field.isRequired && isEmptyValue(value)) {
            errors.push((field.name || field.key) + " is required.");
        }

        return {
            type: type,
            value: value
        };
    }

    function collectArrayValue(field, element, errors) {
        const itemDefinition = normalizeField(field.arrayItem || { key: "item", name: "Item", dataType: "String" });
        const values = [];
        element.querySelectorAll(":scope > .schema-metadata-array > .schema-metadata-array-list > .schema-metadata-array-item").forEach(function (item) {
            values.push(collectArrayItemValue(itemDefinition, item, errors));
        });
        return values;
    }

    function collectArrayItemValue(itemDefinition, item, errors) {
        const type = normalizeType(itemDefinition.dataType);
        if (type === "object") {
            const value = {};
            itemDefinition.children.forEach(function (child) {
                const childElement = findFieldElement(item, child.key);
                const childValue = collectFieldValue(child, childElement, errors);
                if (!isEmptyValue(childValue.value) || child.isRequired) {
                    value[child.key] = childValue;
                }
            });
            return value;
        }

        if (type === "array") {
            const nested = collectFieldValue(itemDefinition, item, errors);
            return nested.value;
        }

        return readScalarValue(type, item);
    }

    function readScalarValue(type, element) {
        const input = element.querySelector(".schema-metadata-value");
        if (!input) {
            return "";
        }

        if (type === "boolean") {
            return input.checked;
        }

        if (type === "number") {
            if (input.value === "") {
                return "";
            }
            return Number(input.value);
        }

        if (type === "integer") {
            if (input.value === "") {
                return "";
            }
            return parseInt(input.value, 10);
        }

        return input.value || "";
    }

    function findFieldElement(root, key) {
        return root.querySelector(":scope > .schema-metadata-field[data-key='" + cssEscape(key) + "']") ||
            root.querySelector(":scope > .schema-metadata-object > .schema-metadata-field[data-key='" + cssEscape(key) + "']");
    }

    function readCurrentMetadata() {
        return readJson(hiddenInput ? hiddenInput.value : "{}", {});
    }

    function readValueNode(key) {
        const existing = currentMetadata[key];
        if (existing && typeof existing === "object" && existing.type !== undefined) {
            return existing;
        }

        return {};
    }

    function normalizeExistingValue(field, value) {
        if (value && typeof value === "object" && value.type !== undefined && value.value !== undefined) {
            return value;
        }

        return {
            type: normalizeType(field.dataType),
            value: value
        };
    }

    function createEmptyValue(field) {
        const type = normalizeType(field.dataType);
        if (type === "object") {
            return {};
        }
        if (type === "array") {
            return [];
        }
        if (type === "boolean") {
            return false;
        }
        return "";
    }

    function isEmptyValue(value) {
        if (value === false) {
            return false;
        }
        if (value === null || value === undefined || value === "") {
            return true;
        }
        if (Array.isArray(value)) {
            return value.length === 0;
        }
        if (typeof value === "object") {
            return Object.keys(value).length === 0;
        }
        return false;
    }

    function showValidation(errors) {
        validationBox.classList.remove("d-none");
        validationBox.innerHTML = errors.map(escapeHtml).join("<br>");
    }

    function clearValidation() {
        validationBox.classList.add("d-none");
        validationBox.textContent = "";
    }

    function updateSummary() {
        if (!summaryNode) {
            return;
        }
        const total = definition.fields.length;
        const captured = Object.keys(readCurrentMetadata()).length;
        if (total === 0) {
            summaryNode.textContent = "No metadata fields configured.";
            openButton.disabled = true;
            return;
        }
        openButton.disabled = false;
        summaryNode.textContent = captured + " of " + total + " metadata fields captured.";
    }

    function openModal(id) {
        const modal = document.getElementById(id);
        if (modal) {
            modal.classList.add("show");
            modal.setAttribute("aria-hidden", "false");
        }
    }

    function closeModal(id) {
        const modal = document.getElementById(id);
        if (modal) {
            modal.classList.remove("show");
            modal.setAttribute("aria-hidden", "true");
        }
    }

    function normalizeDefinition(value) {
        const fields = Array.isArray(value.fields || value.Fields) ? value.fields || value.Fields : [];
        return {
            fields: fields.map(normalizeField).filter(function (field) { return field.key; })
        };
    }

    function normalizeField(value) {
        const field = value || {};
        const children = Array.isArray(field.children || field.Children) ? field.children || field.Children : [];
        return {
            key: field.key || field.Key || "",
            name: field.name || field.Name || field.key || field.Key || "",
            description: field.description || field.Description || "",
            dataType: normalizeType(field.dataType || field.DataType || "String"),
            isRequired: field.isRequired === true || field.IsRequired === true,
            defaultValue: field.defaultValue || field.DefaultValue || "",
            children: children.map(normalizeField).filter(function (child) { return child.key; }),
            arrayItem: field.arrayItem || field.ArrayItem ? normalizeField(field.arrayItem || field.ArrayItem) : null
        };
    }

    function normalizeType(value) {
        const text = String(value || "String").toLowerCase();
        if (text === "bool") {
            return "boolean";
        }
        if (text === "datetime") {
            return "datetime";
        }
        if (text === "object") {
            return "object";
        }
        if (text === "array") {
            return "array";
        }
        if (text === "number" || text === "integer" || text === "boolean" || text === "date") {
            return text;
        }
        return "string";
    }

    function readDefaultValue(field) {
        if (!field.defaultValue) {
            return "";
        }
        const parsed = readJson(field.defaultValue, field.defaultValue);
        if (typeof parsed === "object") {
            return "";
        }
        return String(parsed);
    }

    function readJson(text, fallback) {
        if (!text) {
            return fallback;
        }
        try {
            return JSON.parse(text);
        } catch (error) {
            return fallback;
        }
    }

    function cssEscape(value) {
        if (window.CSS && window.CSS.escape) {
            return window.CSS.escape(value);
        }
        return String(value || "").replace(/'/g, "\\'");
    }

    function escapeHtml(value) {
        return String(value || "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;");
    }
}());
