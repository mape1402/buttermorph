namespace ButterMorph.Web.Razor;

using ButterMorph.Abstractions;
using ButterMorph.Design;
using ButterMorph.SchemaDesign;
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

    // Coordinates isolated workspaces and their lifecycle.
    private readonly IButterMorphDesignerSessionManager designerSessions;

    // Stores web-only state for schema designer workspaces.
    private readonly DesignerContextStateStore contextStates;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaDesignerModel"/> class.
    /// </summary>
    /// <param name="sessionStore">The schema design session store.</param>
    /// <param name="schemaExplorer">The schema explorer.</param>
    /// <param name="options">The Razor designer options.</param>
    /// <param name="hosts">The optional host integrations.</param>
    /// <param name="designerSessions">The designer lifecycle coordinator.</param>
    /// <param name="contextStates">The web-only context state store.</param>
    public SchemaDesignerModel(
        ISchemaDesignSessionStore sessionStore,
        ISchemaExplorer schemaExplorer,
        IOptions<ButterMorphRazorDesignerOptions> options,
        IEnumerable<IButterMorphSchemaDesignerHost> hosts,
        IButterMorphDesignerSessionManager designerSessions,
        DesignerContextStateStore contextStates)
    {
        this.sessionStore = sessionStore;
        this.schemaExplorer = schemaExplorer;
        this.options = options.Value;
        this.hosts = hosts;
        this.designerSessions = designerSessions;
        this.contextStates = contextStates;
    }

    /// <summary>
    /// Gets the host-configured designer theme style.
    /// </summary>
    public string ThemeStyle => this.BuildThemeStyle(options);

    /// <summary>
    /// Gets the host-configured designer theme mode.
    /// </summary>
    public string ThemeMode => this.ResolveThemeModeName(options);

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
        if (DesignerSessionKeyResolver.TryCreateEditorSessionRedirect(this, options, out string sessionUrl))
        {
            return Redirect(sessionUrl);
        }

        designerSessions.Acquire(SessionHandle);
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
        MarkDirtyWhenSucceeded(result);
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

        MarkDirtyWhenSucceeded(result);

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
        MarkDirtyWhenSucceeded(result);
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
        MarkDirtyWhenSucceeded(result);
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
        MarkDirtyWhenSucceeded(result);
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
        string contextKey = ContextKey;
        ButterMorphDesignerSessionSnapshot snapshot = designerSessions.GetSnapshot(SessionHandle);
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
                JsonSchema = JsonSchemaText,
                SessionId = SessionHandle.SessionId,
                ExpectedSchemaRevision = snapshot?.SchemaRevision ?? string.Empty
            });
            break;
        }

        Message = saveResult.Message;

        if (saveResult.Succeeded)
        {
            designerSessions.MarkSaved(SessionHandle, saveResult.SchemaRevision, saveResult.SchemaRevision);
            HostSaveCompleted = IsPopupRequest();
            SafeReturnUrl = ResolveSafeReturnUrl();
        }
        else
        {
            if (saveResult.IsConflict && !string.IsNullOrWhiteSpace(saveResult.SchemaRevision))
            {
                designerSessions.ObserveHostSnapshot(SessionHandle, saveResult.SchemaRevision, saveResult.SchemaRevision);
            }

            if (saveResult.IsConflict && string.IsNullOrWhiteSpace(saveResult.Message))
            {
                Message = "Schema could not be saved because the host document changed. Reopen the context to compare the current schema.";
            }
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

        string contextKey = ContextKey;

        if (string.IsNullOrWhiteSpace(contextKey))
        {
            return;
        }

        foreach (IButterMorphSchemaDesignerHost host in hosts)
        {
            ButterMorphSchemaDesignerLoadResult result = await host.Load(new ButterMorphSchemaDesignerLoadRequest
            {
                ContextKey = contextKey,
                SessionId = SessionHandle.SessionId
            });

            ShowManualActions = result.ShowManualActions;
            ContextState.ShowSchemaActions = result.ShowManualActions;

            ButterMorphDesignerSessionRefreshResult refresh = designerSessions.ObserveHostSnapshot(SessionHandle, result.SchemaRevision, result.SchemaRevision);
            if (result.Schema != null && (refresh.Disposition is ButterMorphDesignerSessionRefreshDisposition.Initial or ButterMorphDesignerSessionRefreshDisposition.Reload))
            {
                CurrentSession().Load(result.Schema);
            }

            if (refresh.Disposition == ButterMorphDesignerSessionRefreshDisposition.PreserveLocalChanges)
            {
                Message = "The host schema changed. Your unsaved edits were kept; reopen after saving to load the remote version.";
            }
            else
            {
                Message = result.Message;
            }
            break;
        }
    }

    // Gets the current schema design session.
    private ISchemaDesignSession CurrentSession()
    {
        designerSessions.Acquire(SessionHandle);
        return sessionStore.GetOrCreate(SessionHandle.StorageKey);
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
        return ContextState.ShowSchemaActions;
    }

    /// <summary>
    /// Marks the browser workspace as active without mutating its schema.
    /// </summary>
    /// <returns>An empty response.</returns>
    public IActionResult OnPostTouchSession()
    {
        designerSessions.Touch(SessionHandle);
        return new EmptyResult();
    }

    /// <summary>
    /// Closes the browser workspace and releases all server-side state.
    /// </summary>
    /// <returns>An empty response.</returns>
    public IActionResult OnPostCloseSession()
    {
        designerSessions.Close(SessionHandle);
        return new EmptyResult();
    }

    private ButterMorphDesignerSessionHandle SessionHandle => new()
    {
        Kind = ButterMorphDesignerSessionKind.Schema,
        ContextKey = ContextKey,
        SessionId = DesignerSessionKeyResolver.ResolveEditorSessionId(this, options)
    };

    private string ContextKey => DesignerSessionKeyResolver.ResolveContextKey(this, options);

    private DesignerContextState ContextState => contextStates.GetOrCreate(SessionHandle.StorageKey, options);

    private void MarkDirtyWhenSucceeded(ISchemaDesignOperationResult result)
    {
        if (result.Succeeded)
        {
            designerSessions.MarkDirty(SessionHandle);
        }
    }

    /// <summary>
    /// Builds a form action that preserves the host context and isolated editor session.
    /// </summary>
    /// <param name="handler">The Razor Page handler.</param>
    /// <returns>The handler URL.</returns>
    public string BuildHandlerUrl(string handler)
    {
        return Request.PathBase + Request.Path + Request.QueryString.Add("handler", handler);
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
