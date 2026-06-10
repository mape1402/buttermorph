namespace ButterMorph.UnitTests;

using System;
using System.Collections.Generic;
using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Navigation;

/// <summary>
/// Verifies navigation engine behavior.
/// </summary>
public sealed class NavigationEngineTests
{
    /// <summary>
    /// Confirms that source-qualified paths resolve through the execution context.
    /// </summary>
    [Fact]
    public void SelectResolvesSourceQualifiedPath()
    {
        IExecutionContext context = CreateContext();
        INavigationEngine engine = new NavigationEngine(new PathResolver());

        IScalarStructureNode node = (IScalarStructureNode)engine.Select(context, "$source.Customer.Name");

        Assert.Equal("Ada", node.Value.RawValue);
    }

    /// <summary>
    /// Confirms that different source aliases resolve independently.
    /// </summary>
    [Fact]
    public void SelectResolvesMultipleSources()
    {
        IExecutionContext context = CreateContext();
        INavigationEngine engine = new NavigationEngine(new PathResolver());

        IScalarStructureNode node = (IScalarStructureNode)engine.Select(context, "$secondary.Status");

        Assert.Equal("Active", node.Value.RawValue);
    }

    /// <summary>
    /// Confirms that a source alias path returns the source root.
    /// </summary>
    [Fact]
    public void SelectReturnsSourceRoot()
    {
        IExecutionContext context = CreateContext();
        INavigationEngine engine = new NavigationEngine(new PathResolver());

        IStructureNode node = engine.Select(context, "$source");

        Assert.Same(context.Sources["source"].Root, node);
    }

    /// <summary>
    /// Confirms that paths without a source token throw.
    /// </summary>
    [Fact]
    public void SelectThrowsWhenPathDoesNotStartWithSourceToken()
    {
        IExecutionContext context = CreateContext();
        INavigationEngine engine = new NavigationEngine(new PathResolver());

        Assert.Throws<FormatException>(() => engine.Select(context, "source.Customer.Name"));
    }

    /// <summary>
    /// Confirms that missing source aliases throw.
    /// </summary>
    [Fact]
    public void SelectThrowsWhenSourceIsMissing()
    {
        IExecutionContext context = CreateContext();
        INavigationEngine engine = new NavigationEngine(new PathResolver());

        Assert.Throws<KeyNotFoundException>(() => engine.Select(context, "$missing.Customer.Name"));
    }

    // Creates an execution context with multiple source graphs.
    private static IExecutionContext CreateContext()
    {
        return new ExecutionContext
        {
            Sources = new Dictionary<string, IStructureGraph>
            {
                ["source"] = NavigationTestGraphFactory.CreateCustomerGraph(),
                ["secondary"] = NavigationTestGraphFactory.CreateStatusGraph()
            },
            Diagnostics = new DiagnosticCollection()
        };
    }
}
