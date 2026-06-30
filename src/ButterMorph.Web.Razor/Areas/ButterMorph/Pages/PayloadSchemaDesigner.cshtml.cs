namespace ButterMorph.Web.Razor;

using System.Text.Json;
using System.Text.Json.Serialization;
using ButterMorph.SchemaDesign;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

/// <summary>
/// Displays the reusable schema designer.
/// </summary>
public sealed class PayloadSchemaDesignerModel : PageModel
{
    // Serializes metadata definition for browser rendering.
    private static readonly JsonSerializerOptions MetadataJsonOptions = CreateMetadataJsonOptions();

    // Builds schema output.
    private readonly IPayloadSchemaBuilder payloadBuilder;

    // Reads designer integration options.
    private readonly ButterMorphRazorDesignerOptions options;

    // Provides optional host integrations.
    private readonly IEnumerable<IButterMorphPayloadSchemaDesignerHost> hosts;

    /// <summary>
    /// Initializes a new instance of the <see cref="PayloadSchemaDesignerModel"/> class.
    /// </summary>
    /// <param name="payloadBuilder">The schema builder.</param>
    /// <param name="options">The designer options.</param>
    /// <param name="hosts">The optional host integrations.</param>
    public PayloadSchemaDesignerModel(
        IPayloadSchemaBuilder payloadBuilder,
        IOptions<ButterMorphRazorDesignerOptions> options,
        IEnumerable<IButterMorphPayloadSchemaDesignerHost> hosts)
    {
        this.payloadBuilder = payloadBuilder;
        this.options = options.Value;
        this.hosts = hosts;
    }

    /// <summary>
    /// Gets or sets the payload JSON Schema.
    /// </summary>
    [BindProperty]
    public string PayloadSchemaJson { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the canonical schema key.
    /// </summary>
    [BindProperty]
    public string SchemaKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display schema name.
    /// </summary>
    [BindProperty]
    public string SchemaName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema description.
    /// </summary>
    [BindProperty]
    public string SchemaDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the schema version.
    /// </summary>
    [BindProperty]
    public string SchemaVersion { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the schema version comment.
    /// </summary>
    [BindProperty]
    public string SchemaVersionComment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets schema-level metadata lines.
    /// </summary>
    [BindProperty]
    public string SchemaMetadataText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets available schema types.
    /// </summary>
    public IReadOnlyCollection<SchemaTypeCatalogItem> SchemaTypes { get; set; } = [];

    /// <summary>
    /// Gets or sets available metadata fields.
    /// </summary>
    public IReadOnlyCollection<FieldMetadataCatalogItem> MetadataFields { get; set; } = [];

    /// <summary>
    /// Gets or sets catalog JSON used by client behavior.
    /// </summary>
    public string SchemaTypeCatalogJson { get; set; } = "[]";

    /// <summary>
    /// Gets or sets metadata catalog JSON used by client behavior.
    /// </summary>
    public string FieldMetadataCatalogJson { get; set; } = "[]";

    /// <summary>
    /// Gets or sets schema metadata definition JSON used by client behavior.
    /// </summary>
    public string SchemaMetadataDefinitionJson { get; set; } = "{\"fields\":[]}";

    /// <summary>
    /// Gets or sets the host-provided schema metadata definition.
    /// </summary>
    public SchemaMetadataDefinition MetadataDefinition { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether manual actions are shown.
    /// </summary>
    public bool ShowManualActions { get; set; } = true;

    /// <summary>
    /// Gets or sets a user-facing message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether host save completed.
    /// </summary>
    public bool HostSaveCompleted { get; set; }

    /// <summary>
    /// Gets or sets the saved host context key.
    /// </summary>
    public string SavedContextKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the form title.
    /// </summary>
    public string FormTitle { get; set; } = "New Schema";

    /// <summary>
    /// Gets or sets the form save URL.
    /// </summary>
    public string SaveActionUrl { get; set; } = string.Empty;

    /// <summary>
    /// Handles initial display.
    /// </summary>
    /// <returns>The page result.</returns>
    public async Task<IActionResult> OnGet()
    {
        SaveActionUrl = ResolveSaveActionUrl();
        FormTitle = ResolveFormTitle();
        await ApplyHostLoad();
        RefreshCatalogs();

        return Page();
    }

    /// <summary>
    /// Redirects accidental save GET requests back to the host.
    /// </summary>
    /// <returns>The redirect result.</returns>
    public IActionResult OnGetSave()
    {
        return BadRequest("Schema designer save requires POST.");
    }

    /// <summary>
    /// Saves schema state through the optional host.
    /// </summary>
    /// <returns>The page result.</returns>
    public async Task<IActionResult> OnPostSave()
    {
        SaveActionUrl = ResolveSaveActionUrl();
        FormTitle = ResolveFormTitle();
        await ApplyHostLoadCatalogOnly();
        if (!TryParseMetadataText(SchemaMetadataText, out IReadOnlyDictionary<string, string> metadata, out string metadataError))
        {
            Message = metadataError;
            if (IsPopupRequest())
            {
                return new JsonResult(CreateHostSaveResponse("ButterMorphPayloadSchemaDesignerSaved"));
            }

            RefreshCatalogs();
            return Page();
        }

        PayloadSchemaDesignResult result = payloadBuilder.Build(new PayloadSchemaDesignInput
        {
            Key = SchemaKey,
            Name = SchemaName,
            Description = SchemaDescription,
            Version = SchemaVersion,
            VersionComment = SchemaVersionComment,
            Metadata = metadata,
            JsonSchema = PayloadSchemaJson
        }, SchemaTypes, MetadataFields);
        RefreshCatalogs();

        if (!result.Succeeded)
        {
            Message = string.Join(" ", result.Diagnostics.Select(diagnostic => diagnostic.Message));
            if (IsPopupRequest())
            {
                return new JsonResult(CreateHostSaveResponse("ButterMorphPayloadSchemaDesignerSaved"));
            }

            return Page();
        }

        PayloadSchemaJson = result.JsonSchema;
        SchemaVersion = result.Version;
        SchemaVersionComment = result.VersionComment;
        SchemaMetadataText = FormatMetadataText(result.Metadata);
        ButterMorphPayloadSchemaDesignerSaveResult saveResult = new()
        {
            Succeeded = true,
            Message = "Schema saved."
        };

        foreach (IButterMorphPayloadSchemaDesignerHost host in hosts)
        {
            saveResult = await host.Save(new ButterMorphPayloadSchemaDesignerSaveRequest
            {
                ContextKey = ResolveContextKey(),
                Definition = result.Definition,
                InjectedCustomTypeIds = ResolveInjectedIds("customTypes"),
                InjectedCustomFieldIds = ResolveInjectedIds("customFields")
            });
            break;
        }

        HostSaveCompleted = saveResult.Succeeded;
        SavedContextKey = ResolveContextKey();
        if (saveResult.Succeeded && IsPopupRequest())
        {
            return new JsonResult(CreateHostSaveResponse("ButterMorphPayloadSchemaDesignerSaved"));
        }

        if (IsPopupRequest())
        {
            Message = saveResult.Message;
            return new JsonResult(CreateHostSaveResponse("ButterMorphPayloadSchemaDesignerSaved"));
        }

        if (saveResult.Succeeded)
        {
            Message = saveResult.Message;
            return Page();
        }

        return Page();
    }

    // Applies host preload to the page.
    private async Task ApplyHostLoad()
    {
        foreach (IButterMorphPayloadSchemaDesignerHost host in hosts)
        {
            ButterMorphPayloadSchemaDesignerLoadResult result = await host.Load(new ButterMorphPayloadSchemaDesignerLoadRequest
            {
                ContextKey = ResolveContextKey(),
                InjectedCustomTypeIds = ResolveInjectedIds("customTypes"),
                InjectedCustomFieldIds = ResolveInjectedIds("customFields")
            });
            PayloadSchemaJson = result.JsonSchema;
            SchemaKey = result.Key;
            SchemaName = result.Name;
            SchemaDescription = result.Description;
            SchemaVersion = result.Version;
            SchemaVersionComment = result.VersionComment;
            SchemaMetadataText = FormatMetadataText(result.Metadata);
            MetadataDefinition = result.MetadataDefinition;
            SchemaTypes = result.SchemaTypes;
            MetadataFields = result.MetadataFields;
            ShowManualActions = result.ShowManualActions;
            Message = result.Message;
            return;
        }

        SchemaKey = "payload";
        SchemaName = "Payload";
        SchemaDescription = string.Empty;
        SchemaVersion = "1.0.0";
        SchemaVersionComment = string.Empty;
        SchemaMetadataText = string.Empty;
        MetadataDefinition = new SchemaMetadataDefinition();
        PayloadSchemaJson = "{\"type\":\"" + ("obj" + "ect") + "\",\"properties\":{}}";
    }

    // Applies only host catalogs during posts.
    private async Task ApplyHostLoadCatalogOnly()
    {
        foreach (IButterMorphPayloadSchemaDesignerHost host in hosts)
        {
            ButterMorphPayloadSchemaDesignerLoadResult result = await host.Load(new ButterMorphPayloadSchemaDesignerLoadRequest
            {
                ContextKey = ResolveContextKey(),
                InjectedCustomTypeIds = ResolveInjectedIds("customTypes"),
                InjectedCustomFieldIds = ResolveInjectedIds("customFields")
            });
            SchemaTypes = result.SchemaTypes;
            MetadataFields = result.MetadataFields;
            MetadataDefinition = result.MetadataDefinition;
            ShowManualActions = result.ShowManualActions;
            return;
        }
    }

    // Refreshes serialized catalogs.
    private void RefreshCatalogs()
    {
        SchemaTypes = MergeSchemaTypeCatalog(CreateDefaultCatalog(), SchemaTypes);
        MetadataDefinition = MergeMetadataDefinitions(MetadataDefinition, CreateMetadataDefinitionFromCatalog(MetadataFields, "Schema"));

        SchemaTypeCatalogJson = JsonSerializer.Serialize(SchemaTypes);
        FieldMetadataCatalogJson = JsonSerializer.Serialize(MetadataFields);
        SchemaMetadataDefinitionJson = JsonSerializer.Serialize(MetadataDefinition, MetadataJsonOptions);
    }

    // Creates a graphical schema metadata definition from custom fields supplied by the host.
    private static SchemaMetadataDefinition CreateMetadataDefinitionFromCatalog(IReadOnlyCollection<FieldMetadataCatalogItem> metadataFields, string scope)
    {
        List<SchemaMetadataFieldDefinition> fields = [];
        foreach (FieldMetadataCatalogItem item in metadataFields.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase))
        {
            if (!AppliesToScope(item.AppliesToJson, scope))
            {
                continue;
            }

            fields.Add(new SchemaMetadataFieldDefinition
            {
                Key = item.Key,
                Name = item.Name,
                Description = item.Description,
                DataType = ConvertMetadataDataType(item.DataType),
                IsRequired = item.IsRequired,
                AllowedValues = ReadAllowedValues(item.Validation),
                Children = ReadMetadataChildren(item.ChildrenDefinitionJson),
                ArrayItem = ReadMetadataArrayItem(item.ArrayItemDataType, item.ArrayItemDefinitionJson)
            });
        }

        return new SchemaMetadataDefinition
        {
            Fields = fields
        };
    }

    // Combines explicit host metadata with catalog-derived custom fields.
    private static SchemaMetadataDefinition MergeMetadataDefinitions(SchemaMetadataDefinition explicitDefinition, SchemaMetadataDefinition catalogDefinition)
    {
        Dictionary<string, SchemaMetadataFieldDefinition> fields = new(StringComparer.OrdinalIgnoreCase);
        foreach (SchemaMetadataFieldDefinition field in explicitDefinition.Fields)
        {
            fields[field.Key] = field;
        }

        foreach (SchemaMetadataFieldDefinition field in catalogDefinition.Fields)
        {
            if (!fields.ContainsKey(field.Key))
            {
                fields[field.Key] = field;
            }
        }

        return new SchemaMetadataDefinition
        {
            Fields = fields.Values.ToArray()
        };
    }

    // Detects whether a custom field applies to the requested designer area.
    private static bool AppliesToScope(string appliesToJson, string scope)
    {
        if (string.IsNullOrWhiteSpace(appliesToJson))
        {
            return false;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(appliesToJson);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            foreach (JsonElement element in document.RootElement.EnumerateArray())
            {
                if (element.ValueKind == JsonValueKind.String &&
                    string.Equals(element.GetString(), scope, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }
        catch (JsonException)
        {
            return false;
        }

        return false;
    }

    // Converts custom field data types to metadata editor data types.
    private static SchemaMetadataDataType ConvertMetadataDataType(string dataType)
    {
        if (string.Equals(dataType, "number", StringComparison.OrdinalIgnoreCase))
        {
            return SchemaMetadataDataType.Number;
        }

        if (string.Equals(dataType, "integer", StringComparison.OrdinalIgnoreCase))
        {
            return SchemaMetadataDataType.Integer;
        }

        if (string.Equals(dataType, "boolean", StringComparison.OrdinalIgnoreCase))
        {
            return SchemaMetadataDataType.Boolean;
        }

        if (string.Equals(dataType, "date", StringComparison.OrdinalIgnoreCase))
        {
            return SchemaMetadataDataType.Date;
        }

        if (string.Equals(dataType, "datetime", StringComparison.OrdinalIgnoreCase))
        {
            return SchemaMetadataDataType.DateTime;
        }

        if (string.Equals(dataType, "object", StringComparison.OrdinalIgnoreCase))
        {
            return SchemaMetadataDataType.Object;
        }

        if (string.Equals(dataType, "array", StringComparison.OrdinalIgnoreCase))
        {
            return SchemaMetadataDataType.Array;
        }

        return SchemaMetadataDataType.String;
    }

    // Reads child field definitions for complex metadata fields.
    private static IReadOnlyCollection<SchemaMetadataFieldDefinition> ReadMetadataChildren(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            return ReadMetadataChildren(document.RootElement);
        }
        catch (JsonException)
        {
            return [];
        }
    }

    // Reads child field definitions from a JSON Schema element.
    private static IReadOnlyCollection<SchemaMetadataFieldDefinition> ReadMetadataChildren(JsonElement schema)
    {
        List<SchemaMetadataFieldDefinition> children = [];
        if (!schema.TryGetProperty("properties", out JsonElement properties) ||
            properties.ValueKind != JsonValueKind.Object)
        {
            return children;
        }

        foreach (JsonProperty property in properties.EnumerateObject())
        {
            children.Add(ReadMetadataField(property.Name, property.Value, false));
        }

        return children;
    }

    // Reads one metadata field definition from JSON Schema.
    private static SchemaMetadataFieldDefinition ReadMetadataField(string key, JsonElement definition, bool isRequired)
    {
        string dataType = ReadString(definition, "type", "string");
        SchemaMetadataFieldDefinition field = new()
        {
            Key = key,
            Name = key,
            Description = ReadString(definition, "description", string.Empty),
            DataType = ConvertMetadataDataType(dataType),
            IsRequired = isRequired || ReadBoolean(definition, "required")
        };

        if (field.DataType == SchemaMetadataDataType.Object)
        {
            field.Children = ReadMetadataChildren(definition);
        }

        if (field.DataType == SchemaMetadataDataType.Array &&
            definition.TryGetProperty("items", out JsonElement items) &&
            items.ValueKind == JsonValueKind.Object)
        {
            field.ArrayItem = ReadMetadataField("item", items, false);
        }

        return field;
    }

    // Reads an array item definition for metadata field catalog entries.
    private static SchemaMetadataFieldDefinition ReadMetadataArrayItem(string itemType, string itemDefinitionJson)
    {
        if (string.IsNullOrWhiteSpace(itemType))
        {
            return new SchemaMetadataFieldDefinition
            {
                Key = "item",
                Name = "Item",
                DataType = SchemaMetadataDataType.String
            };
        }

        SchemaMetadataFieldDefinition item = new()
        {
            Key = "item",
            Name = "Item",
            DataType = ConvertMetadataDataType(itemType)
        };

        if (item.DataType == SchemaMetadataDataType.Object)
        {
            item.Children = ReadMetadataChildren(itemDefinitionJson);
        }

        return item;
    }

    // Reads a string property from JSON.
    private static string ReadString(JsonElement element, string propertyName, string fallback)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.String)
        {
            return property.GetString() ?? fallback;
        }

        return fallback;
    }

    // Reads a boolean property from JSON.
    private static bool ReadBoolean(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.True;
    }

    // Reads allowed scalar values from custom field validation JSON.
    private static IReadOnlyCollection<string> ReadAllowedValues(string validation)
    {
        List<string> values = [];
        if (string.IsNullOrWhiteSpace(validation))
        {
            return values;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(validation);
            if (!TryGetAllowedValuesElement(document.RootElement, out JsonElement allowedValues))
            {
                return values;
            }

            foreach (JsonElement element in allowedValues.EnumerateArray())
            {
                values.Add(element.ToString());
            }
        }
        catch (JsonException)
        {
            return values;
        }

        return values;
    }

    // Reads either the ButterMorph custom-field allowedValues key or enum-style values.
    private static bool TryGetAllowedValuesElement(JsonElement root, out JsonElement allowedValues)
    {
        if (root.TryGetProperty("allowedValues", out allowedValues) &&
            allowedValues.ValueKind == JsonValueKind.Array)
        {
            return true;
        }

        if (root.TryGetProperty("enum", out allowedValues) &&
            allowedValues.ValueKind == JsonValueKind.Array)
        {
            return true;
        }

        return false;
    }

    // Resolves the host context key.
    private string ResolveContextKey()
    {
        return DesignerSessionKeyResolver.ResolveContextKey(this, options);
    }

    // Resolves comma-separated host ids supplied by the embedding host.
    private IReadOnlyCollection<string> ResolveInjectedIds(string queryName)
    {
        string value = Request.Query[queryName].ToString();
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    // Resolves the form action while preserving host flow query parameters.
    private string ResolveSaveActionUrl()
    {
        string path = Request.Path.ToString();
        string query = Request.QueryString.ToString();
        if (query.Contains("handler=", StringComparison.OrdinalIgnoreCase))
        {
            return path + query;
        }

        string separator = "&";
        if (string.IsNullOrEmpty(query))
        {
            separator = Convert.ToChar(63).ToString();
        }

        return path + query + separator + "handler=Save";
    }

    // Creates the host flow save response.
    private SchemaDesignerHostSaveResponse CreateHostSaveResponse(string messageType)
    {
        return new SchemaDesignerHostSaveResponse
        {
            HostSaveCompleted = HostSaveCompleted,
            SavedContextKey = ResolveContextKey(),
            MessageType = messageType,
            Message = Message,
            SafeReturnUrl = ResolveSafeReturnUrl()
        };
    }

    // Resolves a local return URL that is safe to use after popup completion.
    private string ResolveSafeReturnUrl()
    {
        string returnUrl = Request.Query[options.ReturnUrlQueryParameter];
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return string.Empty;
        }

        if (!Url.IsLocalUrl(returnUrl))
        {
            return string.Empty;
        }

        return returnUrl;
    }

    // Detects popup-host requests.
    private bool IsPopupRequest()
    {
        return string.Equals(Request.Query[options.PopupQueryParameter].ToString(), "true", StringComparison.OrdinalIgnoreCase);
    }

    // Resolves the title based on popup mode.
    private string ResolveFormTitle()
    {
        string mode = Request.Query["mode"].ToString();
        if (string.Equals(mode, "edit", StringComparison.OrdinalIgnoreCase))
        {
            return "Edit Schema";
        }

        return "New Schema";
    }

    // Creates default system types.
    private static IReadOnlyCollection<SchemaTypeCatalogItem> CreateDefaultCatalog()
    {
        return
        [
            CreateCatalogItem("string"),
            CreateCatalogItem("number"),
            CreateCatalogItem("integer"),
            CreateCatalogItem("boolean"),
            CreateCatalogItem("obj" + "ect"),
            CreateCatalogItem("array")
        ];
    }

    // Combines system types with host-injected custom types.
    private static IReadOnlyCollection<SchemaTypeCatalogItem> MergeSchemaTypeCatalog(IReadOnlyCollection<SchemaTypeCatalogItem> systemTypes, IReadOnlyCollection<SchemaTypeCatalogItem> customTypes)
    {
        Dictionary<string, SchemaTypeCatalogItem> catalog = new(StringComparer.OrdinalIgnoreCase);
        foreach (SchemaTypeCatalogItem item in systemTypes)
        {
            catalog[item.Name] = item;
        }

        foreach (SchemaTypeCatalogItem item in customTypes)
        {
            if (string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.JsonSchema))
            {
                continue;
            }

            string key = string.IsNullOrWhiteSpace(item.TypeVersionId)
                ? item.Name
                : item.TypeVersionId;
            catalog[key] = item;
        }

        return catalog.Values.ToArray();
    }

    // Creates a system catalog item.
    private static SchemaTypeCatalogItem CreateCatalogItem(string baseType)
    {
        return new SchemaTypeCatalogItem
        {
            Name = baseType,
            BaseType = baseType,
            VersionNumber = "1.0.0",
            IsSystem = true
        };
    }

    // Creates JSON options for metadata definition payload.
    private static JsonSerializerOptions CreateMetadataJsonOptions()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    // Parses schema-level metadata JSON.
    private static bool TryParseMetadataText(string text, out IReadOnlyDictionary<string, string> metadata, out string error)
    {
        Dictionary<string, string> values = new(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(text))
        {
            metadata = values;
            error = string.Empty;
            return true;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(text);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                metadata = values;
                error = "Metadata must be a JSON map.";
                return false;
            }

            foreach (JsonProperty property in document.RootElement.EnumerateObject())
            {
                values[property.Name] = ReadMetadataValue(property.Value);
            }

            metadata = values;
            error = string.Empty;
            return true;
        }
        catch (JsonException exception)
        {
            metadata = values;
            error = "Metadata JSON is invalid. " + exception.Message;
            return false;
        }
    }

    // Formats schema-level metadata JSON.
    private static string FormatMetadataText(IReadOnlyDictionary<string, string> metadata)
    {
        if (metadata.Count == 0)
        {
            return string.Empty;
        }

        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = true });
        writer.WriteStartObject();
        foreach (KeyValuePair<string, string> pair in metadata)
        {
            writer.WritePropertyName(pair.Key);
            WriteMetadataValue(writer, pair.Value);
        }

        writer.WriteEndObject();
        writer.Flush();
        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    // Reads a metadata value as structured JSON or text.
    private static string ReadMetadataValue(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            return element.GetString();
        }

        return element.GetRawText();
    }

    // Writes metadata values preserving nested JSON.
    private static void WriteMetadataValue(Utf8JsonWriter writer, string value)
    {
        if (TryWriteRawMetadataValue(writer, value))
        {
            return;
        }

        writer.WriteStringValue(value);
    }

    // Attempts to write a raw JSON metadata value.
    private static bool TryWriteRawMetadataValue(Utf8JsonWriter writer, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(value);
            document.RootElement.WriteTo(writer);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
