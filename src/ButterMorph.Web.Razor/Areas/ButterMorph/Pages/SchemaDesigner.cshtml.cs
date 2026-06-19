namespace ButterMorph.Web.Razor;

using ButterMorph.Abstractions;
using ButterMorph.Design;
using ButterMorph.SchemaDesign;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

/// <summary>
/// Displays and edits structure schemas.
/// </summary>
public sealed class SchemaDesignerModel : PageModel
{
    // Stores editable schema sessions.
    private readonly ISchemaDesignSessionStore sessionStore;

    // Creates UI-ready schema trees.
    private readonly ISchemaExplorer schemaExplorer;

    // Reads designer integration options.
    private readonly ButterMorphRazorDesignerOptions options;

    // Provides optional host integration.
    private readonly IEnumerable<IButterMorphSchemaDesignerHost> hosts;

    // Keeps host visibility preferences by context key.
    private static readonly ConcurrentDictionary<string, bool> ManualActionStates = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaDesignerModel"/> class.
    /// </summary>
    /// <param name="sessionStore">The schema design session store.</param>
    /// <param name="schemaExplorer">The schema explorer.</param>
    /// <param name="options">The Razor designer options.</param>
    /// <param name="hosts">The optional host integrations.</param>
    public SchemaDesignerModel(
        ISchemaDesignSessionStore sessionStore,
        ISchemaExplorer schemaExplorer,
        IOptions<ButterMorphRazorDesignerOptions> options,
        IEnumerable<IButterMorphSchemaDesignerHost> hosts)
    {
        this.sessionStore = sessionStore;
        this.schemaExplorer = schemaExplorer;
        this.options = options.Value;
        this.hosts = hosts;
    }

    /// <summary>
    /// Gets or sets the selected node path.
    /// </summary>
    [BindProperty]
    public string SelectedPath { get; set; } = "$root";

    /// <summary>
    /// Gets or sets the parent path used for add operations.
    /// </summary>
    [BindProperty]
    public string ParentPath { get; set; } = "$root";

    /// <summary>
    /// Gets or sets the edited node name.
    /// </summary>
    [BindProperty]
    public string NodeName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the edited node kind.
    /// </summary>
    [BindProperty]
    public string NodeKind { get; set; } = nameof(SchemaNodeKind.Scalar);

    /// <summary>
    /// Gets or sets the edited scalar data type.
    /// </summary>
    [BindProperty]
    public string DataType { get; set; } = "string";

    /// <summary>
    /// Gets or sets a value indicating whether the node is required.
    /// </summary>
    [BindProperty]
    public bool IsRequired { get; set; }

    /// <summary>
    /// Gets or sets the edited metadata key.
    /// </summary>
    [BindProperty]
    public string MetadataKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the edited metadata value.
    /// </summary>
    [BindProperty]
    public string MetadataValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets JSON Schema text used by the JSON view.
    /// </summary>
    [BindProperty]
    public string JsonSchemaText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether manual actions are shown.
    /// </summary>
    public bool ShowManualActions { get; set; } = true;

    /// <summary>
    /// Gets or sets the schema tree root.
    /// </summary>
    public ISchemaTreeNode TreeRoot { get; set; }

    /// <summary>
    /// Gets or sets the selected tree node.
    /// </summary>
    public ISchemaTreeNode SelectedNode { get; set; }

    /// <summary>
    /// Gets or sets the user-facing message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the host save completed.
    /// </summary>
    public bool HostSaveCompleted { get; set; }

    /// <summary>
    /// Gets or sets the safe return URL.
    /// </summary>
    public string SafeReturnUrl { get; set; } = string.Empty;

    /// <summary>
    /// Handles initial schema designer display.
    /// </summary>
    /// <returns>The page result.</returns>
    public async Task<IActionResult> OnGet()
    {
        await ApplyHostPreload();
        RefreshView();

        return Page();
    }

    /// <summary>
    /// Adds a node to the schema.
    /// </summary>
    /// <returns>The page result.</returns>
    public IActionResult OnPostAddNode()
    {
        ISchemaDesignSession session = CurrentSession();
        ISchemaDesignOperationResult result = session.AddNode(ParentPath, NodeName, ParseKind(NodeKind), DataType);
        Message = CreateMessage(result, "Node added.");
        SelectedPath = ParentPath;
        RefreshView();

        return Page();
    }

    /// <summary>
    /// Selects a schema node.
    /// </summary>
    /// <returns>The page result.</returns>
    public IActionResult OnPostSelect()
    {
        RefreshView();

        return Page();
    }

    /// <summary>
    /// Updates the selected node.
    /// </summary>
    /// <returns>The page result.</returns>
    public IActionResult OnPostUpdateNode()
    {
        ISchemaDesignSession session = CurrentSession();
        ISchemaDesignOperationResult result = session.UpdateNode(SelectedPath, NodeName, ParseKind(NodeKind), DataType, IsRequired);

        if (result.Succeeded && !string.IsNullOrWhiteSpace(MetadataValue))
        {
            result = session.SetMetadata(SelectedPath, "description", MetadataValue);
        }

        Message = CreateMessage(result, "Node updated.");
        RefreshView();

        return Page();
    }

    /// <summary>
    /// Removes the selected node.
    /// </summary>
    /// <returns>The page result.</returns>
    public IActionResult OnPostRemoveNode()
    {
        ISchemaDesignSession session = CurrentSession();
        ISchemaDesignOperationResult result = session.RemoveNode(SelectedPath);
        Message = CreateMessage(result, "Node removed.");
        SelectedPath = "$root";
        RefreshView();

        return Page();
    }

    /// <summary>
    /// Sets metadata on the selected node.
    /// </summary>
    /// <returns>The page result.</returns>
    public IActionResult OnPostSetMetadata()
    {
        ISchemaDesignSession session = CurrentSession();
        ISchemaDesignOperationResult result = session.SetMetadata(SelectedPath, MetadataKey, MetadataValue);
        Message = CreateMessage(result, "Metadata updated.");
        RefreshView();

        return Page();
    }

    /// <summary>
    /// Imports JSON Schema text.
    /// </summary>
    /// <returns>The page result.</returns>
    public IActionResult OnPostImportJson()
    {
        ISchemaDesignSession session = CurrentSession();
        ISchemaDesignOperationResult result = session.ImportJsonSchema("Schema", JsonSchemaText);
        Message = CreateMessage(result, "JSON Schema imported.");
        SelectedPath = "$root";
        RefreshView();

        return Page();
    }

    /// <summary>
    /// Saves schema state through the optional host.
    /// </summary>
    /// <returns>The page result.</returns>
    public async Task<IActionResult> OnPostSave()
    {
        ISchemaDesignSession session = CurrentSession();
        JsonSchemaText = session.ExportJsonSchema();
        string contextKey = DesignerSessionKeyResolver.ResolveContextKey(this, options);
        ButterMorphSchemaDesignerSaveResult saveResult = new()
        {
            Succeeded = true,
            Message = "Schema saved."
        };

        foreach (IButterMorphSchemaDesignerHost host in hosts)
        {
            saveResult = await host.Save(new ButterMorphSchemaDesignerSaveRequest
            {
                ContextKey = contextKey,
                Schema = session.Schema,
                JsonSchema = JsonSchemaText
            });
            break;
        }

        Message = saveResult.Message;

        if (saveResult.Succeeded)
        {
            HostSaveCompleted = IsPopupRequest();
            SafeReturnUrl = ResolveSafeReturnUrl();
        }

        RefreshView();

        return Page();
    }

    // Applies host preload when configured.
    private async Task ApplyHostPreload()
    {
        if (!options.UseHostPreload)
        {
            return;
        }

        string contextKey = DesignerSessionKeyResolver.ResolveContextKey(this, options);

        if (string.IsNullOrWhiteSpace(contextKey))
        {
            return;
        }

        foreach (IButterMorphSchemaDesignerHost host in hosts)
        {
            ButterMorphSchemaDesignerLoadResult result = await host.Load(new ButterMorphSchemaDesignerLoadRequest
            {
                ContextKey = contextKey
            });

            ShowManualActions = result.ShowManualActions;
            ManualActionStates[contextKey] = result.ShowManualActions;

            if (result.Schema != null)
            {
                CurrentSession().Load(result.Schema);
            }

            Message = result.Message;
            break;
        }
    }

    // Gets the current schema design session.
    private ISchemaDesignSession CurrentSession()
    {
        return sessionStore.GetOrCreate(DesignerSessionKeyResolver.Resolve(this, options));
    }

    // Refreshes properties used by Razor.
    private void RefreshView()
    {
        ISchemaDesignSession session = CurrentSession();
        ShowManualActions = ResolveManualActions();
        TreeRoot = schemaExplorer.Explore(session.Schema);
        SelectedNode = FindTreeNode(TreeRoot, SelectedPath);

        if (SelectedNode == null)
        {
            SelectedNode = TreeRoot;
            SelectedPath = TreeRoot.Path;
        }

        NodeName = SelectedNode.Name;

        if (SelectedNode.Name == "$item")
        {
            NodeName = "Item";
        }
        NodeKind = SelectedNode.Kind.ToString();
        DataType = SelectedNode.DataType;
        IsRequired = SelectedNode.IsRequired;
        JsonSchemaText = session.ExportJsonSchema();
    }

    // Resolves manual action visibility for the current context.
    private bool ResolveManualActions()
    {
        string contextKey = DesignerSessionKeyResolver.ResolveContextKey(this, options);

        if (string.IsNullOrWhiteSpace(contextKey))
        {
            return options.ShowSchemaActions;
        }

        if (ManualActionStates.TryGetValue(contextKey, out bool showManualActions))
        {
            return showManualActions;
        }

        return options.ShowSchemaActions;
    }

    // Finds a tree node by path.
    private static ISchemaTreeNode FindTreeNode(ISchemaTreeNode node, string path)
    {
        if (string.Equals(node.Path, path, StringComparison.Ordinal))
        {
            return node;
        }

        foreach (ISchemaTreeNode child in node.Children)
        {
            ISchemaTreeNode result = FindTreeNode(child, path);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    // Parses a schema node kind.
    private static SchemaNodeKind ParseKind(string kind)
    {
        if (Enum.TryParse(kind, out SchemaNodeKind parsed))
        {
            return parsed;
        }

        return SchemaNodeKind.Scalar;
    }

    // Creates a compact operation message.
    private static string CreateMessage(ISchemaDesignOperationResult result, string successMessage)
    {
        if (result.Succeeded)
        {
            return successMessage;
        }

        foreach (DiagnosticEntry diagnostic in result.Diagnostics)
        {
            return diagnostic.Message;
        }

        return "Schema operation failed.";
    }

    // Detects popup mode.
    private bool IsPopupRequest()
    {
        return Request.Query.TryGetValue(options.PopupQueryParameter, out Microsoft.Extensions.Primitives.StringValues values)
            && values.Any(value => string.Equals(value, "true", StringComparison.OrdinalIgnoreCase));
    }

    // Resolves a local return URL.
    private string ResolveSafeReturnUrl()
    {
        if (!Request.Query.TryGetValue(options.ReturnUrlQueryParameter, out Microsoft.Extensions.Primitives.StringValues values))
        {
            return string.Empty;
        }

        foreach (string value in values)
        {
            if (!string.IsNullOrWhiteSpace(value) && Url.IsLocalUrl(value))
            {
                return value;
            }
        }

        return string.Empty;
    }
}
