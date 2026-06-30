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
        customTypes[item.Id] = item;
    }

    /// <summary>
    /// Saves a custom field.
    /// </summary>
    /// <param name="item">The custom field.</param>
    public void SaveCustomField(StudioCustomField item)
    {
        item.SavedAt = DateTimeOffset.UtcNow;
        customFields[item.Id] = item;
    }

    /// <summary>
    /// Saves a schema.
    /// </summary>
    /// <param name="item">The schema.</param>
    public void SaveSchema(StudioSchema item)
    {
        item.SavedAt = DateTimeOffset.UtcNow;
        schemas[item.Id] = item;
    }

    /// <summary>
    /// Saves a mapping.
    /// </summary>
    /// <param name="item">The mapping.</param>
    public void SaveMapping(StudioMapping item)
    {
        item.SavedAt = DateTimeOffset.UtcNow;
        mappings[item.Id] = item;
    }

    /// <summary>
    /// Attempts to get a custom type.
    /// </summary>
    /// <param name="id">The host-owned id.</param>
    /// <param name="item">The custom type.</param>
    /// <returns>True when found.</returns>
    public bool TryGetCustomType(string id, out StudioCustomType item)
    {
        return customTypes.TryGetValue(id, out item);
    }

    /// <summary>
    /// Attempts to get a custom field.
    /// </summary>
    /// <param name="id">The host-owned id.</param>
    /// <param name="item">The custom field.</param>
    /// <returns>True when found.</returns>
    public bool TryGetCustomField(string id, out StudioCustomField item)
    {
        return customFields.TryGetValue(id, out item);
    }

    /// <summary>
    /// Attempts to get a schema.
    /// </summary>
    /// <param name="id">The host-owned id.</param>
    /// <param name="item">The schema.</param>
    /// <returns>True when found.</returns>
    public bool TryGetSchema(string id, out StudioSchema item)
    {
        return schemas.TryGetValue(id, out item);
    }

    /// <summary>
    /// Attempts to get a mapping.
    /// </summary>
    /// <param name="id">The host-owned id.</param>
    /// <param name="item">The mapping.</param>
    /// <returns>True when found.</returns>
    public bool TryGetMapping(string id, out StudioMapping item)
    {
        return mappings.TryGetValue(id, out item);
    }

    /// <summary>
    /// Deletes one item by kind and key.
    /// </summary>
    /// <param name="kind">The item kind.</param>
    /// <param name="id">The host-owned id.</param>
    /// <returns>True when removed.</returns>
    public bool Delete(string kind, string id)
    {
        return kind switch
        {
            "customTypes" => customTypes.TryRemove(id, out StudioCustomType removedType),
            "customFields" => customFields.TryRemove(id, out StudioCustomField removedField),
            "schemas" => schemas.TryRemove(id, out StudioSchema removedSchema),
            "mappings" => mappings.TryRemove(id, out StudioMapping removedMapping),
            _ => false
        };
    }
}

