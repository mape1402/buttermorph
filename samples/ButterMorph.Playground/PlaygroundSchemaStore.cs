using System.Collections.Concurrent;

/// <summary>
/// Stores playground schema saves in memory.
/// </summary>
internal sealed class PlaygroundSchemaStore
{
    // Keeps schema saves by context key.
    private readonly ConcurrentDictionary<string, PlaygroundSchemaSave> saves = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Saves a schema result.
    /// </summary>
    /// <param name="save">The schema save.</param>
    public void Save(PlaygroundSchemaSave save)
    {
        saves[save.ContextKey] = save;
    }

    /// <summary>
    /// Saves schema item state received from the browser.
    /// </summary>
    /// <param name="item">The browser schema item.</param>
    public void SaveClientItem(PlaygroundSchemaClientItem item)
    {
        saves[item.ContextKey] = new PlaygroundSchemaSave
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
            Key = item.Key,
            DataType = item.DataType,
            AppliesToJson = item.AppliesToJson,
            ValidationJson = item.ValidationJson
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
}
