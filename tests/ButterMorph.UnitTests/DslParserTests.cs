namespace ButterMorph.UnitTests;

using System.Collections.Generic;
using System.Linq;
using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Dsl;
using ButterMorph.Execution;
using ButterMorph.Functions;
using ButterMorph.Navigation;
using ButterMorph.Transformation;
using ButterMorph.Validation;

/// <summary>
/// Verifies shape-neutral DSL parsing behavior.
/// </summary>
public sealed class DslParserTests
{
    /// <summary>
    /// Confirms that scalar mappings and metadata are parsed.
    /// </summary>
    [Fact]
    public void ParseCreatesMappingsAndMetadata()
    {
        ITransformationDocument document = Parse(
            """
            metadata {
              version: "1"
            }

            target {
              Customer {
                Name: $source.Customer.Name
                FirstOrderId: $source.Orders[0].Id
              }
            }
            """);

        Assert.Equal("1", document.Metadata["version"]);
        Assert.Equal(2, document.Mappings.Count);
        Assert.Contains(document.Mappings, mapping => mapping.TargetPath == "Customer.Name");
        Assert.Contains(document.Mappings, mapping => mapping.TargetPath == "Customer.FirstOrderId");
        IPathExpression expression = Assert.IsAssignableFrom<IPathExpression>(document.Mappings.First().SourceExpression);
        Assert.Equal("$source.Customer.Name", expression.Path);
    }

    /// <summary>
    /// Confirms that literal expressions are parsed.
    /// </summary>
    [Fact]
    public void ParseCreatesLiteralExpressions()
    {
        ITransformationDocument document = Parse(
            """
            target {
              Name: "Ada"
              Age: 37
              Active: true
              Missing: null
            }
            """);

        Assert.Equal(4, document.Mappings.Count);
        AssertLiteral(document.Mappings.ElementAt(0).SourceExpression, "String", "Ada", false);
        AssertLiteral(document.Mappings.ElementAt(1).SourceExpression, "Number", "37", false);
        AssertLiteral(document.Mappings.ElementAt(2).SourceExpression, "Boolean", "true", false);
        AssertLiteral(document.Mappings.ElementAt(3).SourceExpression, "Null", string.Empty, true);
    }

    /// <summary>
    /// Confirms that functions, conditionals and projections are parsed.
    /// </summary>
    [Fact]
    public void ParseCreatesAdvancedExpressions()
    {
        ITransformationDocument document = Parse(
            """
            target {
              FullName: concat($source.Customer.Name, " Lovelace")
              Status: when(true, "Active", "Inactive")
              OrderIds: project $source.Orders as order => order.Id
            }
            """);

        IFunctionCallExpression function = Assert.IsAssignableFrom<IFunctionCallExpression>(document.Mappings.ElementAt(0).SourceExpression);
        IConditionalExpression condition = Assert.IsAssignableFrom<IConditionalExpression>(document.Mappings.ElementAt(1).SourceExpression);
        ICollectionProjectionExpression projection = Assert.IsAssignableFrom<ICollectionProjectionExpression>(document.Mappings.ElementAt(2).SourceExpression);

        Assert.Equal("concat", function.FunctionKey);
        Assert.Equal(2, function.Arguments.Count);
        Assert.NotNull(condition.ThenExpression);
        Assert.Equal("order", projection.ItemAlias);
    }

    /// <summary>
    /// Confirms that inline map-shaped and ordered expressions are parsed.
    /// </summary>
    [Fact]
    public void ParseCreatesInlineExpressions()
    {
        ITransformationDocument document = Parse(
            """
            target {
              Customer: { Name: $source.Customer.Name, Active: true }
              Codes: [$source.Orders[0].Id, "fallback"]
            }
            """);

        IObjectExpression map = Assert.IsAssignableFrom<IObjectExpression>(document.Mappings.ElementAt(0).SourceExpression);
        IArrayExpression ordered = Assert.IsAssignableFrom<IArrayExpression>(document.Mappings.ElementAt(1).SourceExpression);

        Assert.Equal(2, map.Properties.Count);
        Assert.Equal(2, ordered.Items.Count);
    }

    /// <summary>
    /// Confirms that validation declarations preserve typed arguments.
    /// </summary>
    [Fact]
    public void ParseCreatesValidationRules()
    {
        ITransformationDocument document = Parse(
            """
            validate {
              Customer.Name: required
              Customer.Age: min(18)
              Customer.Email: format("email")
            }
            """);

        Assert.Equal(3, document.Validations.Count);
        IValidationRule minRule = document.Validations.ElementAt(1);
        IValidationRule formatRule = document.Validations.ElementAt(2);

        Assert.Equal("min", minRule.RuleKey);
        Assert.Single(minRule.Arguments);
        AssertLiteral(minRule.Arguments.First(), "Number", "18", false);
        Assert.Equal("format", formatRule.RuleKey);
        AssertLiteral(formatRule.Arguments.First(), "String", "email", false);
    }

    /// <summary>
    /// Confirms that invalid syntax produces positioned format errors.
    /// </summary>
    [Fact]
    public void ParseThrowsForInvalidSyntax()
    {
        FormatException exception = Assert.Throws<FormatException>(() => Parse(
            """
            target {
              Name $source.Customer.Name
            }
            """));

        Assert.Contains("Line", exception.Message, System.StringComparison.Ordinal);
        Assert.Contains("column", exception.Message, System.StringComparison.Ordinal);
    }

    /// <summary>
    /// Confirms that parsed DSL can drive the transformation runtime.
    /// </summary>
    [Fact]
    public void ParsedDocumentExecutesTransformation()
    {
        FunctionRegistry registry = new();
        registry.Register("capture", new CapturingFunction(new ScalarFunctionResult
        {
            Value = new ScalarValue
            {
                DataType = "String",
                RawValue = "Ada Lovelace",
                IsNull = false
            }
        }));
        TransformationEngine engine = CreateEngine(registry);
        ITransformationDocument document = Parse(
            """
            target {
              FullName: capture($source.Customer.Name)
              OrderIds: project $source.Orders as order => order.Id
            }
            """);

        TransformationResult result = engine.Transform(new TransformationRequest
        {
            Sources = new Dictionary<string, IStructureGraph>
            {
                ["source"] = NavigationTestGraphFactory.CreateCustomerGraph()
            },
            Definition = document
        });

        IScalarStructureNode fullName = Assert.IsAssignableFrom<IScalarStructureNode>(new PathResolver().Resolve(result.ResultGraph.Root, "FullName"));
        IScalarStructureNode orderId = Assert.IsAssignableFrom<IScalarStructureNode>(new PathResolver().Resolve(result.ResultGraph.Root, "OrderIds[0]"));

        Assert.True(result.Succeeded);
        Assert.Equal("Ada Lovelace", fullName.Value.RawValue);
        Assert.Equal("A1", orderId.Value.RawValue);
    }

    /// <summary>
    /// Confirms that validation handlers receive parsed arguments.
    /// </summary>
    [Fact]
    public void ValidationEngineReceivesParsedRuleArguments()
    {
        CapturingValidationRuleHandler handler = new();
        ValidationRuleRegistry registry = new();
        registry.Register("min", handler);
        ValidationEngine engine = new(new PathResolver(), registry);
        ITransformationDocument document = Parse(
            """
            validate {
              Customer.Name: min(2)
            }
            """);

        ValidationResult result = engine.Validate(new ValidationRequest
        {
            SourceGraph = NavigationTestGraphFactory.CreateCustomerGraph(),
            Definition = new ValidationDocument
            {
                Rules = document.Validations
            }
        });

        Assert.True(result.IsValid);
        Assert.Single(handler.CapturedRule.Arguments);
        AssertLiteral(handler.CapturedRule.Arguments.First(), "Number", "2", false);
    }

    // Parses DSL content into a transformation document.
    private static ITransformationDocument Parse(string content)
    {
        IDslDocument document = new DslParser().Parse(new DslDefinition
        {
            Content = content
        });

        return Assert.IsAssignableFrom<ITransformationDocument>(document);
    }

    // Creates a transformation engine with real navigation and test functions.
    private static TransformationEngine CreateEngine(IFunctionRegistry registry)
    {
        PathResolver pathResolver = new();
        NavigationEngine navigationEngine = new(pathResolver);
        TransformationExpressionEvaluator evaluator = new(navigationEngine, pathResolver, registry);

        return new TransformationEngine(evaluator, new ExecutionContextFactory());
    }

    // Confirms scalar literal expression values.
    private static void AssertLiteral(ITransformationExpression expression, string dataType, string rawValue, bool isNull)
    {
        IScalarLiteralExpression literal = Assert.IsAssignableFrom<IScalarLiteralExpression>(expression);

        Assert.Equal(dataType, literal.Value.DataType);
        Assert.Equal(rawValue, literal.Value.RawValue);
        Assert.Equal(isNull, literal.Value.IsNull);
    }
}
