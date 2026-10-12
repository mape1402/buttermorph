namespace ButterMorph.StudioPlayground.Services;

using System.Collections.Concurrent;
using ButterMorph.StudioPlayground.Models;
using ButterMorph.Web.Razor;

/// <summary>
/// Stores host-owned Studio Playground state in memory.
/// </summary>
internal sealed class StudioStore
{
    // Concurrent dictionaries simulate host persistence for the sample.
    private readonly ConcurrentDictionary<string, StudioCustomType> customTypes = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, StudioCustomField> customFields = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, StudioSchema> schemas = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, StudioMapping> mappings = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, StudioMappingSetup> mappingSetups = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, StudioValidation> validations = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, StudioValidationSetup> validationSetups = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, StudioLanguageTypeMappingDocument> languageTypeMappings = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, StudioLanguageTypeBundle> languageTypeBundles = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets all custom types.
    /// </summary>
    public IReadOnlyCollection<StudioCustomType> CustomTypes => customTypes.Values.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToArray();

    /// <summary>
    /// Gets all custom fields.
    /// </summary>
    public IReadOnlyCollection<StudioCustomField> CustomFields => customFields.Values.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToArray();

    /// <summary>
    /// Gets all schemas.
    /// </summary>
    public IReadOnlyCollection<StudioSchema> Schemas => schemas.Values.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToArray();

    /// <summary>
    /// Gets all mappings.
    /// </summary>
    public IReadOnlyCollection<StudioMapping> Mappings => mappings.Values.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToArray();

    /// <summary>
    /// Gets all validations.
    /// </summary>
    public IReadOnlyCollection<StudioValidation> Validations => validations.Values.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToArray();

    /// <summary>
    /// Gets all language type mapping documents.
    /// </summary>
    public IReadOnlyCollection<StudioLanguageTypeMappingDocument> LanguageTypeMappings => languageTypeMappings.Values
        .OrderBy(item => item.LanguageKey, StringComparer.OrdinalIgnoreCase)
        .ThenBy(item => item.Document.Name, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    /// <summary>
    /// Gets all language type bundles.
    /// </summary>
    public IReadOnlyCollection<StudioLanguageTypeBundle> LanguageTypeBundles => languageTypeBundles.Values
        .OrderBy(item => item.LanguageKey, StringComparer.OrdinalIgnoreCase)
        .ThenBy(item => item.Bundle.AssemblyKey, StringComparer.OrdinalIgnoreCase)
        .ToArray();

    /// <summary>
    /// Saves a custom type.
    /// </summary>
    /// <param name="item">The custom type.</param>
    public void SaveCustomType(StudioCustomType item)
    {
        item.SavedAt = DateTimeOffset.UtcNow;
        customTypes[item.Id] = item;
    }

    /// <summary>
    /// Saves a custom field.
    /// </summary>
    /// <param name="item">The custom field.</param>
    public void SaveCustomField(StudioCustomField item)
    {
        item.SavedAt = DateTimeOffset.UtcNow;
        customFields[item.Id] = item;
    }

    /// <summary>
    /// Saves a schema.
    /// </summary>
    /// <param name="item">The schema.</param>
    public void SaveSchema(StudioSchema item)
    {
        item.SavedAt = DateTimeOffset.UtcNow;
        schemas[item.Id] = item;
    }

    /// <summary>
    /// Saves a mapping.
    /// </summary>
    /// <param name="item">The mapping.</param>
    public void SaveMapping(StudioMapping item)
    {
        item.SavedAt = DateTimeOffset.UtcNow;
        mappings[item.Id] = item;
    }

    /// <summary>
    /// Saves a validation.
    /// </summary>
    /// <param name="item">The validation.</param>
    public void SaveValidation(StudioValidation item)
    {
        item.SavedAt = DateTimeOffset.UtcNow;
        validations[item.Id] = item;
    }

    /// <summary>
    /// Saves a language type mapping document.
    /// </summary>
    /// <param name="item">The language type mapping document.</param>
    public void SaveLanguageTypeMapping(StudioLanguageTypeMappingDocument item)
    {
        NormalizeLanguageTypeMapping(item);
        item.SavedAt = DateTimeOffset.UtcNow;
        languageTypeMappings[item.Id] = item;
    }

    /// <summary>
    /// Saves a language type bundle.
    /// </summary>
    /// <param name="item">The language type bundle.</param>
    public void SaveLanguageTypeBundle(StudioLanguageTypeBundle item)
    {
        item.SavedAt = DateTimeOffset.UtcNow;
        languageTypeBundles[item.Id] = item;
    }

    /// <summary>
    /// Saves a temporary mapping setup.
    /// </summary>
    /// <param name="item">The setup.</param>
    public void SaveMappingSetup(StudioMappingSetup item)
    {
        mappingSetups[item.Id] = item;
    }

    /// <summary>
    /// Saves a temporary validation setup.
    /// </summary>
    /// <param name="item">The setup.</param>
    public void SaveValidationSetup(StudioValidationSetup item)
    {
        validationSetups[item.Id] = item;
    }

    /// <summary>
    /// Replaces the runtime state from browser-owned persisted state.
    /// </summary>
    /// <param name="snapshot">The browser state snapshot.</param>
    public void ReplaceFromSnapshot(StudioStateSnapshot snapshot)
    {
        customTypes.Clear();
        customFields.Clear();
        schemas.Clear();
        mappings.Clear();
        validations.Clear();
        languageTypeMappings.Clear();
        languageTypeBundles.Clear();

        foreach (StudioCustomType item in snapshot.CustomTypes)
        {
            if (!string.IsNullOrWhiteSpace(item.Id))
            {
                customTypes[item.Id] = item;
            }
        }

        foreach (StudioCustomField item in snapshot.CustomFields)
        {
            if (!string.IsNullOrWhiteSpace(item.Id))
            {
                customFields[item.Id] = item;
            }
        }

        foreach (StudioSchema item in snapshot.Schemas)
        {
            if (!string.IsNullOrWhiteSpace(item.Id))
            {
                schemas[item.Id] = item;
            }
        }

        foreach (StudioMapping item in snapshot.Mappings)
        {
            if (!string.IsNullOrWhiteSpace(item.Id))
            {
                mappings[item.Id] = item;
            }
        }

        foreach (StudioValidation item in snapshot.Validations)
        {
            if (!string.IsNullOrWhiteSpace(item.Id))
            {
                validations[item.Id] = item;
            }
        }

        foreach (StudioLanguageTypeMappingDocument item in snapshot.LanguageTypeMappings)
        {
            if (!string.IsNullOrWhiteSpace(item.Id))
            {
                SaveLanguageTypeMapping(item);
            }
        }

        foreach (StudioLanguageTypeProfile item in snapshot.LanguageTypeProfiles)
        {
            if (!string.IsNullOrWhiteSpace(item.Id) && !languageTypeMappings.ContainsKey(item.Id))
            {
                SaveLanguageTypeMapping(new StudioLanguageTypeMappingDocument
                {
                    Id = item.Id,
                    LanguageKey = item.LanguageKey,
                    Document = ButterMorphLanguageTypeMappingDocumentConverter.FromProfile(item.Profile),
                    SavedAt = item.SavedAt
                });
            }
        }

        foreach (StudioLanguageTypeBundle item in snapshot.LanguageTypeBundles)
        {
            if (!string.IsNullOrWhiteSpace(item.Id))
            {
                languageTypeBundles[item.Id] = item;
            }
        }
    }

    /// <summary>
    /// Attempts to get a custom type.
    /// </summary>
    /// <param name="id">The host-owned id.</param>
    /// <param name="item">The custom type.</param>
    /// <returns>True when found.</returns>
    public bool TryGetCustomType(string id, out StudioCustomType item)
    {
        return customTypes.TryGetValue(id, out item);
    }

    /// <summary>
    /// Attempts to get a custom field.
    /// </summary>
    /// <param name="id">The host-owned id.</param>
    /// <param name="item">The custom field.</param>
    /// <returns>True when found.</returns>
    public bool TryGetCustomField(string id, out StudioCustomField item)
    {
        return customFields.TryGetValue(id, out item);
    }

    /// <summary>
    /// Attempts to get a schema.
    /// </summary>
    /// <param name="id">The host-owned id.</param>
    /// <param name="item">The schema.</param>
    /// <returns>True when found.</returns>
    public bool TryGetSchema(string id, out StudioSchema item)
    {
        return schemas.TryGetValue(id, out item);
    }

    /// <summary>
    /// Attempts to get a mapping.
    /// </summary>
    /// <param name="id">The host-owned id.</param>
    /// <param name="item">The mapping.</param>
    /// <returns>True when found.</returns>
    public bool TryGetMapping(string id, out StudioMapping item)
    {
        return mappings.TryGetValue(id, out item);
    }

    /// <summary>
    /// Attempts to get a validation.
    /// </summary>
    /// <param name="id">The host-owned id.</param>
    /// <param name="item">The validation.</param>
    /// <returns>True when found.</returns>
    public bool TryGetValidation(string id, out StudioValidation item)
    {
        return validations.TryGetValue(id, out item);
    }

    /// <summary>
    /// Attempts to get a language type mapping document.
    /// </summary>
    /// <param name="id">The host-owned id.</param>
    /// <param name="item">The language type mapping document.</param>
    /// <returns>True when found.</returns>
    public bool TryGetLanguageTypeMapping(string id, out StudioLanguageTypeMappingDocument item)
    {
        return languageTypeMappings.TryGetValue(id, out item);
    }

    /// <summary>
    /// Attempts to get a language type bundle.
    /// </summary>
    /// <param name="id">The host-owned id.</param>
    /// <param name="item">The language type bundle.</param>
    /// <returns>True when found.</returns>
    public bool TryGetLanguageTypeBundle(string id, out StudioLanguageTypeBundle item)
    {
        return languageTypeBundles.TryGetValue(id, out item);
    }

    /// <summary>
    /// Attempts to get a temporary mapping setup.
    /// </summary>
    /// <param name="id">The host-owned id.</param>
    /// <param name="item">The mapping setup.</param>
    /// <returns>True when found.</returns>
    public bool TryGetMappingSetup(string id, out StudioMappingSetup item)
    {
        return mappingSetups.TryGetValue(id, out item);
    }

    /// <summary>
    /// Attempts to get a temporary validation setup.
    /// </summary>
    /// <param name="id">The host-owned id.</param>
    /// <param name="item">The validation setup.</param>
    /// <returns>True when found.</returns>
    public bool TryGetValidationSetup(string id, out StudioValidationSetup item)
    {
        return validationSetups.TryGetValue(id, out item);
    }

    /// <summary>
    /// Deletes a temporary mapping setup.
    /// </summary>
    /// <param name="id">The host-owned id.</param>
    public void DeleteMappingSetup(string id)
    {
        mappingSetups.TryRemove(id, out StudioMappingSetup removedSetup);
    }

    /// <summary>
    /// Deletes a temporary validation setup.
    /// </summary>
    /// <param name="id">The host-owned id.</param>
    public void DeleteValidationSetup(string id)
    {
        validationSetups.TryRemove(id, out StudioValidationSetup removedSetup);
    }

    /// <summary>
    /// Deletes one item by kind and key.
    /// </summary>
    /// <param name="kind">The item kind.</param>
    /// <param name="id">The host-owned id.</param>
    /// <returns>True when removed.</returns>
    public bool Delete(string kind, string id)
    {
        return kind switch
        {
            "customTypes" => customTypes.TryRemove(id, out StudioCustomType removedType),
            "customFields" => customFields.TryRemove(id, out StudioCustomField removedField),
            "schemas" => schemas.TryRemove(id, out StudioSchema removedSchema),
            "mappings" => mappings.TryRemove(id, out StudioMapping removedMapping) | mappingSetups.TryRemove(id, out StudioMappingSetup removedSetup),
            "validations" => validations.TryRemove(id, out StudioValidation removedValidation) | validationSetups.TryRemove(id, out StudioValidationSetup removedValidationSetup),
            "languageTypeMappings" => languageTypeMappings.TryRemove(id, out StudioLanguageTypeMappingDocument removedLanguageTypeMapping),
            "languageTypeProfiles" => languageTypeMappings.TryRemove(id, out StudioLanguageTypeMappingDocument removedLanguageTypeProfile),
            "languageTypeBundles" => languageTypeBundles.TryRemove(id, out StudioLanguageTypeBundle removedLanguageTypeBundle),
            _ => false
        };
    }

    /// <summary>
    /// Deletes one language type bundle.
    /// </summary>
    /// <param name="id">The bundle id.</param>
    /// <returns>True when removed.</returns>
    public bool DeleteLanguageTypeBundle(string id)
    {
        return languageTypeBundles.TryRemove(id, out StudioLanguageTypeBundle removed);
    }

    private static void NormalizeLanguageTypeMapping(StudioLanguageTypeMappingDocument item)
    {
        item.Document ??= new ButterMorphLanguageTypeMappingDocument();
        ButterMorphLanguageTypeMappingDocumentConverter.Normalize(item.Document);
        item.Id = string.IsNullOrWhiteSpace(item.Id) ? item.Document.Key : item.Id;
        item.LanguageKey = string.IsNullOrWhiteSpace(item.LanguageKey) ? item.Document.Language : item.LanguageKey;
    }
}

