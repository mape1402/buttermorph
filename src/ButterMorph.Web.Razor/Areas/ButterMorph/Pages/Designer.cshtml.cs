namespace ButterMorph.Web.Razor;

using ButterMorph.Abstractions;
using ButterMorph.Design;
using ButterMorph.Json.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="DesignerModel"/> class.
    /// </summary>
    /// <param name="sessionStore">The session store.</param>
    /// <param name="schemaExplorer">The schema explorer.</param>
    /// <param name="dslExporter">The DSL exporter.</param>
    /// <param name="schemaImporter">The JSON Schema importer.</param>
    public DesignerModel(
        IMappingDesignSessionStore sessionStore,
        ISchemaExplorer schemaExplorer,
        IDslExporter dslExporter,
        IJsonSchemaImporter schemaImporter)
    {
        _sessionStore = sessionStore;
        _schemaExplorer = schemaExplorer;
        _dslExporter = dslExporter;
        _schemaImporter = schemaImporter;
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
    /// Displays the designer.
    /// </summary>
    public void OnGet()
    {
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
            Message = "Mappings synchronized.";
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
        Message = CreateMessage(result, "DSL synchronized.");
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
    public IActionResult OnPostSaveTargetMappings()
    {
        IReadOnlyCollection<DiagnosticEntry> diagnostics = SavePostedMappings();

        if (diagnostics.Count == 0)
        {
            Message = "Mappings saved.";
            RunSemanticDiagnostics();
        }
        else
        {
            Message = "Some target mappings could not be saved.";
            Diagnostics = diagnostics;
        }

        LoadViewState();

        return Page();
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
    private IMappingDesignSession Session => _sessionStore.GetOrCreate(DesignerSessionKeyResolver.Resolve(this));

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
                Root = sourceTree
            });
            sourceNodes.AddRange(CreateSourceNodes(schemaPair.Key, SchemaTreeFlattener.Flatten(explored)));
        }

        Dictionary<string, string> expressions = CreateExpressionDictionary(document);
        Dictionary<string, IReadOnlyCollection<string>> diagnostics = CreateDiagnosticDictionary(Diagnostics);
        SourceNodes = sourceNodes;
        SourceSchemas = sourceSchemas;
        TargetNodes = SchemaTreeFlattener.Flatten(_schemaExplorer.Explore(document.TargetSchema));
        Mappings = CreateMappings(document);
        TargetFields = CreateTargetFields(TargetNodes, Mappings);
        TargetTree = SchemaTreeDisplayBuilder.BuildTarget(_schemaExplorer.Explore(document.TargetSchema), expressions, diagnostics);

        if (string.IsNullOrWhiteSpace(DslContent))
        {
            DslContent = Session.ExportDsl();
        }
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
        }

        return expressions;
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

        return diagnostics;
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
            DiagnosticsCount = Diagnostics.Count
        };
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
