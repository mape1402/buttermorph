namespace ButterMorph.Web.Razor;

using ButterMorph.Design;
using ButterMorph.SchemaDesign;
using Microsoft.Extensions.Options;

// Default single-process workspace coordinator. Distributed hosts deliberately replace this boundary
// with their own shared coordination strategy rather than accidentally sharing mutable sessions.
internal sealed class ButterMorphDesignerSessionManager : IButterMorphDesignerSessionManager
{
    private readonly object gate = new();
    private readonly Dictionary<string, SessionEntry> entries = new(StringComparer.Ordinal);
    private readonly ButterMorphDesignerSessionOptions options;
    private readonly IMappingDesignSessionStore mappingSessions;
    private readonly IValidationDesignSessionStore validationSessions;
    private readonly ISchemaDesignSessionStore schemaSessions;
    private readonly DesignerContextStateStore contextStates;

    public ButterMorphDesignerSessionManager(
        IOptions<ButterMorphRazorDesignerOptions> options,
        IMappingDesignSessionStore mappingSessions,
        IValidationDesignSessionStore validationSessions,
        ISchemaDesignSessionStore schemaSessions,
        DesignerContextStateStore contextStates)
    {
        this.options = options.Value.Sessions;
        this.mappingSessions = mappingSessions;
        this.validationSessions = validationSessions;
        this.schemaSessions = schemaSessions;
        this.contextStates = contextStates;
    }

    public ButterMorphDesignerSessionSnapshot Acquire(ButterMorphDesignerSessionHandle handle)
    {
        Validate(handle);
        List<ButterMorphDesignerSessionHandle> released = [];
        SessionEntry entry;
        DateTimeOffset now = DateTimeOffset.UtcNow;

        lock (gate)
        {
            CollectExpired(now, released);

            if (!entries.TryGetValue(handle.StorageKey, out entry))
            {
                EnsureCapacity(handle, released);
                entry = new SessionEntry(Clone(handle), now);
                entries.Add(handle.StorageKey, entry);
            }

            entry.IsActive = true;
            entry.LastActivityUtc = now;
        }

        ReleaseResources(released);
        return CreateSnapshot(entry);
    }

    public void Touch(ButterMorphDesignerSessionHandle handle)
    {
        lock (gate)
        {
            if (entries.TryGetValue(handle.StorageKey, out SessionEntry entry))
            {
                entry.IsActive = true;
                entry.LastActivityUtc = DateTimeOffset.UtcNow;
            }
        }
    }

    public void MarkDirty(ButterMorphDesignerSessionHandle handle)
    {
        lock (gate)
        {
            if (entries.TryGetValue(handle.StorageKey, out SessionEntry entry))
            {
                entry.IsDirty = true;
                entry.IsActive = true;
                entry.LastActivityUtc = DateTimeOffset.UtcNow;
            }
        }
    }

    public void MarkSaved(ButterMorphDesignerSessionHandle handle, string schemaRevision, string designRevision)
    {
        lock (gate)
        {
            if (!entries.TryGetValue(handle.StorageKey, out SessionEntry entry))
            {
                return;
            }

            entry.SchemaRevision = PreferIncomingRevision(schemaRevision, entry.ObservedSchemaRevision, entry.SchemaRevision);
            entry.DesignRevision = PreferIncomingRevision(designRevision, entry.ObservedDesignRevision, entry.DesignRevision);
            entry.ObservedSchemaRevision = entry.SchemaRevision;
            entry.ObservedDesignRevision = entry.DesignRevision;
            entry.IsDirty = false;
            entry.IsInvalidated = false;
            entry.HasPendingRemoteChanges = false;
            entry.LastActivityUtc = DateTimeOffset.UtcNow;
        }
    }

    public ButterMorphDesignerSessionRefreshResult ObserveHostSnapshot(ButterMorphDesignerSessionHandle handle, string schemaRevision, string designRevision)
    {
        lock (gate)
        {
            if (!entries.TryGetValue(handle.StorageKey, out SessionEntry entry))
            {
                throw new InvalidOperationException("The designer session was not acquired.");
            }

            bool initial = !entry.HasHostSnapshot;
            bool changed = !initial && (entry.IsInvalidated || HaveRevisionsChanged(entry.SchemaRevision, schemaRevision) || HaveRevisionsChanged(entry.DesignRevision, designRevision));
            entry.ObservedSchemaRevision = schemaRevision ?? string.Empty;
            entry.ObservedDesignRevision = designRevision ?? string.Empty;
            entry.HasHostSnapshot = true;
            entry.LastActivityUtc = DateTimeOffset.UtcNow;

            if (initial)
            {
                entry.SchemaRevision = schemaRevision ?? string.Empty;
                entry.DesignRevision = designRevision ?? string.Empty;
                entry.IsInvalidated = false;
                return new ButterMorphDesignerSessionRefreshResult { Disposition = ButterMorphDesignerSessionRefreshDisposition.Initial, HasHostChanges = true };
            }

            if (!changed)
            {
                return new ButterMorphDesignerSessionRefreshResult { Disposition = ButterMorphDesignerSessionRefreshDisposition.Unchanged };
            }

            if (entry.IsDirty)
            {
                entry.HasPendingRemoteChanges = true;
                return new ButterMorphDesignerSessionRefreshResult { Disposition = ButterMorphDesignerSessionRefreshDisposition.PreserveLocalChanges, HasHostChanges = true };
            }

            entry.SchemaRevision = schemaRevision ?? string.Empty;
            entry.DesignRevision = designRevision ?? string.Empty;
            entry.IsInvalidated = false;
            entry.HasPendingRemoteChanges = false;
            return new ButterMorphDesignerSessionRefreshResult { Disposition = ButterMorphDesignerSessionRefreshDisposition.Reload, HasHostChanges = true };
        }
    }

    public int InvalidateContext(string contextKey, ButterMorphDesignerSessionKind? kind = null)
    {
        if (string.IsNullOrWhiteSpace(contextKey))
        {
            return 0;
        }

        int count = 0;
        lock (gate)
        {
            foreach (SessionEntry entry in entries.Values)
            {
                if (!string.Equals(entry.Handle.ContextKey, contextKey, StringComparison.Ordinal) || (kind.HasValue && entry.Handle.Kind != kind.Value))
                {
                    continue;
                }

                entry.IsInvalidated = true;
                count++;
            }
        }

        return count;
    }

    public bool Close(ButterMorphDesignerSessionHandle handle)
    {
        SessionEntry entry = null;
        lock (gate)
        {
            if (entries.Remove(handle.StorageKey, out entry))
            {
                entry.IsActive = false;
            }
        }

        if (entry == null)
        {
            return false;
        }

        ReleaseResources([entry.Handle]);
        return true;
    }

    public void Deactivate(ButterMorphDesignerSessionHandle handle)
    {
        lock (gate)
        {
            if (entries.TryGetValue(handle.StorageKey, out SessionEntry entry))
            {
                entry.IsActive = false;
                entry.LastActivityUtc = DateTimeOffset.UtcNow;
            }
        }
    }

    public int SweepExpired(DateTimeOffset? utcNow = null)
    {
        List<ButterMorphDesignerSessionHandle> released = [];
        lock (gate)
        {
            CollectExpired(utcNow ?? DateTimeOffset.UtcNow, released);
        }

        ReleaseResources(released);
        return released.Count;
    }

    public ButterMorphDesignerSessionSnapshot GetSnapshot(ButterMorphDesignerSessionHandle handle)
    {
        lock (gate)
        {
            return entries.TryGetValue(handle.StorageKey, out SessionEntry entry)
                ? CreateSnapshot(entry)
                : null;
        }
    }

    private void EnsureCapacity(ButterMorphDesignerSessionHandle incoming, List<ButterMorphDesignerSessionHandle> released)
    {
        int maxSessions = Math.Max(1, options.MaximumSessions);
        int maxPerContext = Math.Max(1, options.MaximumSessionsPerContext);
        while (entries.Count >= maxSessions || CountForContext(incoming) >= maxPerContext)
        {
            SessionEntry candidate = entries.Values
                .Where(entry => !entry.IsActive)
                .OrderBy(entry => entry.LastActivityUtc)
                .FirstOrDefault();

            if (candidate == null)
            {
                throw new InvalidOperationException("The configured ButterMorph designer session limit was reached by active sessions. Close an editor or increase the session limits.");
            }

            entries.Remove(candidate.Handle.StorageKey);
            released.Add(candidate.Handle);
        }
    }

    private int CountForContext(ButterMorphDesignerSessionHandle handle)
    {
        return entries.Values.Count(entry => entry.Handle.Kind == handle.Kind && string.Equals(entry.Handle.ContextKey, handle.ContextKey, StringComparison.Ordinal));
    }

    private void CollectExpired(DateTimeOffset now, List<ButterMorphDesignerSessionHandle> released)
    {
        TimeSpan inactiveTimeout = NormalizeTimeout(options.InactiveTimeout, TimeSpan.FromMinutes(20));
        TimeSpan activeTimeout = NormalizeTimeout(options.ActiveTimeout, TimeSpan.FromHours(8));
        foreach (SessionEntry entry in entries.Values.ToArray())
        {
            TimeSpan timeout = entry.IsActive ? activeTimeout : inactiveTimeout;
            if (now - entry.LastActivityUtc < timeout)
            {
                continue;
            }

            entries.Remove(entry.Handle.StorageKey);
            released.Add(entry.Handle);
        }
    }

    private void ReleaseResources(IEnumerable<ButterMorphDesignerSessionHandle> handles)
    {
        foreach (ButterMorphDesignerSessionHandle handle in handles)
        {
            switch (handle.Kind)
            {
                case ButterMorphDesignerSessionKind.Mapping:
                    mappingSessions.Remove(handle.StorageKey);
                    break;
                case ButterMorphDesignerSessionKind.Validation:
                    validationSessions.Remove(handle.StorageKey);
                    break;
                case ButterMorphDesignerSessionKind.Schema:
                    schemaSessions.Remove(handle.StorageKey);
                    break;
            }

            contextStates.Remove(handle.StorageKey);
        }
    }

    private static bool HaveRevisionsChanged(string baseline, string incoming)
    {
        if (string.IsNullOrWhiteSpace(baseline) || string.IsNullOrWhiteSpace(incoming))
        {
            // Existing hosts without revisions are refreshed on every reopen.
            return true;
        }

        return !string.Equals(baseline, incoming, StringComparison.Ordinal);
    }

    private static string PreferIncomingRevision(string incoming, string observed, string fallback)
    {
        if (!string.IsNullOrWhiteSpace(incoming))
        {
            return incoming;
        }

        if (!string.IsNullOrWhiteSpace(observed))
        {
            return observed;
        }

        return fallback ?? string.Empty;
    }

    private static TimeSpan NormalizeTimeout(TimeSpan configured, TimeSpan fallback)
    {
        return configured > TimeSpan.Zero ? configured : fallback;
    }

    private static void Validate(ButterMorphDesignerSessionHandle handle)
    {
        ArgumentNullException.ThrowIfNull(handle);
        if (string.IsNullOrWhiteSpace(handle.SessionId))
        {
            throw new ArgumentException("A designer session id is required.", nameof(handle));
        }
    }

    private static ButterMorphDesignerSessionSnapshot CreateSnapshot(SessionEntry entry)
    {
        return new ButterMorphDesignerSessionSnapshot
        {
            Handle = Clone(entry.Handle),
            IsDirty = entry.IsDirty,
            IsInvalidated = entry.IsInvalidated,
            HasPendingRemoteChanges = entry.HasPendingRemoteChanges,
            SchemaRevision = entry.SchemaRevision,
            DesignRevision = entry.DesignRevision,
            LastActivityUtc = entry.LastActivityUtc
        };
    }

    private static ButterMorphDesignerSessionHandle Clone(ButterMorphDesignerSessionHandle handle)
    {
        return new ButterMorphDesignerSessionHandle
        {
            Kind = handle.Kind,
            ContextKey = handle.ContextKey ?? string.Empty,
            SessionId = handle.SessionId ?? string.Empty
        };
    }

    private sealed class SessionEntry
    {
        internal SessionEntry(ButterMorphDesignerSessionHandle handle, DateTimeOffset now)
        {
            Handle = handle;
            LastActivityUtc = now;
        }

        internal ButterMorphDesignerSessionHandle Handle { get; }
        internal DateTimeOffset LastActivityUtc { get; set; }
        internal bool IsActive { get; set; }
        internal bool IsDirty { get; set; }
        internal bool IsInvalidated { get; set; }
        internal bool HasPendingRemoteChanges { get; set; }
        internal bool HasHostSnapshot { get; set; }
        internal string SchemaRevision { get; set; } = string.Empty;
        internal string DesignRevision { get; set; } = string.Empty;
        internal string ObservedSchemaRevision { get; set; } = string.Empty;
        internal string ObservedDesignRevision { get; set; } = string.Empty;
    }
}
