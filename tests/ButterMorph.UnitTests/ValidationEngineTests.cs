namespace ButterMorph.UnitTests;

using System.Collections.Generic;
using System.Linq;
using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Execution;
using ButterMorph.Functions;
using ButterMorph.Json;
using ButterMorph.Navigation;
using ButterMorph.Transformation;
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
    /// Confirms that validation assertions can read payload fields through functions.
    /// </summary>
    [Fact]
    public void ValidateExecutesAssertionExpressionsWithFieldReferences()
    {
        ValidationEngine engine = CreateEngineWithAssertions(new ValidationRuleRegistry());
        ValidationRequest request = CreateAssertionRequest("{\"quantity\":9}");

        ValidationResult result = engine.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Diagnostics, diagnostic => diagnostic.Code == "BMVL004" && diagnostic.Path == "quantity");
    }

    /// <summary>
    /// Confirms that schema validation and assertion validation pass together.
    /// </summary>
    [Fact]
    public void ValidateReturnsValidWhenSchemaAndAssertionsPass()
    {
        ValidationEngine engine = CreateEngineWithAssertions(new ValidationRuleRegistry());
        ValidationRequest request = CreateAssertionRequest("{\"quantity\":12}");

        ValidationResult result = engine.Validate(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Diagnostics);
    }

    /// <summary>
    /// Confirms that validation assertions can compare fields from several sources.
    /// </summary>
    [Fact]
    public void ValidateExecutesAssertionsAcrossSeveralSources()
    {
        ValidationEngine engine = CreateEngineWithAssertions(new ValidationRuleRegistry());
        ValidationRequest request = new()
        {
            Sources = new Dictionary<string, IStructureGraph>
            {
                ["order"] = ReadJson("{\"quantity\":12}"),
                ["limits"] = ReadJson("{\"minimum\":10}")
            },
            Definition = new ValidationDocument
            {
                Assertions =
                [
                    new ValidationAssertion
                    {
                        Expression = new FunctionCallExpression
                        {
                            FunctionKey = "gt",
                            Arguments =
                            [
                                new PathExpression
                                {
                                    Path = "$order.quantity"
                                },
                                new PathExpression
                                {
                                    Path = "$limits.minimum"
                                }
                            ]
                        },
                        Message = "Quantity must be greater than minimum.",
                        Path = "$order.quantity"
                    }
                ]
            }
        };

        ValidationResult result = engine.Validate(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Diagnostics);
    }

    // Creates a validation engine with real path resolution.
    private static ValidationEngine CreateEngine(IValidationRuleRegistry registry)
    {
        return new ValidationEngine(new PathResolver(), registry);
    }

    // Creates a validation engine with schema and assertion dependencies.
    private static ValidationEngine CreateEngineWithAssertions(IValidationRuleRegistry registry)
    {
        PathResolver pathResolver = new();
        NavigationEngine navigationEngine = new(pathResolver);
        FunctionRegistry functionRegistry = new();
        functionRegistry.Register("gt", new GreaterThanFunction());
        TransformationExpressionEvaluator evaluator = new(navigationEngine, pathResolver, functionRegistry);

        return new ValidationEngine(pathResolver, registry, new SchemaValidator(), evaluator, new ExecutionContextFactory());
    }

    // Creates a validation request with schema and assertion DSL document.
    private static ValidationRequest CreateAssertionRequest(string json)
    {
        IStructureGraph graph = ReadJson(json);
        IStructureSchema schema = new StructureSchema
        {
            Key = "Order",
            Name = "Order",
            Root = new SchemaNode
            {
                Name = "$root",
                Kind = SchemaNodeKind.Object,
                DataType = "object",
                Children =
                [
                    new SchemaNode
                    {
                        Name = "quantity",
                        Kind = SchemaNodeKind.Scalar,
                        DataType = "integer",
                        IsRequired = true
                    }
                ]
            }
        };

        return new ValidationRequest
        {
            SourceGraph = graph,
            PayloadAlias = "source",
            Sources = new Dictionary<string, IStructureGraph>
            {
                ["source"] = graph
            },
            Schema = schema,
            Schemas = new Dictionary<string, IStructureSchema>
            {
                ["Order"] = schema
            },
            Definition = new ValidationDocument
            {
                Assertions =
                [
                    new ValidationAssertion
                    {
                        Expression = new FunctionCallExpression
                        {
                            FunctionKey = "gt",
                            Arguments =
                            [
                                new PathExpression
                                {
                                    Path = "$source.quantity"
                                },
                                new ScalarLiteralExpression
                                {
                                    Value = new ScalarValue
                                    {
                                        DataType = "Number",
                                        RawValue = "10",
                                        IsNull = false
                                    }
                                }
                            ]
                        },
                        Message = "Quantity must be greater than 10.",
                        Path = "$source.quantity"
                    }
                ]
            }
        };
    }

    // Reads a JSON payload into a structure graph.
    private static IStructureGraph ReadJson(string json)
    {
        return new JsonReader().Read(new StructureInput
        {
            Format = "json",
            Content = json
        });
    }

    // Confirms that a validation result contains a diagnostic code.
    private static void AssertDiagnostic(ValidationResult result, string code)
    {
        Assert.Contains(result.Diagnostics, diagnostic => string.Equals(diagnostic.Code, code, System.StringComparison.Ordinal));
        Assert.All(result.Diagnostics, diagnostic => Assert.Equal("Error", diagnostic.Severity));
    }
}
