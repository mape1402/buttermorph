namespace ButterMorph.UnitTests;

using System.Collections.Generic;
using System.Linq;
using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Execution;
using ButterMorph.Functions;
using ButterMorph.Navigation;
using ButterMorph.Transformation;

/// <summary>
/// Verifies minimal transformation engine behavior.
/// </summary>
public sealed class TransformationEngineTests
{
    /// <summary>
    /// Confirms that a scalar source value maps to a target graph path.
    /// </summary>
    [Fact]
    public void TransformMapsScalarToTargetObject()
    {
        TransformationEngine engine = CreateEngine();
        TransformationRequest request = CreateRequest(
        [
            new TransformationMapping
            {
                SourceExpression = CreatePathExpression("$source.Customer.Name"),
                TargetPath = "Customer.FullName"
            }
        ]);

        TransformationResult result = engine.Transform(request);
        IScalarStructureNode target = (IScalarStructureNode)new PathResolver().Resolve(result.ResultGraph.Root, "Customer.FullName");

        Assert.True(result.Succeeded);
        Assert.Empty(result.Diagnostics);
        Assert.Equal("Ada", target.Value.RawValue);
        Assert.Equal("String", target.Value.DataType);
    }

    /// <summary>
    /// Confirms that multiple mappings reuse shared target objects.
    /// </summary>
    [Fact]
    public void TransformMapsMultipleValuesToSharedTargetObject()
    {
        TransformationEngine engine = CreateEngine();
        TransformationRequest request = CreateRequest(
        [
            new TransformationMapping
            {
                SourceExpression = CreatePathExpression("$source.Customer.Name"),
                TargetPath = "Customer.FullName"
            },
            new TransformationMapping
            {
                SourceExpression = CreatePathExpression("$source.Orders[0].Id"),
                TargetPath = "Customer.FirstOrderId"
            }
        ]);

        TransformationResult result = engine.Transform(request);
        IScalarStructureNode fullName = (IScalarStructureNode)new PathResolver().Resolve(result.ResultGraph.Root, "Customer.FullName");
        IScalarStructureNode firstOrderId = (IScalarStructureNode)new PathResolver().Resolve(result.ResultGraph.Root, "Customer.FirstOrderId");

        Assert.True(result.Succeeded);
        Assert.Equal("Ada", fullName.Value.RawValue);
        Assert.Equal("A1", firstOrderId.Value.RawValue);
    }

    /// <summary>
    /// Confirms that missing source paths produce diagnostics.
    /// </summary>
    [Fact]
    public void TransformReturnsDiagnosticWhenSourceExpressionPathIsMissing()
    {
        TransformationEngine engine = CreateEngine();
        TransformationRequest request = CreateRequest(
        [
            new TransformationMapping
            {
                SourceExpression = CreatePathExpression("$source.Customer.Unknown"),
                TargetPath = "Customer.Unknown"
            }
        ]);

        TransformationResult result = engine.Transform(request);

        Assert.False(result.Succeeded);
        AssertDiagnostic(result, "BMTR002");
    }

    /// <summary>
    /// Confirms that source nodes can be assigned as target subtrees.
    /// </summary>
    [Fact]
    public void TransformMapsSourceNodeToTargetObject()
    {
        TransformationEngine engine = CreateEngine();
        TransformationRequest request = CreateRequest(
        [
            new TransformationMapping
            {
                SourceExpression = CreatePathExpression("$source.Customer"),
                TargetPath = "Customer"
            }
        ]);

        TransformationResult result = engine.Transform(request);
        IScalarStructureNode target = (IScalarStructureNode)new PathResolver().Resolve(result.ResultGraph.Root, "Customer.Name");

        Assert.True(result.Succeeded);
        Assert.Equal("Ada", target.Value.RawValue);
    }

    /// <summary>
    /// Confirms that target array syntax builds ordered target nodes.
    /// </summary>
    [Fact]
    public void TransformMapsScalarToTargetArrayPath()
    {
        TransformationEngine engine = CreateEngine();
        TransformationRequest request = CreateRequest(
        [
            new TransformationMapping
            {
                SourceExpression = CreatePathExpression("$source.Customer.Name"),
                TargetPath = "Orders[0].Name"
            }
        ]);

        TransformationResult result = engine.Transform(request);
        IScalarStructureNode target = (IScalarStructureNode)new PathResolver().Resolve(result.ResultGraph.Root, "Orders[0].Name");

        Assert.True(result.Succeeded);
        Assert.Equal("Ada", target.Value.RawValue);
    }

    /// <summary>
    /// Confirms that duplicated target paths are rejected.
    /// </summary>
    [Fact]
    public void TransformReturnsDiagnosticForDuplicateTargetPath()
    {
        TransformationEngine engine = CreateEngine();
        TransformationRequest request = CreateRequest(
        [
            new TransformationMapping
            {
                SourceExpression = CreatePathExpression("$source.Customer.Name"),
                TargetPath = "Customer.Name"
            },
            new TransformationMapping
            {
                SourceExpression = CreatePathExpression("$source.Orders[0].Id"),
                TargetPath = "Customer.Name"
            }
        ]);

        TransformationResult result = engine.Transform(request);

        Assert.False(result.Succeeded);
        AssertDiagnostic(result, "BMTR005");
    }

    /// <summary>
    /// Confirms that non-transformation documents are rejected.
    /// </summary>
    [Fact]
    public void TransformReturnsDiagnosticWhenDefinitionIsNotTransformationDocument()
    {
        TransformationEngine engine = CreateEngine();
        TransformationRequest request = new()
        {
            Sources = CreateSources(),
            Definition = new DslDocument()
        };

        TransformationResult result = engine.Transform(request);

        Assert.False(result.Succeeded);
        AssertDiagnostic(result, "BMTR001");
    }

    /// <summary>
    /// Confirms that scalar literal expressions can be assigned.
    /// </summary>
    [Fact]
    public void TransformMapsScalarLiteralToTarget()
    {
        TransformationEngine engine = CreateEngine();
        TransformationRequest request = CreateRequest(
        [
            new TransformationMapping
            {
                SourceExpression = new ScalarLiteralExpression
                {
                    Value = new ScalarValue
                    {
                        DataType = "String",
                        RawValue = "Ada",
                        IsNull = false
                    }
                },
                TargetPath = "Customer.Name"
            }
        ]);

        TransformationResult result = engine.Transform(request);
        IScalarStructureNode target = (IScalarStructureNode)new PathResolver().Resolve(result.ResultGraph.Root, "Customer.Name");

        Assert.True(result.Succeeded);
        Assert.Equal("Ada", target.Value.RawValue);
    }

    /// <summary>
    /// Confirms that scalar collections can be assigned to target arrays.
    /// </summary>
    [Fact]
    public void TransformMapsScalarCollectionToTargetArray()
    {
        TransformationEngine engine = CreateEngine();
        TransformationRequest request = CreateRequest(
        [
            new TransformationMapping
            {
                SourceExpression = new ScalarCollectionLiteralExpression
                {
                    Values =
                    [
                        CreateScalarValue("A"),
                        CreateScalarValue("B")
                    ]
                },
                TargetPath = "Tags"
            }
        ]);

        TransformationResult result = engine.Transform(request);
        IScalarStructureNode first = (IScalarStructureNode)new PathResolver().Resolve(result.ResultGraph.Root, "Tags[0]");
        IScalarStructureNode second = (IScalarStructureNode)new PathResolver().Resolve(result.ResultGraph.Root, "Tags[1]");

        Assert.True(result.Succeeded);
        Assert.Equal("A", first.Value.RawValue);
        Assert.Equal("B", second.Value.RawValue);
    }

    /// <summary>
    /// Confirms that collection results cannot be assigned directly to indexed targets.
    /// </summary>
    [Fact]
    public void TransformReturnsDiagnosticWhenCollectionTargetsIndexedPath()
    {
        TransformationEngine engine = CreateEngine();
        TransformationRequest request = CreateRequest(
        [
            new TransformationMapping
            {
                SourceExpression = new ScalarCollectionLiteralExpression
                {
                    Values =
                    [
                        CreateScalarValue("A")
                    ]
                },
                TargetPath = "Tags[0]"
            }
        ]);

        TransformationResult result = engine.Transform(request);

        Assert.False(result.Succeeded);
        AssertDiagnostic(result, "BMTR006");
    }

    /// <summary>
    /// Confirms that function scalar results can be assigned to target paths.
    /// </summary>
    [Fact]
    public void TransformMapsFunctionResultToTarget()
    {
        FunctionRegistry registry = new();
        registry.Register("capture", new CapturingFunction(new ScalarFunctionResult
        {
            Value = CreateScalarValue("FunctionValue")
        }));
        TransformationEngine engine = CreateEngine(registry);
        TransformationRequest request = CreateRequest(
        [
            new TransformationMapping
            {
                SourceExpression = new FunctionCallExpression
                {
                    FunctionKey = "capture",
                    Arguments = []
                },
                TargetPath = "Customer.Value"
            }
        ]);

        TransformationResult result = engine.Transform(request);
        IScalarStructureNode target = (IScalarStructureNode)new PathResolver().Resolve(result.ResultGraph.Root, "Customer.Value");

        Assert.True(result.Succeeded);
        Assert.Equal("FunctionValue", target.Value.RawValue);
    }

    /// <summary>
    /// Confirms that projections can map source arrays to target arrays.
    /// </summary>
    [Fact]
    public void TransformMapsProjectionToTargetArray()
    {
        TransformationEngine engine = CreateEngine();
        TransformationRequest request = CreateRequest(
        [
            new TransformationMapping
            {
                SourceExpression = new CollectionProjectionExpression
                {
                    SourceExpression = CreatePathExpression("$source.Orders"),
                    ItemAlias = "order",
                    BodyExpression = CreatePathExpression("order.Id")
                },
                TargetPath = "OrderIds"
            }
        ]);

        TransformationResult result = engine.Transform(request);
        IScalarStructureNode target = (IScalarStructureNode)new PathResolver().Resolve(result.ResultGraph.Root, "OrderIds[0]");

        Assert.True(result.Succeeded);
        Assert.Equal("A1", target.Value.RawValue);
    }

    // Creates the transformation engine with real navigation dependencies.
    private static TransformationEngine CreateEngine()
    {
        return CreateEngine(new FunctionRegistry());
    }

    // Creates the transformation engine with a function registry.
    private static TransformationEngine CreateEngine(IFunctionRegistry functionRegistry)
    {
        PathResolver pathResolver = new();
        NavigationEngine navigationEngine = new(pathResolver);
        TransformationExpressionEvaluator evaluator = new(navigationEngine, pathResolver, functionRegistry);
        return new TransformationEngine(evaluator, new ExecutionContextFactory());
    }

    // Creates a transformation request with test sources and mappings.
    private static TransformationRequest CreateRequest(IReadOnlyCollection<ITransformationMapping> mappings)
    {
        return new TransformationRequest
        {
            Sources = CreateSources(),
            Definition = new TransformationDocument
            {
                Definition = new DslDefinition
                {
                    Content = string.Empty
                },
                Mappings = mappings
            }
        };
    }

    // Creates source graphs used by transformation tests.
    private static IReadOnlyDictionary<string, IStructureGraph> CreateSources()
    {
        return new Dictionary<string, IStructureGraph>
        {
            ["source"] = NavigationTestGraphFactory.CreateCustomerGraph()
        };
    }

    // Creates a path expression for transformation tests.
    private static IPathExpression CreatePathExpression(string path)
    {
        return new PathExpression
        {
            Path = path
        };
    }

    // Creates a string scalar value for transformation tests.
    private static IScalarValue CreateScalarValue(string rawValue)
    {
        return new ScalarValue
        {
            DataType = "String",
            RawValue = rawValue,
            IsNull = false
        };
    }

    // Confirms that a transformation result contains a diagnostic code.
    private static void AssertDiagnostic(TransformationResult result, string code)
    {
        Assert.Contains(result.Diagnostics, diagnostic => string.Equals(diagnostic.Code, code, System.StringComparison.Ordinal));
        Assert.All(result.Diagnostics, diagnostic => Assert.Equal("Error", diagnostic.Severity));
    }
}
