namespace ButterMorph.UnitTests;

using ButterMorph.DependencyInjection;
using ButterMorph.Design;
using ButterMorph.SchemaDesign;
using ButterMorph.Web.Razor;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Verifies lifecycle behavior shared by the interactive designer workspaces.
/// </summary>
public sealed class DesignerSessionLifecycleTests
{
    /// <summary>
    /// Confirms that simultaneous editors for the same host context do not share workspace state.
    /// </summary>
    [Fact]
    public void SameContextUsesIsolatedWorkspaces()
    {
        using ServiceProvider provider = CreateProvider();
        IButterMorphDesignerSessionManager manager = provider.GetRequiredService<IButterMorphDesignerSessionManager>();
        IMappingDesignSessionStore store = provider.GetRequiredService<IMappingDesignSessionStore>();
        ButterMorphDesignerSessionHandle first = Handle("customer-42", "first");
        ButterMorphDesignerSessionHandle second = Handle("customer-42", "second");

        manager.Acquire(first);
        manager.Acquire(second);
        store.GetOrCreate(first.StorageKey).AddPathMapping("$customer.Name", "Name");

        Assert.Single(store.GetOrCreate(first.StorageKey).Document.Mappings);
        Assert.Empty(store.GetOrCreate(second.StorageKey).Document.Mappings);
    }

    /// <summary>
    /// Confirms that a dirty workspace preserves local content when the host snapshot changes.
    /// </summary>
    [Fact]
    public void DirtyWorkspacePreservesChangesAcrossHostRefresh()
    {
        using ServiceProvider provider = CreateProvider();
        IButterMorphDesignerSessionManager manager = provider.GetRequiredService<IButterMorphDesignerSessionManager>();
        ButterMorphDesignerSessionHandle handle = Handle("customer-42", "dirty");

        manager.Acquire(handle);
        ButterMorphDesignerSessionRefreshResult initial = manager.ObserveHostSnapshot(handle, "schemas-1", "mapping-1");
        manager.MarkDirty(handle);
        ButterMorphDesignerSessionRefreshResult refresh = manager.ObserveHostSnapshot(handle, "schemas-2", "mapping-2");

        Assert.Equal(ButterMorphDesignerSessionRefreshDisposition.Initial, initial.Disposition);
        Assert.Equal(ButterMorphDesignerSessionRefreshDisposition.PreserveLocalChanges, refresh.Disposition);
        Assert.True(manager.GetSnapshot(handle).HasPendingRemoteChanges);
    }

    /// <summary>
    /// Confirms that clean workspaces reload when the host revisions change.
    /// </summary>
    [Fact]
    public void CleanWorkspaceReloadsAcrossHostRefresh()
    {
        using ServiceProvider provider = CreateProvider();
        IButterMorphDesignerSessionManager manager = provider.GetRequiredService<IButterMorphDesignerSessionManager>();
        ButterMorphDesignerSessionHandle handle = Handle("customer-42", "clean");

        manager.Acquire(handle);
        manager.ObserveHostSnapshot(handle, "schemas-1", "mapping-1");
        ButterMorphDesignerSessionRefreshResult refresh = manager.ObserveHostSnapshot(handle, "schemas-2", "mapping-2");

        Assert.Equal(ButterMorphDesignerSessionRefreshDisposition.Reload, refresh.Disposition);
    }

    /// <summary>
    /// Confirms that invalidating a context preserves dirty work instead of silently replacing it.
    /// </summary>
    [Fact]
    public void InvalidationMarksMatchingContextsStale()
    {
        using ServiceProvider provider = CreateProvider();
        IButterMorphDesignerSessionManager manager = provider.GetRequiredService<IButterMorphDesignerSessionManager>();
        ButterMorphDesignerSessionHandle handle = Handle("customer-42", "invalidated");

        manager.Acquire(handle);
        manager.ObserveHostSnapshot(handle, "schemas-1", "mapping-1");
        manager.MarkDirty(handle);

        Assert.Equal(1, manager.InvalidateContext("customer-42", ButterMorphDesignerSessionKind.Mapping));

        ButterMorphDesignerSessionRefreshResult refresh = manager.ObserveHostSnapshot(handle, "schemas-1", "mapping-1");
        Assert.Equal(ButterMorphDesignerSessionRefreshDisposition.PreserveLocalChanges, refresh.Disposition);
    }

    /// <summary>
    /// Confirms that explicit close releases the underlying mapping workspace.
    /// </summary>
    [Fact]
    public void CloseReleasesAssociatedDesignerState()
    {
        using ServiceProvider provider = CreateProvider();
        IButterMorphDesignerSessionManager manager = provider.GetRequiredService<IButterMorphDesignerSessionManager>();
        IMappingDesignSessionStore store = provider.GetRequiredService<IMappingDesignSessionStore>();
        ButterMorphDesignerSessionHandle handle = Handle("customer-42", "close");

        manager.Acquire(handle);
        store.GetOrCreate(handle.StorageKey).AddPathMapping("$customer.Name", "Name");

        Assert.True(manager.Close(handle));
        Assert.Null(manager.GetSnapshot(handle));
        Assert.Empty(store.GetOrCreate(handle.StorageKey).Document.Mappings);
    }

    /// <summary>
    /// Confirms that closing one editor leaves another editor for the same context intact.
    /// </summary>
    [Fact]
    public void CloseOnlyReleasesTheRequestedEditor()
    {
        using ServiceProvider provider = CreateProvider();
        IButterMorphDesignerSessionManager manager = provider.GetRequiredService<IButterMorphDesignerSessionManager>();
        ButterMorphDesignerSessionHandle first = Handle("customer-42", "first");
        ButterMorphDesignerSessionHandle second = Handle("customer-42", "second");

        manager.Acquire(first);
        manager.Acquire(second);

        Assert.True(manager.Close(first));
        Assert.Null(manager.GetSnapshot(first));
        Assert.NotNull(manager.GetSnapshot(second));
    }

    /// <summary>
    /// Confirms that abandoned inactive workspaces are expired by policy.
    /// </summary>
    [Fact]
    public void SweepExpiresInactiveWorkspaces()
    {
        using ServiceProvider provider = CreateProvider(options => options.Sessions.InactiveTimeout = TimeSpan.FromMinutes(1));
        IButterMorphDesignerSessionManager manager = provider.GetRequiredService<IButterMorphDesignerSessionManager>();
        ButterMorphDesignerSessionHandle handle = Handle("customer-42", "expired");

        manager.Acquire(handle);
        manager.Deactivate(handle);

        Assert.Equal(1, manager.SweepExpired(DateTimeOffset.UtcNow.AddMinutes(2)));
        Assert.Null(manager.GetSnapshot(handle));
    }

    /// <summary>
    /// Confirms that a workspace without browser heartbeats becomes inactive before the longer active timeout.
    /// </summary>
    [Fact]
    public void SweepDeactivatesAbandonedActiveWorkspaceBeforeActiveTimeout()
    {
        using ServiceProvider provider = CreateProvider(options =>
        {
            options.Sessions.HeartbeatTimeout = TimeSpan.FromMinutes(1);
            options.Sessions.InactiveTimeout = TimeSpan.FromMinutes(1);
            options.Sessions.ActiveTimeout = TimeSpan.FromHours(8);
        });
        IButterMorphDesignerSessionManager manager = provider.GetRequiredService<IButterMorphDesignerSessionManager>();
        ButterMorphDesignerSessionHandle handle = Handle("customer-42", "abandoned");

        ButterMorphDesignerSessionSnapshot initial = manager.Acquire(handle);
        DateTimeOffset abandonedAt = initial.LastActivityUtc.AddMinutes(2);

        Assert.Equal(0, manager.SweepExpired(abandonedAt));
        Assert.False(manager.GetSnapshot(handle).IsActive);
        Assert.Equal(1, manager.SweepExpired(abandonedAt.AddMinutes(2)));
        Assert.Null(manager.GetSnapshot(handle));
    }

    /// <summary>
    /// Confirms that capacity eviction only removes inactive workspaces.
    /// </summary>
    [Fact]
    public void CapacityEvictsInactiveWorkspacesButNeverActiveOnes()
    {
        using ServiceProvider provider = CreateProvider(options => options.Sessions.MaximumSessions = 1);
        IButterMorphDesignerSessionManager manager = provider.GetRequiredService<IButterMorphDesignerSessionManager>();
        ButterMorphDesignerSessionHandle first = Handle("first", "one");
        ButterMorphDesignerSessionHandle second = Handle("second", "two");

        manager.Acquire(first);
        ButterMorphDesignerSessionCapacityException exception = Assert.Throws<ButterMorphDesignerSessionCapacityException>(() => manager.Acquire(second));
        Assert.False(exception.IsContextLimit);

        manager.Deactivate(first);
        manager.Acquire(second);

        Assert.Null(manager.GetSnapshot(first));
        Assert.NotNull(manager.GetSnapshot(second));
    }

    /// <summary>
    /// Confirms that a context-specific capacity limit does not evict an unrelated inactive workspace.
    /// </summary>
    [Fact]
    public void ContextCapacityRejectsOnlyTheOverLimitContext()
    {
        using ServiceProvider provider = CreateProvider(options =>
        {
            options.Sessions.MaximumSessions = 4;
            options.Sessions.MaximumSessionsPerContext = 1;
        });
        IButterMorphDesignerSessionManager manager = provider.GetRequiredService<IButterMorphDesignerSessionManager>();
        ButterMorphDesignerSessionHandle first = Handle("customer-42", "first");
        ButterMorphDesignerSessionHandle otherContext = Handle("order-42", "other");
        ButterMorphDesignerSessionHandle secondForFirstContext = Handle("customer-42", "second");

        manager.Acquire(first);
        manager.Acquire(otherContext);
        manager.Deactivate(otherContext);

        ButterMorphDesignerSessionCapacityException exception = Assert.Throws<ButterMorphDesignerSessionCapacityException>(() => manager.Acquire(secondForFirstContext));

        Assert.True(exception.IsContextLimit);
        Assert.NotNull(manager.GetSnapshot(first));
        Assert.NotNull(manager.GetSnapshot(otherContext));
    }

    private static ServiceProvider CreateProvider(Action<ButterMorphRazorDesignerOptions> configure = null)
    {
        ServiceCollection services = new();
        services.AddButterMorph().AddRazorDesigner(configure ?? (_ => { }));
        return services.BuildServiceProvider();
    }

    private static ButterMorphDesignerSessionHandle Handle(string contextKey, string sessionId)
    {
        return new ButterMorphDesignerSessionHandle
        {
            Kind = ButterMorphDesignerSessionKind.Mapping,
            ContextKey = contextKey,
            SessionId = sessionId
        };
    }
}
