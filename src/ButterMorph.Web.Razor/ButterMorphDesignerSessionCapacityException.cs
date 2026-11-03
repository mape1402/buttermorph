namespace ButterMorph.Web.Razor;

/// <summary>
/// Describes an attempt to open a designer workspace after its configured capacity was reached by active sessions.
/// </summary>
public sealed class ButterMorphDesignerSessionCapacityException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ButterMorphDesignerSessionCapacityException"/> class.
    /// </summary>
    /// <param name="isContextLimit">Whether the limit applies to the current context and designer kind.</param>
    /// <param name="limit">The configured limit that was reached.</param>
    public ButterMorphDesignerSessionCapacityException(bool isContextLimit, int limit)
        : base(isContextLimit
            ? "This designer context already has the maximum number of active editor sessions. Close an editor and try again."
            : "The host has reached the maximum number of active designer sessions. Close an editor and try again.")
    {
        IsContextLimit = isContextLimit;
        Limit = limit;
    }

    /// <summary>Gets whether the context-specific capacity was reached.</summary>
    public bool IsContextLimit { get; }

    /// <summary>Gets the configured capacity that was reached.</summary>
    public int Limit { get; }
}
