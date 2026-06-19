namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

/// <summary>
/// Creates a UUID value.
/// </summary>
public sealed class UuidFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Creates a UUID value.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "uuid", 0, 0);
        return _tools.StringResult(Guid.NewGuid().ToString());
    }
}
