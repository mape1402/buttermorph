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
    /// Gets or sets posted assertion expressions.
    /// </summary>
    [BindProperty]
    public List<string> AssertionExpressions { get; set; } = [];

    /// <summary>
    /// Gets or sets posted assertion kinds.
    /// </summary>
    [BindProperty]
    public List<string> AssertionKinds { get; set; } = [];

    /// <summary>
    /// Gets or sets posted simple assertion field paths.
    /// </summary>
    [BindProperty]
    public List<string> SimpleFieldPaths { get; set; } = [];

    /// <summary>
    /// Gets or sets posted simple assertion operators.
    /// </summary>
    [BindProperty]
    public List<string> SimpleOperators { get; set; } = [];

    /// <summary>
    /// Gets or sets posted simple assertion values.
    /// </summary>
    [BindProperty]
    public List<string> SimpleValues { get; set; } = [];

    /// <summary>
    /// Gets or sets posted assertion messages.
    /// </summary>
    [BindProperty]
    public List<string> AssertionMessages { get; set; } = [];

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
        SourceSchemas = CreateSourceSchemas();
        FunctionCategories = CreateFunctionCategories();
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
        List<IValidationAssertion> assertions = [];

        SavePostedAssertions(assertions, diagnostics);

        if (diagnostics.Count > 0)
        {
            return diagnostics;
        }

        IValidationOperationResult result = Session.ReplaceDocument(assertions);

        if (!result.Succeeded)
        {
            diagnostics.AddRange(result.Diagnostics);
        }

        return diagnostics;
    }

    // Saves posted validation assertions.
    private void SavePostedAssertions(List<IValidationAssertion> assertions, List<DiagnosticEntry> diagnostics)
    {
        int count = new[]
        {
            CountPostedValues(AssertionKinds),
            CountPostedValues(AssertionExpressions),
            CountPostedValues(SimpleFieldPaths),
            CountPostedValues(SimpleOperators),
            CountPostedValues(SimpleValues),
            CountPostedValues(AssertionMessages)
        }.Max();

        for (int index = 0; index < count; index++)
        {
            string kind = NormalizeAssertionKind(GetPostedValue(AssertionKinds, index));
            string expression = GetPostedValue(AssertionExpressions, index).Trim();
            string message = GetPostedValue(AssertionMessages, index).Trim();

            if (string.Equals(kind, "Simple", StringComparison.Ordinal))
            {
                SavePostedSimpleAssertion(index, message, assertions, diagnostics);
                continue;
            }

            if (string.IsNullOrWhiteSpace(expression) && string.IsNullOrWhiteSpace(message))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(expression))
            {
                diagnostics.Add(CreateDiagnostic("BVDG006", "Validation assertion expression is required.", string.Empty));
                continue;
            }

            try
            {
                IValidationAssertion assertion = ParseAssertion(expression, message);
                assertions.Add(new ValidationAssertion
                {
                    Expression = assertion.Expression,
                    Message = string.IsNullOrWhiteSpace(message) ? assertion.Message : message,
                    Path = string.Empty
                });
            }
            catch (FormatException exception)
            {
                diagnostics.Add(CreateDiagnostic("BVDG007", exception.Message, string.Empty));
            }
        }
    }

    // Saves a simple field rule from guided field/operator/value inputs.
    private void SavePostedSimpleAssertion(
        int index,
        string message,
        List<IValidationAssertion> assertions,
        List<DiagnosticEntry> diagnostics)
    {
        string fieldPath = GetPostedValue(SimpleFieldPaths, index).Trim();
        string operatorKey = NormalizeSimpleOperator(GetPostedValue(SimpleOperators, index));
        string value = GetPostedValue(SimpleValues, index).Trim();

        if (string.IsNullOrWhiteSpace(fieldPath) && string.IsNullOrWhiteSpace(value) && string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(fieldPath))
        {
            diagnostics.Add(CreateDiagnostic("BVDG008", "Simple validation field is required.", string.Empty));
            return;
        }

        if (RequiresSimpleValue(operatorKey) && string.IsNullOrWhiteSpace(value))
        {
            diagnostics.Add(CreateDiagnostic("BVDG009", "Simple validation value is required.", fieldPath));
            return;
        }

        string expression = BuildSimpleAssertionExpression(fieldPath, operatorKey, value);

        try
        {
            IValidationAssertion assertion = ParseAssertion(expression, message);
            assertions.Add(new ValidationAssertion
            {
                Expression = assertion.Expression,
                Message = string.IsNullOrWhiteSpace(message) ? assertion.Message : message,
                Path = fieldPath
            });
        }
        catch (FormatException exception)
        {
            diagnostics.Add(CreateDiagnostic("BVDG010", exception.Message, fieldPath));
        }
    }

    // Parses one visual validation assertion row.
    private IValidationAssertion ParseAssertion(string expression, string message)
    {
        string assertionMessage = string.IsNullOrWhiteSpace(message) ? "Validation assertion failed." : message;
        IValidationDocument document = ParseValidationDocument(
            "validate {" + Environment.NewLine +
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

    // Creates editable assertion rows.
    private IReadOnlyCollection<ValidationAssertionDisplayModel> CreateAssertions(IValidationDocument document)
    {
        List<ValidationAssertionDisplayModel> assertions = [];

        foreach (IValidationAssertion assertion in document.Assertions)
        {
            assertions.Add(CreateAssertionDisplay(assertion));
        }

        assertions.Add(new ValidationAssertionDisplayModel
        {
            Kind = "Simple"
        });
        return assertions;
    }

    // Creates one editable assertion row from an existing assertion.
    private ValidationAssertionDisplayModel CreateAssertionDisplay(IValidationAssertion assertion)
    {
        ValidationAssertionDisplayModel display = new()
        {
            Kind = "Complex",
            Expression = ExportAssertionExpression(assertion),
            Message = assertion.Message
        };

        if (TryCreateSimpleDisplay(assertion.Expression, out string fieldPath, out string operatorKey, out string value))
        {
            display.Kind = "Simple";
            display.FieldPath = fieldPath;
            display.Operator = operatorKey;
            display.Value = value;
        }

        return display;
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

    // Exports an expression by wrapping it in a temporary assertion.
    private string ExportExpressionText(ITransformationExpression expression)
    {
        return ExportAssertionExpression(new ValidationAssertion
        {
            Expression = expression,
            Message = "Expression"
        });
    }

    // Reads a simple field rule from an expression when the visual designer can represent it directly.
    private bool TryCreateSimpleDisplay(
        ITransformationExpression expression,
        out string fieldPath,
        out string operatorKey,
        out string value)
    {
        fieldPath = string.Empty;
        operatorKey = "exists";
        value = string.Empty;

        if (expression is not IFunctionCallExpression function)
        {
            return false;
        }

        ITransformationExpression[] arguments = function.Arguments.ToArray();
        string functionKey = function.FunctionKey;

        if (string.Equals(functionKey, "not", StringComparison.Ordinal) &&
            arguments.Length == 1 &&
            arguments[0] is IFunctionCallExpression nested &&
            string.Equals(nested.FunctionKey, "isEmpty", StringComparison.Ordinal) &&
            TryReadPathArgument(nested.Arguments.ToArray(), out fieldPath))
        {
            operatorKey = "notEmpty";
            return true;
        }

        if ((string.Equals(functionKey, "exists", StringComparison.Ordinal) ||
            string.Equals(functionKey, "isEmpty", StringComparison.Ordinal)) &&
            TryReadPathArgument(arguments, out fieldPath))
        {
            operatorKey = functionKey;
            return true;
        }

        if (!TryReadTwoArgumentSimpleOperator(functionKey, arguments, out fieldPath, out value))
        {
            return false;
        }

        operatorKey = NormalizeSimpleOperator(functionKey);
        return true;
    }

    // Reads a single path argument.
    private static bool TryReadPathArgument(IReadOnlyList<ITransformationExpression> arguments, out string fieldPath)
    {
        fieldPath = string.Empty;

        if (arguments.Count != 1 || arguments[0] is not IPathExpression path)
        {
            return false;
        }

        fieldPath = path.Path;
        return true;
    }

    // Reads a two-argument simple operator call.
    private bool TryReadTwoArgumentSimpleOperator(
        string functionKey,
        IReadOnlyList<ITransformationExpression> arguments,
        out string fieldPath,
        out string value)
    {
        fieldPath = string.Empty;
        value = string.Empty;

        if (!IsTwoArgumentSimpleOperator(functionKey) ||
            arguments.Count != 2 ||
            arguments[0] is not IPathExpression path)
        {
            return false;
        }

        fieldPath = path.Path;
        value = ReadSimpleValue(arguments[1]);
        return true;
    }

    // Reads a simple right-hand value for display.
    private string ReadSimpleValue(ITransformationExpression expression)
    {
        if (expression is IPathExpression path)
        {
            return path.Path;
        }

        if (expression is IScalarLiteralExpression scalar)
        {
            if (scalar.Value.IsNull)
            {
                return "null";
            }

            if (string.Equals(scalar.Value.DataType, "Boolean", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(scalar.Value.DataType, "Number", StringComparison.OrdinalIgnoreCase))
            {
                return scalar.Value.RawValue;
            }

            return scalar.Value.RawValue;
        }

        return ExportExpressionText(expression);
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

    // Normalizes assertion kind values posted by the designer.
    private static string NormalizeAssertionKind(string kind)
    {
        if (string.Equals(kind, "Simple", StringComparison.OrdinalIgnoreCase))
        {
            return "Simple";
        }

        return "Complex";
    }

    // Normalizes simple operator aliases.
    private static string NormalizeSimpleOperator(string operatorKey)
    {
        string normalized = (operatorKey ?? string.Empty).Trim();

        return normalized switch
        {
            "required" => "exists",
            "eq" => "eq",
            "neq" => "neq",
            "gt" => "gt",
            "gte" => "gte",
            "lt" => "lt",
            "lte" => "lte",
            "contains" => "contains",
            "startsWith" => "startsWith",
            "endsWith" => "endsWith",
            "regexMatch" => "regexMatch",
            "isEmpty" => "isEmpty",
            "notEmpty" => "notEmpty",
            _ => "exists"
        };
    }

    // Builds the expression used by a simple field rule.
    private static string BuildSimpleAssertionExpression(string fieldPath, string operatorKey, string value)
    {
        string normalizedOperator = NormalizeSimpleOperator(operatorKey);

        if (string.Equals(normalizedOperator, "notEmpty", StringComparison.Ordinal))
        {
            return "not(isEmpty(" + fieldPath + "))";
        }

        if (!RequiresSimpleValue(normalizedOperator))
        {
            return normalizedOperator + "(" + fieldPath + ")";
        }

        return normalizedOperator + "(" + fieldPath + ", " + FormatSimpleValue(value) + ")";
    }

    // Indicates whether a simple operator needs a right-hand value.
    private static bool RequiresSimpleValue(string operatorKey)
    {
        return !string.Equals(NormalizeSimpleOperator(operatorKey), "exists", StringComparison.Ordinal) &&
            !string.Equals(NormalizeSimpleOperator(operatorKey), "isEmpty", StringComparison.Ordinal) &&
            !string.Equals(NormalizeSimpleOperator(operatorKey), "notEmpty", StringComparison.Ordinal);
    }

    // Indicates whether an expression function can be represented as a two-argument simple rule.
    private static bool IsTwoArgumentSimpleOperator(string functionKey)
    {
        return string.Equals(functionKey, "eq", StringComparison.Ordinal) ||
            string.Equals(functionKey, "neq", StringComparison.Ordinal) ||
            string.Equals(functionKey, "gt", StringComparison.Ordinal) ||
            string.Equals(functionKey, "gte", StringComparison.Ordinal) ||
            string.Equals(functionKey, "lt", StringComparison.Ordinal) ||
            string.Equals(functionKey, "lte", StringComparison.Ordinal) ||
            string.Equals(functionKey, "contains", StringComparison.Ordinal) ||
            string.Equals(functionKey, "startsWith", StringComparison.Ordinal) ||
            string.Equals(functionKey, "endsWith", StringComparison.Ordinal) ||
            string.Equals(functionKey, "regexMatch", StringComparison.Ordinal);
    }

    // Formats a simple right-hand value as DSL.
    private static string FormatSimpleValue(string value)
    {
        string trimmed = value.Trim();

        if (IsDslValueExpression(trimmed))
        {
            return trimmed;
        }

        return WriteString(trimmed);
    }

    // Detects values that are already DSL expressions instead of raw string literals.
    private static bool IsDslValueExpression(string value)
    {
        if (value.Length == 0)
        {
            return false;
        }

        if (value.StartsWith("$", StringComparison.Ordinal) ||
            (value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal)) ||
            (value.StartsWith("[", StringComparison.Ordinal) && value.EndsWith("]", StringComparison.Ordinal)) ||
            (value.StartsWith("{", StringComparison.Ordinal) && value.EndsWith("}", StringComparison.Ordinal)) ||
            string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "false", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, "null", StringComparison.OrdinalIgnoreCase) ||
            double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
        {
            return true;
        }

        int openIndex = value.IndexOf('(', StringComparison.Ordinal);

        return openIndex > 0 && value.EndsWith(")", StringComparison.Ordinal);
    }

    // Gets a posted value by index without throwing for uneven lists.
    private static string GetPostedValue(IReadOnlyList<string> values, int index)
    {
        if (values == null || index >= values.Count)
        {
            return string.Empty;
        }

        return values[index] ?? string.Empty;
    }

    // Counts posted values without throwing for missing form lists.
    private static int CountPostedValues(IReadOnlyCollection<string> values)
    {
        return values?.Count ?? 0;
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
