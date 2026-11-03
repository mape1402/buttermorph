namespace ButterMorph.Web.Razor;

/// <summary>
/// Configures retention and isolation for interactive designer sessions.
/// </summary>
public sealed class ButterMorphDesignerSessionOptions
{
    /// <summary>
    /// Gets or sets the query parameter that carries the opaque editor session identifier.
    /// </summary>
    public string SessionQueryParameter { get; set; } = "session";

    /// <summary>
    /// Gets or sets how long a closed or inactive session is retained.
    /// </summary>
    public TimeSpan InactiveTimeout { get; set; } = TimeSpan.FromMinutes(20);

    /// <summary>
    /// Gets or sets the maximum idle time for an open editor before it is eligible for cleanup.
    /// </summary>
    public TimeSpan ActiveTimeout { get; set; } = TimeSpan.FromHours(8);

    /// <summary>
    /// Gets or sets how long an editor can stop reporting browser activity before it is treated as inactive.
    /// </summary>
    /// <remarks>
    /// This protects the host from sessions left behind by a closed browser that could not send an explicit close request.
    /// </remarks>
    public TimeSpan HeartbeatTimeout { get; set; } = TimeSpan.FromMinutes(3);

    /// <summary>
    /// Gets or sets how often the background cleanup service evaluates retained sessions.
    /// </summary>
    public TimeSpan SweepInterval { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Gets or sets the maximum number of retained sessions in this process.
    /// </summary>
    public int MaximumSessions { get; set; } = 256;

    /// <summary>
    /// Gets or sets the maximum number of retained sessions for one context and designer kind.
    /// </summary>
    public int MaximumSessionsPerContext { get; set; } = 16;
}
