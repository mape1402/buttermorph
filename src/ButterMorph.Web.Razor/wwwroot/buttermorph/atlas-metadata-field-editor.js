(function () {
    const dataTypeSelect = document.getElementById("metadata-data-type");
    const allowedValuesHidden = document.getElementById("allowed-values-hidden");
    const allowedValueInput = document.getElementById("allowed-value-input");
    const allowedValuesChips = document.getElementById("allowed-values-chips");
    const appliesToHidden = document.querySelector("input[name='Input.AppliesTo']");
    const appliesToOptions = document.querySelectorAll(".metadata-applies-to-option");
    let allowedValues = readInitialAllowedValues();

    if (!dataTypeSelect) {
        return;
    }

    dataTypeSelect.addEventListener("change", refreshValidationFields);
    appliesToOptions.forEach(function (option) {
        option.addEventListener("change", syncAppliesTo);
    });
    allowedValueInput?.addEventListener("keydown", function (event) {
        if (event.key === "Enter") {
            event.preventDefault();
            addAllowedValue();
        }
    });

    syncAppliesTo();
    syncAllowedValues();
    refreshValidationFields();

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

    function refreshValidationFields() {
        const type = dataTypeSelect.value;
        document.querySelectorAll(".metadata-validation").forEach(function (node) {
            node.classList.add("d-none");
        });
        document.querySelectorAll(".metadata-validation-" + type).forEach(function (node) {
            node.classList.remove("d-none");
        });
        document.querySelectorAll(".metadata-validation-allowed-values").forEach(function (node) {
            node.classList.toggle("d-none", type === "boolean" || type === "date");
        });
        if (type === "boolean" || type === "date") {
            allowedValues = [];
            syncAllowedValues();
        }
        refreshAllowedValueInput(type);
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
        allowedValueInput.disabled = type === "boolean" || type === "date";
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
            remove.setAttribute("aria-label", "Eliminar " + value);
            remove.addEventListener("click", function () { removeAllowedValue(value); });
            chip.appendChild(remove);
            allowedValuesChips.appendChild(chip);
        });
    }
}());
