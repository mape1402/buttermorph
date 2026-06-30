namespace ButterMorph.StudioPlayground.Services;

using System.Text.Json;
using ButterMorph.Abstractions;
using ButterMorph.Json.Schema;
using ButterMorph.SchemaDesign;
using ButterMorph.StudioPlayground.Models;
using ButterMorph.Web.Razor;

/// <summary>
/// Implements ButterMorph designer host callbacks for the Studio Playground.
/// </summary>
internal sealed class StudioButterMorphHost :
    IButterMorphDesignerHost,
    IButterMorphSchemaTypeDesignerHost,
    IButterMorphFieldMetadataDesignerHost,
    IButterMorphPayloadSchemaDesignerHost
{
    private readonly StudioStore store;
    private readonly IJsonSchemaImporter schemaImporter;
    private static readonly JsonSerializerOptions ResultJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="StudioButterMorphHost"/> class.
    /// </summary>
    /// <param name="store">The host-owned store.</param>
    /// <param name="schemaImporter">The JSON Schema importer.</param>
    public StudioButterMorphHost(StudioStore store, IJsonSchemaImporter schemaImporter)
    {
        this.store = store;
        this.schemaImporter = schemaImporter;
    }

    /// <inheritdoc />
    public Task<ButterMorphDesignerLoadResult> Load(ButterMorphDesignerLoadRequest request)
    {
        if (!store.TryGetMapping(request.ContextKey, out StudioMapping mapping))
        {
            mapping = CreateDraftMapping(request.ContextKey);
        }

        Dictionary<string, IStructureSchema> sourceSchemas = new(StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, string> source in mapping.SourceSchemaIds)
        {
            if (store.TryGetSchema(source.Value, out StudioSchema schema) &&
                TryImportSchema(schema, out IStructureSchema importedSchema))
            {
                sourceSchemas[source.Key] = importedSchema;
            }
        }

        IStructureSchema targetSchema = null;
        if (store.TryGetSchema(mapping.TargetSchemaId, out StudioSchema target) &&
            TryImportSchema(target, out IStructureSchema importedTarget))
        {
            targetSchema = importedTarget;
        }

        return Task.FromResult(new ButterMorphDesignerLoadResult
        {
            SourceSchemas = sourceSchemas,
            TargetSchema = targetSchema,
            InitialDocument = mapping.Document,
            ShowSchemaActions = false,
            Message = "Loaded from Studio host."
        });
    }

    /// <inheritdoc />
    public Task<ButterMorphDesignerSaveResult> Save(ButterMorphDesignerSaveRequest request)
    {
        StudioMapping mapping = store.TryGetMapping(request.ContextKey, out StudioMapping existing)
            ? existing
            : CreateDraftMapping(request.ContextKey);

        mapping.Document = request.Document;
        mapping.DslContent = request.DslContent;
        store.SaveMapping(mapping);

        return Task.FromResult(new ButterMorphDesignerSaveResult
        {
            Succeeded = true,
            Message = "Mapping saved into Studio host."
        });
    }

    /// <inheritdoc />
    Task<ButterMorphSchemaTypeDesignerLoadResult> IButterMorphSchemaTypeDesignerHost.Load(ButterMorphSchemaTypeDesignerLoadRequest request)
    {
        SchemaTypeDesignInput input = new();
        if (store.TryGetCustomType(request.ContextKey, out StudioCustomType item))
        {
            input.Key = item.Key;
            input.Name = item.Name;
            input.Description = item.Description;
            input.VersionNumber = item.Version;
            input.BaseType = item.BaseType;
            input.Comment = item.Comment;
            input.PayloadSchemaJson = item.JsonSchema;
        }

        return Task.FromResult(new ButterMorphSchemaTypeDesignerLoadResult
        {
            Input = input,
            SchemaTypes = CreateTypeCatalog(store.CustomTypes),
            ShowManualActions = false,
            Message = "Catalog loaded from Studio host."
        });
    }

    /// <inheritdoc />
    Task<ButterMorphSchemaTypeDesignerSaveResult> IButterMorphSchemaTypeDesignerHost.Save(ButterMorphSchemaTypeDesignerSaveRequest request)
    {
        SchemaTypeDefinition definition = request.Definition;
        store.SaveCustomType(new StudioCustomType
        {
            Id = request.ContextKey,
            Key = definition.Key,
            Name = definition.Name,
            Description = definition.Description,
            Version = definition.Version,
            BaseType = definition.BaseType,
            Comment = definition.Comment,
            JsonSchema = definition.JsonSchema,
            ButterMorphResultJson = SerializeButterMorphDefinition(definition)
        });

        return Task.FromResult(new ButterMorphSchemaTypeDesignerSaveResult
        {
            Succeeded = true,
            Message = "Custom type saved into Studio host."
        });
    }

    /// <inheritdoc />
    Task<ButterMorphFieldMetadataDesignerLoadResult> IButterMorphFieldMetadataDesignerHost.Load(ButterMorphFieldMetadataDesignerLoadRequest request)
    {
        FieldMetadataDesignInput input = new();
        if (store.TryGetCustomField(request.ContextKey, out StudioCustomField item))
        {
            input = CreateFieldInput(item);
        }

        return Task.FromResult(new ButterMorphFieldMetadataDesignerLoadResult
        {
            Input = input,
            ShowManualActions = false,
            Message = "Metadata field loaded from Studio host."
        });
    }

    /// <inheritdoc />
    Task<ButterMorphFieldMetadataDesignerSaveResult> IButterMorphFieldMetadataDesignerHost.Save(ButterMorphFieldMetadataDesignerSaveRequest request)
    {
        CustomFieldDefinition definition = request.Definition;
        store.SaveCustomField(new StudioCustomField
        {
            Id = request.ContextKey,
            Key = definition.Key,
            Name = definition.Name,
            Description = definition.Description,
            DataType = definition.DataType,
            AppliesToJson = definition.AppliesToJson,
            IsRequired = definition.IsRequired,
            IsActive = definition.IsActive,
            ValidationJson = definition.ValidationJson,
            ChildrenDefinitionJson = definition.ChildrenDefinitionJson,
            ArrayItemDataType = definition.ArrayItemDataType,
            ArrayItemDefinitionJson = definition.ArrayItemDefinitionJson,
            ButterMorphResultJson = SerializeButterMorphDefinition(definition)
        });

        return Task.FromResult(new ButterMorphFieldMetadataDesignerSaveResult
        {
            Succeeded = true,
            Message = "Custom field saved into Studio host."
        });
    }

    /// <inheritdoc />
    Task<ButterMorphPayloadSchemaDesignerLoadResult> IButterMorphPayloadSchemaDesignerHost.Load(ButterMorphPayloadSchemaDesignerLoadRequest request)
    {
        StudioSchema schema = store.TryGetSchema(request.ContextKey, out StudioSchema existing)
            ? existing
            : new StudioSchema { Id = request.ContextKey, Version = "1.0.0" };

        IReadOnlyCollection<string> customTypeIds = ResolveInjectedIds(request.InjectedCustomTypeIds, schema.InjectedCustomTypeKeys);
        IReadOnlyCollection<string> customFieldIds = ResolveInjectedIds(request.InjectedCustomFieldIds, schema.InjectedCustomFieldKeys);
        IReadOnlyCollection<StudioCustomType> injectedTypes = store.CustomTypes
            .Where(item => customTypeIds.Contains(item.Id, StringComparer.OrdinalIgnoreCase))
            .ToArray();
        IReadOnlyCollection<StudioCustomField> injectedFields = store.CustomFields
            .Where(item => customFieldIds.Contains(item.Id, StringComparer.OrdinalIgnoreCase))
            .ToArray();

        return Task.FromResult(new ButterMorphPayloadSchemaDesignerLoadResult
        {
            Key = schema.Key,
            Name = schema.Name,
            Description = schema.Description,
            Version = schema.Version,
            VersionComment = schema.VersionComment,
            JsonSchema = schema.JsonSchema,
            SchemaTypes = CreateTypeCatalog(injectedTypes),
            MetadataFields = CreateFieldCatalog(injectedFields),
            ShowManualActions = false,
            Message = "Schema designer catalog loaded from Studio host."
        });
    }

    /// <inheritdoc />
    Task<ButterMorphPayloadSchemaDesignerSaveResult> IButterMorphPayloadSchemaDesignerHost.Save(ButterMorphPayloadSchemaDesignerSaveRequest request)
    {
        PayloadSchemaDefinition definition = request.Definition;
        StudioSchema schema = store.TryGetSchema(request.ContextKey, out StudioSchema existing)
            ? existing
            : new StudioSchema { Id = request.ContextKey };

        schema.Key = definition.Key;
        schema.Name = definition.Name;
        schema.Description = definition.Description;
        schema.Version = definition.Version;
        schema.VersionComment = definition.VersionComment;
        schema.JsonSchema = SerializeButterMorphDefinition(definition);
        schema.ButterMorphResultJson = schema.JsonSchema;
        IReadOnlyCollection<string> savedTypeIds = ResolveInjectedIds(request.InjectedCustomTypeIds, schema.InjectedCustomTypeKeys);
        IReadOnlyCollection<string> savedFieldIds = ResolveInjectedIds(request.InjectedCustomFieldIds, schema.InjectedCustomFieldKeys);
        schema.InjectedCustomTypeKeys.Clear();
        schema.InjectedCustomTypeKeys.AddRange(savedTypeIds);
        schema.InjectedCustomFieldKeys.Clear();
        schema.InjectedCustomFieldKeys.AddRange(savedFieldIds);
        store.SaveSchema(schema);

        return Task.FromResult(new ButterMorphPayloadSchemaDesignerSaveResult
        {
            Succeeded = true,
            Message = "Schema saved into Studio host."
        });
    }

    /// <summary>
    /// Imports one stored schema into the canonical schema model.
    /// </summary>
    /// <param name="schema">The stored schema.</param>
    /// <param name="importedSchema">The imported schema.</param>
    /// <returns>True when import succeeded.</returns>
    public bool TryImportSchema(StudioSchema schema, out IStructureSchema importedSchema)
    {
        JsonSchemaConversionResult result = schemaImporter.Import(new JsonSchemaImportRequest
        {
            Name = schema.Key,
            Version = schema.Version,
            JsonSchema = schema.JsonSchema
        });

        importedSchema = result.Schema;
        return result.Succeeded;
    }

    /// <summary>
    /// Creates type catalog items from stored types.
    /// </summary>
    /// <param name="items">The stored types.</param>
    /// <returns>The catalog items.</returns>
    public static IReadOnlyCollection<SchemaTypeCatalogItem> CreateTypeCatalog(IEnumerable<StudioCustomType> items)
    {
        return items
            .Where(item => !string.IsNullOrWhiteSpace(item.Name) && !string.IsNullOrWhiteSpace(item.JsonSchema))
            .Select(item => new SchemaTypeCatalogItem
            {
                TypeId = item.Key,
                TypeVersionId = item.Id,
                Name = item.Name,
                VersionNumber = item.Version,
                BaseType = item.BaseType,
                JsonSchema = item.JsonSchema,
                IsSystem = false
            })
            .ToArray();
    }

    /// <summary>
    /// Creates field catalog items from stored fields.
    /// </summary>
    /// <param name="items">The stored fields.</param>
    /// <returns>The catalog items.</returns>
    public static IReadOnlyCollection<FieldMetadataCatalogItem> CreateFieldCatalog(IEnumerable<StudioCustomField> items)
    {
        return items
            .Where(item => item.IsActive && !string.IsNullOrWhiteSpace(item.Key))
            .Select(item => new FieldMetadataCatalogItem
            {
                Id = item.Id,
                Key = item.Key,
                Name = item.Name,
                Description = item.Description,
                DataType = item.DataType,
                IsRequired = item.IsRequired,
                Validation = item.ValidationJson,
                AppliesToJson = item.AppliesToJson,
                ChildrenDefinitionJson = item.ChildrenDefinitionJson,
                ArrayItemDataType = item.ArrayItemDataType,
                ArrayItemDefinitionJson = item.ArrayItemDefinitionJson
            })
            .ToArray();
    }

    private static StudioMapping CreateDraftMapping(string id)
    {
        return new StudioMapping
        {
            Id = id,
            Name = id
        };
    }

    private static string SerializeButterMorphDefinition<T>(T definition)
    {
        return JsonSerializer.Serialize(definition, ResultJsonOptions);
    }

    private static IReadOnlyCollection<string> ResolveInjectedIds(IReadOnlyCollection<string> requestIds, IReadOnlyCollection<string> storedIds)
    {
        if (requestIds.Count > 0)
        {
            return requestIds.ToArray();
        }

        return storedIds.ToArray();
    }

    private static FieldMetadataDesignInput CreateFieldInput(StudioCustomField item)
    {
        CustomFieldDefinition definition = ReadFieldDefinition(item);
        FieldMetadataDesignInput input = new()
        {
            Name = definition.Name,
            Key = definition.Key,
            Description = definition.Description,
            DataType = definition.DataType,
            AppliesTo = ConvertJsonArrayToLines(definition.AppliesToJson),
            IsRequired = definition.IsRequired,
            IsActive = definition.IsActive,
            ChildrenDefinitionJson = definition.ChildrenDefinitionJson,
            ArrayItemDataType = definition.ArrayItemDataType,
            ArrayItemDefinitionJson = definition.ArrayItemDefinitionJson
        };

        ApplyValidation(input, definition.ValidationJson);
        return input;
    }

    private static CustomFieldDefinition ReadFieldDefinition(StudioCustomField item)
    {
        if (!string.IsNullOrWhiteSpace(item.ButterMorphResultJson))
        {
            try
            {
                CustomFieldDefinition definition = JsonSerializer.Deserialize<CustomFieldDefinition>(item.ButterMorphResultJson, ResultJsonOptions);
                if (definition != null && !string.IsNullOrWhiteSpace(definition.Key))
                {
                    return definition;
                }
            }
            catch (JsonException)
            {
            }
        }

        return new CustomFieldDefinition
        {
            Key = item.Key,
            Name = item.Name,
            Description = item.Description,
            DataType = item.DataType,
            AppliesToJson = item.AppliesToJson,
            IsRequired = item.IsRequired,
            IsActive = item.IsActive,
            ValidationJson = item.ValidationJson,
            ChildrenDefinitionJson = item.ChildrenDefinitionJson,
            ArrayItemDataType = item.ArrayItemDataType,
            ArrayItemDefinitionJson = item.ArrayItemDefinitionJson
        };
    }

    private static string ConvertJsonArrayToLines(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            List<string> values = [];
            foreach (JsonElement element in document.RootElement.EnumerateArray())
            {
                values.Add(element.ToString());
            }

            return string.Join(Environment.NewLine, values);
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    private static void ApplyValidation(FieldMetadataDesignInput input, string validationJson)
    {
        if (string.IsNullOrWhiteSpace(validationJson))
        {
            return;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(validationJson);
            JsonElement root = document.RootElement;
            input.MinLength = ReadValidationValue(root, "minLength");
            input.MaxLength = ReadValidationValue(root, "maxLength");
            input.Pattern = ReadValidationValue(root, "pattern");
            input.Minimum = ReadValidationValue(root, "minimum");
            input.Maximum = ReadValidationValue(root, "maximum");
            input.DateMinimum = ReadValidationValue(root, "dateMinimum");
            input.DateMaximum = ReadValidationValue(root, "dateMaximum");
            input.AllowedValues = ReadAllowedValues(root);
        }
        catch (JsonException)
        {
        }
    }

    private static string ReadValidationValue(JsonElement root, string propertyName)
    {
        if (root.TryGetProperty(propertyName, out JsonElement element))
        {
            return element.ToString();
        }

        return string.Empty;
    }

    private static string ReadAllowedValues(JsonElement root)
    {
        if (!root.TryGetProperty("allowedValues", out JsonElement values) ||
            values.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        List<string> lines = [];
        foreach (JsonElement value in values.EnumerateArray())
        {
            lines.Add(value.ToString());
        }

        return string.Join(Environment.NewLine, lines);
    }
}

