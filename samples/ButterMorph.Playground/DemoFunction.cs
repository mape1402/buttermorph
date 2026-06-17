using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Demo function used by the playground capability catalog.
/// </summary>
public sealed class DemoFunction : IFunction
{
    /// <summary>
    /// Executes the demo function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        return new ScalarFunctionResult
        {
            Value = new ScalarValue
            {
                DataType = "String",
                RawValue = string.Empty,
                IsNull = false
            }
        };
    }
}
