namespace ButterMorph.Functions;

using ButterMorph.Abstractions;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Hashes text using a named algorithm.
/// </summary>
public sealed class HashFunction : IFunction
{
    
    /// <summary>
    /// Gets the function description shown in design-time tooling.
    /// </summary>
    public string Description => "Hashes text using a named algorithm.";

    // Shared conversion helpers for this function.
    private readonly FunctionTools _tools = new();

    /// <summary>
    /// Executes the function.
    /// </summary>
    /// <param name="context">The function execution context.</param>
    /// <returns>The function result.</returns>
    public IFunctionResult Execute(FunctionExecutionContext context)
    {
        _tools.Require(context, "hash", 2, 2);
        string algorithm = _tools.Text(context.Arguments[0]);
        string text = _tools.Text(context.Arguments[1]);
        byte[] bytes = Encoding.UTF8.GetBytes(text);

        if (string.Equals(algorithm, "sha1", StringComparison.OrdinalIgnoreCase))
        {
            return _tools.StringResult(Convert.ToHexString(SHA1.HashData(bytes)).ToLowerInvariant());
        }

        if (string.Equals(algorithm, "sha512", StringComparison.OrdinalIgnoreCase))
        {
            return _tools.StringResult(Convert.ToHexString(SHA512.HashData(bytes)).ToLowerInvariant());
        }

        return _tools.StringResult(Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant());
    }
}
