namespace ButterMorph.UnitTests;

using System.Collections.Generic;
using System.Linq;
using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Execution;
using ButterMorph.Navigation;
using ButterMorph.Transformations;

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
                SourcePath = "$source.Customer.Name",
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
                SourcePath = "$source.Customer.Name",
                TargetPath = "Customer.FullName"
            },
            new TransformationMapping
            {
                SourcePath = "$source.Orders[0].Id",
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
    public void TransformReturnsDiagnosticWhenSourcePathIsMissing()
    {
        TransformationEngine engine = CreateEngine();
        TransformationRequest request = CreateRequest(
        [
            new TransformationMapping
            {
                SourcePath = "$source.Customer.Unknown",
                TargetPath = "Customer.Unknown"
            }
        ]);

        TransformationResult result = engine.Transform(request);

        Assert.False(result.Succeeded);
        AssertDiagnostic(result, "BMTR002");
    }

    /// <summary>
    /// Confirms that non-scalar source nodes produce diagnostics.
    /// </summary>
    [Fact]
    public void TransformReturnsDiagnosticWhenSourceIsNotScalar()
    {
        TransformationEngine engine = CreateEngine();
        TransformationRequest request = CreateRequest(
        [
            new TransformationMapping
            {
                SourcePath = "$source.Customer",
                TargetPath = "Customer"
            }
        ]);

        TransformationResult result = engine.Transform(request);

        Assert.False(result.Succeeded);
        AssertDiagnostic(result, "BMTR003");
    }

    /// <summary>
    /// Confirms that target array syntax is rejected in transformation v1.
    /// </summary>
    [Fact]
    public void TransformReturnsDiagnosticForTargetArraySyntax()
    {
        TransformationEngine engine = CreateEngine();
        TransformationRequest request = CreateRequest(
        [
            new TransformationMapping
            {
                SourcePath = "$source.Customer.Name",
                TargetPath = "Orders[0].Name"
            }
        ]);

        TransformationResult result = engine.Transform(request);

        Assert.False(result.Succeeded);
        AssertDiagnostic(result, "BMTR004");
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
                SourcePath = "$source.Customer.Name",
                TargetPath = "Customer.Name"
            },
            new TransformationMapping
            {
                SourcePath = "$source.Orders[0].Id",
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

    // Creates the transformation engine with real navigation dependencies.
    private static TransformationEngine CreateEngine()
    {
        return new TransformationEngine(new NavigationEngine(new PathResolver()), new ExecutionContextFactory());
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

    // Confirms that a transformation result contains a diagnostic code.
    private static void AssertDiagnostic(TransformationResult result, string code)
    {
        Assert.Contains(result.Diagnostics, diagnostic => string.Equals(diagnostic.Code, code, System.StringComparison.Ordinal));
        Assert.All(result.Diagnostics, diagnostic => Assert.Equal("Error", diagnostic.Severity));
    }
}
