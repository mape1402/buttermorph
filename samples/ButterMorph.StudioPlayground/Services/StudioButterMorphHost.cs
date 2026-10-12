namespace ButterMorph.StudioPlayground.Services;

using System.Text.Json;
using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Json.Schema;
using ButterMorph.SchemaDesign;
using ButterMorph.StudioPlayground.Models;
using ButterMorph.Web.Razor;

/// <summary>
/// Implements ButterMorph designer host callbacks for the Studio Playground.
/// </summary>
internal sealed class StudioButterMorphHost :
    IButterMorphDesignerHost,
    IButterMorphValidationDesignerHost,
    IButterMorphMappingViewerHost,
    IButterMorphValidationViewerHost,
    IButterMorphSchemaTypeDesignerHost,
    IButterMorphFieldMetadataDesignerHost,
    IButterMorphPayloadSchemaDesignerHost,
    IButterMorphPayloadSchemaViewerHost,
    IButterMorphLanguageTypeMappingHost
{
    private readonly StudioStore store;
    private readonly IJsonSchemaImporter schemaImporter;
    private readonly IDslParser dslParser;
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
    /// <param name="dslParser">The DSL parser.</param>
    public StudioButterMorphHost(StudioStore store, IJsonSchemaImporter schemaImporter, IDslParser dslParser)
    {
        this.store = store;
        this.schemaImporter = schemaImporter;
        this.dslParser = dslParser;
    }

    /// <inheritdoc />
    public Task<ButterMorphDesignerLoadResult> Load(ButterMorphDesignerLoadRequest request)
    {
        StudioMapping mapping = ResolveMappingForLoad(request.ContextKey);

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
            InitialDslContent = mapping.DslContent,
            ShowSchemaActions = mapping.ShowSchemaActions,
            Message = string.Empty
        });
    }

    /// <inheritdoc />
    public Task<ButterMorphDesignerSaveResult> Save(ButterMorphDesignerSaveRequest request)
    {
        StudioMapping mapping = store.TryGetMapping(request.ContextKey, out StudioMapping existing)
            ? existing
            : CreateMappingFromSetup(request.ContextKey);

        mapping.Document = request.Document;
        mapping.DslContent = request.DslContent;
        store.SaveMapping(mapping);
        store.DeleteMappingSetup(request.ContextKey);

        return Task.FromResult(new ButterMorphDesignerSaveResult
        {
            Succeeded = true,
            Message = "Mapping saved into Studio host."
        });
    }

    /// <inheritdoc />
    public Task<ButterMorphValidationDesignerLoadResult> Load(ButterMorphValidationDesignerLoadRequest request)
    {
        StudioValidation validation = ResolveValidationForLoad(request.ContextKey);
        Dictionary<string, IStructureSchema> sourceSchemas = CreateSourceSchemas(validation.SourceSchemaIds);

        return Task.FromResult(new ButterMorphValidationDesignerLoadResult
        {
            SourceSchemas = sourceSchemas,
            SourceMetadata = CreateSourceMetadata(validation.SourceSchemaIds, sourceSchemas),
            InitialDocument = ResolveValidationDocument(validation),
            InitialDslContent = validation.DslContent,
            Message = string.Empty
        });
    }

    /// <inheritdoc />
    public Task<ButterMorphValidationDesignerSaveResult> Save(ButterMorphValidationDesignerSaveRequest request)
    {
        StudioValidation validation = store.TryGetValidation(request.ContextKey, out StudioValidation existing)
            ? existing
            : CreateValidationFromSetup(request.ContextKey);

        validation.Document = request.Document;
        validation.DslContent = request.DslContent;
        store.SaveValidation(validation);
        store.DeleteValidationSetup(request.ContextKey);

        return Task.FromResult(new ButterMorphValidationDesignerSaveResult
        {
            Succeeded = true,
            Message = "Validation saved into Studio host."
        });
    }

    /// <inheritdoc />
    Task<ButterMorphMappingViewerLoadResult> IButterMorphMappingViewerHost.Load(ButterMorphMappingViewerLoadRequest request)
    {
        if (!store.TryGetMapping(request.ContextKey, out StudioMapping mapping))
        {
            return Task.FromResult(new ButterMorphMappingViewerLoadResult
            {
                Key = request.ContextKey,
                Name = request.ContextKey,
                DslContent = string.Empty,
                Message = "Mapping was not found in Studio host."
            });
        }

        IStructureSchema targetSchema = null;
        if (store.TryGetSchema(mapping.TargetSchemaId, out StudioSchema target) &&
            TryImportSchema(target, out IStructureSchema importedTarget))
        {
            targetSchema = importedTarget;
        }

        return Task.FromResult(new ButterMorphMappingViewerLoadResult
        {
            Key = mapping.Id,
            Name = mapping.Name,
            Description = "Mapping document",
            Document = ResolveMappingDocument(mapping),
            TargetSchema = targetSchema,
            DslContent = mapping.DslContent,
            Message = string.Empty
        });
    }

    /// <inheritdoc />
    Task<ButterMorphValidationViewerLoadResult> IButterMorphValidationViewerHost.Load(ButterMorphValidationViewerLoadRequest request)
    {
        if (!store.TryGetValidation(request.ContextKey, out StudioValidation validation))
        {
            return Task.FromResult(new ButterMorphValidationViewerLoadResult
            {
                Key = request.ContextKey,
                Name = request.ContextKey,
                DslContent = string.Empty,
                Message = "Validation was not found in Studio host."
            });
        }

        return Task.FromResult(new ButterMorphValidationViewerLoadResult
        {
            Key = validation.Id,
            Name = validation.Name,
            Description = "Validation document",
            Document = ResolveValidationDocument(validation),
            DslContent = validation.DslContent,
            Message = string.Empty
        });
    }

    /// <inheritdoc />
    Task<ButterMorphSchemaTypeDesignerLoadResult> IButterMorphSchemaTypeDesignerHost.Load(ButterMorphSchemaTypeDesignerLoadRequest request)
    {
        SchemaTypeDefinition definition = null;
        if (store.TryGetCustomType(request.ContextKey, out StudioCustomType item))
        {
            definition = ReadDefinition<SchemaTypeDefinition>(item.ButterMorphResultJson);
        }

        return Task.FromResult(new ButterMorphSchemaTypeDesignerLoadResult
        {
            Definition = definition,
            SchemaTypes = CreateTypeCatalog(store.CustomTypes),
            ShowManualActions = false,
            Message = string.Empty
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
        CustomFieldDefinition definition = null;
        if (store.TryGetCustomField(request.ContextKey, out StudioCustomField item))
        {
            definition = ReadDefinition<CustomFieldDefinition>(item.ButterMorphResultJson);
        }

        return Task.FromResult(new ButterMorphFieldMetadataDesignerLoadResult
        {
            Definition = definition,
            ShowManualActions = false,
            Message = string.Empty
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
            Version = definition.Version,
            VersionComment = definition.VersionComment,
            DataType = definition.DataType,
            AppliesToJson = SerializeStringArray(definition.AppliesTo),
            IsRequired = definition.IsRequired,
            IsActive = definition.IsActive,
            ValidationJson = SerializeElementMap(definition.Validation),
            ChildrenDefinitionJson = SerializeElement(definition.ChildrenDefinition),
            ArrayItemDataType = definition.ArrayItemDataType,
            ArrayItemDefinitionJson = SerializeElement(definition.ArrayItemDefinition),
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
        string schemaJson = ResolveSchemaJson(schema);

        return Task.FromResult(new ButterMorphPayloadSchemaDesignerLoadResult
        {
            Definition = ReadDefinition<PayloadSchemaDefinition>(schema.ButterMorphResultJson),
            Key = schema.Key,
            Name = schema.Name,
            Description = schema.Description,
            Version = schema.Version,
            VersionComment = schema.VersionComment,
            JsonSchema = schemaJson,
            SchemaTypes = CreateTypeCatalog(injectedTypes),
            MetadataFields = CreateFieldCatalog(injectedFields),
            ShowManualActions = false,
            Message = string.Empty
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

    /// <inheritdoc />
    Task<ButterMorphPayloadSchemaViewerLoadResult> IButterMorphPayloadSchemaViewerHost.Load(ButterMorphPayloadSchemaViewerLoadRequest request)
    {
        if (!store.TryGetSchema(request.ContextKey, out StudioSchema schema))
        {
            return Task.FromResult(new ButterMorphPayloadSchemaViewerLoadResult
            {
                JsonSchema = "{}",
                Message = "Schema was not found in Studio host."
            });
        }

        return Task.FromResult(new ButterMorphPayloadSchemaViewerLoadResult
        {
            Definition = ReadDefinition<PayloadSchemaDefinition>(schema.ButterMorphResultJson),
            JsonSchema = ResolveSchemaJson(schema),
            Message = string.Empty
        });
    }

    /// <inheritdoc />
    Task<ButterMorphLanguageTypeMappingLoadResult> IButterMorphLanguageTypeMappingHost.Load(ButterMorphLanguageTypeMappingLoadRequest request)
    {
        List<ButterMorphLanguageTypeMappingDocument> documents = store.LanguageTypeMappings
            .Where(item => string.IsNullOrWhiteSpace(request.LanguageKey) || string.Equals(item.LanguageKey, request.LanguageKey, StringComparison.OrdinalIgnoreCase))
            .Select(item => NormalizeLanguageTypeMappingDocument(item.Document))
            .ToList();
        if (!string.IsNullOrWhiteSpace(request.ContextKey) &&
            documents.All(document => !string.Equals(document.Key, request.ContextKey, StringComparison.OrdinalIgnoreCase)))
        {
            documents.Add(new ButterMorphLanguageTypeMappingDocument
            {
                Key = string.Empty,
                Name = string.Empty,
                Description = string.Empty,
                Language = string.IsNullOrWhiteSpace(request.LanguageKey) ? "csharp" : request.LanguageKey
            });
        }

        IReadOnlyCollection<ButterMorphLanguageTypeBundle> bundles = store.LanguageTypeBundles
            .Where(item => string.IsNullOrWhiteSpace(request.LanguageKey) || string.Equals(item.LanguageKey, request.LanguageKey, StringComparison.OrdinalIgnoreCase))
            .Select(item => item.Bundle)
            .ToArray();
        string activeDocumentKey = documents.Any(document => string.Equals(document.Key, request.ContextKey, StringComparison.OrdinalIgnoreCase))
            ? request.ContextKey
            : documents.FirstOrDefault(document => string.IsNullOrWhiteSpace(document.Key))?.Key ?? documents.FirstOrDefault()?.Key ?? string.Empty;

        return Task.FromResult(new ButterMorphLanguageTypeMappingLoadResult
        {
            ButterMorphTypes = CreateButterMorphTypeCatalog(),
            Documents = documents,
            Profiles = documents.Select(document => ButterMorphLanguageTypeMappingDocumentConverter.ToProfile(document)).ToArray(),
            Bundles = bundles,
            ActiveDocumentKey = activeDocumentKey,
            ActiveProfileId = activeDocumentKey
        });
    }

    /// <inheritdoc />
    Task<ButterMorphLanguageTypeMappingSaveResult> IButterMorphLanguageTypeMappingHost.SaveProfile(ButterMorphLanguageTypeMappingSaveRequest request)
    {
        ButterMorphLanguageTypeMappingDocument document = request.Document;
        if (string.IsNullOrWhiteSpace(document?.Key) && request.Profile != null)
        {
            document = ButterMorphLanguageTypeMappingDocumentConverter.FromProfile(request.Profile);
        }

        document ??= new ButterMorphLanguageTypeMappingDocument();
        ButterMorphLanguageTypeMappingDocumentConverter.Normalize(document);
        string documentId = document.Key;

        store.SaveLanguageTypeMapping(new StudioLanguageTypeMappingDocument
        {
            Id = documentId,
            LanguageKey = document.Language,
            Document = document
        });

        return Task.FromResult(new ButterMorphLanguageTypeMappingSaveResult
        {
            Succeeded = true,
            Document = document,
            Profile = ButterMorphLanguageTypeMappingDocumentConverter.ToProfile(document),
            Message = "Language type mappings saved into Studio host."
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
    /// Creates source schemas for a host-owned source setup.
    /// </summary>
    /// <param name="sourceSchemaIds">The source schema ids keyed by source alias.</param>
    /// <returns>The imported source schemas.</returns>
    public Dictionary<string, IStructureSchema> CreateSourceSchemas(IReadOnlyDictionary<string, string> sourceSchemaIds)
    {
        Dictionary<string, IStructureSchema> sourceSchemas = new(StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, string> source in sourceSchemaIds)
        {
            if (store.TryGetSchema(source.Value, out StudioSchema schema) &&
                TryImportSchema(schema, out IStructureSchema importedSchema))
            {
                sourceSchemas[source.Key] = importedSchema;
            }
        }

        return sourceSchemas;
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
                Version = item.Version,
                VersionComment = item.VersionComment,
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

    private IReadOnlyCollection<ButterMorphTypeMappingCatalogItem> CreateButterMorphTypeCatalog()
    {
        List<ButterMorphTypeMappingCatalogItem> items =
        [
            TypeMapping("string", "String", "builtin", "Base text value."),
            TypeMapping("number", "Number", "builtin", "Base numeric value."),
            TypeMapping("boolean", "Boolean", "builtin", "Base boolean value."),
            TypeMapping("date", "Date", "builtin", "Base date value."),
            TypeMapping("datetime", "DateTime", "builtin", "Base date-time value."),
            TypeMapping("time", "Time", "builtin", "Base time value."),
            TypeMapping("timespan", "TimeSpan", "builtin", "Base duration value."),
            TypeMapping("object", "Object", "builtin", "Base object value.")
        ];

        foreach (StudioCustomType customType in store.CustomTypes)
        {
            items.Add(TypeMapping(customType.Key, customType.Name, customType.Version, customType.Description));
        }

        return items;
    }

    private static ButterMorphTypeMappingCatalogItem TypeMapping(string key, string displayName, string version, string description)
    {
        return new ButterMorphTypeMappingCatalogItem
        {
            Key = key,
            DisplayName = displayName,
            Version = version,
            Description = description
        };
    }

    private static ButterMorphLanguageTypeMappingDocument NormalizeLanguageTypeMappingDocument(ButterMorphLanguageTypeMappingDocument document)
    {
        ButterMorphLanguageTypeMappingDocumentConverter.Normalize(document);
        return document;
    }

    private StudioMapping ResolveMappingForLoad(string id)
    {
        if (store.TryGetMapping(id, out StudioMapping mapping))
        {
            return mapping;
        }

        return CreateMappingFromSetup(id);
    }

    private StudioValidation ResolveValidationForLoad(string id)
    {
        if (store.TryGetValidation(id, out StudioValidation validation))
        {
            return validation;
        }

        return CreateValidationFromSetup(id);
    }

    /// <summary>
    /// Resolves the mapping document from the runtime document or the persisted DSL.
    /// </summary>
    /// <param name="mapping">The mapping.</param>
    /// <returns>The resolved transformation document.</returns>
    public ITransformationDocument ResolveMappingDocument(StudioMapping mapping)
    {
        if (mapping.Document != null)
        {
            return mapping.Document;
        }

        if (!string.IsNullOrWhiteSpace(mapping.DslContent))
        {
            try
            {
                return dslParser.Parse(new DslDefinition { Content = mapping.DslContent }) as ITransformationDocument
                    ?? new TransformationDocument();
            }
            catch (FormatException)
            {
                return new TransformationDocument();
            }
        }

        return new TransformationDocument();
    }

    /// <summary>
    /// Resolves the validation document from the runtime document or the persisted DSL.
    /// </summary>
    /// <param name="validation">The validation.</param>
    /// <returns>The resolved validation document.</returns>
    public IValidationDocument ResolveValidationDocument(StudioValidation validation)
    {
        if (validation.Document != null)
        {
            return validation.Document;
        }

        if (!string.IsNullOrWhiteSpace(validation.DslContent))
        {
            try
            {
                return dslParser.Parse(new DslDefinition { Content = validation.DslContent }) as IValidationDocument
                    ?? new ValidationDocument();
            }
            catch (FormatException)
            {
                return new ValidationDocument();
            }
        }

        return new ValidationDocument();
    }

    private StudioMapping CreateMappingFromSetup(string id)
    {
        StudioMapping mapping = new()
        {
            Id = id,
            Name = id,
            ShowSchemaActions = true,
            Document = new TransformationDocument()
        };

        if (!store.TryGetMappingSetup(id, out StudioMappingSetup setup))
        {
            return mapping;
        }

        mapping.Name = string.IsNullOrWhiteSpace(setup.Name) ? id : setup.Name;
        mapping.TargetSchemaId = setup.TargetSchemaId;
        mapping.ShowSchemaActions = setup.ShowSchemaActions;
        foreach (KeyValuePair<string, string> source in setup.SourceSchemaIds)
        {
            mapping.SourceSchemaIds[source.Key] = source.Value;
        }

        return mapping;
    }

    private StudioValidation CreateValidationFromSetup(string id)
    {
        StudioValidation validation = new()
        {
            Id = id,
            Name = id,
            Document = new ValidationDocument()
        };

        if (!store.TryGetValidationSetup(id, out StudioValidationSetup setup))
        {
            return validation;
        }

        validation.Name = string.IsNullOrWhiteSpace(setup.Name) ? id : setup.Name;
        foreach (KeyValuePair<string, string> source in setup.SourceSchemaIds)
        {
            validation.SourceSchemaIds[source.Key] = source.Value;
        }

        return validation;
    }

    private static IReadOnlyDictionary<string, ButterMorphDesignerSourceMetadata> CreateSourceMetadata(
        IReadOnlyDictionary<string, string> sourceSchemaIds,
        IReadOnlyDictionary<string, IStructureSchema> sourceSchemas)
    {
        Dictionary<string, ButterMorphDesignerSourceMetadata> sourceMetadata = new(StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, string> source in sourceSchemaIds)
        {
            if (!sourceSchemas.TryGetValue(source.Key, out IStructureSchema schema))
            {
                continue;
            }

            sourceMetadata[source.Key] = new ButterMorphDesignerSourceMetadata
            {
                DisplayName = string.IsNullOrWhiteSpace(schema.Name) ? source.Key : schema.Name,
                Description = schema.Description,
                Tags = string.IsNullOrWhiteSpace(schema.Key) ? [] : [schema.Key]
            };
        }

        return sourceMetadata;
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

    private static T ReadDefinition<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json, ResultJsonOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    private static string SerializeStringArray(IReadOnlyCollection<string> values)
    {
        return JsonSerializer.Serialize(values ?? []);
    }

    private static string SerializeElement(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Undefined)
        {
            return string.Empty;
        }

        return element.GetRawText();
    }

    private static string SerializeElementMap(IReadOnlyDictionary<string, JsonElement> values)
    {
        if (values == null || values.Count == 0)
        {
            return "{}";
        }

        return JsonSerializer.Serialize(values, ResultJsonOptions);
    }

    private static string ResolveSchemaJson(StudioSchema schema)
    {
        return string.IsNullOrWhiteSpace(schema.JsonSchema)
            ? schema.ButterMorphResultJson
            : schema.JsonSchema;
    }
}

