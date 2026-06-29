namespace ButterMorph.StudioPlayground.Services;

using System.Collections.Concurrent;
using ButterMorph.StudioPlayground.Models;

/// <summary>
/// Stores host-owned Studio Playground state in memory.
/// </summary>
internal sealed class StudioStore
{
    // Concurrent dictionaries simulate host persistence for the sample.
    private readonly ConcurrentDictionary<string, StudioCustomType> customTypes = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, StudioCustomField> customFields = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, StudioSchema> schemas = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, StudioMapping> mappings = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets all custom types.
    /// </summary>
    public IReadOnlyCollection<StudioCustomType> CustomTypes => customTypes.Values.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToArray();

    /// <summary>
    /// Gets all custom fields.
    /// </summary>
    public IReadOnlyCollection<StudioCustomField> CustomFields => customFields.Values.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToArray();

    /// <summary>
    /// Gets all schemas.
    /// </summary>
    public IReadOnlyCollection<StudioSchema> Schemas => schemas.Values.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToArray();

    /// <summary>
    /// Gets all mappings.
    /// </summary>
    public IReadOnlyCollection<StudioMapping> Mappings => mappings.Values.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToArray();

    /// <summary>
    /// Saves a custom type.
    /// </summary>
    /// <param name="item">The custom type.</param>
    public void SaveCustomType(StudioCustomType item)
    {
        item.SavedAt = DateTimeOffset.UtcNow;
        customTypes[item.ContextKey] = item;
    }

    /// <summary>
    /// Saves a custom field.
    /// </summary>
    /// <param name="item">The custom field.</param>
    public void SaveCustomField(StudioCustomField item)
    {
        item.SavedAt = DateTimeOffset.UtcNow;
        customFields[item.ContextKey] = item;
    }

    /// <summary>
    /// Saves a schema.
    /// </summary>
    /// <param name="item">The schema.</param>
    public void SaveSchema(StudioSchema item)
    {
        item.SavedAt = DateTimeOffset.UtcNow;
        schemas[item.ContextKey] = item;
    }

    /// <summary>
    /// Saves a mapping.
    /// </summary>
    /// <param name="item">The mapping.</param>
    public void SaveMapping(StudioMapping item)
    {
        item.SavedAt = DateTimeOffset.UtcNow;
        mappings[item.ContextKey] = item;
    }

    /// <summary>
    /// Attempts to get a custom type.
    /// </summary>
    /// <param name="contextKey">The context key.</param>
    /// <param name="item">The custom type.</param>
    /// <returns>True when found.</returns>
    public bool TryGetCustomType(string contextKey, out StudioCustomType item)
    {
        return customTypes.TryGetValue(contextKey, out item);
    }

    /// <summary>
    /// Attempts to get a custom field.
    /// </summary>
    /// <param name="contextKey">The context key.</param>
    /// <param name="item">The custom field.</param>
    /// <returns>True when found.</returns>
    public bool TryGetCustomField(string contextKey, out StudioCustomField item)
    {
        return customFields.TryGetValue(contextKey, out item);
    }

    /// <summary>
    /// Attempts to get a schema.
    /// </summary>
    /// <param name="contextKey">The context key.</param>
    /// <param name="item">The schema.</param>
    /// <returns>True when found.</returns>
    public bool TryGetSchema(string contextKey, out StudioSchema item)
    {
        return schemas.TryGetValue(contextKey, out item);
    }

    /// <summary>
    /// Attempts to get a mapping.
    /// </summary>
    /// <param name="contextKey">The context key.</param>
    /// <param name="item">The mapping.</param>
    /// <returns>True when found.</returns>
    public bool TryGetMapping(string contextKey, out StudioMapping item)
    {
        return mappings.TryGetValue(contextKey, out item);
    }

    /// <summary>
    /// Deletes one item by kind and key.
    /// </summary>
    /// <param name="kind">The item kind.</param>
    /// <param name="contextKey">The context key.</param>
    /// <returns>True when removed.</returns>
    public bool Delete(string kind, string contextKey)
    {
        return kind switch
        {
            "customTypes" => customTypes.TryRemove(contextKey, out StudioCustomType removedType),
            "customFields" => customFields.TryRemove(contextKey, out StudioCustomField removedField),
            "schemas" => schemas.TryRemove(contextKey, out StudioSchema removedSchema),
            "mappings" => mappings.TryRemove(contextKey, out StudioMapping removedMapping),
            _ => false
        };
    }
}
