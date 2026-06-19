namespace ButterMorph.SchemaDesign;

using System.Collections.Concurrent;
using ButterMorph.Json.Schema;

/// <summary>
/// Stores schema design sessions by key.
/// </summary>
public sealed class SchemaDesignSessionStore : ISchemaDesignSessionStore
{
    // Keeps in-memory sessions for host and playground usage.
    private readonly ConcurrentDictionary<string, ISchemaDesignSession> sessions = new();

    // Imports JSON Schema into sessions.
    private readonly IJsonSchemaImporter importer;

    // Exports JSON Schema from sessions.
    private readonly IJsonSchemaExporter exporter;

    /// <summary>
    /// Initializes a new instance of the <see cref="SchemaDesignSessionStore"/> class.
    /// </summary>
    /// <param name="importer">The JSON Schema importer.</param>
    /// <param name="exporter">The JSON Schema exporter.</param>
    public SchemaDesignSessionStore(IJsonSchemaImporter importer, IJsonSchemaExporter exporter)
    {
        this.importer = importer;
        this.exporter = exporter;
    }

    /// <summary>
    /// Gets an existing session or creates one.
    /// </summary>
    /// <param name="key">The session key.</param>
    /// <returns>The schema design session.</returns>
    public ISchemaDesignSession GetOrCreate(string key)
    {
        return sessions.GetOrAdd(key, _ => new SchemaDesignSession(importer, exporter));
    }
}
