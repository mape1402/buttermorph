namespace ButterMorph.UnitTests;

using System.Collections.Generic;
using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Verifies validation rule registry behavior.
/// </summary>
public sealed class ValidationRuleRegistryTests
{
    /// <summary>
    /// Confirms that a registered handler can be resolved by key.
    /// </summary>
    [Fact]
    public void ResolveReturnsRegisteredHandler()
    {
        ValidationRuleRegistry registry = new();
        IValidationRuleHandler handler = new PassingValidationRuleHandler();

        registry.Register("required", handler);

        IValidationRuleHandler resolved = registry.Resolve("required");

        Assert.Same(handler, resolved);
    }

    /// <summary>
    /// Confirms that missing handlers fail with a key lookup error.
    /// </summary>
    [Fact]
    public void ResolveThrowsWhenHandlerIsMissing()
    {
        ValidationRuleRegistry registry = new();

        Assert.Throws<KeyNotFoundException>(() => registry.Resolve("missing"));
    }
}
