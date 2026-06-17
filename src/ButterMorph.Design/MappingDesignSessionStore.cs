namespace ButterMorph.Design;

/// <summary>
/// Stores reusable mapping design sessions by key.
/// </summary>
public sealed class MappingDesignSessionStore : IMappingDesignSessionStore
{
    // Creates sessions when a key has not been seen.
    private readonly IMappingDesignSessionFactory _factory;

    // Stores sessions for the reusable web designer.
    private readonly Dictionary<string, IMappingDesignSession> _sessions = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="MappingDesignSessionStore"/> class.
    /// </summary>
    /// <param name="factory">The session factory.</param>
    public MappingDesignSessionStore(IMappingDesignSessionFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Gets an existing session or creates a new one.
    /// </summary>
    /// <param name="key">The session key.</param>
    /// <returns>The mapping design session.</returns>
    public IMappingDesignSession GetOrCreate(string key)
    {
        if (_sessions.TryGetValue(key, out IMappingDesignSession session))
        {
            return session;
        }

        session = _factory.Create();
        _sessions[key] = session;

        return session;
    }
}
