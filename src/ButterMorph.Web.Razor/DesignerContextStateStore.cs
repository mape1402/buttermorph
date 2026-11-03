namespace ButterMorph.Web.Razor;

/// <summary>
/// Stores web-only designer context state by isolated session key.
/// </summary>
public sealed class DesignerContextStateStore
{
    // Stores context state for active designer sessions.
    private readonly Dictionary<string, DesignerContextState> _states = new(StringComparer.Ordinal);

    // Gets existing context state or creates a new state entry.
    internal DesignerContextState GetOrCreate(string sessionKey, ButterMorphRazorDesignerOptions options)
    {
        lock (_states)
        {
            if (_states.TryGetValue(sessionKey, out DesignerContextState state))
            {
                return state;
            }

            state = new DesignerContextState
            {
                ShowSchemaActions = options.ShowSchemaActions
            };
            _states[sessionKey] = state;

            return state;
        }
    }

    // Removes state belonging to a released designer session.
    internal bool Remove(string sessionKey)
    {
        lock (_states)
        {
            return _states.Remove(sessionKey);
        }
    }
}
