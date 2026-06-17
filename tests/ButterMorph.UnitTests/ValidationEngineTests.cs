namespace ButterMorph.UnitTests;

using System.Collections.Generic;
using System.Linq;
using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Navigation;
using ButterMorph.Validation;

/// <summary>
/// Verifies pluggable validation engine behavior.
/// </summary>
public sealed class ValidationEngineTests
{
    /// <summary>
    /// Confirms that non-validation documents are rejected.
    /// </summary>
    [Fact]
    public void ValidateReturnsDiagnosticWhenDefinitionIsNotValidationDocument()
    {
        ValidationEngine engine = CreateEngine(new ValidationRuleRegistry());
        ValidationRequest request = new()
        {
            SourceGraph = NavigationTestGraphFactory.CreateCustomerGraph(),
            Definition = new DslDocument()
        };

        ValidationResult result = engine.Validate(request);

        Assert.False(result.IsValid);
        AssertDiagnostic(result, "BMVL001");
    }

    /// <summary>
    /// Confirms that missing paths are reported as diagnostics.
    /// </summary>
    [Fact]
    public void ValidateReturnsDiagnosticWhenPathIsMissing()
    {
        ValidationRuleRegistry registry = new();
        registry.Register("pass", new PassingValidationRuleHandler());
        ValidationEngine engine = CreateEngine(registry);
        ValidationRequest request = CreateRequest(
        [
            CreateRule("Customer.Unknown", "pass")
        ]);

        ValidationResult result = engine.Validate(request);

        Assert.False(result.IsValid);
        AssertDiagnostic(result, "BMVL002");
    }

    /// <summary>
    /// Confirms that missing handlers are reported as diagnostics.
    /// </summary>
    [Fact]
    public void ValidateReturnsDiagnosticWhenHandlerIsMissing()
    {
        ValidationEngine engine = CreateEngine(new ValidationRuleRegistry());
        ValidationRequest request = CreateRequest(
        [
            CreateRule("Customer.Name", "missing")
        ]);

        ValidationResult result = engine.Validate(request);

        Assert.False(result.IsValid);
        AssertDiagnostic(result, "BMVL003");
    }

    /// <summary>
    /// Confirms that a registered handler receives the resolved node.
    /// </summary>
    [Fact]
    public void ValidateExecutesRegisteredHandler()
    {
        CapturingValidationRuleHandler handler = new();
        ValidationRuleRegistry registry = new();
        registry.Register("capture", handler);
        ValidationEngine engine = CreateEngine(registry);
        ValidationRequest request = CreateRequest(
        [
            CreateRule("Customer.Name", "capture")
        ]);

        ValidationResult result = engine.Validate(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Diagnostics);
        Assert.Equal("Name", handler.CapturedNode.Name);
        Assert.Equal("Customer.Name", handler.CapturedPath);
    }

    /// <summary>
    /// Confirms that diagnostics from several rules are accumulated.
    /// </summary>
    [Fact]
    public void ValidateAccumulatesDiagnosticsFromMultipleRules()
    {
        ValidationRuleRegistry registry = new();
        registry.Register("diagnostic", new DiagnosticValidationRuleHandler());
        ValidationEngine engine = CreateEngine(registry);
        ValidationRequest request = CreateRequest(
        [
            CreateRule("Customer.Name", "diagnostic"),
            CreateRule("Orders[0].Id", "diagnostic")
        ]);

        ValidationResult result = engine.Validate(request);

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Diagnostics.Count);
        Assert.All(result.Diagnostics, diagnostic => Assert.Equal("TEST001", diagnostic.Code));
    }

    /// <summary>
    /// Confirms that validation passes when all handlers produce no diagnostics.
    /// </summary>
    [Fact]
    public void ValidateReturnsValidWhenAllHandlersPass()
    {
        ValidationRuleRegistry registry = new();
        registry.Register("pass", new PassingValidationRuleHandler());
        ValidationEngine engine = CreateEngine(registry);
        ValidationRequest request = CreateRequest(
        [
            CreateRule("Customer.Name", "pass"),
            CreateRule("Orders[0].Id", "pass")
        ]);

        ValidationResult result = engine.Validate(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Diagnostics);
    }

    // Creates a validation engine with real path resolution.
    private static ValidationEngine CreateEngine(IValidationRuleRegistry registry)
    {
        return new ValidationEngine(new PathResolver(), registry);
    }

    // Creates a validation request with test graph and rules.
    private static ValidationRequest CreateRequest(IReadOnlyCollection<IValidationRule> rules)
    {
        return new ValidationRequest
        {
            SourceGraph = NavigationTestGraphFactory.CreateCustomerGraph(),
            Definition = new ValidationDocument
            {
                Definition = new DslDefinition
                {
                    Content = string.Empty
                },
                Rules = rules
            }
        };
    }

    // Creates a validation rule for tests.
    private static IValidationRule CreateRule(string path, string ruleKey)
    {
        return new ValidationRule
        {
            Path = path,
            RuleKey = ruleKey,
            Arguments = []
        };
    }

    // Confirms that a validation result contains a diagnostic code.
    private static void AssertDiagnostic(ValidationResult result, string code)
    {
        Assert.Contains(result.Diagnostics, diagnostic => string.Equals(diagnostic.Code, code, System.StringComparison.Ordinal));
        Assert.All(result.Diagnostics, diagnostic => Assert.Equal("Error", diagnostic.Severity));
    }
}
