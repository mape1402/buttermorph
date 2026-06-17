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

    /// <summary>
    /// Confirms that a descriptor can be registered and resolved with a handler.
    /// </summary>
    [Fact]
    public void ResolveDescriptorReturnsRegisteredDescriptor()
    {
        ValidationRuleRegistry registry = new();
        IValidationRuleHandler handler = new PassingValidationRuleHandler();
        IValidationRuleDescriptor descriptor = new ValidationRuleDescriptor
        {
            Key = "required",
            DisplayName = "Required",
            Description = "Requires a value.",
            ValueKind = FunctionValueKind.Scalar,
            IsRequired = true,
            Parameters =
            [
                new ValidationRuleParameterDescriptor
                {
                    Key = "message",
                    DisplayName = "Message",
                    Description = "Failure message.",
                    ValueKind = FunctionValueKind.Scalar,
                    IsRequired = false
                }
            ]
        };

        registry.Register("required", handler, descriptor);

        IValidationRuleDescriptor resolved = registry.ResolveDescriptor("required");

        Assert.Same(descriptor, resolved);
        Assert.Single(registry.ListDescriptors());
    }

    /// <summary>
    /// Confirms that descriptor registrations replace previous values.
    /// </summary>
    [Fact]
    public void RegisterDescriptorReplacesExistingDescriptor()
    {
        ValidationRuleRegistry registry = new();
        IValidationRuleHandler handler = new PassingValidationRuleHandler();
        IValidationRuleDescriptor first = new ValidationRuleDescriptor
        {
            Key = "required",
            DisplayName = "First",
            ValueKind = FunctionValueKind.Scalar
        };
        IValidationRuleDescriptor second = new ValidationRuleDescriptor
        {
            Key = "required",
            DisplayName = "Second",
            ValueKind = FunctionValueKind.StructureNode
        };

        registry.Register("required", handler, first);
        registry.Register("required", handler, second);

        Assert.Same(second, registry.ResolveDescriptor("required"));
        Assert.Single(registry.ListDescriptors());
    }

    /// <summary>
    /// Confirms that missing descriptors fail with a key lookup error.
    /// </summary>
    [Fact]
    public void ResolveDescriptorThrowsWhenDescriptorIsMissing()
    {
        ValidationRuleRegistry registry = new();

        Assert.Throws<KeyNotFoundException>(() => registry.ResolveDescriptor("missing"));
    }
}
