namespace ButterMorph.Abstractions;

/// <summary>
/// Represents the execution context passed to a DSL function.
/// </summary>
public sealed class FunctionExecutionContext
{
    /// <summary>
    /// Gets or sets the shared execution context.
    /// </summary>
    public IExecutionContext ExecutionContext { get; set; }

    /// <summary>
    /// Gets or sets the function arguments.
    /// </summary>
    public IReadOnlyList<IFunctionArgument> Arguments { get; set; } = new List<IFunctionArgument>();
}
