(function () {
    const dataTypeSelect = document.getElementById("metadata-data-type");
    const allowedValuesHidden = document.getElementById("allowed-values-hidden");
    const allowedValueInput = document.getElementById("allowed-value-input");
    const allowedValuesChips = document.getElementById("allowed-values-chips");
    const appliesToHidden = document.querySelector("input[name='Input.AppliesTo']");
    const appliesToOptions = document.querySelectorAll(".metadata-applies-to-option");
    const childrenHidden = document.getElementById("metadata-children-definition-json");
    const arrayItemTypeHidden = document.getElementById("metadata-array-item-type-hidden");
    const arrayItemDefinitionHidden = document.getElementById("metadata-array-item-definition-json");
    const objectFields = document.getElementById("metadata-object-fields");
    const arrayObjectFields = document.getElementById("metadata-array-object-fields");
    const arrayItemType = document.getElementById("metadata-array-item-type");
    const arrayObjectWrap = document.getElementById("metadata-array-object-fields-wrap");
    const validationSection = document.querySelector(".metadata-validation-section");
    const allowedValuesSection = document.querySelector(".metadata-allowed-values-section");
    const fieldTemplate = document.getElementById("schema-field-template");
    const modalStack = [];
    const modalBaseZIndex = 2000;
    const modalZIndexStep = 20;
    let activeObjectSchemaContext = null;
    let objectSchemaStack = [];
    let activeValidationField = null;
    let allowedValues = readInitialAllowedValues();

    if (!dataTypeSelect) {
        return;
    }

    dataTypeSelect.addEventListener("change", refreshDesigner);
    arrayItemType?.addEventListener("change", refreshDesigner);
    appliesToOptions.forEach(function (option) {
        option.addEventListener("change", syncAppliesTo);
    });
    allowedValueInput?.addEventListener("keydown", function (event) {
        if (event.key === "Enter") {
            event.preventDefault();
            addAllowedValue();
        }
    });
    document.querySelector("[data-metadata-add-object-field]")?.addEventListener("click", function () {
        objectFields.appendChild(createDefinitionRow());
        syncDefinitions();
    });
    document.querySelector("[data-metadata-add-array-object-field]")?.addEventListener("click", function () {
        arrayObjectFields.appendChild(createDefinitionRow());
        syncDefinitions();
    });
    window.ButterMorphSchemaBeforeSave = validateMetadataFieldDesigner;

    document.querySelector("form")?.addEventListener("submit", function (event) {
        if (!validateMetadataFieldDesigner()) {
            event.preventDefault();
            return;
        }

        syncDefinitions();
    });
    document.querySelectorAll("[data-modal-close]").forEach(function (button) {
        button.addEventListener("click", function () {
            closeModal(button.getAttribute("data-modal-close"));
        });
    });
    document.getElementById("object-schema-add-field-btn")?.addEventListener("click", function () {
        if (activeObjectSchemaContext && activeObjectSchemaContext.listNode) {
            activeObjectSchemaContext.listNode.appendChild(createDefinitionRow());
            updateSummaries();
            syncDefinitions();
        }
    });
    document.getElementById("object-schema-back-btn")?.addEventListener("click", navigateObjectBack);
    document.getElementById("save-field-validation-btn")?.addEventListener("click", saveValidation);

    hydrateDefinitionRows(objectFields, childrenHidden?.value || "");
    hydrateDefinitionRows(arrayObjectFields, arrayItemDefinitionHidden?.value || "");
    if (arrayItemTypeHidden?.value && arrayItemType) {
        arrayItemType.value = arrayItemTypeHidden.value;
    }

    syncAppliesTo();
    syncAllowedValues();
    refreshDesigner();

    function syncAppliesTo() {
        if (!appliesToHidden) {
            return;
        }

        const selected = [];
        appliesToOptions.forEach(function (option) {
            if (option.checked) {
                selected.push(option.value);
            }
        });
        appliesToHidden.value = selected.join("\n");
    }

    function refreshDesigner() {
        const type = dataTypeSelect.value;
        const hasValidation = type === "string" || type === "number" || type === "integer" || type === "date" || type === "datetime";
        const hasAllowedValues = type === "string" || type === "number" || type === "integer";
        validationSection?.classList.toggle("d-none", !hasValidation);
        allowedValuesSection?.classList.toggle("d-none", !hasAllowedValues);
        document.querySelectorAll(".metadata-validation").forEach(function (node) {
            node.classList.add("d-none");
        });
        document.querySelectorAll(".metadata-validation-" + type).forEach(function (node) {
            node.classList.remove("d-none");
        });
        document.querySelectorAll(".metadata-validation-allowed-values").forEach(function (node) {
            node.classList.toggle("d-none", !hasAllowedValues);
        });
        document.querySelectorAll(".metadata-complex").forEach(function (node) {
            node.classList.add("d-none");
        });
        document.querySelectorAll(".metadata-complex-" + type).forEach(function (node) {
            node.classList.remove("d-none");
        });
        if (!hasAllowedValues) {
            allowedValues = [];
            syncAllowedValues();
        }
        refreshAllowedValueInput(type);
        if (arrayObjectWrap && arrayItemType) {
            arrayObjectWrap.classList.toggle("d-none", arrayItemType.value !== "object");
        }
        syncDefinitions();
    }

    function readInitialAllowedValues() {
        if (!allowedValuesHidden?.value) {
            return [];
        }
        return allowedValuesHidden.value.split(/\r?\n/).map(function (value) { return value.trim(); }).filter(function (value, index, values) { return value && values.indexOf(value) === index; });
    }

    function refreshAllowedValueInput(type) {
        if (!allowedValueInput) {
            return;
        }
        allowedValueInput.value = "";
        allowedValueInput.type = type === "number" || type === "integer" ? "number" : "text";
        allowedValueInput.step = type === "integer" ? "1" : type === "number" ? "any" : "";
        allowedValueInput.disabled = type === "boolean" || type === "date" || type === "datetime" || type === "object" || type === "array";
    }

    function addAllowedValue() {
        const value = normalizeAllowedValue(allowedValueInput.value, dataTypeSelect.value);
        if (!value || allowedValues.includes(value)) {
            allowedValueInput.value = "";
            return;
        }
        allowedValues.push(value);
        allowedValueInput.value = "";
        syncAllowedValues();
    }

    function normalizeAllowedValue(value, type) {
        const trimmed = value.trim();
        if (!trimmed) {
            return "";
        }
        if (type === "integer") {
            const parsed = Number.parseInt(trimmed, 10);
            return Number.isInteger(Number(trimmed)) ? String(parsed) : "";
        }
        if (type === "number") {
            const parsed = Number.parseFloat(trimmed);
            return Number.isNaN(parsed) ? "" : String(parsed);
        }
        return trimmed;
    }

    function removeAllowedValue(value) {
        allowedValues = allowedValues.filter(function (item) { return item !== value; });
        syncAllowedValues();
    }

    function syncAllowedValues() {
        if (allowedValuesHidden) {
            allowedValuesHidden.value = allowedValues.join("\n");
        }
        renderAllowedValueChips();
    }

    function renderAllowedValueChips() {
        if (!allowedValuesChips) {
            return;
        }
        allowedValuesChips.innerHTML = "";
        allowedValues.forEach(function (value) {
            const chip = document.createElement("span");
            chip.className = "badge";
            chip.textContent = value;
            const remove = document.createElement("button");
            remove.type = "button";
            remove.textContent = "x";
            remove.setAttribute("aria-label", "Remove " + value);
            remove.addEventListener("click", function () { removeAllowedValue(value); });
            chip.appendChild(remove);
            allowedValuesChips.appendChild(chip);
        });
    }

    function createDefinitionRow(name, type, description, required, definition) {
        const row = fieldTemplate.content.firstElementChild.cloneNode(true);
        row.classList.add("metadata-definition-row");
        row.querySelector(".field-metadata-btn")?.remove();
        row.querySelector(".add-child-field-btn")?.remove();
        row.querySelector(".add-array-object-field-btn")?.remove();
        row.querySelector(".add-nested-array-item-btn")?.remove();
        ensureMetadataTypeOptions(row.querySelector(".field-type-select"));
        ensureMetadataTypeOptions(row.querySelector(".array-item-type-select"));
        row.querySelector(".field-name-input").classList.add("metadata-definition-name");
        row.querySelector(".field-type-select").classList.add("metadata-definition-type");
        row.querySelector(".field-description-input").classList.add("metadata-definition-description");
        row.querySelector(".field-required-input").classList.add("metadata-definition-required-input");
        row.querySelector(".child-fields-list").classList.add("metadata-definition-nested");
        row.querySelector(".array-item-type-select").classList.add("metadata-definition-array-type");
        row.querySelector(".array-object-fields-list").classList.add("metadata-definition-array-children");
        row.querySelector(".schema-array-builder").classList.add("metadata-definition-array");
        row.querySelector(".field-name-input").value = name || "";
        row.querySelector(".field-type-select").value = type || "string";
        row.querySelector(".field-description-input").value = description || "";
        row.querySelector(".field-required-input").checked = required === true;
        row.dataset.validation = JSON.stringify(readValidation(definition || {}));

        row.querySelector(".remove-field-btn")?.addEventListener("click", function () {
            row.remove();
            syncDefinitions();
            updateSummaries();
        });
        row.querySelector(".field-validation-btn")?.addEventListener("click", function () {
            openValidation(row);
        });
        row.querySelector(".edit-object-fields-btn")?.addEventListener("click", function () {
            openObjectEditor(row.querySelector(".child-fields-list"), createFieldPath(row, "Object"), row);
        });
        row.querySelector(".edit-array-object-fields-btn")?.addEventListener("click", function () {
            openObjectEditor(row.querySelector(".array-object-fields-list"), createFieldPath(row, "Array") + "[]", row);
        });
        row.querySelectorAll("input, select").forEach(function (input) {
            input.addEventListener("input", function () {
                updateSummaries();
                syncDefinitions();
            });
            input.addEventListener("change", function () {
                refreshDefinitionRow(row);
                updateSummaries();
                syncDefinitions();
            });
        });
        hydrateNestedDefinition(row, definition || {});
        refreshDefinitionRow(row);
        updateSummaries();
        return row;
    }

    function ensureMetadataTypeOptions(select) {
        if (!select) {
            return;
        }
        ["date", "datetime"].forEach(function (type) {
            if (Array.from(select.options).some(function (option) { return option.value === type; })) {
                return;
            }
            const option = document.createElement("option");
            option.value = type;
            option.textContent = type;
            const objectOption = Array.from(select.options).find(function (item) { return item.value === "object"; });
            select.insertBefore(option, objectOption || null);
        });
    }

    function openObjectEditor(listNode, title, ownerField) {
        if (!listNode) {
            return;
        }
        const context = {
            listNode: listNode,
            ownerField: ownerField,
            title: title || "Object Properties",
            homeParent: listNode.parentElement,
            homeNextSibling: listNode.nextSibling
        };
        objectSchemaStack.push(context);
        showObjectContext(context);
        openModal(document.getElementById("object-schema-modal"));
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
        syncDefinitions();
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
            names.unshift(current.querySelector(".field-name-input")?.value?.trim() || fallback);
            const parentList = current.parentElement;
            current = parentList ? parentList.closest(".schema-field") : null;
        }
        return names.length === 0 ? fallback : names.join(" / ");
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
        const validation = readJson(field.dataset.validation || "{}", {});
        getValidationKeys(field).forEach(function (key) {
            validationHost.appendChild(createValidationInput(key, validation[key] || ""));
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

    function saveValidation() {
        if (!activeValidationField) {
            return;
        }
        const validation = {};
        document.querySelectorAll("#field-validation-fields input").forEach(function (input) {
            if (input.value) {
                validation[input.dataset.key] = input.value;
            }
        });
        activeValidationField.dataset.validation = JSON.stringify(validation);
        closeModal("field-validation-modal");
        syncDefinitions();
    }

    function getValidationKeys(row) {
        const type = row.querySelector(".metadata-definition-type")?.value || "string";
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
        if (type === "date" || type === "datetime") {
            return ["dateMinimum", "dateMaximum"];
        }
        return [];
    }

    function readValidation(definition) {
        const validation = {};
        ["minLength", "maxLength", "pattern", "minimum", "maximum", "precision", "scale", "minItems", "maxItems", "dateMinimum", "dateMaximum"].forEach(function (key) {
            if (definition[key] !== undefined && definition[key] !== null && definition[key] !== "") {
                validation[key] = definition[key];
            }
        });
        return validation;
    }

    function applyValidation(definition, validation) {
        Object.keys(validation || {}).forEach(function (key) {
            if (validation[key] !== "") {
                definition[key] = validation[key];
            }
        });
    }

    function formatValidationLabel(key) {
        return key.replace(/([A-Z])/g, " $1").replace(/^./, function (letter) { return letter.toUpperCase(); });
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
        document.querySelectorAll(".metadata-definition-row").forEach(function (row) {
            const objectSummary = row.querySelector(".object-summary");
            const arraySummary = row.querySelector(".array-object-summary");
            const childCount = row.querySelector(".child-fields-list")?.children.length || 0;
            const arrayCount = row.querySelector(".array-object-fields-list")?.children.length || 0;
            if (objectSummary) {
                objectSummary.textContent = childCount === 0 ? "No fields configured" : childCount + " fields configured";
            }
            if (arraySummary) {
                arraySummary.textContent = arrayCount === 0 ? "No fields configured" : arrayCount + " fields configured";
            }
        });
    }

    function hydrateNestedDefinition(row, definition) {
        const type = definition.type || row.querySelector(".metadata-definition-type")?.value || "string";
        row.querySelector(".metadata-definition-type").value = type;
        if (type === "object") {
            hydrateDefinitionRows(row.querySelector(".metadata-definition-nested"), JSON.stringify(definition));
        }
        if (type === "array") {
            const item = definition.items || { type: "string" };
            row.querySelector(".metadata-definition-array-type").value = item.type || "string";
            if ((item.type || "") === "object") {
                hydrateDefinitionRows(row.querySelector(".metadata-definition-array-children"), JSON.stringify(item));
            }
        }
    }

    function hydrateDefinitionRows(host, json) {
        if (!host) {
            return;
        }
        host.innerHTML = "";
        const schema = readJson(json, {});
        const properties = schema.properties || {};
        Object.keys(properties).forEach(function (key) {
            const definition = properties[key] || {};
            host.appendChild(createDefinitionRow(key, definition.type || "string", definition.description || "", definition.required === true, definition));
        });
    }

    function refreshDefinitionRow(row) {
        const type = row.querySelector(".metadata-definition-type")?.value || "string";
        const nested = row.querySelector(".metadata-definition-nested");
        const array = row.querySelector(".metadata-definition-array");
        const arrayType = row.querySelector(".metadata-definition-array-type")?.value || "string";
        const arrayChildren = row.querySelector(".metadata-definition-array-children");
        const objectButton = row.querySelector(".edit-object-fields-btn");
        const arrayButton = row.querySelector(".edit-array-object-fields-btn");
        const validationButton = row.querySelector(".field-validation-btn");
        nested?.classList.toggle("d-none", type !== "object");
        array?.classList.toggle("d-none", type !== "array");
        arrayChildren?.classList.toggle("d-none", type !== "array" || arrayType !== "object");
        objectButton?.classList.toggle("d-none", type !== "object");
        arrayButton?.classList.toggle("d-none", type !== "array" || arrayType !== "object");
        validationButton?.classList.toggle("d-none", getValidationKeys(row).length === 0);
    }

    function syncDefinitions() {
        if (childrenHidden) {
            childrenHidden.value = JSON.stringify(buildDefinition(objectFields));
        }
        if (arrayItemTypeHidden && arrayItemType) {
            arrayItemTypeHidden.value = arrayItemType.value;
        }
        if (arrayItemDefinitionHidden) {
            arrayItemDefinitionHidden.value = JSON.stringify(buildDefinition(arrayObjectFields));
        }
    }

    function validateMetadataFieldDesigner() {
        syncAppliesTo();
        syncDefinitions();

        const errors = [];
        requireInput("Input_Key", "Key", errors);
        requireInput("Input_Name", "Name", errors);
        requireInput("Input_Version", "Version", errors);
        validateScope(errors);
        validateConstraintRanges(errors);

        const type = dataTypeSelect.value;
        if (type === "object" && !hasDefinitionRows(objectFields)) {
            errors.push("Object custom field must define at least one nested field.");
        }

        if (type === "array") {
            const itemType = arrayItemType?.value || "";
            if (!itemType) {
                errors.push("Array custom field must define an item type.");
            }
            if (itemType === "object" && !hasDefinitionRows(arrayObjectFields)) {
                errors.push("Array object custom field must define at least one item field.");
            }
        }

        if (errors.length > 0) {
            showDesignerMessage("Custom field validation failed. Review the details and fix the highlighted configuration.", errors);
            return false;
        }

        showDesignerMessage("");
        return true;
    }

    function requireInput(id, label, errors) {
        const input = document.getElementById(id);
        if (!input || String(input.value || "").trim()) {
            return;
        }

        errors.push(label + " is required.");
    }

    function validateScope(errors) {
        const selected = Array.from(appliesToOptions).filter(function (option) {
            return option.checked;
        });
        if (selected.length === 0) {
            errors.push("Select at least one availability scope.");
        }
    }

    function validateConstraintRanges(errors) {
        const type = dataTypeSelect.value;
        if (type === "string") {
            validateNumberRange("Input_MinLength", "Input_MaxLength", "Min Length", "Max Length", errors);
            return;
        }
        if (type === "number" || type === "integer") {
            validateNumberRange("Input_Minimum", "Input_Maximum", "Minimum", "Maximum", errors);
        }
        if (type === "date" || type === "datetime") {
            validateDateRange("Input_DateMinimum", "Input_DateMaximum", "Date Minimum", "Date Maximum", errors);
        }
    }

    function validateNumberRange(minId, maxId, minLabel, maxLabel, errors) {
        const minInput = document.getElementById(minId);
        const maxInput = document.getElementById(maxId);
        if (!minInput || !maxInput || minInput.value === "" || maxInput.value === "") {
            return;
        }

        const minValue = Number(minInput.value);
        const maxValue = Number(maxInput.value);
        if (Number.isFinite(minValue) && Number.isFinite(maxValue) && minValue > maxValue) {
            errors.push(minLabel + " cannot be greater than " + maxLabel + ".");
        }
    }

    function validateDateRange(minId, maxId, minLabel, maxLabel, errors) {
        const minInput = document.getElementById(minId);
        const maxInput = document.getElementById(maxId);
        if (!minInput || !maxInput || !minInput.value || !maxInput.value) {
            return;
        }
        if (minInput.value > maxInput.value) {
            errors.push(minLabel + " cannot be greater than " + maxLabel + ".");
        }
    }

    function hasDefinitionRows(host) {
        return !!host && Array.from(host.children).some(function (child) {
            return child.classList && child.classList.contains("metadata-definition-row") &&
                String(child.querySelector(".metadata-definition-name")?.value || "").trim();
        });
    }

    function showDesignerMessage(message, details) {
        if (window.ButterMorphShowSchemaMessage) {
            window.ButterMorphShowSchemaMessage(message, details);
        }
    }

    function buildDefinition(host) {
        const schema = { type: "object", properties: {} };
        if (!host) {
            return schema;
        }
        host.querySelectorAll(":scope > .metadata-definition-row").forEach(function (row) {
            const key = row.querySelector(".metadata-definition-name")?.value.trim() || "";
            if (!key) {
                return;
            }
            const definition = buildRowDefinition(row);
            if (row.querySelector(".metadata-definition-required-input")?.checked) {
                definition.required = true;
            }
            schema.properties[key] = definition;
        });
        return schema;
    }

    function buildRowDefinition(row) {
        const type = row.querySelector(".metadata-definition-type")?.value || "string";
        const definition = { type: type };
        const description = row.querySelector(".metadata-definition-description")?.value.trim() || "";
        if (description) {
            definition.description = description;
        }
        applyValidation(definition, readJson(row.dataset.validation || "{}", {}));
        if (type === "object") {
            const childSchema = buildDefinition(row.querySelector(".metadata-definition-nested"));
            definition.properties = childSchema.properties;
        }
        if (type === "array") {
            const itemType = row.querySelector(".metadata-definition-array-type")?.value || "string";
            definition.items = { type: itemType };
            if (itemType === "object") {
                const itemSchema = buildDefinition(row.querySelector(".metadata-definition-array-children"));
                definition.items.properties = itemSchema.properties;
            }
        }
        return definition;
    }

    function readJson(text, fallback) {
        try {
            return JSON.parse(text || "");
        } catch (error) {
            return fallback;
        }
    }
}());
