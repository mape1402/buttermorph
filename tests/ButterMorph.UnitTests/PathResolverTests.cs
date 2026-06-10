namespace ButterMorph.UnitTests;

using System;
using System.Collections.Generic;
using ButterMorph.Abstractions;
using ButterMorph.Navigation;

/// <summary>
/// Verifies path resolver behavior.
/// </summary>
public sealed class PathResolverTests
{
    /// <summary>
    /// Confirms that simple property paths resolve by node name.
    /// </summary>
    [Fact]
    public void ResolveFindsSimpleProperty()
    {
        IStructureGraph graph = NavigationTestGraphFactory.CreateCustomerGraph();
        IPathResolver resolver = new PathResolver();

        IStructureNode node = resolver.Resolve(graph.Root, "Customer");

        Assert.Equal("Customer", node.Name);
        Assert.Equal(StructureNodeKind.Object, node.Kind);
    }

    /// <summary>
    /// Confirms that nested property paths resolve by node name.
    /// </summary>
    [Fact]
    public void ResolveFindsNestedProperty()
    {
        IStructureGraph graph = NavigationTestGraphFactory.CreateCustomerGraph();
        IPathResolver resolver = new PathResolver();

        IScalarStructureNode node = (IScalarStructureNode)resolver.Resolve(graph.Root, "Customer.Name");

        Assert.Equal("Ada", node.Value.RawValue);
    }

    /// <summary>
    /// Confirms that array index paths resolve by child order.
    /// </summary>
    [Fact]
    public void ResolveFindsArrayIndex()
    {
        IStructureGraph graph = NavigationTestGraphFactory.CreateCustomerGraph();
        IPathResolver resolver = new PathResolver();

        IStructureNode node = resolver.Resolve(graph.Root, "Orders[0]");

        Assert.Equal("0", node.Name);
    }

    /// <summary>
    /// Confirms that array index paths can continue into child nodes.
    /// </summary>
    [Fact]
    public void ResolveFindsArrayIndexProperty()
    {
        IStructureGraph graph = NavigationTestGraphFactory.CreateCustomerGraph();
        IPathResolver resolver = new PathResolver();

        IScalarStructureNode node = (IScalarStructureNode)resolver.Resolve(graph.Root, "Orders[0].Id");

        Assert.Equal("A1", node.Value.RawValue);
    }

    /// <summary>
    /// Confirms that an empty path returns the root node.
    /// </summary>
    [Fact]
    public void ResolveReturnsRootForEmptyPath()
    {
        IStructureGraph graph = NavigationTestGraphFactory.CreateCustomerGraph();
        IPathResolver resolver = new PathResolver();

        IStructureNode node = resolver.Resolve(graph.Root, string.Empty);

        Assert.Same(graph.Root, node);
    }

    /// <summary>
    /// Confirms that $root returns the root node.
    /// </summary>
    [Fact]
    public void ResolveReturnsRootForRootToken()
    {
        IStructureGraph graph = NavigationTestGraphFactory.CreateCustomerGraph();
        IPathResolver resolver = new PathResolver();

        IStructureNode node = resolver.Resolve(graph.Root, "$root");

        Assert.Same(graph.Root, node);
    }

    /// <summary>
    /// Confirms that missing children throw.
    /// </summary>
    [Fact]
    public void ResolveThrowsForMissingProperty()
    {
        IStructureGraph graph = NavigationTestGraphFactory.CreateCustomerGraph();
        IPathResolver resolver = new PathResolver();

        Assert.Throws<KeyNotFoundException>(() => resolver.Resolve(graph.Root, "Customer.Unknown"));
    }

    /// <summary>
    /// Confirms that invalid indexes throw.
    /// </summary>
    [Fact]
    public void ResolveThrowsForInvalidIndex()
    {
        IStructureGraph graph = NavigationTestGraphFactory.CreateCustomerGraph();
        IPathResolver resolver = new PathResolver();

        Assert.Throws<FormatException>(() => resolver.Resolve(graph.Root, "Orders[x]"));
    }

    /// <summary>
    /// Confirms that out of range indexes throw.
    /// </summary>
    [Fact]
    public void ResolveThrowsForOutOfRangeIndex()
    {
        IStructureGraph graph = NavigationTestGraphFactory.CreateCustomerGraph();
        IPathResolver resolver = new PathResolver();

        Assert.Throws<IndexOutOfRangeException>(() => resolver.Resolve(graph.Root, "Orders[1]"));
    }

    /// <summary>
    /// Confirms that scalar traversal throws.
    /// </summary>
    [Fact]
    public void ResolveThrowsWhenTraversingScalar()
    {
        IStructureGraph graph = NavigationTestGraphFactory.CreateCustomerGraph();
        IPathResolver resolver = new PathResolver();

        Assert.Throws<InvalidOperationException>(() => resolver.Resolve(graph.Root, "Customer.Name.First"));
    }
}
