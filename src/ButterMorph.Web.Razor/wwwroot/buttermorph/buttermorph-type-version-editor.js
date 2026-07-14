(function () {
    const baseSelect = document.getElementById("type-base-select");
    const form = document.getElementById("event-editor-form");
    const arrayItemSelect = document.getElementById("array-item-type-select");
    const arrayItemTypeInput = document.getElementById("array-item-type");
    const arrayItemTypeVersionInput = document.getElementById("array-item-type-version-id");
    const allowedValuesInput = document.getElementById("type-allowed-values-json");
    const catalog = readTypeCatalog();

    if (!baseSelect || !form) {
        return;
    }

    populateArrayItemSelect();
    hydrateAllowedValues();
    refresh();

    baseSelect.addEventListener("change", refresh);
    arrayItemSelect?.addEventListener("change", function () {
        syncArrayItemInputs();
        refresh();
    });
    window.ButterMorphSchemaBeforeSave = validateTypeDesigner;

    form.addEventListener("submit", function (event) {
        if (!validateTypeDesigner()) {
            event.preventDefault();
            return;
        }

        syncArrayItemInputs();
        syncAllowedValuesInput();
    });

    function refresh() {
        const baseType = baseSelect.value;
        document.querySelectorAll(".type-constraints").forEach(function (node) {
            node.classList.add("d-none");
        });
        document.querySelectorAll(".type-constraints-" + baseType).forEach(function (node) {
            node.classList.remove("d-none");
        });
        const selectedItemType = arrayItemSelect?.selectedOptions?.[0]?.dataset?.baseType || arrayItemSelect?.value || "";
        if (baseType === "array" && selectedItemType === "object") {
            document.querySelectorAll(".type-constraints-object").forEach(function (node) {
                node.classList.remove("d-none");
            });
        }
        syncAllowedValuesInput();
    }

    function hydrateAllowedValues() {
        document.querySelectorAll(".type-enum-chip-editor").forEach(function (editor) {
            const type = editor.dataset.enumType || "string";
            const input = editor.querySelector(".type-enum-input");
            const chips = editor.querySelector(".type-enum-chips");
            if (!input || !chips) {
                return;
            }
            readAllowedValues().forEach(function (value) { addEnumChip(chips, value); });
            input.addEventListener("keydown", function (event) {
                if (event.key !== "Enter") {
                    return;
                }
                event.preventDefault();
                const value = normalizeEnumInput(input.value, type);
                if (value === null) {
                    return;
                }
                addEnumChip(chips, value);
                input.value = "";
                syncAllowedValuesInput();
            });
        });
    }

    function readAllowedValues() {
        try {
            const parsed = JSON.parse(allowedValuesInput?.value || "[]");
            return Array.isArray(parsed) ? parsed : [];
        } catch (error) {
            return [];
        }
    }

    function normalizeEnumInput(value, type) {
        const trimmed = String(value || "").trim();
        if (!trimmed) {
            return null;
        }
        if (type === "string") {
            return trimmed;
        }
        const numeric = Number(trimmed);
        if (!Number.isFinite(numeric)) {
            return null;
        }
        if (type === "integer" && !Number.isInteger(numeric)) {
            return null;
        }
        return numeric;
    }

    function addEnumChip(container, value) {
        const normalized = String(value);
        if (Array.from(container.querySelectorAll(".type-enum-chip")).some(function (chip) { return chip.dataset.value === normalized; })) {
            return;
        }
        const chip = document.createElement("span");
        chip.className = "type-enum-chip";
        chip.dataset.value = normalized;
        chip.dataset.rawValue = JSON.stringify(value);
        chip.textContent = normalized;
        const remove = document.createElement("button");
        remove.type = "button";
        remove.textContent = "x";
        remove.addEventListener("click", function () { chip.remove(); syncAllowedValuesInput(); });
        chip.appendChild(remove);
        container.appendChild(chip);
    }

    function syncAllowedValuesInput() {
        if (!allowedValuesInput) {
            return;
        }
        const activeEditor = document.querySelector(".type-constraints-" + baseSelect.value + " .type-enum-chip-editor");
        if (!activeEditor) {
            allowedValuesInput.value = "[]";
            return;
        }
        const values = Array.from(activeEditor.querySelectorAll(".type-enum-chip")).map(function (chip) {
            return JSON.parse(chip.dataset.rawValue || "null");
        }).filter(function (value) { return value !== null; });
        allowedValuesInput.value = JSON.stringify(values);
    }

    function readTypeCatalog() {
        const node = document.getElementById("schema-type-catalog");
        if (!node) {
            return defaultCatalog();
        }
        try {
            const parsed = JSON.parse(node.textContent || "[]");
            return Array.isArray(parsed) && parsed.length > 0 ? parsed : defaultCatalog();
        } catch (error) {
            return defaultCatalog();
        }
    }

    function defaultCatalog() {
        return ["string", "number", "integer", "boolean", "object", "array"].map(function (type) {
            return { name: type, baseType: type, versionNumber: "1.0.0", isSystem: true };
        });
    }

    function populateArrayItemSelect() {
        if (!arrayItemSelect) {
            return;
        }
        arrayItemSelect.innerHTML = "";
        const basicGroup = document.createElement("optgroup");
        basicGroup.label = "Basic";
        const customGroup = document.createElement("optgroup");
        customGroup.label = "Custom";
        catalog.forEach(function (item) {
            const normalized = normalizeCatalogItem(item);
            if (!isValidCatalogItem(normalized)) {
                return;
            }
            const option = document.createElement("option");
            option.value = normalized.isSystem && !normalized.typeVersionId ? normalized.baseType : normalized.typeVersionId;
            option.textContent = normalized.isSystem ? normalized.name : normalized.name + " (" + normalized.versionNumber + ")";
            option.dataset.baseType = normalized.baseType;
            option.dataset.isSystem = normalized.isSystem ? "true" : "false";
            option.dataset.typeVersionId = normalized.typeVersionId || "";
            (normalized.isSystem ? basicGroup : customGroup).appendChild(option);
        });
        arrayItemSelect.appendChild(basicGroup);
        if (customGroup.children.length > 0) {
            arrayItemSelect.appendChild(customGroup);
        }
        const savedVersionId = arrayItemTypeVersionInput?.value;
        const savedType = arrayItemTypeInput?.value;
        if (savedVersionId && Array.from(arrayItemSelect.options).some(function (option) { return option.value === savedVersionId; })) {
            arrayItemSelect.value = savedVersionId;
        } else if (savedType && Array.from(arrayItemSelect.options).some(function (option) { return option.value === savedType; })) {
            arrayItemSelect.value = savedType;
        }
        syncArrayItemInputs();
    }

    function syncArrayItemInputs() {
        const selected = arrayItemSelect?.selectedOptions?.[0];
        if (!selected || !arrayItemTypeInput || !arrayItemTypeVersionInput) {
            return;
        }
        if (selected.dataset.isSystem === "true" && !selected.dataset.typeVersionId) {
            arrayItemTypeInput.value = selected.dataset.baseType || selected.value;
            arrayItemTypeVersionInput.value = "";
            return;
        }
        arrayItemTypeInput.value = selected.dataset.baseType || "";
        arrayItemTypeVersionInput.value = selected.dataset.typeVersionId || selected.value;
    }

    function validateTypeDesigner() {
        const errors = [];
        requireInput("Input_Key", "Key", errors);
        requireInput("Input_Name", "Name", errors);
        requireInput("Input_VersionNumber", "Version", errors);
        validateConstraintRanges(errors);

        const baseType = baseSelect.value;
        if (baseType === "object" && !hasSchemaFields(document.getElementById("schema-root-fields"))) {
            errors.push("Object custom type must define at least one field.");
        }

        const selectedItemType = arrayItemSelect?.selectedOptions?.[0]?.dataset?.baseType || arrayItemSelect?.value || "";
        if (baseType === "array" && !selectedItemType) {
            errors.push("Array custom type must define an item type.");
        }

        if (baseType === "array" && selectedItemType === "object" && !hasSchemaFields(document.getElementById("schema-root-fields"))) {
            errors.push("Array object custom type must define at least one item field.");
        }

        if (errors.length > 0) {
            showDesignerMessage("Custom type validation failed. Review the details and fix the highlighted configuration.", errors);
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

    function validateConstraintRanges(errors) {
        const baseType = baseSelect.value;
        if (baseType === "string") {
            validateNumberRange("Input_MinLength", "Input_MaxLength", "Min Length", "Max Length", errors);
            return;
        }

        if (baseType === "number" || baseType === "integer") {
            validateNumberRange("Input_Minimum", "Input_Maximum", "Minimum", "Maximum", errors);
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

    function hasSchemaFields(list) {
        return !!list && Array.from(list.children).some(function (child) {
            return child.classList && child.classList.contains("schema-field") &&
                String(child.querySelector(".field-name-input")?.value || "").trim();
        });
    }

    function showDesignerMessage(message, details) {
        if (window.ButterMorphShowSchemaMessage) {
            window.ButterMorphShowSchemaMessage(message, details);
        }
    }

    function normalizeCatalogItem(item) {
        return {
            typeVersionId: item.typeVersionId || item.TypeVersionId || "",
            name: item.name || item.Name || "",
            versionNumber: item.versionNumber || item.VersionNumber || "",
            baseType: item.baseType || item.BaseType || item.name || item.Name || "string",
            isSystem: item.isSystem === true || item.IsSystem === true
        };
    }

    function isValidCatalogItem(item) {
        if (item.isSystem) {
            return !!item.name && !!item.baseType;
        }

        return !!item.name &&
            !!item.baseType &&
            !!item.typeVersionId;
    }
}());



