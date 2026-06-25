using ButterMorph.Abstractions;
using ButterMorph.Json.Schema;
using ButterMorph.SchemaDesign;
using ButterMorph.Web.Razor;

/// <summary>
/// Provides playground schema designer host integration.
/// </summary>
internal sealed class PlaygroundSchemaDesignerHost :
    IButterMorphSchemaDesignerHost,
    IButterMorphSchemaTypeDesignerHost,
    IButterMorphFieldMetadataDesignerHost,
    IButterMorphPayloadSchemaDesignerHost
{
    // JSON text used for map-shaped schemas.
    private const string MapType = "obj" + "ect";

    // Stores saved schemas in memory.
    private readonly PlaygroundSchemaStore store;

    // Exports initial schemas for the playground shell.
    private readonly IJsonSchemaExporter exporter;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaygroundSchemaDesignerHost"/> class.
    /// </summary>
    /// <param name="store">The schema save store.</param>
    /// <param name="exporter">The JSON Schema exporter.</param>
    public PlaygroundSchemaDesignerHost(PlaygroundSchemaStore store, IJsonSchemaExporter exporter)
    {
        this.store = store;
        this.exporter = exporter;
    }

    /// <summary>
    /// Loads legacy schema designer state.
    /// </summary>
    /// <param name="request">The load request.</param>
    /// <returns>The load result.</returns>
    public Task<ButterMorphSchemaDesignerLoadResult> Load(ButterMorphSchemaDesignerLoadRequest request)
    {
        if (TryCreateSchema(request.ContextKey, out IStructureSchema schema))
        {
            return Task.FromResult(new ButterMorphSchemaDesignerLoadResult
            {
                Schema = schema,
                ShowManualActions = false
            });
        }

        return Task.FromResult(new ButterMorphSchemaDesignerLoadResult());
    }

    /// <summary>
    /// Saves legacy schema designer state.
    /// </summary>
    /// <param name="request">The save request.</param>
    /// <returns>The save result.</returns>
    public Task<ButterMorphSchemaDesignerSaveResult> Save(ButterMorphSchemaDesignerSaveRequest request)
    {
        store.Save(new PlaygroundSchemaSave
        {
            ContextKey = request.ContextKey,
            Schema = request.Schema,
            JsonSchema = request.JsonSchema,
            SavedAt = DateTimeOffset.UtcNow.ToString("O")
        });

        return Task.FromResult(new ButterMorphSchemaDesignerSaveResult
        {
            Succeeded = true,
            Message = "Schema saved."
        });
    }

    /// <summary>
    /// Loads schema type designer state.
    /// </summary>
    /// <param name="request">The load request.</param>
    /// <returns>The load result.</returns>
    public Task<ButterMorphSchemaTypeDesignerLoadResult> Load(ButterMorphSchemaTypeDesignerLoadRequest request)
    {
        if (TryGetDesignState(request.ContextKey, out PlaygroundSchemaSave save))
        {
            return Task.FromResult(new ButterMorphSchemaTypeDesignerLoadResult
            {
                Input = CreateTypeInput(save),
                SchemaTypes = CreateTypeCatalog(),
                ShowManualActions = false
            });
        }

        return Task.FromResult(new ButterMorphSchemaTypeDesignerLoadResult
        {
            Input = CreateTypeInput(request.ContextKey),
            SchemaTypes = CreateTypeCatalog(),
            ShowManualActions = false
        });
    }

    /// <summary>
    /// Saves schema type designer state.
    /// </summary>
    /// <param name="request">The save request.</param>
    /// <returns>The save result.</returns>
    public Task<ButterMorphSchemaTypeDesignerSaveResult> Save(ButterMorphSchemaTypeDesignerSaveRequest request)
    {
        store.Save(new PlaygroundSchemaSave
        {
            ContextKey = request.ContextKey,
            Kind = "type",
            Key = request.Result.Key,
            DisplayName = request.Result.Name,
            Description = request.Result.Description,
            DesignerPath = "/buttermorph/schema-types/designer",
            JsonSchema = request.Result.JsonSchema,
            SavedAt = DateTimeOffset.UtcNow.ToString("O"),
            VersionNumber = request.Result.VersionNumber,
            BaseType = request.Result.BaseType,
            Comment = request.Result.Comment,
            VersionComment = request.Result.Comment
        });

        return Task.FromResult(new ButterMorphSchemaTypeDesignerSaveResult
        {
            Succeeded = true,
            Message = "Schema type saved."
        });
    }

    /// <summary>
    /// Loads field metadata designer state.
    /// </summary>
    /// <param name="request">The load request.</param>
    /// <returns>The load result.</returns>
    public Task<ButterMorphFieldMetadataDesignerLoadResult> Load(ButterMorphFieldMetadataDesignerLoadRequest request)
    {
        if (TryGetDesignState(request.ContextKey, out PlaygroundSchemaSave save))
        {
            return Task.FromResult(new ButterMorphFieldMetadataDesignerLoadResult
            {
                Input = CreateMetadataInput(save),
                ShowManualActions = false
            });
        }

        return Task.FromResult(new ButterMorphFieldMetadataDesignerLoadResult
        {
            Input = CreateMetadataInput(request.ContextKey),
            ShowManualActions = false
        });
    }

    /// <summary>
    /// Saves field metadata designer state.
    /// </summary>
    /// <param name="request">The save request.</param>
    /// <returns>The save result.</returns>
    public Task<ButterMorphFieldMetadataDesignerSaveResult> Save(ButterMorphFieldMetadataDesignerSaveRequest request)
    {
        store.Save(new PlaygroundSchemaSave
        {
            ContextKey = request.ContextKey,
            Kind = "field",
            DisplayName = request.Result.Name,
            Description = request.Result.Description,
            DesignerPath = "/buttermorph/metadata-fields/designer",
            JsonSchema = request.Result.ValidationJson,
            SavedAt = DateTimeOffset.UtcNow.ToString("O"),
            Key = request.Result.Key,
            DataType = request.Result.DataType,
            AppliesToJson = request.Result.AppliesToJson,
            ValidationJson = request.Result.ValidationJson,
            IsRequired = request.Result.IsRequired,
            IsActive = request.Result.IsActive,
            ChildrenDefinitionJson = request.Result.ChildrenDefinitionJson,
            ArrayItemDataType = request.Result.ArrayItemDataType,
            ArrayItemDefinitionJson = request.Result.ArrayItemDefinitionJson
        });

        return Task.FromResult(new ButterMorphFieldMetadataDesignerSaveResult
        {
            Succeeded = true,
            Message = "Custom field saved."
        });
    }

    /// <summary>
    /// Loads payload schema designer state.
    /// </summary>
    /// <param name="request">The load request.</param>
    /// <returns>The load result.</returns>
    public Task<ButterMorphPayloadSchemaDesignerLoadResult> Load(ButterMorphPayloadSchemaDesignerLoadRequest request)
    {
        if (TryGetDesignState(request.ContextKey, out PlaygroundSchemaSave save))
        {
            return Task.FromResult(new ButterMorphPayloadSchemaDesignerLoadResult
            {
                Key = save.Key,
                Name = save.DisplayName,
                Description = save.Description,
                Version = ResolveValue(save.VersionNumber, "1.0.0"),
                VersionComment = save.VersionComment,
                Metadata = ParseMetadataJson(save.MetadataJson),
                MetadataDefinition = CreatePayloadMetadataDefinition(),
                JsonSchema = save.JsonSchema,
                SchemaTypes = CreateTypeCatalog(),
                MetadataFields = CreateMetadataCatalog(),
                ShowManualActions = false
            });
        }

        return Task.FromResult(new ButterMorphPayloadSchemaDesignerLoadResult
        {
            Key = ResolveSchemaKey(request.ContextKey),
            Name = ResolveDisplayName(request.ContextKey),
            Description = ResolveDescription(request.ContextKey),
            Version = "1.0.0",
            VersionComment = string.Empty,
            Metadata = new Dictionary<string, string>(StringComparer.Ordinal),
            MetadataDefinition = CreatePayloadMetadataDefinition(),
            JsonSchema = CreatePayloadJson(request.ContextKey),
            SchemaTypes = CreateTypeCatalog(),
            MetadataFields = CreateMetadataCatalog(),
            ShowManualActions = false
        });
    }

    /// <summary>
    /// Saves payload schema designer state.
    /// </summary>
    /// <param name="request">The save request.</param>
    /// <returns>The save result.</returns>
    public Task<ButterMorphPayloadSchemaDesignerSaveResult> Save(ButterMorphPayloadSchemaDesignerSaveRequest request)
    {
        store.Save(new PlaygroundSchemaSave
        {
            ContextKey = request.ContextKey,
            Kind = "payload",
            Key = request.Result.Key,
            DisplayName = request.Result.Name,
            Description = request.Result.Description,
            DesignerPath = "/buttermorph/payload-schema/designer",
            JsonSchema = request.Result.JsonSchema,
            SavedAt = DateTimeOffset.UtcNow.ToString("O"),
            VersionNumber = request.Result.Version,
            VersionComment = request.Result.VersionComment,
            MetadataJson = SerializeMetadata(request.Result.Metadata)
        });

        return Task.FromResult(new ButterMorphPayloadSchemaDesignerSaveResult
        {
            Succeeded = true,
            Message = "Payload schema saved."
        });
    }

    /// <summary>
    /// Lists prepared schema tool scenarios.
    /// </summary>
    /// <returns>The prepared schema tool scenarios.</returns>
    public IReadOnlyCollection<PlaygroundSchemaScenarioSummary> ListScenarios()
    {
        return
        [
            new PlaygroundSchemaScenarioSummary
            {
                ContextKey = "payload-customer-profile",
                DisplayName = "Edit schema",
                Description = "Build a reusable structure from datatypes and metadata.",
                DesignerPath = "/buttermorph/payload-schema/designer"
            }
        ];
    }

    /// <summary>
    /// Attempts to create a schema for a prepared scenario.
    /// </summary>
    /// <param name="contextKey">The schema context key.</param>
    /// <param name="schema">The created schema.</param>
    /// <returns><see langword="true"/> when the context exists.</returns>
    public bool TryCreateSchema(string contextKey, out IStructureSchema schema)
    {
        if (string.Equals(contextKey, "customer-schema", StringComparison.OrdinalIgnoreCase))
        {
            schema = PlaygroundDesignerHost.CreateCustomerSchema();
            return true;
        }

        if (string.Equals(contextKey, "invoice-schema", StringComparison.OrdinalIgnoreCase))
        {
            schema = PlaygroundDesignerHost.CreateInvoiceSchema();
            return true;
        }

        if (string.Equals(contextKey, "support-schema", StringComparison.OrdinalIgnoreCase))
        {
            schema = PlaygroundDesignerHost.CreateTicketSchema();
            return true;
        }

        schema = PlaygroundDesignerHost.CreateCustomerSchema();
        return false;
    }

    /// <summary>
    /// Creates a JSON view for a context.
    /// </summary>
    /// <param name="contextKey">The schema context key.</param>
    /// <returns>The schema view.</returns>
    public PlaygroundSchemaView CreateView(string contextKey)
    {
        if (store.TryGet(contextKey, out PlaygroundSchemaSave save))
        {
            return new PlaygroundSchemaView
            {
                ContextKey = contextKey,
                Kind = save.Kind,
                DisplayName = ResolveSavedDisplayName(contextKey, save),
                Description = save.Description,
                DesignerPath = ResolveDesignerPath(contextKey, save),
                JsonSchema = save.JsonSchema,
                SavedAt = save.SavedAt,
                VersionNumber = ResolveValue(save.VersionNumber, "1.0.0"),
                BaseType = save.BaseType,
                Comment = save.Comment,
                VersionComment = save.VersionComment,
                Key = save.Key,
                DataType = save.DataType,
                AppliesToJson = save.AppliesToJson,
                ValidationJson = save.ValidationJson,
                IsRequired = save.IsRequired,
                IsActive = save.IsActive,
                ChildrenDefinitionJson = save.ChildrenDefinitionJson,
                ArrayItemDataType = save.ArrayItemDataType,
                ArrayItemDefinitionJson = save.ArrayItemDefinitionJson
            };
        }

        PlaygroundSchemaView view = new()
        {
            ContextKey = contextKey,
            Kind = ResolveKind(contextKey),
            DisplayName = ResolveDisplayName(contextKey),
            Description = ResolveDescription(contextKey),
            DesignerPath = ResolveDesignerPath(contextKey),
            JsonSchema = CreateInitialJson(contextKey),
            SavedAt = string.Empty
        };

        if (view.Kind == "field")
        {
            FieldMetadataDesignInput input = CreateMetadataInput(contextKey);
            FieldMetadataDefinitionBuilder builder = new();
            FieldMetadataDesignResult result = builder.Build(input);
            view.Key = input.Key;
            view.DataType = input.DataType;
            view.AppliesToJson = result.AppliesToJson;
            view.ValidationJson = result.ValidationJson;
            view.IsRequired = input.IsRequired;
            view.IsActive = input.IsActive;
            view.ChildrenDefinitionJson = input.ChildrenDefinitionJson;
            view.ArrayItemDataType = input.ArrayItemDataType;
            view.ArrayItemDefinitionJson = input.ArrayItemDefinitionJson;
        }

        if (view.Kind == "type")
        {
            SchemaTypeDesignInput input = CreateTypeInput(contextKey);
            view.VersionNumber = input.VersionNumber;
            view.BaseType = input.BaseType;
            view.Comment = input.Comment;
            view.VersionComment = input.Comment;
            view.Key = input.Key;
        }

        if (view.Kind == "payload")
        {
            view.Key = ResolveSchemaKey(contextKey);
            view.VersionNumber = "1.0.0";
            view.VersionComment = string.Empty;
            view.MetadataJson = "{}";
        }

        return view;
    }

    /// <summary>
    /// Saves schema item state received from browser storage.
    /// </summary>
    /// <param name="item">The schema item.</param>
    public void SaveClientItem(PlaygroundSchemaClientItem item)
    {
        store.SaveClientItem(item);
    }

    // Resolves draft state first so popup edits can preload without committing to the playground list.
    private bool TryGetDesignState(string contextKey, out PlaygroundSchemaSave save)
    {
        if (store.TryGetDraft(contextKey, out save))
        {
            return true;
        }

        return store.TryGet(contextKey, out save);
    }

    // Creates initial JSON for the selected schema tool.
    private string CreateInitialJson(string contextKey)
    {
        return CreatePayloadJson(contextKey);
    }

    // Creates schema type input for demo contexts.
    private static SchemaTypeDesignInput CreateTypeInput(string contextKey)
    {
        return new SchemaTypeDesignInput();
    }

    // Creates schema type input from a saved item.
    private static SchemaTypeDesignInput CreateTypeInput(PlaygroundSchemaSave save)
    {
        SchemaTypeDesignInput input = new()
        {
            Name = save.DisplayName,
            Key = save.Key,
            Description = save.Description,
            VersionNumber = ResolveValue(save.VersionNumber, "1.0.0"),
            BaseType = ResolveValue(save.BaseType, "string"),
            Comment = ResolveValue(save.Comment, save.VersionComment)
        };

        HydrateTypeSchema(input, save.JsonSchema);

        return input;
    }

    // Hydrates editable schema type fields from saved JSON Schema.
    private static void HydrateTypeSchema(SchemaTypeDesignInput input, string jsonSchema)
    {
        if (string.IsNullOrWhiteSpace(jsonSchema))
        {
            return;
        }

        try
        {
            using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(jsonSchema);
            System.Text.Json.JsonElement root = document.RootElement;

            if (root.TryGetProperty("type", out System.Text.Json.JsonElement typeElement))
            {
                input.BaseType = ResolveValue(typeElement.ToString(), input.BaseType);
            }

            ReadSchemaString(root, "key", value => input.Key = ResolveValue(input.Key, value));
            ReadSchemaString(root, "name", value => input.Name = ResolveValue(input.Name, value));
            ReadSchemaString(root, "description", value => input.Description = ResolveValue(input.Description, value));
            ReadSchemaString(root, "version", value => input.VersionNumber = ResolveValue(input.VersionNumber, value));
            ReadSchemaString(root, "versionComment", value => input.Comment = ResolveValue(input.Comment, value));
            ReadSchemaString(root, "minLength", value => input.MinLength = value);
            ReadSchemaString(root, "maxLength", value => input.MaxLength = value);
            ReadSchemaString(root, "pattern", value => input.Pattern = value);
            ReadSchemaString(root, "minimum", value => input.Minimum = value);
            ReadSchemaString(root, "maximum", value => input.Maximum = value);
            ReadSchemaString(root, "precision", value => input.Precision = value);
            ReadSchemaString(root, "scale", value => input.Scale = value);
            ReadSchemaString(root, "minItems", value => input.MinItems = value);
            ReadSchemaString(root, "maxItems", value => input.MaxItems = value);

            if (root.TryGetProperty("enum", out System.Text.Json.JsonElement enumElement))
            {
                input.AllowedValuesJson = enumElement.GetRawText();
            }

            HydrateArrayItem(input, root);

            if (string.Equals(input.BaseType, MapType, StringComparison.OrdinalIgnoreCase))
            {
                input.PayloadSchemaJson = jsonSchema;
            }
        }
        catch (System.Text.Json.JsonException)
        {
            input.PayloadSchemaJson = jsonSchema;
        }
    }

    // Hydrates array item type fields from saved JSON Schema.
    private static void HydrateArrayItem(SchemaTypeDesignInput input, System.Text.Json.JsonElement root)
    {
        if (!root.TryGetProperty("items", out System.Text.Json.JsonElement itemsElement))
        {
            return;
        }

        if (itemsElement.TryGetProperty("typeVersionId", out System.Text.Json.JsonElement versionElement))
        {
            input.ArrayItemTypeVersionId = versionElement.ToString();
        }

        if (itemsElement.TryGetProperty("type", out System.Text.Json.JsonElement typeElement))
        {
            input.ArrayItemType = typeElement.ToString();
        }

        if (string.Equals(input.ArrayItemType, MapType, StringComparison.OrdinalIgnoreCase) &&
            itemsElement.TryGetProperty("properties", out System.Text.Json.JsonElement propertiesElement))
        {
            input.PayloadSchemaJson = "{\"type\":\"" + MapType + "\",\"properties\":" + propertiesElement.GetRawText() + ReadRequiredSuffix(itemsElement) + "}";
        }
    }

    // Reads required suffix for hydrated array object schemas.
    private static string ReadRequiredSuffix(System.Text.Json.JsonElement element)
    {
        if (element.TryGetProperty("required", out System.Text.Json.JsonElement requiredElement))
        {
            return ",\"required\":" + requiredElement.GetRawText();
        }

        return string.Empty;
    }

    // Reads a JSON Schema property as compact text.
    private static void ReadSchemaString(System.Text.Json.JsonElement root, string propertyName, Action<string> assign)
    {
        if (root.TryGetProperty(propertyName, out System.Text.Json.JsonElement element))
        {
            assign(element.ToString());
        }
    }

    // Creates field metadata input for demo contexts.
    private static FieldMetadataDesignInput CreateMetadataInput(string contextKey)
    {
        return new FieldMetadataDesignInput
        {
            DataType = "string",
            IsActive = true
        };
    }

    // Creates field metadata input from a saved item.
    private static FieldMetadataDesignInput CreateMetadataInput(PlaygroundSchemaSave save)
    {
        return new FieldMetadataDesignInput
        {
            Name = save.DisplayName,
            Key = save.Key,
            Description = save.Description,
            DataType = ResolveValue(save.DataType, "string"),
            AppliesTo = ConvertJsonArrayToLines(save.AppliesToJson),
            IsRequired = save.IsRequired,
            IsActive = save.IsActive,
            AllowedValues = ConvertAllowedValuesToLines(save.ValidationJson),
            ChildrenDefinitionJson = save.ChildrenDefinitionJson,
            ArrayItemDataType = save.ArrayItemDataType,
            ArrayItemDefinitionJson = save.ArrayItemDefinitionJson
        };
    }

    // Creates payload schema JSON.
    private string CreatePayloadJson(string contextKey)
    {
        if (!string.Equals(contextKey, "payload-customer-profile", StringComparison.OrdinalIgnoreCase))
        {
            string key = ResolveSchemaKey(contextKey);
            return "{\"key\":\"" + key + "\",\"name\":\"" + key + "\",\"version\":\"1.0.0\",\"type\":\"" + MapType + "\",\"properties\":{}}";
        }

        JsonSchemaConversionResult result = exporter.Export(new JsonSchemaExportRequest
        {
            Schema = PlaygroundDesignerHost.CreateCustomerSchema()
        });

        return result.JsonSchema;
    }

    // Creates available schema types.
    private IReadOnlyCollection<SchemaTypeCatalogItem> CreateTypeCatalog()
    {
        List<SchemaTypeCatalogItem> items =
        [
            CreateSystemType("string"),
            CreateSystemType("number"),
            CreateSystemType("integer"),
            CreateSystemType("boolean"),
            CreateSystemType(MapType),
            CreateSystemType("array")
        ];

        foreach (PlaygroundSchemaSave save in store.ListDesignStates())
        {
            if (!IsSavedType(save))
            {
                continue;
            }

            items.Add(new SchemaTypeCatalogItem
            {
                TypeId = ResolveValue(save.Key, save.ContextKey),
                TypeVersionId = save.ContextKey,
                Name = ResolveValue(save.DisplayName, save.Key),
                VersionNumber = ResolveValue(save.VersionNumber, "1.0.0"),
                BaseType = ResolveValue(save.BaseType, "string"),
                JsonSchema = save.JsonSchema,
                IsSystem = false
            });
        }

        return items;
    }

    // Creates available metadata fields.
    private IReadOnlyCollection<FieldMetadataCatalogItem> CreateMetadataCatalog()
    {
        List<FieldMetadataCatalogItem> items = [];

        foreach (PlaygroundSchemaSave save in store.ListDesignStates())
        {
            if (!IsSavedField(save))
            {
                continue;
            }

            items.Add(new FieldMetadataCatalogItem
            {
                Id = ResolveValue(save.Key, save.ContextKey),
                Name = ResolveValue(save.DisplayName, save.Key),
                Key = save.Key,
                Description = save.Description,
                DataType = ResolveValue(save.DataType, "string"),
                IsRequired = save.IsRequired,
                AppliesToJson = save.AppliesToJson,
                Validation = save.ValidationJson,
                ChildrenDefinitionJson = save.ChildrenDefinitionJson,
                ArrayItemDataType = save.ArrayItemDataType,
                ArrayItemDefinitionJson = save.ArrayItemDefinitionJson
            });
        }

        return items;
    }

    // Detects saved custom types that are valid for injection into schema designers.
    private static bool IsSavedType(PlaygroundSchemaSave save)
    {
        return string.Equals(save.Kind, "type", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(save.Key) &&
            !string.IsNullOrWhiteSpace(save.DisplayName) &&
            !string.IsNullOrWhiteSpace(save.BaseType) &&
            !string.IsNullOrWhiteSpace(save.JsonSchema);
    }

    // Detects saved custom fields that are valid for injection into schema designers.
    private static bool IsSavedField(PlaygroundSchemaSave save)
    {
        return string.Equals(save.Kind, "field", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(save.Key) &&
            !string.IsNullOrWhiteSpace(save.DisplayName) &&
            !string.IsNullOrWhiteSpace(save.DataType) &&
            !string.IsNullOrWhiteSpace(save.AppliesToJson);
    }

    // Creates host-defined payload metadata fields for the playground.
    private static SchemaMetadataDefinition CreatePayloadMetadataDefinition()
    {
        return new SchemaMetadataDefinition();
    }

    // Creates a system catalog item.
    private static SchemaTypeCatalogItem CreateSystemType(string baseType)
    {
        return new SchemaTypeCatalogItem
        {
            Name = baseType,
            BaseType = baseType,
            VersionNumber = "1.0.0",
            IsSystem = true
        };
    }

    // Resolves a schema scenario display name.
    private string ResolveDisplayName(string contextKey)
    {
        foreach (PlaygroundSchemaScenarioSummary scenario in ListScenarios())
        {
            if (string.Equals(scenario.ContextKey, contextKey, StringComparison.OrdinalIgnoreCase))
            {
                return scenario.DisplayName;
            }
        }

        return contextKey;
    }

    // Resolves display name for saved items.
    private string ResolveSavedDisplayName(string contextKey, PlaygroundSchemaSave save)
    {
        if (!string.IsNullOrWhiteSpace(save.DisplayName))
        {
            return save.DisplayName;
        }

        return ResolveDisplayName(contextKey);
    }

    // Resolves schema item description.
    private static string ResolveDescription(string contextKey)
    {
        if (contextKey.StartsWith("datatype-", StringComparison.OrdinalIgnoreCase))
        {
            return "Create a versioned custom datatype.";
        }

        if (contextKey.StartsWith("metadata-", StringComparison.OrdinalIgnoreCase))
        {
            return "Create a reusable custom field.";
        }

        return "Build a reusable structure from datatypes and metadata.";
    }

    // Resolves schema item kind.
    private static string ResolveKind(string contextKey)
    {
        if (contextKey.StartsWith("datatype-", StringComparison.OrdinalIgnoreCase))
        {
            return "type";
        }

        if (contextKey.StartsWith("metadata-", StringComparison.OrdinalIgnoreCase))
        {
            return "field";
        }

        return "payload";
    }

    // Resolves a canonical schema key for known playground contexts.
    private static string ResolveSchemaKey(string contextKey)
    {
        if (string.Equals(contextKey, "payload-customer-profile", StringComparison.OrdinalIgnoreCase))
        {
            return "customer-profile";
        }

        if (contextKey.StartsWith("payload-", StringComparison.OrdinalIgnoreCase))
        {
            return contextKey["payload-".Length..];
        }

        if (contextKey.StartsWith("datatype-", StringComparison.OrdinalIgnoreCase))
        {
            return contextKey["datatype-".Length..];
        }

        return contextKey;
    }

    // Resolves designer path for saved items.
    private static string ResolveDesignerPath(string contextKey, PlaygroundSchemaSave save)
    {
        if (!string.IsNullOrWhiteSpace(save.DesignerPath))
        {
            return save.DesignerPath;
        }

        return ResolveDesignerPath(contextKey);
    }

    // Resolves designer path from context.
    private static string ResolveDesignerPath(string contextKey)
    {
        string kind = ResolveKind(contextKey);
        if (kind == "type")
        {
            return "/buttermorph/schema-types/designer";
        }

        if (kind == "field")
        {
            return "/buttermorph/metadata-fields/designer";
        }

        return "/buttermorph/payload-schema/designer";
    }

    // Resolves string fallback.
    private static string ResolveValue(string value, string fallback)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return fallback;
    }

    // Converts a JSON string array into lines.
    private static string ConvertJsonArrayToLines(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        try
        {
            using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(json);
            List<string> values = [];
            foreach (System.Text.Json.JsonElement element in document.RootElement.EnumerateArray())
            {
                values.Add(element.GetString());
            }

            return string.Join(Environment.NewLine, values);
        }
        catch (System.Text.Json.JsonException)
        {
            return string.Empty;
        }
    }

    // Converts validation allowed values into lines.
    private static string ConvertAllowedValuesToLines(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        try
        {
            using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(json);
            if (!document.RootElement.TryGetProperty("allowedValues", out System.Text.Json.JsonElement valuesElement))
            {
                return string.Empty;
            }

            List<string> values = [];
            foreach (System.Text.Json.JsonElement element in valuesElement.EnumerateArray())
            {
                values.Add(element.ToString());
            }

            return string.Join(Environment.NewLine, values);
        }
        catch (System.Text.Json.JsonException)
        {
            return string.Empty;
        }
    }

    // Serializes schema metadata for playground state.
    private static string SerializeMetadata(IReadOnlyDictionary<string, string> metadata)
    {
        if (metadata == null || metadata.Count == 0)
        {
            return "{}";
        }

        return System.Text.Json.JsonSerializer.Serialize(metadata);
    }

    // Parses schema metadata stored as JSON text.
    private static IReadOnlyDictionary<string, string> ParseMetadataJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        try
        {
            using System.Text.Json.JsonDocument document = System.Text.Json.JsonDocument.Parse(json);
            return ReadMetadataElement(document.RootElement);
        }
        catch (System.Text.Json.JsonException)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }
    }

    // Reads metadata element values as strings.
    private static IReadOnlyDictionary<string, string> ReadMetadataElement(System.Text.Json.JsonElement element)
    {
        Dictionary<string, string> metadata = new(StringComparer.Ordinal);
        if (element.ValueKind != System.Text.Json.JsonValueKind.Object)
        {
            return metadata;
        }

        foreach (System.Text.Json.JsonProperty property in element.EnumerateObject())
        {
            if (property.Value.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                metadata[property.Name] = property.Value.GetString();
            }
            else
            {
                metadata[property.Name] = property.Value.GetRawText();
            }
        }

        return metadata;
    }
}
