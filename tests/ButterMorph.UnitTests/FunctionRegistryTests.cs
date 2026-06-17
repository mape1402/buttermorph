namespace ButterMorph.UnitTests;

using System.Collections.Generic;
using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Functions;

/// <summary>
/// Verifies function registry behavior.
/// </summary>
public sealed class FunctionRegistryTests
{
    /// <summary>
    /// Confirms that a registered function can be resolved.
    /// </summary>
    [Fact]
    public void ResolveReturnsRegisteredFunction()
    {
        FunctionRegistry registry = new();
        IFunction function = new CapturingFunction(new ScalarFunctionResult
        {
            Value = new ScalarValue()
        });

        registry.Register("fn", function);

        IFunction resolved = registry.Resolve("fn");

        Assert.Same(function, resolved);
    }

    /// <summary>
    /// Confirms that resolving a missing function fails with a key lookup error.
    /// </summary>
    [Fact]
    public void ResolveThrowsWhenFunctionIsMissing()
    {
        FunctionRegistry registry = new();

        Assert.Throws<KeyNotFoundException>(() => registry.Resolve("missing"));
    }
}
