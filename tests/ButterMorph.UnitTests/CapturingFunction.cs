namespace ButterMorph.UnitTests;

using ButterMorph.Abstractions;

/// <summary>
/// Test function that captures arguments and returns a configured result.
/// </summary>
internal sealed class CapturingFunction : IFunction
{
    // Stores the configured function result returned during execution.
    private readonly IFunctionResult _result;

    /// <summary>
    /// Initializes a new instance of the <see cref="CapturingFunction"/> class.
    /// </summary>
    /// <param name="result">The function result returned during execution.</param>
    public CapturingFunction(IFunctionResult result)
    {
        _result = result;
    }

    // Stores the arguments captured during the last execution.
    internal IReadOnlyList<IFunctionArgument> LastArguments { get; private set; } = [];

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        LastArguments = context.Arguments;
        return _result;
    }
}
