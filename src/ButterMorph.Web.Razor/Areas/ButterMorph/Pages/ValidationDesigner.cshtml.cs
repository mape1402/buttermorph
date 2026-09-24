namespace ButterMorph.Web.Razor;

using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Design;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using System.Globalization;

/// <summary>
/// Displays and edits validation documents.
/// </summary>
public sealed class ValidationDesignerModel : PageModel
{
    // Stores validation design sessions for the web designer.
    private readonly IValidationDesignSessionStore _sessionStore;

    // Explores schemas for UI rendering.
    private readonly ISchemaExplorer _schemaExplorer;

    // Lists design-time capabilities such as registered functions.
    private readonly ICapabilityExplorer _capabilityExplorer;

    // Parses temporary visual validation snippets.
    private readonly IDslParser _dslParser;

    // Exports validation DSL and individual validation expressions.
    private readonly IValidationDslExporter _validationDslExporter;

    // Reads integration options for the reusable designer.
    private readonly ButterMorphRazorDesignerOptions _options;

    // Provides optional host application integration.
    private readonly IEnumerable<IButterMorphValidationDesignerHost> _designerHosts;

    // Stores web-only state for validation designer contexts.
    private static readonly DesignerContextStateStore ContextStates = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationDesignerModel"/> class.
    /// </summary>
    /// <param name="sessionStore">The session store.</param>
    /// <param name="schemaExplorer">The schema explorer.</param>
    /// <param name="capabilityExplorer">The capability explorer.</param>
    /// <param name="dslParser">The DSL parser.</param>
    /// <param name="validationDslExporter">The validation DSL exporter.</param>
    /// <param name="options">The Razor designer options.</param>
    /// <param name="designerHosts">The optional designer host integrations.</param>
    public ValidationDesignerModel(
        IValidationDesignSessionStore sessionStore,
        ISchemaExplorer schemaExplorer,
        ICapabilityExplorer capabilityExplorer,
        IDslParser dslParser,
        IValidationDslExporter validationDslExporter,
        IOptions<ButterMorphRazorDesignerOptions> options,
        IEnumerable<IButterMorphValidationDesignerHost> designerHosts)
    {
        _sessionStore = sessionStore;
        _schemaExplorer = schemaExplorer;
        _capabilityExplorer = capabilityExplorer;
        _dslParser = dslParser;
        _validationDslExporter = validationDslExporter;
        _options = options.Value;
        _designerHosts = designerHosts;
    }

    /// <summary>
    /// Gets the host-configured designer theme style.
    /// </summary>
    public string ThemeStyle => ButterMorphDesignerThemeStyle.Build(_options);

    /// <summary>
    /// Gets the host-configured designer theme mode.
    /// </summary>
    public string ThemeMode => _options.Theme.DefaultMode.ToString().ToLowerInvariant();

    /// <summary>
    /// Gets or sets the payload alias used by scoped assertions.
    /// </summary>
    [BindProperty]
    public string PayloadAlias { get; set; } = "source";

    /// <summary>
    /// Gets or sets the schema key used by scoped assertions.
    /// </summary>
    [BindProperty]
    public string SchemaKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets posted rule paths.
    /// </summary>
    [BindProperty]
    public List<string> RulePaths { get; set; } = [];

    /// <summary>
    /// Gets or sets posted rule keys.
    /// </summary>
    [BindProperty]
    public List<string> RuleKeys { get; set; } = [];

    /// <summary>
    /// Gets or sets posted rule arguments.
    /// </summary>
    [BindProperty]
    public List<string> RuleArguments { get; set; } = [];

    /// <summary>
    /// Gets or sets posted assertion expressions.
    /// </summary>
    [BindProperty]
    public List<string> AssertionExpressions { get; set; } = [];

    /// <summary>
    /// Gets or sets posted assertion messages.
    /// </summary>
    [BindProperty]
    public List<string> AssertionMessages { get; set; } = [];

    /// <summary>
    /// Gets or sets posted assertion diagnostic paths.
    /// </summary>
    [BindProperty]
    public List<string> AssertionPaths { get; set; } = [];

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
    /// Gets source schema toolbox groups.
    /// </summary>
    public IReadOnlyCollection<SourceSchemaDisplayModel> SourceSchemas { get; private set; } = [];

    /// <summary>
    /// Gets function toolbox categories.
    /// </summary>
    public IReadOnlyCollection<FunctionToolboxCategoryDisplayModel> FunctionCategories { get; private set; } = [];

    /// <summary>
    /// Gets the current validation rules.
    /// </summary>
    public IReadOnlyCollection<ValidationRuleDisplayModel> Rules { get; private set; } = [];

    /// <summary>
    /// Gets the current validation assertions.
    /// </summary>
    public IReadOnlyCollection<ValidationAssertionDisplayModel> Assertions { get; private set; } = [];

    /// <summary>
    /// Gets validation diagnostics.
    /// </summary>
    public IReadOnlyCollection<DiagnosticEntry> Diagnostics { get; private set; } = [];

    /// <summary>
    /// Gets or sets the status message.
    /// </summary>
    public string Message { get; set; } = "Ready.";

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
    /// Displays the validation designer.
    /// </summary>
    /// <returns>The asynchronous page task.</returns>
    public async Task OnGet()
    {
        await PreloadHostState();
        LoadViewState();
    }

    /// <summary>
    /// Synchronizes visual validation changes into the current DSL content.
    /// </summary>
    /// <returns>The synchronization response.</returns>
    public IActionResult OnPostSyncVisual()
    {
        IReadOnlyCollection<DiagnosticEntry> diagnostics = SavePostedValidationDocument();

        if (diagnostics.Count == 0)
        {
            Message = string.Empty;
            return new JsonResult(CreateSyncResponse(true, Message));
        }

        Diagnostics = diagnostics;
        Message = "Some validations could not be synchronized.";
        return new JsonResult(CreateSyncResponse(false, Message));
    }

    /// <summary>
    /// Synchronizes DSL content into visual validation rows.
    /// </summary>
    /// <returns>The synchronization response.</returns>
    public IActionResult OnPostSyncDsl()
    {
        IValidationOperationResult result = Session.ImportDsl(DslContent);

        if (result.Succeeded)
        {
            Message = string.Empty;
            Diagnostics = [];
            return new JsonResult(CreateSyncResponse(true, Message));
        }

        Diagnostics = result.Diagnostics;
        Message = CreateMessage(result, string.Empty);
        return new JsonResult(CreateSyncResponse(false, Message));
    }

    /// <summary>
    /// Saves the validation document through the optional host integration.
    /// </summary>
    /// <returns>The page result.</returns>
    public async Task<IActionResult> OnPostSaveValidationDocument()
    {
        IReadOnlyCollection<DiagnosticEntry> diagnostics = SavePostedValidationDocument();

        if (diagnostics.Count == 0)
        {
            bool hostSaved = await SaveHostState();

            if (hostSaved)
            {
                ResolveHostCompletionState();
            }
        }
        else
        {
            Message = "Some validations could not be saved. Open the DSL view to review diagnostics.";
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
        IValidationOperationResult result = Session.ImportDsl(DslContent);
        Message = CreateMessage(result, "DSL imported.");
        ActiveView = "Dsl";
        Diagnostics = result.Succeeded ? [] : result.Diagnostics;
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
        Diagnostics = [];
        LoadViewState();

        return Page();
    }

    // Gets the current validation design session.
    private string SessionKey => "validation:" + DesignerSessionKeyResolver.Resolve(this, _options);

    // Gets the host context key for the current request.
    private string ContextKey => DesignerSessionKeyResolver.ResolveContextKey(this, _options);

    // Gets a value indicating whether the designer is running as a popup.
    private bool IsPopupRequest => string.Equals(
        Request.Query[_options.PopupQueryParameter],
        "true",
        StringComparison.OrdinalIgnoreCase);

    // Gets web-only state for the current designer context.
    private DesignerContextState ContextState => ContextStates.GetOrCreate(SessionKey, _options);

    // Gets the current validation design session.
    private IValidationDesignSession Session => _sessionStore.GetOrCreate(SessionKey);

    // Applies host-provided state to the current session when available.
    private async Task PreloadHostState()
    {
        DesignerContextState state = ContextState;

        if (!_options.UseHostPreload || state.HostPreloadApplied)
        {
            return;
        }

        IButterMorphValidationDesignerHost host = FindHost();

        if (host == null)
        {
            state.HostPreloadApplied = true;
            return;
        }

        ButterMorphValidationDesignerLoadResult result = await host.Load(new ButterMorphValidationDesignerLoadRequest
        {
            ContextKey = ContextKey
        });

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
        IButterMorphValidationDesignerHost host = FindHost();

        if (host == null)
        {
            Message = "Validations saved.";
            return true;
        }

        ButterMorphValidationDesignerSaveResult result = await host.Save(new ButterMorphValidationDesignerSaveRequest
        {
            ContextKey = ContextKey,
            Document = Session.Document,
            SourceMetadata = ContextState.SourceMetadata,
            DslContent = Session.ExportDsl()
        });

        if (result.Succeeded)
        {
            Message = ResolveMessage(result.Message, "Validations saved.");
            Diagnostics = [];
            return true;
        }

        Diagnostics = result.Diagnostics;
        Message = ResolveMessage(result.Message, "Validations could not be saved.");
        return false;
    }

    // Applies schemas and validation document content returned by the host.
    private void ApplyHostLoadResult(ButterMorphValidationDesignerLoadResult result)
    {
        ContextState.SourceSchemas.Clear();
        ContextState.SourceMetadata.Clear();

        foreach (KeyValuePair<string, IStructureSchema> schema in result.SourceSchemas)
        {
            ContextState.SourceSchemas[schema.Key] = schema.Value;
        }

        if (result.SourceMetadata != null)
        {
            foreach (KeyValuePair<string, ButterMorphDesignerSourceMetadata> metadata in result.SourceMetadata)
            {
                if (string.IsNullOrWhiteSpace(metadata.Key) || metadata.Value == null)
                {
                    continue;
                }

                ContextState.SourceMetadata[metadata.Key] = CloneSourceMetadata(metadata.Value);
            }
        }

        if (result.InitialDocument != null)
        {
            Session.LoadDocument(result.InitialDocument);
        }
        else if (!string.IsNullOrWhiteSpace(result.InitialDslContent))
        {
            Session.ImportDsl(result.InitialDslContent);
        }
    }

    // Gets the last registered host integration so consuming apps can override defaults.
    private IButterMorphValidationDesignerHost FindHost()
    {
        IButterMorphValidationDesignerHost selectedHost = null;

        foreach (IButterMorphValidationDesignerHost host in _designerHosts)
        {
            selectedHost = host;
        }

        return selectedHost;
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

    // Loads UI state from the current session.
    private void LoadViewState()
    {
        IValidationDocument document = Session.Document;
        PayloadAlias = document.PayloadAlias;
        SchemaKey = document.SchemaKey;
        SourceSchemas = CreateSourceSchemas();
        FunctionCategories = CreateFunctionCategories();
        Rules = CreateRules(document);
        Assertions = CreateAssertions(document);

        if (string.IsNullOrWhiteSpace(DslContent))
        {
            DslContent = Session.ExportDsl();
        }
    }

    // Saves posted visual rows into the current document.
    private IReadOnlyCollection<DiagnosticEntry> SavePostedValidationDocument()
    {
        List<DiagnosticEntry> diagnostics = [];
        List<IValidationRule> rules = [];
        List<IValidationAssertion> assertions = [];

        SavePostedRules(rules, diagnostics);
        SavePostedAssertions(assertions, diagnostics);

        if (diagnostics.Count > 0)
        {
            return diagnostics;
        }

        IValidationOperationResult result = Session.ReplaceDocument(PayloadAlias, SchemaKey, rules, assertions);

        if (!result.Succeeded)
        {
            diagnostics.AddRange(result.Diagnostics);
        }

        return diagnostics;
    }

    // Saves posted validation rules.
    private void SavePostedRules(List<IValidationRule> rules, List<DiagnosticEntry> diagnostics)
    {
        int count = Math.Max(RulePaths.Count, Math.Max(RuleKeys.Count, RuleArguments.Count));

        for (int index = 0; index < count; index++)
        {
            string path = GetPostedValue(RulePaths, index).Trim();
            string ruleKey = GetPostedValue(RuleKeys, index).Trim();
            string arguments = GetPostedValue(RuleArguments, index).Trim();

            if (string.IsNullOrWhiteSpace(path) && string.IsNullOrWhiteSpace(ruleKey) && string.IsNullOrWhiteSpace(arguments))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(ruleKey))
            {
                diagnostics.Add(CreateDiagnostic("BVDG004", "Validation rule path and rule key are required.", path));
                continue;
            }

            try
            {
                rules.Add(ParseRule(path, ruleKey, arguments));
            }
            catch (FormatException exception)
            {
                diagnostics.Add(CreateDiagnostic("BVDG005", exception.Message, path));
            }
        }
    }

    // Saves posted validation assertions.
    private void SavePostedAssertions(List<IValidationAssertion> assertions, List<DiagnosticEntry> diagnostics)
    {
        int count = Math.Max(AssertionExpressions.Count, Math.Max(AssertionMessages.Count, AssertionPaths.Count));

        for (int index = 0; index < count; index++)
        {
            string expression = GetPostedValue(AssertionExpressions, index).Trim();
            string message = GetPostedValue(AssertionMessages, index).Trim();
            string path = GetPostedValue(AssertionPaths, index).Trim();

            if (string.IsNullOrWhiteSpace(expression) && string.IsNullOrWhiteSpace(message) && string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(expression))
            {
                diagnostics.Add(CreateDiagnostic("BVDG006", "Validation assertion expression is required.", path));
                continue;
            }

            try
            {
                IValidationAssertion assertion = ParseAssertion(expression, message);
                assertions.Add(new ValidationAssertion
                {
                    Expression = assertion.Expression,
                    Message = string.IsNullOrWhiteSpace(message) ? assertion.Message : message,
                    Path = string.IsNullOrWhiteSpace(path) ? assertion.Path : path
                });
            }
            catch (FormatException exception)
            {
                diagnostics.Add(CreateDiagnostic("BVDG007", exception.Message, path));
            }
        }
    }

    // Parses one visual validation rule row.
    private IValidationRule ParseRule(string path, string ruleKey, string arguments)
    {
        string invocation = ruleKey;

        if (!string.IsNullOrWhiteSpace(arguments))
        {
            invocation += "(" + arguments + ")";
        }

        IValidationDocument document = ParseValidationDocument(
            "validate {" + Environment.NewLine +
            "  " + path + ": " + invocation + Environment.NewLine +
            "}");

        return document.Rules.First();
    }

    // Parses one visual validation assertion row.
    private IValidationAssertion ParseAssertion(string expression, string message)
    {
        string payloadAlias = ResolvePayloadAlias(PayloadAlias);
        string schemaKey = string.IsNullOrWhiteSpace(SchemaKey) ? "Schema" : SchemaKey.Trim();
        string assertionMessage = string.IsNullOrWhiteSpace(message) ? "Validation assertion failed." : message;
        IValidationDocument document = ParseValidationDocument(
            "validate $" + payloadAlias + " against " + schemaKey + " {" + Environment.NewLine +
            "  assert " + expression + ": " + WriteString(assertionMessage) + Environment.NewLine +
            "}");

        return document.Assertions.First();
    }

    // Parses DSL into a validation document.
    private IValidationDocument ParseValidationDocument(string dsl)
    {
        IDslDocument document = _dslParser.Parse(new DslDefinition
        {
            Content = dsl
        });

        if (document is not IValidationDocument validationDocument)
        {
            throw new FormatException("Validation DSL did not produce a validation document.");
        }

        return validationDocument;
    }

    // Creates source schema display models from host-provided state.
    private IReadOnlyCollection<SourceSchemaDisplayModel> CreateSourceSchemas()
    {
        List<SourceSchemaDisplayModel> sourceSchemas = [];

        foreach (KeyValuePair<string, IStructureSchema> schemaPair in ContextState.SourceSchemas)
        {
            ButterMorphDesignerSourceMetadata sourceMetadata = ResolveSourceMetadata(schemaPair.Key, schemaPair.Value);
            ISchemaTreeNode explored = _schemaExplorer.Explore(schemaPair.Value);
            sourceSchemas.Add(new SourceSchemaDisplayModel
            {
                Key = schemaPair.Key,
                DisplayName = ResolveSourceDisplayName(sourceMetadata, schemaPair.Value, schemaPair.Key),
                Description = ResolveSourceDescription(sourceMetadata, schemaPair.Value),
                Tags = ResolveSourceTags(sourceMetadata, schemaPair.Value),
                SchemaKey = schemaPair.Value.Key,
                SchemaName = schemaPair.Value.Name,
                Version = schemaPair.Value.Version,
                Topic = ReadMetadata(schemaPair.Value.Metadata, "topic", string.Empty),
                Root = SchemaTreeDisplayBuilder.BuildSource(schemaPair.Key, explored)
            });
        }

        return sourceSchemas;
    }

    // Creates editable rule rows.
    private IReadOnlyCollection<ValidationRuleDisplayModel> CreateRules(IValidationDocument document)
    {
        List<ValidationRuleDisplayModel> rules = [];

        foreach (IValidationRule rule in document.Rules)
        {
            rules.Add(new ValidationRuleDisplayModel
            {
                Path = rule.Path,
                RuleKey = rule.RuleKey,
                Arguments = ExportRuleArguments(rule)
            });
        }

        rules.Add(new ValidationRuleDisplayModel());
        return rules;
    }

    // Creates editable assertion rows.
    private IReadOnlyCollection<ValidationAssertionDisplayModel> CreateAssertions(IValidationDocument document)
    {
        List<ValidationAssertionDisplayModel> assertions = [];

        foreach (IValidationAssertion assertion in document.Assertions)
        {
            assertions.Add(new ValidationAssertionDisplayModel
            {
                Expression = ExportAssertionExpression(assertion),
                Message = assertion.Message,
                Path = assertion.Path
            });
        }

        assertions.Add(new ValidationAssertionDisplayModel());
        return assertions;
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

    // Creates a live synchronization response.
    private DesignerSyncResponse CreateSyncResponse(bool succeeded, string message)
    {
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
            Mappings = new Dictionary<string, string>(),
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

            if (!TryReadLineColumn(diagnostic.Message, editorDiagnostic))
            {
                int pathIndex = FindDiagnosticPathIndex(dslContent, diagnostic.Path);

                if (pathIndex >= 0)
                {
                    ApplyTextLocation(dslContent, pathIndex, editorDiagnostic);
                }
            }

            editorDiagnostics.Add(editorDiagnostic);
        }

        return editorDiagnostics;
    }

    // Exports one assertion expression as DSL text.
    private string ExportAssertionExpression(IValidationAssertion assertion)
    {
        string dsl = _validationDslExporter.Export(new ValidationDocument
        {
            PayloadAlias = ResolvePayloadAlias(PayloadAlias),
            SchemaKey = string.IsNullOrWhiteSpace(SchemaKey) ? "Schema" : SchemaKey,
            Assertions =
            [
                assertion
            ]
        });
        const string assertMarker = "assert ";
        int start = dsl.IndexOf(assertMarker, StringComparison.Ordinal);

        if (start < 0)
        {
            return string.Empty;
        }

        start += assertMarker.Length;
        int end = dsl.IndexOf(": ", start, StringComparison.Ordinal);

        if (end < start)
        {
            return dsl[start..].Trim();
        }

        return dsl[start..end].Trim();
    }

    // Exports one rule argument list as DSL text.
    private string ExportRuleArguments(IValidationRule rule)
    {
        if (rule.Arguments.Count == 0)
        {
            return string.Empty;
        }

        string dsl = _validationDslExporter.Export(new ValidationDocument
        {
            Rules =
            [
                rule
            ]
        });
        string marker = rule.RuleKey + "(";
        int start = dsl.IndexOf(marker, StringComparison.Ordinal);

        if (start < 0)
        {
            return string.Empty;
        }

        start += marker.Length;
        int end = dsl.IndexOf(")", start, StringComparison.Ordinal);

        if (end < start)
        {
            return string.Empty;
        }

        return dsl[start..end].Trim();
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

    // Resolves source metadata with schema-derived fallbacks.
    private ButterMorphDesignerSourceMetadata ResolveSourceMetadata(string sourceKey, IStructureSchema schema)
    {
        if (ContextState.SourceMetadata.TryGetValue(sourceKey, out ButterMorphDesignerSourceMetadata metadata))
        {
            return metadata;
        }

        metadata = new ButterMorphDesignerSourceMetadata
        {
            DisplayName = ResolveSourceDisplayName(null, schema, sourceKey),
            Description = ResolveSourceDescription(null, schema),
            Tags = ResolveSourceTags(null, schema)
        };
        ContextState.SourceMetadata[sourceKey] = metadata;
        return metadata;
    }

    // Creates a source metadata copy.
    private static ButterMorphDesignerSourceMetadata CloneSourceMetadata(ButterMorphDesignerSourceMetadata metadata)
    {
        return new ButterMorphDesignerSourceMetadata
        {
            DisplayName = metadata.DisplayName,
            Description = metadata.Description,
            Tags = metadata.Tags?.ToArray() ?? []
        };
    }

    // Resolves source display name.
    private static string ResolveSourceDisplayName(ButterMorphDesignerSourceMetadata metadata, IStructureSchema schema, string sourceKey)
    {
        if (!string.IsNullOrWhiteSpace(metadata?.DisplayName))
        {
            return metadata.DisplayName;
        }

        if (!string.IsNullOrWhiteSpace(schema.Name))
        {
            return schema.Name;
        }

        return sourceKey;
    }

    // Resolves source description.
    private static string ResolveSourceDescription(ButterMorphDesignerSourceMetadata metadata, IStructureSchema schema)
    {
        if (!string.IsNullOrWhiteSpace(metadata?.Description))
        {
            return metadata.Description;
        }

        return ReadMetadata(schema.Metadata, "description", string.Empty);
    }

    // Resolves source tags.
    private static IReadOnlyCollection<string> ResolveSourceTags(ButterMorphDesignerSourceMetadata metadata, IStructureSchema schema)
    {
        if (metadata?.Tags != null && metadata.Tags.Count > 0)
        {
            return metadata.Tags;
        }

        string tags = ReadMetadata(schema.Metadata, "tags", string.Empty);

        if (string.IsNullOrWhiteSpace(tags))
        {
            return [];
        }

        return tags.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
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
        if (metadata != null && metadata.TryGetValue(key, out string value) && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return fallback;
    }

    // Creates a friendly operation message.
    private static string CreateMessage(IValidationOperationResult result, string successMessage)
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

    // Resolves a normalized payload alias.
    private static string ResolvePayloadAlias(string payloadAlias)
    {
        if (string.IsNullOrWhiteSpace(payloadAlias))
        {
            return "source";
        }

        return payloadAlias.Trim().TrimStart('$');
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

    // Creates an error diagnostic.
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

    // Escapes a string literal deterministically.
    private static string WriteString(string value)
    {
        System.Text.StringBuilder builder = new();
        builder.Append('"');

        foreach (char character in value)
        {
            if (character == '"')
            {
                builder.Append("\\\"");
            }
            else if (character == '\\')
            {
                builder.Append("\\\\");
            }
            else if (character == '\n')
            {
                builder.Append("\\n");
            }
            else if (character == '\r')
            {
                builder.Append("\\r");
            }
            else if (character == '\t')
            {
                builder.Append("\\t");
            }
            else
            {
                builder.Append(character);
            }
        }

        builder.Append('"');
        return builder.ToString();
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

    // Finds the diagnostic path or validation statement in the DSL text.
    private static int FindDiagnosticPathIndex(string dslContent, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return -1;
        }

        int pathIndex = dslContent.IndexOf(path, StringComparison.Ordinal);

        if (pathIndex >= 0)
        {
            return pathIndex;
        }

        return -1;
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

        if (!int.TryParse(lineText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int line))
        {
            return false;
        }

        if (!int.TryParse(columnText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int column))
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
}
