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
            DisplayName = request.Result.Name,
            Description = request.Result.Description,
            DesignerPath = "/buttermorph/schema-types/designer",
            JsonSchema = request.Result.JsonSchema,
            SavedAt = DateTimeOffset.UtcNow.ToString("O"),
            VersionNumber = request.Result.VersionNumber,
            BaseType = request.Result.BaseType,
            Comment = request.Result.Comment
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
            SortOrder = request.Result.SortOrder
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
                JsonSchema = save.JsonSchema,
                SchemaTypes = CreateTypeCatalog(),
                MetadataFields = CreateMetadataCatalog(),
                ShowManualActions = false
            });
        }

        return Task.FromResult(new ButterMorphPayloadSchemaDesignerLoadResult
        {
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
            DisplayName = ResolveDisplayName(request.ContextKey),
            Description = "Payload schema",
            DesignerPath = "/buttermorph/payload-schema/designer",
            JsonSchema = request.Result.JsonSchema,
            SavedAt = DateTimeOffset.UtcNow.ToString("O")
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
                ContextKey = "datatype-customer-code",
                DisplayName = "Edit custom datatype",
                Description = "Create a versioned CustomerCode datatype with string constraints.",
                DesignerPath = "/buttermorph/schema-types/designer"
            },
            new PlaygroundSchemaScenarioSummary
            {
                ContextKey = "metadata-classification",
                DisplayName = "Edit custom field",
                Description = "Create a reusable metadata field with validation and allowed values.",
                DesignerPath = "/buttermorph/metadata-fields/designer"
            },
            new PlaygroundSchemaScenarioSummary
            {
                ContextKey = "payload-customer-profile",
                DisplayName = "Edit payload schema",
                Description = "Build an Atlas-compatible payload structure from datatypes and metadata.",
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
                VersionNumber = save.VersionNumber,
                BaseType = save.BaseType,
                Comment = save.Comment,
                Key = save.Key,
                DataType = save.DataType,
                AppliesToJson = save.AppliesToJson,
                ValidationJson = save.ValidationJson,
                IsRequired = save.IsRequired,
                IsActive = save.IsActive,
                SortOrder = save.SortOrder
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
            view.SortOrder = input.SortOrder;
        }

        if (view.Kind == "type")
        {
            SchemaTypeDesignInput input = CreateTypeInput(contextKey);
            view.VersionNumber = input.VersionNumber;
            view.BaseType = input.BaseType;
            view.Comment = input.Comment;
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
        if (string.Equals(contextKey, "datatype-customer-code", StringComparison.OrdinalIgnoreCase))
        {
            return "{\"type\":\"string\",\"description\":\"Customer code used by integrations.\",\"minLength\":3,\"maxLength\":24,\"pattern\":\"^[A-Z0-9-]+$\"}";
        }

        if (string.Equals(contextKey, "metadata-classification", StringComparison.OrdinalIgnoreCase))
        {
            return "{\"dataType\":\"string\",\"allowedValues\":[\"Internal\",\"Partner\",\"Public\"]}";
        }

        return CreatePayloadJson(contextKey);
    }

    // Creates schema type input for demo contexts.
    private static SchemaTypeDesignInput CreateTypeInput(string contextKey)
    {
        if (!string.Equals(contextKey, "datatype-customer-code", StringComparison.OrdinalIgnoreCase))
        {
            return new SchemaTypeDesignInput();
        }

        return new SchemaTypeDesignInput
        {
            Name = "CustomerCode",
            Description = "Customer code used by integrations.",
            VersionNumber = "1.0.0",
            BaseType = "string",
            MinLength = "3",
            MaxLength = "24",
            Pattern = "^[A-Z0-9-]+$",
            Comment = "Initial version"
        };
    }

    // Creates schema type input from a saved item.
    private static SchemaTypeDesignInput CreateTypeInput(PlaygroundSchemaSave save)
    {
        SchemaTypeDesignInput input = new()
        {
            Name = save.DisplayName,
            Description = save.Description,
            VersionNumber = ResolveValue(save.VersionNumber, "1.0.0"),
            BaseType = ResolveValue(save.BaseType, "string"),
            Comment = save.Comment
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

            ReadSchemaString(root, "description", value => input.Description = ResolveValue(input.Description, value));
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
        if (!string.Equals(contextKey, "metadata-classification", StringComparison.OrdinalIgnoreCase))
        {
            return new FieldMetadataDesignInput
            {
                DataType = "string",
                IsActive = true
            };
        }

        return new FieldMetadataDesignInput
        {
            Name = "Classification",
            Key = "classification",
            Description = "Classifies the field for downstream consumers.",
            DataType = "string",
            AppliesTo = "Payload\nProperty",
            IsRequired = false,
            IsActive = true,
            SortOrder = 10,
            AllowedValues = "Internal\nPartner\nPublic"
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
            SortOrder = save.SortOrder,
            AllowedValues = ConvertAllowedValuesToLines(save.ValidationJson)
        };
    }

    // Creates payload schema JSON.
    private string CreatePayloadJson(string contextKey)
    {
        if (!string.Equals(contextKey, "payload-customer-profile", StringComparison.OrdinalIgnoreCase))
        {
            return "{\"type\":\"" + MapType + "\",\"properties\":{}}";
        }

        JsonSchemaConversionResult result = exporter.Export(new JsonSchemaExportRequest
        {
            Schema = PlaygroundDesignerHost.CreateCustomerSchema()
        });

        return result.JsonSchema;
    }

    // Creates available schema types.
    private static IReadOnlyCollection<SchemaTypeCatalogItem> CreateTypeCatalog()
    {
        return
        [
            CreateSystemType("string"),
            CreateSystemType("number"),
            CreateSystemType("integer"),
            CreateSystemType("boolean"),
            CreateSystemType(MapType),
            CreateSystemType("array"),
            new SchemaTypeCatalogItem
            {
                TypeId = "customer-code",
                TypeVersionId = "customer-code-v1",
                Name = "CustomerCode",
                VersionNumber = "1.0.0",
                BaseType = "string",
                JsonSchema = "{\"type\":\"string\",\"minLength\":3,\"maxLength\":24}",
                IsSystem = false
            }
        ];
    }

    // Creates available metadata fields.
    private static IReadOnlyCollection<FieldMetadataCatalogItem> CreateMetadataCatalog()
    {
        return
        [
            new FieldMetadataCatalogItem
            {
                Id = "classification",
                Name = "Classification",
                Key = "classification",
                Description = "Classifies the field.",
                DataType = "string",
                IsRequired = false,
                Validation = "{\"allowedValues\":[\"Internal\",\"Partner\",\"Public\"]}"
            },
            new FieldMetadataCatalogItem
            {
                Id = "sourceSystem",
                Name = "Source System",
                Key = "sourceSystem",
                Description = "Declares the source system.",
                DataType = "string",
                IsRequired = false,
                Validation = "{}"
            }
        ];
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
        if (string.Equals(contextKey, "datatype-customer-code", StringComparison.OrdinalIgnoreCase))
        {
            return "Create a versioned CustomerCode datatype with string constraints.";
        }

        if (string.Equals(contextKey, "metadata-classification", StringComparison.OrdinalIgnoreCase))
        {
            return "Create a reusable metadata field with validation and allowed values.";
        }

        return "Build an Atlas-compatible payload structure from datatypes and metadata.";
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
}
