namespace ButterMorph.Functions;

using ButterMorph.Abstractions;
using System.Security.Cryptography;

/// <summary>
/// Creates a sortable unique identifier value.
/// </summary>
public sealed class UlidFunction : IFunction
{
    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "ulid", 0, 0);
        byte[] random = RandomNumberGenerator.GetBytes(10);
        string time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString("x12", System.Globalization.CultureInfo.InvariantCulture);
        return _tools.StringResult(time + Convert.ToHexString(random).ToLowerInvariant());
    }
}
