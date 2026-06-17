namespace ButterMorph.UnitTests;

using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Functions;
using ButterMorph.Navigation;
using ButterMorph.Transformation;

/// <summary>
/// Verifies transformation expression evaluator behavior.
/// </summary>
public sealed class TransformationExpressionEvaluatorTests
{
    /// <summary>
    /// Confirms that scalar paths evaluate to scalar results.
    /// </summary>
    [Fact]
    public void EvaluatePathReturnsScalarResult()
    {
        TransformationExpressionEvaluator evaluator = CreateEvaluator(new FunctionRegistry());

        ITransformationExpressionEvaluationResult result = evaluator.Evaluate(CreateContext(new PathExpression
        {
            Path = "$source.Customer.Name"
        }));

        IScalarFunctionResult scalarResult = Assert.IsAssignableFrom<IScalarFunctionResult>(result.Result);
        Assert.True(result.Succeeded);
        Assert.Equal("Ada", scalarResult.Value.RawValue);
    }

    /// <summary>
    /// Confirms that map-shaped paths evaluate to node results.
    /// </summary>
    [Fact]
    public void EvaluatePathReturnsNodeResult()
    {
        TransformationExpressionEvaluator evaluator = CreateEvaluator(new FunctionRegistry());

        ITransformationExpressionEvaluationResult result = evaluator.Evaluate(CreateContext(new PathExpression
        {
            Path = "$source.Customer"
        }));

        IStructureNodeFunctionResult nodeResult = Assert.IsAssignableFrom<IStructureNodeFunctionResult>(result.Result);
        Assert.True(result.Succeeded);
        Assert.Equal("Customer", nodeResult.Node.Name);
    }

    /// <summary>
    /// Confirms that array paths evaluate to node collection results.
    /// </summary>
    [Fact]
    public void EvaluatePathReturnsNodeCollectionResult()
    {
        TransformationExpressionEvaluator evaluator = CreateEvaluator(new FunctionRegistry());

        ITransformationExpressionEvaluationResult result = evaluator.Evaluate(CreateContext(new PathExpression
        {
            Path = "$source.Orders"
        }));

        IStructureNodeCollectionFunctionResult collectionResult = Assert.IsAssignableFrom<IStructureNodeCollectionFunctionResult>(result.Result);
        Assert.True(result.Succeeded);
        Assert.Single(collectionResult.Nodes);
    }

    /// <summary>
    /// Confirms that scalar literal expressions evaluate to scalar results.
    /// </summary>
    [Fact]
    public void EvaluateScalarLiteralReturnsScalarResult()
    {
        TransformationExpressionEvaluator evaluator = CreateEvaluator(new FunctionRegistry());

        ITransformationExpressionEvaluationResult result = evaluator.Evaluate(CreateContext(new ScalarLiteralExpression
        {
            Value = CreateScalarValue("Literal")
        }));

        IScalarFunctionResult scalarResult = Assert.IsAssignableFrom<IScalarFunctionResult>(result.Result);
        Assert.True(result.Succeeded);
        Assert.Equal("Literal", scalarResult.Value.RawValue);
    }

    /// <summary>
    /// Confirms that scalar collection literals evaluate to scalar collection results.
    /// </summary>
    [Fact]
    public void EvaluateScalarCollectionLiteralReturnsScalarCollectionResult()
    {
        TransformationExpressionEvaluator evaluator = CreateEvaluator(new FunctionRegistry());

        ITransformationExpressionEvaluationResult result = evaluator.Evaluate(CreateContext(new ScalarCollectionLiteralExpression
        {
            Values =
            [
                CreateScalarValue("A"),
                CreateScalarValue("B")
            ]
        }));

        IScalarCollectionFunctionResult collectionResult = Assert.IsAssignableFrom<IScalarCollectionFunctionResult>(result.Result);
        Assert.True(result.Succeeded);
        Assert.Equal(2, collectionResult.Values.Count);
    }

    /// <summary>
    /// Confirms that function call arguments preserve their function value kinds.
    /// </summary>
    [Fact]
    public void EvaluateFunctionCallPassesTypedArguments()
    {
        CapturingFunction function = new(new ScalarFunctionResult
        {
            Value = CreateScalarValue("Done")
        });
        FunctionRegistry registry = new();
        registry.Register("capture", function);
        TransformationExpressionEvaluator evaluator = CreateEvaluator(registry);

        ITransformationExpressionEvaluationResult result = evaluator.Evaluate(CreateContext(new FunctionCallExpression
        {
            FunctionKey = "capture",
            Arguments =
            [
                new PathExpression
                {
                    Path = "$source.Customer.Name"
                },
                new ScalarCollectionLiteralExpression
                {
                    Values =
                    [
                        CreateScalarValue("A")
                    ]
                },
                new PathExpression
                {
                    Path = "$source.Customer"
                },
                new PathExpression
                {
                    Path = "$source.Orders"
                }
            ]
        }));

        Assert.True(result.Succeeded);
        Assert.Equal(FunctionValueKind.Scalar, function.LastArguments[0].Kind);
        Assert.Equal(FunctionValueKind.ScalarCollection, function.LastArguments[1].Kind);
        Assert.Equal(FunctionValueKind.StructureNode, function.LastArguments[2].Kind);
        Assert.Equal(FunctionValueKind.StructureNodeCollection, function.LastArguments[3].Kind);
    }

    /// <summary>
    /// Confirms that missing functions produce diagnostics.
    /// </summary>
    [Fact]
    public void EvaluateFunctionCallReturnsDiagnosticWhenFunctionIsMissing()
    {
        TransformationExpressionEvaluator evaluator = CreateEvaluator(new FunctionRegistry());

        ITransformationExpressionEvaluationResult result = evaluator.Evaluate(CreateContext(new FunctionCallExpression
        {
            FunctionKey = "missing",
            Arguments = []
        }));

        Assert.False(result.Succeeded);
        AssertDiagnostic(result, "BMEX003");
    }

    /// <summary>
    /// Confirms that conditional expressions evaluate the selected branch.
    /// </summary>
    [Fact]
    public void EvaluateConditionalReturnsSelectedBranch()
    {
        TransformationExpressionEvaluator evaluator = CreateEvaluator(new FunctionRegistry());

        ITransformationExpressionEvaluationResult result = evaluator.Evaluate(CreateContext(new ConditionalExpression
        {
            Condition = new ScalarLiteralExpression
            {
                Value = new ScalarValue
                {
                    DataType = "Boolean",
                    RawValue = "true",
                    IsNull = false
                }
            },
            ThenExpression = new ScalarLiteralExpression
            {
                Value = CreateScalarValue("Then")
            },
            ElseExpression = new ScalarLiteralExpression
            {
                Value = CreateScalarValue("Else")
            }
        }));

        IScalarFunctionResult scalarResult = Assert.IsAssignableFrom<IScalarFunctionResult>(result.Result);
        Assert.True(result.Succeeded);
        Assert.Equal("Then", scalarResult.Value.RawValue);
    }

    /// <summary>
    /// Confirms that collection projections expose item aliases.
    /// </summary>
    [Fact]
    public void EvaluateProjectionUsesAliasScope()
    {
        TransformationExpressionEvaluator evaluator = CreateEvaluator(new FunctionRegistry());

        ITransformationExpressionEvaluationResult result = evaluator.Evaluate(CreateContext(new CollectionProjectionExpression
        {
            SourceExpression = new PathExpression
            {
                Path = "$source.Orders"
            },
            ItemAlias = "order",
            BodyExpression = new PathExpression
            {
                Path = "order.Id"
            }
        }));

        IStructureNodeCollectionFunctionResult collectionResult = Assert.IsAssignableFrom<IStructureNodeCollectionFunctionResult>(result.Result);
        IScalarStructureNode node = Assert.IsAssignableFrom<IScalarStructureNode>(Assert.Single(collectionResult.Nodes));

        Assert.True(result.Succeeded);
        Assert.Equal("A1", node.Value.RawValue);
    }

    /// <summary>
    /// Confirms that map-shaped expressions build node results.
    /// </summary>
    [Fact]
    public void EvaluateObjectExpressionBuildsNodeResult()
    {
        TransformationExpressionEvaluator evaluator = CreateEvaluator(new FunctionRegistry());

        ITransformationExpressionEvaluationResult result = evaluator.Evaluate(CreateContext(new ObjectExpression
        {
            Properties =
            [
                new ObjectPropertyExpression
                {
                    Name = "Name",
                    Expression = new PathExpression
                    {
                        Path = "$source.Customer.Name"
                    }
                }
            ]
        }));

        IStructureNodeFunctionResult nodeResult = Assert.IsAssignableFrom<IStructureNodeFunctionResult>(result.Result);
        IScalarStructureNode name = Assert.IsAssignableFrom<IScalarStructureNode>(Assert.Single(nodeResult.Node.Children));

        Assert.True(result.Succeeded);
        Assert.Equal("Name", name.Name);
        Assert.Equal("Ada", name.Value.RawValue);
    }

    /// <summary>
    /// Confirms that ordered expressions build array-shaped node results.
    /// </summary>
    [Fact]
    public void EvaluateArrayExpressionBuildsNodeResult()
    {
        TransformationExpressionEvaluator evaluator = CreateEvaluator(new FunctionRegistry());

        ITransformationExpressionEvaluationResult result = evaluator.Evaluate(CreateContext(new ArrayExpression
        {
            Items =
            [
                new PathExpression
                {
                    Path = "$source.Customer.Name"
                }
            ]
        }));

        IStructureNodeFunctionResult nodeResult = Assert.IsAssignableFrom<IStructureNodeFunctionResult>(result.Result);
        IScalarStructureNode item = Assert.IsAssignableFrom<IScalarStructureNode>(Assert.Single(nodeResult.Node.Children));

        Assert.True(result.Succeeded);
        Assert.Equal(StructureNodeKind.Array, nodeResult.Node.Kind);
        Assert.Equal("0", item.Name);
        Assert.Equal("Ada", item.Value.RawValue);
    }

    // Creates an expression evaluator with real navigation services.
    private static TransformationExpressionEvaluator CreateEvaluator(IFunctionRegistry functionRegistry)
    {
        PathResolver pathResolver = new();
        return new TransformationExpressionEvaluator(new NavigationEngine(pathResolver), pathResolver, functionRegistry);
    }

    // Creates evaluation context for the test source graph.
    private static TransformationExpressionEvaluationContext CreateContext(ITransformationExpression expression)
    {
        return new TransformationExpressionEvaluationContext
        {
            ExecutionContext = new ExecutionContext
            {
                Sources = new Dictionary<string, IStructureGraph>
                {
                    ["source"] = NavigationTestGraphFactory.CreateCustomerGraph()
                },
                Diagnostics = new DiagnosticCollection()
            },
            Expression = expression,
            Aliases = new Dictionary<string, IStructureNode>()
        };
    }

    // Creates a string scalar value for evaluator tests.
    private static IScalarValue CreateScalarValue(string rawValue)
    {
        return new ScalarValue
        {
            DataType = "String",
            RawValue = rawValue,
            IsNull = false
        };
    }

    // Confirms that an evaluation result contains a diagnostic code.
    private static void AssertDiagnostic(ITransformationExpressionEvaluationResult result, string code)
    {
        Assert.Contains(result.Diagnostics, diagnostic => string.Equals(diagnostic.Code, code, System.StringComparison.Ordinal));
        Assert.All(result.Diagnostics, diagnostic => Assert.Equal("Error", diagnostic.Severity));
    }
}
