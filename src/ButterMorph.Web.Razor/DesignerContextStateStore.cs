namespace ButterMorph.Web.Razor;

// Stores web-only designer context state by session key.
internal sealed class DesignerContextStateStore
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
}
