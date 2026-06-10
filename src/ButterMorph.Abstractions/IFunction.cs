namespace ButterMorph.Abstractions;

/// <summary>
/// Defines an executable DSL function.
/// </summary>
public interface IFunction
{
    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    IFunctionResult Execute(FunctionExecutionContext context);
}
