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

    /// <summary>
    /// Confirms that a descriptor can be registered and resolved with a function.
    /// </summary>
    [Fact]
    public void ResolveDescriptorReturnsRegisteredDescriptor()
    {
        FunctionRegistry registry = new();
        IFunction function = new CapturingFunction(new ScalarFunctionResult
        {
            Value = new ScalarValue()
        });
        IFunctionDescriptor descriptor = new FunctionDescriptor
        {
            Key = "fn",
            DisplayName = "Function",
            Description = "Test function.",
            ValueKind = FunctionValueKind.Scalar,
            IsRequired = false,
            Parameters =
            [
                new FunctionParameterDescriptor
                {
                    Key = "value",
                    DisplayName = "Value",
                    Description = "Input value.",
                    ValueKind = FunctionValueKind.Scalar,
                    IsRequired = true
                }
            ]
        };

        registry.Register("fn", function, descriptor);

        IFunctionDescriptor resolved = registry.ResolveDescriptor("fn");

        Assert.Same(descriptor, resolved);
        Assert.Single(registry.ListDescriptors());
    }

    /// <summary>
    /// Confirms that descriptor registrations replace previous values.
    /// </summary>
    [Fact]
    public void RegisterDescriptorReplacesExistingDescriptor()
    {
        FunctionRegistry registry = new();
        IFunction function = new CapturingFunction(new ScalarFunctionResult
        {
            Value = new ScalarValue()
        });
        IFunctionDescriptor first = new FunctionDescriptor
        {
            Key = "fn",
            DisplayName = "First",
            ValueKind = FunctionValueKind.Scalar
        };
        IFunctionDescriptor second = new FunctionDescriptor
        {
            Key = "fn",
            DisplayName = "Second",
            ValueKind = FunctionValueKind.StructureNode
        };

        registry.Register("fn", function, first);
        registry.Register("fn", function, second);

        Assert.Same(second, registry.ResolveDescriptor("fn"));
        Assert.Single(registry.ListDescriptors());
    }

    /// <summary>
    /// Confirms that missing descriptors fail with a key lookup error.
    /// </summary>
    [Fact]
    public void ResolveDescriptorThrowsWhenDescriptorIsMissing()
    {
        FunctionRegistry registry = new();

        Assert.Throws<KeyNotFoundException>(() => registry.ResolveDescriptor("missing"));
    }
}
