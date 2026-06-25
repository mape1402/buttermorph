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
    document.querySelector("form")?.addEventListener("submit", syncDefinitions);

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
        document.querySelectorAll(".metadata-validation").forEach(function (node) {
            node.classList.add("d-none");
        });
        document.querySelectorAll(".metadata-validation-" + type).forEach(function (node) {
            node.classList.remove("d-none");
        });
        document.querySelectorAll(".metadata-validation-allowed-values").forEach(function (node) {
            node.classList.toggle("d-none", type === "boolean" || type === "date" || type === "datetime" || type === "object" || type === "array");
        });
        document.querySelectorAll(".metadata-complex").forEach(function (node) {
            node.classList.add("d-none");
        });
        document.querySelectorAll(".metadata-complex-" + type).forEach(function (node) {
            node.classList.remove("d-none");
        });
        if (type === "boolean" || type === "date" || type === "datetime" || type === "object" || type === "array") {
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
        const row = document.createElement("div");
        row.className = "metadata-definition-row";
        row.innerHTML = "<input class='form-control metadata-definition-name' placeholder='Field key' />" +
            "<select class='form-control metadata-definition-type'><option value='string'>string</option><option value='number'>number</option><option value='integer'>integer</option><option value='boolean'>boolean</option><option value='date'>date</option><option value='datetime'>datetime</option><option value='object'>object</option><option value='array'>array</option></select>" +
            "<input class='form-control metadata-definition-description' placeholder='Short field description' />" +
            "<label class='form-check metadata-definition-required'><input class='form-check-input metadata-definition-required-input' type='checkbox' /> Required</label>" +
            "<div class='metadata-definition-actions'><button type='button' class='btn btn-secondary btn-sm metadata-definition-add-child d-none' title='Add child field'>+</button><button type='button' class='btn btn-outline-danger btn-sm metadata-definition-remove' title='Remove field'>Trash</button></div>" +
            "<div class='metadata-definition-nested d-none'></div>" +
            "<div class='metadata-definition-array d-none'><div class='metadata-definition-array-controls'><label class='form-label'>Array Item Type</label><select class='form-control metadata-definition-array-type'><option value='string'>string</option><option value='number'>number</option><option value='integer'>integer</option><option value='boolean'>boolean</option><option value='date'>date</option><option value='datetime'>datetime</option><option value='object'>object</option><option value='array'>array</option></select></div><div class='metadata-definition-array-children d-none'></div></div>";
        row.querySelector(".metadata-definition-name").value = name || "";
        row.querySelector(".metadata-definition-type").value = type || "string";
        row.querySelector(".metadata-definition-description").value = description || "";
        row.querySelector(".metadata-definition-required-input").checked = required === true;
        row.querySelector(".metadata-definition-remove").addEventListener("click", function () {
            row.remove();
            syncDefinitions();
        });
        row.querySelector(".metadata-definition-add-child").addEventListener("click", function () {
            const host = getActiveChildHost(row);
            host?.appendChild(createDefinitionRow());
            syncDefinitions();
        });
        row.querySelectorAll("input, select").forEach(function (input) {
            input.addEventListener("input", syncDefinitions);
            input.addEventListener("change", function () {
                refreshDefinitionRow(row);
                syncDefinitions();
            });
        });
        hydrateNestedDefinition(row, definition || {});
        refreshDefinitionRow(row);
        return row;
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
        const required = Array.isArray(schema.required) ? schema.required : [];
        const properties = schema.properties || {};
        Object.keys(properties).forEach(function (key) {
            const definition = properties[key] || {};
            host.appendChild(createDefinitionRow(key, definition.type || "string", definition.description || "", required.includes(key) || definition.required === true, definition));
        });
    }

    function refreshDefinitionRow(row) {
        const type = row.querySelector(".metadata-definition-type")?.value || "string";
        const nested = row.querySelector(".metadata-definition-nested");
        const array = row.querySelector(".metadata-definition-array");
        const arrayType = row.querySelector(".metadata-definition-array-type")?.value || "string";
        const arrayChildren = row.querySelector(".metadata-definition-array-children");
        const addChild = row.querySelector(".metadata-definition-add-child");
        nested?.classList.toggle("d-none", type !== "object");
        array?.classList.toggle("d-none", type !== "array");
        arrayChildren?.classList.toggle("d-none", type !== "array" || arrayType !== "object");
        addChild?.classList.toggle("d-none", type !== "object" && !(type === "array" && arrayType === "object"));
    }

    function getActiveChildHost(row) {
        const type = row.querySelector(".metadata-definition-type")?.value || "string";
        if (type === "object") {
            return row.querySelector(".metadata-definition-nested");
        }
        if (type === "array" && row.querySelector(".metadata-definition-array-type")?.value === "object") {
            return row.querySelector(".metadata-definition-array-children");
        }
        return null;
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

    function buildDefinition(host) {
        const schema = { type: "object", properties: {} };
        const required = [];
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
                required.push(key);
            }
            schema.properties[key] = definition;
        });
        if (required.length > 0) {
            schema.required = required;
        }
        return schema;
    }

    function buildRowDefinition(row) {
        const type = row.querySelector(".metadata-definition-type")?.value || "string";
        const definition = { type: type };
        const description = row.querySelector(".metadata-definition-description")?.value.trim() || "";
        if (description) {
            definition.description = description;
        }
        if (type === "object") {
            const childSchema = buildDefinition(row.querySelector(".metadata-definition-nested"));
            definition.properties = childSchema.properties;
            if (childSchema.required) {
                definition.required = childSchema.required;
            }
        }
        if (type === "array") {
            const itemType = row.querySelector(".metadata-definition-array-type")?.value || "string";
            definition.items = { type: itemType };
            if (itemType === "object") {
                const itemSchema = buildDefinition(row.querySelector(".metadata-definition-array-children"));
                definition.items.properties = itemSchema.properties;
                if (itemSchema.required) {
                    definition.items.required = itemSchema.required;
                }
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
