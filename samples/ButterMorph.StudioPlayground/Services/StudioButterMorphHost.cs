namespace ButterMorph.StudioPlayground.Services;

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
        foreach (KeyValuePair<string, string> source in mapping.SourceSchemaKeys)
        {
            if (store.TryGetSchema(source.Value, out StudioSchema schema) &&
                TryImportSchema(schema, out IStructureSchema importedSchema))
            {
                sourceSchemas[source.Key] = importedSchema;
            }
        }

        IStructureSchema targetSchema = null;
        if (store.TryGetSchema(mapping.TargetSchemaKey, out StudioSchema target) &&
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
        SchemaTypeDesignResult result = request.Result;
        store.SaveCustomType(new StudioCustomType
        {
            ContextKey = request.ContextKey,
            Key = result.Key,
            Name = result.Name,
            Description = result.Description,
            Version = result.VersionNumber,
            BaseType = result.BaseType,
            Comment = result.Comment,
            JsonSchema = result.JsonSchema
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
            input.Name = item.Name;
            input.Key = item.Key;
            input.Description = item.Description;
            input.DataType = item.DataType;
            input.AppliesTo = item.AppliesToJson;
            input.IsRequired = item.IsRequired;
            input.IsActive = item.IsActive;
            input.ChildrenDefinitionJson = item.ChildrenDefinitionJson;
            input.ArrayItemDataType = item.ArrayItemDataType;
            input.ArrayItemDefinitionJson = item.ArrayItemDefinitionJson;
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
        FieldMetadataDesignResult result = request.Result;
        store.SaveCustomField(new StudioCustomField
        {
            ContextKey = request.ContextKey,
            Key = result.Key,
            Name = result.Name,
            Description = result.Description,
            DataType = result.DataType,
            AppliesToJson = result.AppliesToJson,
            IsRequired = result.IsRequired,
            IsActive = result.IsActive,
            ValidationJson = result.ValidationJson,
            ChildrenDefinitionJson = result.ChildrenDefinitionJson,
            ArrayItemDataType = result.ArrayItemDataType,
            ArrayItemDefinitionJson = result.ArrayItemDefinitionJson
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
            : new StudioSchema { ContextKey = request.ContextKey, Version = "1.0.0" };

        IReadOnlyCollection<StudioCustomType> injectedTypes = store.CustomTypes
            .Where(item => schema.InjectedCustomTypeKeys.Contains(item.ContextKey, StringComparer.OrdinalIgnoreCase))
            .ToArray();
        IReadOnlyCollection<StudioCustomField> injectedFields = store.CustomFields
            .Where(item => schema.InjectedCustomFieldKeys.Contains(item.ContextKey, StringComparer.OrdinalIgnoreCase))
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
        PayloadSchemaDesignResult result = request.Result;
        StudioSchema schema = store.TryGetSchema(request.ContextKey, out StudioSchema existing)
            ? existing
            : new StudioSchema { ContextKey = request.ContextKey };

        schema.Key = result.Key;
        schema.Name = result.Name;
        schema.Description = result.Description;
        schema.Version = result.Version;
        schema.VersionComment = result.VersionComment;
        schema.JsonSchema = result.JsonSchema;
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
                TypeVersionId = item.ContextKey,
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
                Id = item.ContextKey,
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

    private static StudioMapping CreateDraftMapping(string contextKey)
    {
        return new StudioMapping
        {
            ContextKey = contextKey,
            Name = contextKey
        };
    }
}
