namespace ButterMorph.Web.Razor;

using ButterMorph.Abstractions;
using ButterMorph.Design;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="DesignerModel"/> class.
    /// </summary>
    /// <param name="sessionStore">The session store.</param>
    /// <param name="schemaExplorer">The schema explorer.</param>
    /// <param name="dslExporter">The DSL exporter.</param>
    public DesignerModel(IMappingDesignSessionStore sessionStore, ISchemaExplorer schemaExplorer, IDslExporter dslExporter)
    {
        _sessionStore = sessionStore;
        _schemaExplorer = schemaExplorer;
        _dslExporter = dslExporter;
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
    /// Gets the source schema nodes.
    /// </summary>
    public IReadOnlyCollection<SchemaTreeDisplayNode> SourceNodes { get; private set; } = [];

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
    /// Gets semantic diagnostics.
    /// </summary>
    public IReadOnlyCollection<DiagnosticEntry> Diagnostics { get; private set; } = [];

    /// <summary>
    /// Gets or sets the status message.
    /// </summary>
    public string Message { get; set; } = "Load schemas, add mappings and analyze the document.";

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
    /// Adds a sample path mapping.
    /// </summary>
    /// <returns>The page result.</returns>
    public IActionResult OnPostSampleMapping()
    {
        SourcePath = "$source.Customer.Name";
        TargetPath = "Customer.Name";
        IMappingOperationResult result = Session.AddPathMapping(SourcePath, TargetPath);
        Message = CreateMessage(result, "Sample mapping added.");
        LoadViewState();

        return Page();
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
        List<DiagnosticEntry> diagnostics = [];
        int count = Math.Min(TargetPaths.Count, Expressions.Count);
        int savedCount = 0;

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

            if (result.Succeeded)
            {
                savedCount++;
                continue;
            }

            diagnostics.AddRange(result.Diagnostics);
        }

        Diagnostics = diagnostics;
        if (diagnostics.Count == 0)
        {
            Message = "Target mappings saved for " + savedCount.ToString(System.Globalization.CultureInfo.InvariantCulture) + " fields.";
        }
        else
        {
            Message = "Some target mappings could not be saved.";
        }

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
        Message = "Semantic analysis completed.";
        LoadViewState();

        return Page();
    }

    // Gets the current design session.
    private IMappingDesignSession Session => _sessionStore.GetOrCreate(DesignerSessionKeys.DefaultSessionKey);

    // Loads UI state from the current session.
    private void LoadViewState()
    {
        ITransformationDocument document = Session.Document;
        List<SchemaTreeDisplayNode> sourceNodes = [];

        foreach (KeyValuePair<string, IStructureSchema> schemaPair in document.SourceSchemas)
        {
            sourceNodes.AddRange(CreateSourceNodes(schemaPair.Key, SchemaTreeFlattener.Flatten(_schemaExplorer.Explore(schemaPair.Value))));
        }

        SourceNodes = sourceNodes;
        TargetNodes = SchemaTreeFlattener.Flatten(_schemaExplorer.Explore(document.TargetSchema));
        Mappings = CreateMappings(document);
        TargetFields = CreateTargetFields(TargetNodes, Mappings);
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
        string normalizedPath = targetPath.Replace("[0]", string.Empty, StringComparison.Ordinal);
        return "$source." + normalizedPath;
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
}
