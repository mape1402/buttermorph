namespace ButterMorph.Web.Razor;

using ButterMorph.Abstractions;
using ButterMorph.Design;
using ButterMorph.Json.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.Globalization;

/// <summary>
/// Displays and edits mapping definitions.
/// </summary>
public sealed class DesignerModel : PageModel
{
    // Stores design sessions for the web designer.
    private readonly IMappingDesignSessionStore _sessionStore;

    // Explores schemas for UI rendering.
    private readonly ISchemaExplorer _schemaExplorer;

    // Exports expressions through DSL text for display.
    private readonly IDslExporter _dslExporter;

    // Imports JSON Schema content from toolbox forms.
    private readonly IJsonSchemaImporter _schemaImporter;

    // Lists design-time capabilities such as registered functions.
    private readonly ICapabilityExplorer _capabilityExplorer;

    // Reads integration options for the reusable designer.
    private readonly ButterMorphRazorDesignerOptions _options;

    // Provides optional host application integration.
    private readonly IEnumerable<IButterMorphDesignerHost> _designerHosts;

    // Stores web-only state for designer contexts.
    private static readonly DesignerContextStateStore ContextStates = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DesignerModel"/> class.
    /// </summary>
    /// <param name="sessionStore">The session store.</param>
    /// <param name="schemaExplorer">The schema explorer.</param>
    /// <param name="dslExporter">The DSL exporter.</param>
    /// <param name="schemaImporter">The JSON Schema importer.</param>
    /// <param name="capabilityExplorer">The capability explorer.</param>
    /// <param name="options">The Razor designer options.</param>
    /// <param name="designerHosts">The optional designer host integrations.</param>
    public DesignerModel(
        IMappingDesignSessionStore sessionStore,
        ISchemaExplorer schemaExplorer,
        IDslExporter dslExporter,
        IJsonSchemaImporter schemaImporter,
        ICapabilityExplorer capabilityExplorer,
        IOptions<ButterMorphRazorDesignerOptions> options,
        IEnumerable<IButterMorphDesignerHost> designerHosts)
    {
        _sessionStore = sessionStore;
        _schemaExplorer = schemaExplorer;
        _dslExporter = dslExporter;
        _schemaImporter = schemaImporter;
        _capabilityExplorer = capabilityExplorer;
        _options = options.Value;
        _designerHosts = designerHosts;
    }

    /// <summary>
    /// Gets or sets the source path.
    /// </summary>
    [BindProperty]
    public string SourcePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target path.
    /// </summary>
    [BindProperty]
    public string TargetPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets posted target paths.
    /// </summary>
    [BindProperty]
    public List<string> TargetPaths { get; set; } = [];

    /// <summary>
    /// Gets or sets posted expression values.
    /// </summary>
    [BindProperty]
    public List<string> Expressions { get; set; } = [];

    /// <summary>
    /// Gets or sets posted projection target paths.
    /// </summary>
    [BindProperty]
    public List<string> ProjectionTargetPaths { get; set; } = [];

    /// <summary>
    /// Gets or sets posted projection source expressions.
    /// </summary>
    [BindProperty]
    public List<string> ProjectionSources { get; set; } = [];

    /// <summary>
    /// Gets or sets posted projection aliases.
    /// </summary>
    [BindProperty]
    public List<string> ProjectionAliases { get; set; } = [];

    /// <summary>
    /// Gets or sets posted advanced projection expressions.
    /// </summary>
    [BindProperty]
    public List<string> ProjectionAdvancedExpressions { get; set; } = [];

    /// <summary>
    /// Gets or sets posted projection field array paths.
    /// </summary>
    [BindProperty]
    public List<string> ProjectionFieldArrayPaths { get; set; } = [];

    /// <summary>
    /// Gets or sets posted projection field paths.
    /// </summary>
    [BindProperty]
    public List<string> ProjectionFieldPaths { get; set; } = [];

    /// <summary>
    /// Gets or sets posted projection field expressions.
    /// </summary>
    [BindProperty]
    public List<string> ProjectionFieldExpressions { get; set; } = [];

    /// <summary>
    /// Gets or sets the source display name.
    /// </summary>
    [BindProperty]
    public string SourceName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pasted source schema text.
    /// </summary>
    [BindProperty]
    public string SourceSchemaText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pasted output schema text.
    /// </summary>
    [BindProperty]
    public string OutputSchemaText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the DSL editor content.
    /// </summary>
    [BindProperty]
    public string DslContent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the active designer view.
    /// </summary>
    [BindProperty]
    public string ActiveView { get; set; } = "Visual";

    /// <summary>
    /// Gets the source schema nodes.
    /// </summary>
    public IReadOnlyCollection<SchemaTreeDisplayNode> SourceNodes { get; private set; } = [];

    /// <summary>
    /// Gets source schema toolbox groups.
    /// </summary>
    public IReadOnlyCollection<SourceSchemaDisplayModel> SourceSchemas { get; private set; } = [];

    /// <summary>
    /// Gets function toolbox categories.
    /// </summary>
    public IReadOnlyCollection<FunctionToolboxCategoryDisplayModel> FunctionCategories { get; private set; } = [];

    /// <summary>
    /// Gets the target schema nodes.
    /// </summary>
    public IReadOnlyCollection<SchemaTreeDisplayNode> TargetNodes { get; private set; } = [];

    /// <summary>
    /// Gets the current mappings.
    /// </summary>
    public IReadOnlyCollection<MappingDisplayModel> Mappings { get; private set; } = [];

    /// <summary>
    /// Gets target schema fields with editable mapping expressions.
    /// </summary>
    public IReadOnlyCollection<TargetFieldMappingDisplayModel> TargetFields { get; private set; } = [];

    /// <summary>
    /// Gets the target schema display tree.
    /// </summary>
    public SchemaTreeDisplayNode TargetTree { get; private set; } = new();

    /// <summary>
    /// Gets semantic diagnostics.
    /// </summary>
    public IReadOnlyCollection<DiagnosticEntry> Diagnostics { get; private set; } = [];

    /// <summary>
    /// Gets or sets the status message.
    /// </summary>
    public string Message { get; set; } = "Ready.";

    /// <summary>
    /// Gets a value indicating whether schema action buttons should be shown.
    /// </summary>
    public bool ShowSchemaActions { get; private set; } = true;

    /// <summary>
    /// Gets a value indicating whether the host save flow completed.
    /// </summary>
    public bool HostSaveCompleted { get; private set; }

    /// <summary>
    /// Gets the context key that was saved by the host flow.
    /// </summary>
    public string SavedContextKey { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the local return URL used after popup save.
    /// </summary>
    public string SafeReturnUrl { get; private set; } = string.Empty;

    /// <summary>
    /// Displays the designer.
    /// </summary>
    /// <returns>The asynchronous page task.</returns>
    public async Task OnGet()
    {
        await PreloadHostState();
        LoadViewState();
    }

    /// <summary>
    /// Adds a path mapping.
    /// </summary>
    /// <returns>The page result.</returns>
    public IActionResult OnPostAddMapping()
    {
        IMappingOperationResult result = Session.AddPathMapping(SourcePath, TargetPath);
        Message = CreateMessage(result, "Mapping added.");
        LoadViewState();

        return Page();
    }

    /// <summary>
    /// Loads a source schema from the toolbox.
    /// </summary>
    /// <returns>The page result.</returns>
    public async Task<IActionResult> OnPostLoadSourceSchema()
    {
        string schemaText = await ReadSchemaContent("SourceSchemaFile", SourceSchemaText);

        if (string.IsNullOrWhiteSpace(SourceName))
        {
            Message = "Source name is required.";
            LoadViewState();
            return Page();
        }

        if (string.IsNullOrWhiteSpace(schemaText))
        {
            Message = "Source schema file or text is required.";
            LoadViewState();
            return Page();
        }

        JsonSchemaConversionResult result = _schemaImporter.Import(new JsonSchemaImportRequest
        {
            Name = SourceName,
            JsonSchema = schemaText
        });

        if (result.Succeeded)
        {
            Session.LoadSourceSchema(SourceName, result.Schema);
            Message = "Source '" + SourceName + "' loaded.";
            SourceName = string.Empty;
            SourceSchemaText = string.Empty;
            RunSemanticDiagnostics();
        }
        else
        {
            Message = "Source schema could not be loaded.";
            Diagnostics = result.Diagnostics;
        }

        LoadViewState();

        return Page();
    }

    /// <summary>
    /// Loads the target schema from the toolbox.
    /// </summary>
    /// <returns>The page result.</returns>
    public async Task<IActionResult> OnPostLoadTargetSchema()
    {
        string schemaText = await ReadSchemaContent("OutputSchemaFile", OutputSchemaText);

        if (string.IsNullOrWhiteSpace(schemaText))
        {
            Message = "Output schema file or text is required.";
            LoadViewState();
            return Page();
        }

        JsonSchemaConversionResult result = _schemaImporter.Import(new JsonSchemaImportRequest
        {
            Name = "Target",
            JsonSchema = schemaText
        });

        if (result.Succeeded)
        {
            Session.LoadTargetSchema(result.Schema);
            Message = "Target schema loaded.";
            OutputSchemaText = string.Empty;
            RunSemanticDiagnostics();
        }
        else
        {
            Message = "Target schema could not be loaded.";
            Diagnostics = result.Diagnostics;
        }

        LoadViewState();

        return Page();
    }

    /// <summary>
    /// Synchronizes visual mapping changes into the current DSL content.
    /// </summary>
    /// <returns>The synchronization response.</returns>
    public IActionResult OnPostSyncVisual()
    {
        IReadOnlyCollection<DiagnosticEntry> diagnostics = SavePostedMappings();

        if (diagnostics.Count == 0)
        {
            Message = string.Empty;
            RunSemanticDiagnostics();
            return new JsonResult(CreateSyncResponse(true, Message));
        }

        Diagnostics = diagnostics;
        Message = "Some mappings could not be synchronized.";
        return new JsonResult(CreateSyncResponse(false, Message));
    }

    /// <summary>
    /// Synchronizes DSL content into visual mapping fields.
    /// </summary>
    /// <returns>The synchronization response.</returns>
    public IActionResult OnPostSyncDsl()
    {
        IMappingOperationResult result = Session.ImportDsl(DslContent);

        if (result.Succeeded)
        {
            Message = string.Empty;
            RunSemanticDiagnostics();
            return new JsonResult(CreateSyncResponse(true, Message));
        }

        Diagnostics = result.Diagnostics;
        Message = CreateMessage(result, string.Empty);
        return new JsonResult(CreateSyncResponse(false, Message));
    }

    /// <summary>
    /// Removes a mapping.
    /// </summary>
    /// <returns>The page result.</returns>
    public IActionResult OnPostRemoveMapping()
    {
        Session.RemoveMapping(TargetPath);
        Message = "Mapping removed.";
        LoadViewState();

        return Page();
    }

    /// <summary>
    /// Saves all target field mappings.
    /// </summary>
    /// <returns>The page result.</returns>
    public async Task<IActionResult> OnPostSaveTargetMappings()
    {
        IReadOnlyCollection<DiagnosticEntry> diagnostics = SavePostedMappings();

        if (diagnostics.Count == 0)
        {
            RunSemanticDiagnostics();

            if (Diagnostics.Count == 0)
            {
                bool hostSaved = await SaveHostState();

                if (hostSaved)
                {
                    ResolveHostCompletionState();
                }
            }
            else
            {
                Message = "Mappings saved with diagnostics.";
            }
        }
        else
        {
            Message = "Some target mappings could not be saved.";
            Diagnostics = diagnostics;
        }

        LoadViewState();

        return new JsonResult(CreateSyncResponse(Diagnostics.Count == 0, Message));
    }

    /// <summary>
    /// Imports posted DSL content.
    /// </summary>
    /// <returns>The page result.</returns>
    public IActionResult OnPostImportDsl()
    {
        IMappingOperationResult result = Session.ImportDsl(DslContent);
        Message = CreateMessage(result, "DSL imported.");
        ActiveView = "Dsl";

        if (result.Succeeded)
        {
            RunSemanticDiagnostics();
        }
        else
        {
            Diagnostics = result.Diagnostics;
        }

        LoadViewState();

        return Page();
    }

    /// <summary>
    /// Exports current DSL content.
    /// </summary>
    /// <returns>The page result.</returns>
    public IActionResult OnPostExportDsl()
    {
        DslContent = Session.ExportDsl();
        ActiveView = "Dsl";
        Message = "DSL exported.";
        RunSemanticDiagnostics();
        LoadViewState();

        return Page();
    }

    /// <summary>
    /// Runs semantic analysis.
    /// </summary>
    /// <returns>The page result.</returns>
    public IActionResult OnPostAnalyze()
    {
        SemanticAnalysisResult result = Session.Analyze();
        Diagnostics = result.Diagnostics;
        Message = "Diagnostics refreshed.";
        LoadViewState();

        return Page();
    }

    // Gets the current design session.
    private string SessionKey => DesignerSessionKeyResolver.Resolve(this, _options);

    // Gets the host context key for the current request.
    private string ContextKey => DesignerSessionKeyResolver.ResolveContextKey(this, _options);

    // Gets a value indicating whether the designer is running as a popup.
    private bool IsPopupRequest => string.Equals(
        Request.Query[_options.PopupQueryParameter],
        "true",
        StringComparison.OrdinalIgnoreCase);

    // Gets web-only state for the current designer context.
    private DesignerContextState ContextState => ContextStates.GetOrCreate(SessionKey, _options);

    // Gets the current design session.
    private IMappingDesignSession Session => _sessionStore.GetOrCreate(SessionKey);

    // Applies host-provided state to the current session when available.
    private async Task PreloadHostState()
    {
        DesignerContextState state = ContextState;
        ShowSchemaActions = state.ShowSchemaActions;

        if (!_options.UseHostPreload || state.HostPreloadApplied)
        {
            return;
        }

        IButterMorphDesignerHost host = FindHost();

        if (host == null)
        {
            state.HostPreloadApplied = true;
            return;
        }

        ButterMorphDesignerLoadResult result = await host.Load(new ButterMorphDesignerLoadRequest
        {
            ContextKey = ContextKey
        });

        state.ShowSchemaActions = result.ShowSchemaActions;
        ShowSchemaActions = result.ShowSchemaActions;
        ApplyHostLoadResult(result);

        if (!string.IsNullOrWhiteSpace(result.Message))
        {
            Message = result.Message;
        }

        state.HostPreloadApplied = true;
    }

    // Saves the current session through the optional host integration.
    private async Task<bool> SaveHostState()
    {
        IButterMorphDesignerHost host = FindHost();

        if (host == null)
        {
            Message = "Mappings saved.";
            return true;
        }

        ButterMorphDesignerSaveResult result = await host.Save(new ButterMorphDesignerSaveRequest
        {
            ContextKey = ContextKey,
            Document = Session.Document,
            DslContent = Session.ExportDsl()
        });

        if (result.Succeeded)
        {
            Message = ResolveMessage(result.Message, "Mappings saved.");
            return true;
        }

        Diagnostics = result.Diagnostics;
        Message = ResolveMessage(result.Message, "Mappings could not be saved.");
        return false;
    }

    // Resolves popup completion state after a successful host save.
    private void ResolveHostCompletionState()
    {
        if (!IsPopupRequest || Diagnostics.Count > 0)
        {
            return;
        }

        HostSaveCompleted = true;
        SavedContextKey = ContextKey;
        SafeReturnUrl = ResolveSafeReturnUrl();
    }

    // Resolves a local return URL that is safe to use after popup completion.
    private string ResolveSafeReturnUrl()
    {
        string returnUrl = Request.Query[_options.ReturnUrlQueryParameter];

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

    // Resolves fallback message text when a host does not provide one.
    private static string ResolveMessage(string message, string fallback)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return fallback;
        }

        return message;
    }

    // Applies schemas and document content returned by the host.
    private void ApplyHostLoadResult(ButterMorphDesignerLoadResult result)
    {
        if (result.InitialDocument != null)
        {
            Session.LoadDocument(result.InitialDocument);
        }

        foreach (KeyValuePair<string, IStructureSchema> schema in result.SourceSchemas)
        {
            Session.LoadSourceSchema(schema.Key, schema.Value);
        }

        if (result.TargetSchema != null)
        {
            Session.LoadTargetSchema(result.TargetSchema);
        }

        RunSemanticDiagnostics();
    }

    // Gets the last registered host integration so consuming apps can override defaults.
    private IButterMorphDesignerHost FindHost()
    {
        IButterMorphDesignerHost selectedHost = null;

        foreach (IButterMorphDesignerHost host in _designerHosts)
        {
            selectedHost = host;
        }

        return selectedHost;
    }

    // Reads schema content from uploaded file or pasted text.
    private async Task<string> ReadSchemaContent(string fileFieldName, string textContent)
    {
        foreach (IFormFile file in Request.Form.Files)
        {
            if (!string.Equals(file.Name, fileFieldName, StringComparison.Ordinal) || file.Length <= 0)
            {
                continue;
            }

            using StreamReader reader = new(file.OpenReadStream());
            return await reader.ReadToEndAsync();
        }

        return textContent.Trim();
    }

    // Loads UI state from the current session.
    private void LoadViewState()
    {
        ShowSchemaActions = ContextState.ShowSchemaActions;
        ITransformationDocument document = Session.Document;
        List<SchemaTreeDisplayNode> sourceNodes = [];
        List<SourceSchemaDisplayModel> sourceSchemas = [];

        foreach (KeyValuePair<string, IStructureSchema> schemaPair in document.SourceSchemas)
        {
            ISchemaTreeNode explored = _schemaExplorer.Explore(schemaPair.Value);
            SchemaTreeDisplayNode sourceTree = SchemaTreeDisplayBuilder.BuildSource(schemaPair.Key, explored);
            sourceSchemas.Add(new SourceSchemaDisplayModel
            {
                Key = schemaPair.Key,
                SchemaKey = schemaPair.Value.Key,
                SchemaName = schemaPair.Value.Name,
                Version = schemaPair.Value.Version,
                Topic = ReadMetadata(schemaPair.Value.Metadata, "topic", string.Empty),
                Root = sourceTree
            });
            sourceNodes.AddRange(CreateSourceNodes(schemaPair.Key, SchemaTreeFlattener.Flatten(explored)));
        }

        Dictionary<string, string> expressions = CreateExpressionDictionary(document);
        Dictionary<string, ArrayProjectionDisplayModel> projections = CreateProjectionDictionary(document);
        Dictionary<string, IReadOnlyCollection<string>> diagnostics = CreateDiagnosticDictionary(Diagnostics);
        SourceNodes = sourceNodes;
        SourceSchemas = sourceSchemas;
        FunctionCategories = CreateFunctionCategories();
        TargetNodes = SchemaTreeFlattener.Flatten(_schemaExplorer.Explore(document.TargetSchema));
        Mappings = CreateMappings(document);
        TargetFields = CreateTargetFields(TargetNodes, Mappings);
        TargetTree = SchemaTreeDisplayBuilder.BuildTarget(_schemaExplorer.Explore(document.TargetSchema), expressions, diagnostics, projections);

        if (string.IsNullOrWhiteSpace(DslContent))
        {
            DslContent = Session.ExportDsl();
        }
    }

    // Creates grouped function toolbox display data from registered descriptors.
    private IReadOnlyCollection<FunctionToolboxCategoryDisplayModel> CreateFunctionCategories()
    {
        SortedDictionary<string, List<FunctionToolboxItemDisplayModel>> grouped = new(StringComparer.OrdinalIgnoreCase);

        foreach (IFunctionDescriptor descriptor in _capabilityExplorer.ListFunctions())
        {
            string category = ReadMetadata(descriptor.Metadata, "category", "Custom");

            if (!grouped.TryGetValue(category, out List<FunctionToolboxItemDisplayModel> functions))
            {
                functions = [];
                grouped[category] = functions;
            }

            functions.Add(new FunctionToolboxItemDisplayModel
            {
                Key = descriptor.Key,
                DisplayName = ResolveDisplayName(descriptor),
                Description = descriptor.Description,
                Category = category,
                ValueKind = descriptor.ValueKind,
                Template = CreateFunctionTemplate(descriptor)
            });
        }

        List<FunctionToolboxCategoryDisplayModel> categories = [];

        foreach (KeyValuePair<string, List<FunctionToolboxItemDisplayModel>> group in grouped)
        {
            group.Value.Sort(CompareFunctionItems);
            categories.Add(new FunctionToolboxCategoryDisplayModel
            {
                Name = group.Key,
                Functions = group.Value
            });
        }

        return categories;
    }

    // Compares function toolbox items by key.
    private static int CompareFunctionItems(FunctionToolboxItemDisplayModel left, FunctionToolboxItemDisplayModel right)
    {
        return string.Compare(left.Key, right.Key, StringComparison.OrdinalIgnoreCase);
    }

    // Resolves the display name for a descriptor.
    private static string ResolveDisplayName(IFunctionDescriptor descriptor)
    {
        if (string.IsNullOrWhiteSpace(descriptor.DisplayName))
        {
            return descriptor.Key;
        }

        return descriptor.DisplayName;
    }

    // Creates the insertion template for a function descriptor.
    private static string CreateFunctionTemplate(IFunctionDescriptor descriptor)
    {
        List<string> arguments = [];
        int argumentCount = ResolveTemplateArgumentCount(descriptor);

        for (int index = 0; index < argumentCount; index++)
        {
            arguments.Add(ResolveParameterKey(descriptor.Parameters, index));
        }

        return descriptor.Key + "(" + string.Join(", ", arguments) + ")";
    }

    // Resolves the number of arguments to include in a function template.
    private static int ResolveTemplateArgumentCount(IFunctionDescriptor descriptor)
    {
        string minimumText = ReadMetadata(descriptor.Metadata, "minArgs", string.Empty);

        if (int.TryParse(minimumText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int minimumArguments))
        {
            return minimumArguments;
        }

        int requiredCount = 0;

        foreach (IFunctionParameterDescriptor parameter in descriptor.Parameters)
        {
            if (parameter.IsRequired)
            {
                requiredCount++;
            }
        }

        if (requiredCount > 0)
        {
            return requiredCount;
        }

        return descriptor.Parameters.Count;
    }

    // Resolves a parameter key for template insertion.
    private static string ResolveParameterKey(IReadOnlyCollection<IFunctionParameterDescriptor> parameters, int index)
    {
        int currentIndex = 0;

        foreach (IFunctionParameterDescriptor parameter in parameters)
        {
            if (currentIndex == index)
            {
                if (!string.IsNullOrWhiteSpace(parameter.Key))
                {
                    return parameter.Key;
                }

                break;
            }

            currentIndex++;
        }

        return "argument" + index.ToString(CultureInfo.InvariantCulture);
    }

    // Reads descriptor metadata with a fallback.
    private static string ReadMetadata(IReadOnlyDictionary<string, string> metadata, string key, string fallback)
    {
        if (metadata.TryGetValue(key, out string value) && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return fallback;
    }

    // Creates display rows for mappings.
    private IReadOnlyCollection<MappingDisplayModel> CreateMappings(ITransformationDocument document)
    {
        List<MappingDisplayModel> rows = [];

        foreach (ITransformationMapping mapping in document.Mappings)
        {
            rows.Add(new MappingDisplayModel
            {
                TargetPath = mapping.TargetPath,
                Expression = ExportExpression(mapping)
            });
        }

        return rows;
    }

    // Creates a target path to expression lookup.
    private Dictionary<string, string> CreateExpressionDictionary(ITransformationDocument document)
    {
        Dictionary<string, string> expressions = new(StringComparer.Ordinal);

        foreach (ITransformationMapping mapping in document.Mappings)
        {
            expressions[mapping.TargetPath] = ExportExpression(mapping);

            if (mapping.SourceExpression is ICollectionProjectionExpression projectionExpression)
            {
                AddProjectionExpressionKeys(mapping.TargetPath, projectionExpression, expressions);
            }
        }

        return expressions;
    }

    // Creates array projection display data by target array path.
    private Dictionary<string, ArrayProjectionDisplayModel> CreateProjectionDictionary(ITransformationDocument document)
    {
        Dictionary<string, ArrayProjectionDisplayModel> projections = new(StringComparer.Ordinal);

        foreach (ITransformationMapping mapping in document.Mappings)
        {
            if (mapping.SourceExpression is not ICollectionProjectionExpression projectionExpression)
            {
                continue;
            }

            Dictionary<string, string> fieldExpressions = new(StringComparer.Ordinal);
            string advancedExpression = string.Empty;

            if (projectionExpression.BodyExpression is IObjectExpression mapExpression)
            {
                AddProjectionFieldExpressions(string.Empty, mapExpression, fieldExpressions);
            }
            else
            {
                advancedExpression = ExportExpressionValue(projectionExpression);
            }

            projections[mapping.TargetPath] = new ArrayProjectionDisplayModel
            {
                TargetPath = mapping.TargetPath,
                SourceExpression = ExportExpressionValue(projectionExpression.SourceExpression),
                Alias = ResolveAlias(projectionExpression.ItemAlias),
                AdvancedExpression = advancedExpression,
                FieldExpressions = fieldExpressions
            };
        }

        return projections;
    }

    // Adds editable item template expressions by relative field path.
    private void AddProjectionFieldExpressions(
        string prefix,
        IObjectExpression mapExpression,
        Dictionary<string, string> expressions)
    {
        foreach (IObjectPropertyExpression property in mapExpression.Properties)
        {
            string fieldPath = CreateFieldPath(prefix, property.Name);

            if (property.Expression is IObjectExpression childMapExpression)
            {
                AddProjectionFieldExpressions(fieldPath, childMapExpression, expressions);
                continue;
            }

            expressions[fieldPath] = ExportExpressionValue(property.Expression);
        }
    }

    // Adds composite keys used by the visual array projection editor.
    private void AddProjectionExpressionKeys(
        string targetPath,
        ICollectionProjectionExpression projectionExpression,
        Dictionary<string, string> expressions)
    {
        expressions[CreateProjectionKey(targetPath, "source")] = ExportExpressionValue(projectionExpression.SourceExpression);
        expressions[CreateProjectionKey(targetPath, "alias")] = ResolveAlias(projectionExpression.ItemAlias);

        if (projectionExpression.BodyExpression is IObjectExpression mapExpression)
        {
            AddProjectionFieldExpressionKeys(targetPath, string.Empty, mapExpression, expressions);
            return;
        }

        expressions[CreateProjectionKey(targetPath, "advanced")] = ExportExpressionValue(projectionExpression);
    }

    // Adds composite field keys used by item template inputs.
    private void AddProjectionFieldExpressionKeys(
        string targetPath,
        string prefix,
        IObjectExpression mapExpression,
        Dictionary<string, string> expressions)
    {
        foreach (IObjectPropertyExpression property in mapExpression.Properties)
        {
            string fieldPath = CreateFieldPath(prefix, property.Name);

            if (property.Expression is IObjectExpression childMapExpression)
            {
                AddProjectionFieldExpressionKeys(targetPath, fieldPath, childMapExpression, expressions);
                continue;
            }

            expressions[CreateProjectionFieldKey(targetPath, fieldPath)] = ExportExpressionValue(property.Expression);
        }
    }

    // Saves posted target mappings into the current document.
    private IReadOnlyCollection<DiagnosticEntry> SavePostedMappings()
    {
        List<DiagnosticEntry> diagnostics = [];
        int count = Math.Min(TargetPaths.Count, Expressions.Count);

        for (int index = 0; index < count; index++)
        {
            string targetPath = TargetPaths[index];
            string expressionText = Expressions[index];
            Session.RemoveMapping(targetPath);

            if (string.IsNullOrWhiteSpace(expressionText))
            {
                continue;
            }

            IMappingOperationResult result = Session.AddExpressionTextMapping(expressionText, targetPath);

            if (!result.Succeeded)
            {
                diagnostics.AddRange(result.Diagnostics);
            }
        }

        SavePostedProjections(diagnostics);

        return diagnostics;
    }

    // Saves posted array projection editors as single target array mappings.
    private void SavePostedProjections(List<DiagnosticEntry> diagnostics)
    {
        for (int index = 0; index < ProjectionTargetPaths.Count; index++)
        {
            string targetPath = ProjectionTargetPaths[index];
            string sourceExpression = GetPostedValue(ProjectionSources, index);
            string alias = ResolveAlias(GetPostedValue(ProjectionAliases, index));
            string advancedExpression = GetPostedValue(ProjectionAdvancedExpressions, index);
            Dictionary<string, string> fieldExpressions = GetPostedProjectionFields(targetPath);

            Session.RemoveMapping(targetPath);

            if (string.IsNullOrWhiteSpace(sourceExpression) && string.IsNullOrWhiteSpace(advancedExpression) && fieldExpressions.Count == 0)
            {
                continue;
            }

            string expressionText = advancedExpression;

            if (string.IsNullOrWhiteSpace(expressionText))
            {
                if (string.IsNullOrWhiteSpace(sourceExpression))
                {
                    diagnostics.Add(CreateDiagnostic("BMWR001", "Array projection source is required.", targetPath));
                    continue;
                }

                expressionText = "project " + sourceExpression + " as " + alias + " => " + RenderProjectionBody(fieldExpressions, alias);
            }

            IMappingOperationResult result = Session.AddExpressionTextMapping(expressionText, targetPath);

            if (!result.Succeeded)
            {
                diagnostics.AddRange(result.Diagnostics);
            }
        }
    }

    // Gets posted projection field expressions for a target array.
    private Dictionary<string, string> GetPostedProjectionFields(string targetPath)
    {
        Dictionary<string, string> fields = new(StringComparer.Ordinal);
        int count = Math.Min(ProjectionFieldArrayPaths.Count, Math.Min(ProjectionFieldPaths.Count, ProjectionFieldExpressions.Count));

        for (int index = 0; index < count; index++)
        {
            if (!string.Equals(ProjectionFieldArrayPaths[index], targetPath, StringComparison.Ordinal))
            {
                continue;
            }

            string fieldPath = ProjectionFieldPaths[index];
            string expression = ProjectionFieldExpressions[index];

            if (string.IsNullOrWhiteSpace(fieldPath) || string.IsNullOrWhiteSpace(expression))
            {
                continue;
            }

            fields[fieldPath] = expression;
        }

        return fields;
    }

    // Renders projection body content for DSL import.
    private static string RenderProjectionBody(Dictionary<string, string> fields, string alias)
    {
        if (fields.Count == 0)
        {
            return alias;
        }

        return "{ " + RenderProjectionFields(fields, string.Empty) + " }";
    }

    // Renders projection fields recursively for nested item templates.
    private static string RenderProjectionFields(Dictionary<string, string> fields, string prefix)
    {
        List<string> parts = [];
        List<string> names = GetProjectionFieldNames(fields, prefix);

        foreach (string name in names)
        {
            string fieldPath = CreateFieldPath(prefix, name);

            if (fields.TryGetValue(fieldPath, out string expression))
            {
                parts.Add(name + ": " + expression);
                continue;
            }

            parts.Add(name + ": { " + RenderProjectionFields(fields, fieldPath) + " }");
        }

        return string.Join(", ", parts);
    }

    // Gets immediate field names under a projection path.
    private static List<string> GetProjectionFieldNames(Dictionary<string, string> fields, string prefix)
    {
        SortedSet<string> names = new(StringComparer.Ordinal);

        foreach (string fieldPath in fields.Keys)
        {
            string remaining = fieldPath;

            if (!string.IsNullOrWhiteSpace(prefix))
            {
                string expectedPrefix = prefix + ".";

                if (!fieldPath.StartsWith(expectedPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                remaining = fieldPath[expectedPrefix.Length..];
            }

            int separatorIndex = remaining.IndexOf(".", StringComparison.Ordinal);

            if (separatorIndex >= 0)
            {
                remaining = remaining[..separatorIndex];
            }

            names.Add(remaining);
        }

        return [.. names];
    }

    // Creates a path to diagnostic messages lookup.
    private static Dictionary<string, IReadOnlyCollection<string>> CreateDiagnosticDictionary(IReadOnlyCollection<DiagnosticEntry> diagnostics)
    {
        Dictionary<string, List<string>> grouped = new(StringComparer.Ordinal);

        foreach (DiagnosticEntry diagnostic in diagnostics)
        {
            string path = diagnostic.Path;

            if (string.IsNullOrWhiteSpace(path))
            {
                path = "$root";
            }

            if (!grouped.TryGetValue(path, out List<string> messages))
            {
                messages = [];
                grouped[path] = messages;
            }

            messages.Add(diagnostic.Code + " " + diagnostic.Message);
        }

        Dictionary<string, IReadOnlyCollection<string>> result = new(StringComparer.Ordinal);

        foreach (KeyValuePair<string, List<string>> entry in grouped)
        {
            result[entry.Key] = entry.Value;
        }

        return result;
    }

    // Creates editable target field rows from target schema leaves.
    private static IReadOnlyCollection<TargetFieldMappingDisplayModel> CreateTargetFields(
        IReadOnlyCollection<SchemaTreeDisplayNode> targetNodes,
        IReadOnlyCollection<MappingDisplayModel> mappings)
    {
        Dictionary<string, string> expressions = new(StringComparer.Ordinal);
        List<TargetFieldMappingDisplayModel> fields = [];

        foreach (MappingDisplayModel mapping in mappings)
        {
            expressions[mapping.TargetPath] = mapping.Expression;
        }

        foreach (SchemaTreeDisplayNode node in targetNodes)
        {
            if (node.Kind != SchemaNodeKind.Scalar)
            {
                continue;
            }

            string expression = string.Empty;

            if (expressions.TryGetValue(node.Path, out string storedExpression))
            {
                expression = storedExpression;
            }

            fields.Add(new TargetFieldMappingDisplayModel
            {
                TargetPath = node.Path,
                DisplayName = node.Name,
                Kind = node.Kind,
                DataType = node.DataType,
                Expression = expression,
                Placeholder = CreatePlaceholder(node.Path)
            });
        }

        return fields;
    }

    // Creates source display nodes with absolute source paths.
    private static IReadOnlyCollection<SchemaTreeDisplayNode> CreateSourceNodes(
        string sourceKey,
        IReadOnlyCollection<SchemaTreeDisplayNode> nodes)
    {
        List<SchemaTreeDisplayNode> sourceNodes = [];

        foreach (SchemaTreeDisplayNode node in nodes)
        {
            sourceNodes.Add(new SchemaTreeDisplayNode
            {
                Depth = node.Depth,
                Path = CreateSourcePath(sourceKey, node.Path),
                Name = node.Name,
                Kind = node.Kind,
                DataType = node.DataType
            });
        }

        return sourceNodes;
    }

    // Creates an absolute source path from a schema path.
    private static string CreateSourcePath(string sourceKey, string schemaPath)
    {
        if (string.Equals(schemaPath, "$root", StringComparison.Ordinal))
        {
            return "$" + sourceKey;
        }

        return "$" + sourceKey + "." + schemaPath;
    }

    // Creates a source path placeholder based on the target path.
    private static string CreatePlaceholder(string targetPath)
    {
        return string.Empty;
    }

    // Creates the composite key for projection header inputs.
    private static string CreateProjectionKey(string targetPath, string part)
    {
        return targetPath + "::projection::" + part;
    }

    // Creates the composite key for projection item field inputs.
    private static string CreateProjectionFieldKey(string targetPath, string fieldPath)
    {
        return targetPath + "::projection::field::" + fieldPath;
    }

    // Creates a dotted field path without leading separators.
    private static string CreateFieldPath(string prefix, string name)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            return name;
        }

        return prefix + "." + name;
    }

    // Resolves a safe default item alias for visual projections.
    private static string ResolveAlias(string alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
        {
            return "item";
        }

        return alias;
    }

    // Gets a posted value by index without throwing for uneven lists.
    private static string GetPostedValue(IReadOnlyList<string> values, int index)
    {
        if (index >= values.Count)
        {
            return string.Empty;
        }

        return values[index];
    }

    // Creates a designer diagnostic entry.
    private static DiagnosticEntry CreateDiagnostic(string code, string message, string path)
    {
        return new DiagnosticEntry
        {
            Code = code,
            Message = message,
            Path = path,
            Severity = "Error"
        };
    }

    // Exports a transformation expression by wrapping it in a temporary mapping.
    private string ExportExpressionValue(ITransformationExpression expression)
    {
        return ExportExpression(new ButterMorph.Core.TransformationMapping
        {
            SourceExpression = expression,
            TargetPath = "Value"
        });
    }

    // Exports a single mapping expression by wrapping it in a temporary document.
    private string ExportExpression(ITransformationMapping mapping)
    {
        ITransformationDocument document = new ButterMorph.Core.TransformationDocument
        {
            Mappings =
            [
                mapping
            ]
        };

        string dsl = _dslExporter.Export(document);
        int separatorIndex = dsl.IndexOf(": ", StringComparison.Ordinal);

        if (separatorIndex < 0)
        {
            return string.Empty;
        }

        string expression = dsl[(separatorIndex + 2)..];
        expression = expression.Replace("\r", string.Empty, StringComparison.Ordinal);
        int lineEndIndex = expression.IndexOf("\n", StringComparison.Ordinal);

        if (lineEndIndex >= 0)
        {
            expression = expression[..lineEndIndex];
        }

        return expression.Trim();
    }

    // Creates a friendly operation message.
    private static string CreateMessage(IMappingOperationResult result, string successMessage)
    {
        if (result.Succeeded)
        {
            return successMessage;
        }

        foreach (DiagnosticEntry diagnostic in result.Diagnostics)
        {
            return diagnostic.Message;
        }

        return "Operation failed.";
    }

    // Creates a live synchronization response.
    private DesignerSyncResponse CreateSyncResponse(bool succeeded, string message)
    {
        ITransformationDocument document = Session.Document;
        string dslContent = DslContent;

        if (succeeded)
        {
            dslContent = Session.ExportDsl();
        }

        return new DesignerSyncResponse
        {
            Succeeded = succeeded,
            Message = message,
            DslContent = dslContent,
            Mappings = CreateExpressionDictionary(document),
            DiagnosticsCount = Diagnostics.Count,
            EditorDiagnostics = CreateEditorDiagnostics(dslContent, Diagnostics),
            HostSaveCompleted = HostSaveCompleted,
            SavedContextKey = SavedContextKey,
            SafeReturnUrl = SafeReturnUrl
        };
    }

    // Creates DSL editor diagnostics from regular diagnostics.
    private static IReadOnlyCollection<DesignerEditorDiagnostic> CreateEditorDiagnostics(
        string dslContent,
        IReadOnlyCollection<DiagnosticEntry> diagnostics)
    {
        List<DesignerEditorDiagnostic> editorDiagnostics = [];

        foreach (DiagnosticEntry diagnostic in diagnostics)
        {
            DesignerEditorDiagnostic editorDiagnostic = LocateEditorDiagnostic(dslContent, diagnostic);
            editorDiagnostics.Add(editorDiagnostic);
        }

        return editorDiagnostics;
    }

    // Resolves the most useful editor location for a diagnostic.
    private static DesignerEditorDiagnostic LocateEditorDiagnostic(string dslContent, DiagnosticEntry diagnostic)
    {
        DesignerEditorDiagnostic editorDiagnostic = new()
        {
            Code = diagnostic.Code,
            Message = diagnostic.Message,
            Severity = diagnostic.Severity,
            Path = diagnostic.Path,
            Line = 1,
            Column = 1,
            Length = ResolveDiagnosticLength(diagnostic)
        };

        if (TryReadLineColumn(diagnostic.Message, editorDiagnostic))
        {
            return editorDiagnostic;
        }

        int pathIndex = FindDiagnosticPathIndex(dslContent, diagnostic.Path);

        if (pathIndex >= 0)
        {
            ApplyTextLocation(dslContent, pathIndex, editorDiagnostic);
        }

        return editorDiagnostic;
    }

    // Resolves a useful highlighted length.
    private static int ResolveDiagnosticLength(DiagnosticEntry diagnostic)
    {
        if (!string.IsNullOrWhiteSpace(diagnostic.Path))
        {
            return Math.Max(1, diagnostic.Path.Length);
        }

        return 1;
    }

    // Finds the diagnostic path or target assignment in the DSL text.
    private static int FindDiagnosticPathIndex(string dslContent, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return -1;
        }

        int assignmentIndex = dslContent.IndexOf(path + ":", StringComparison.Ordinal);

        if (assignmentIndex >= 0)
        {
            return assignmentIndex;
        }

        return dslContent.IndexOf(path, StringComparison.Ordinal);
    }

    // Applies one-based line and column from a zero-based text index.
    private static void ApplyTextLocation(string text, int index, DesignerEditorDiagnostic diagnostic)
    {
        int line = 1;
        int column = 1;

        for (int characterIndex = 0; characterIndex < index && characterIndex < text.Length; characterIndex++)
        {
            if (text[characterIndex] == '\n')
            {
                line++;
                column = 1;
                continue;
            }

            column++;
        }

        diagnostic.Line = line;
        diagnostic.Column = column;
    }

    // Tries to parse messages that contain line and column data.
    private static bool TryReadLineColumn(string message, DesignerEditorDiagnostic diagnostic)
    {
        const string lineMarker = "Line ";
        const string columnMarker = "column ";
        int lineMarkerIndex = message.IndexOf(lineMarker, StringComparison.OrdinalIgnoreCase);
        int columnMarkerIndex = message.IndexOf(columnMarker, StringComparison.OrdinalIgnoreCase);

        if (lineMarkerIndex < 0 || columnMarkerIndex < 0)
        {
            return false;
        }

        int lineStart = lineMarkerIndex + lineMarker.Length;
        int columnStart = columnMarkerIndex + columnMarker.Length;
        string lineText = ReadNumberText(message, lineStart);
        string columnText = ReadNumberText(message, columnStart);

        if (!int.TryParse(lineText, CultureInfo.InvariantCulture, out int line))
        {
            return false;
        }

        if (!int.TryParse(columnText, CultureInfo.InvariantCulture, out int column))
        {
            return false;
        }

        diagnostic.Line = Math.Max(1, line);
        diagnostic.Column = Math.Max(1, column);

        return true;
    }

    // Reads a contiguous number from a text position.
    private static string ReadNumberText(string text, int start)
    {
        int end = start;

        while (end < text.Length && char.IsDigit(text[end]))
        {
            end++;
        }

        return text[start..end];
    }

    // Runs semantic diagnostics without exposing analyzer workflow in the UI.
    private void RunSemanticDiagnostics()
    {
        SemanticAnalysisResult result = Session.Analyze();
        Diagnostics = result.Diagnostics;
    }

    // Combines conversion diagnostics.
    private static IReadOnlyCollection<DiagnosticEntry> CombineDiagnostics(
        IReadOnlyCollection<DiagnosticEntry> first,
        IReadOnlyCollection<DiagnosticEntry> second)
    {
        List<DiagnosticEntry> diagnostics = [.. first];
        diagnostics.AddRange(second);

        return diagnostics;
    }
}
