using System.Collections.Concurrent;

/// <summary>
/// Stores playground schema saves in memory.
/// </summary>
internal sealed class PlaygroundSchemaStore
{
    // Keeps schema saves by context key.
    private readonly ConcurrentDictionary<string, PlaygroundSchemaSave> saves = new(StringComparer.OrdinalIgnoreCase);

    // Keeps browser draft state used only to preload designer popups.
    private readonly ConcurrentDictionary<string, PlaygroundSchemaSave> drafts = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Saves a schema result.
    /// </summary>
    /// <param name="save">The schema save.</param>
    public void Save(PlaygroundSchemaSave save)
    {
        saves[save.ContextKey] = save;
        drafts.TryRemove(save.ContextKey, out PlaygroundSchemaSave removedDraft);
    }

    /// <summary>
    /// Saves schema item draft state received from the browser.
    /// </summary>
    /// <param name="item">The browser schema item.</param>
    public void SaveClientItem(PlaygroundSchemaClientItem item)
    {
        drafts[item.ContextKey] = new PlaygroundSchemaSave
        {
            ContextKey = item.ContextKey,
            Kind = item.Kind,
            DisplayName = item.DisplayName,
            Description = item.Description,
            DesignerPath = item.DesignerPath,
            JsonSchema = item.JsonSchema,
            SavedAt = item.SavedAt,
            VersionNumber = item.VersionNumber,
            BaseType = item.BaseType,
            Comment = item.Comment,
            VersionComment = item.VersionComment,
            MetadataJson = item.MetadataJson,
            Key = item.Key,
            DataType = item.DataType,
            AppliesToJson = item.AppliesToJson,
            ValidationJson = item.ValidationJson,
            IsRequired = item.IsRequired,
            IsActive = item.IsActive,
            SortOrder = item.SortOrder,
            ChildrenDefinitionJson = item.ChildrenDefinitionJson,
            ArrayItemDataType = item.ArrayItemDataType,
            ArrayItemDefinitionJson = item.ArrayItemDefinitionJson
        };
    }

    /// <summary>
    /// Attempts to get a saved schema.
    /// </summary>
    /// <param name="contextKey">The context key.</param>
    /// <param name="save">The schema save.</param>
    /// <returns><see langword="true"/> when a save exists.</returns>
    public bool TryGet(string contextKey, out PlaygroundSchemaSave save)
    {
        return saves.TryGetValue(contextKey, out save);
    }

    /// <summary>
    /// Attempts to get a schema draft.
    /// </summary>
    /// <param name="contextKey">The context key.</param>
    /// <param name="save">The schema draft.</param>
    /// <returns><see langword="true"/> when a draft exists.</returns>
    public bool TryGetDraft(string contextKey, out PlaygroundSchemaSave save)
    {
        return drafts.TryGetValue(contextKey, out save);
    }

    /// <summary>
    /// Lists saved schema tool results.
    /// </summary>
    /// <returns>The saved schema results.</returns>
    public IReadOnlyCollection<PlaygroundSchemaSave> ListSaves()
    {
        return saves.Values.ToArray();
    }

    /// <summary>
    /// Lists saved and draft schema tool results for host catalog injection.
    /// </summary>
    /// <returns>The saved and draft schema results.</returns>
    public IReadOnlyCollection<PlaygroundSchemaSave> ListDesignStates()
    {
        Dictionary<string, PlaygroundSchemaSave> states = new(StringComparer.OrdinalIgnoreCase);
        foreach (PlaygroundSchemaSave save in saves.Values)
        {
            states[save.ContextKey] = save;
        }

        foreach (PlaygroundSchemaSave draft in drafts.Values)
        {
            states[draft.ContextKey] = draft;
        }

        return states.Values.ToArray();
    }
}
