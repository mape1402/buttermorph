namespace ButterMorph.UnitTests;

using System;
using System.Linq;
using System.Text.Json;
using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Json;

/// <summary>
/// Verifies JSON reader behavior.
/// </summary>
public sealed class JsonReaderTests
{
    /// <summary>
    /// Confirms that nested JSON values are mapped into structure nodes.
    /// </summary>
    [Fact]
    public void ReadMapsNestedJsonToStructureGraph()
    {
        JsonReader reader = new();
        StructureInput input = new()
        {
            Format = "json",
            Content = """{"name":"Ada","age":37,"active":true,"missing":null,"orders":[10,20]}"""
        };

        IStructureGraph graph = reader.Read(input);
        IStructureNode root = graph.Root;
        IStructureNode orders = root.Children.Single(node => node.Name == "orders");
        IScalarStructureNode name = (IScalarStructureNode)root.Children.Single(node => node.Name == "name");
        IScalarStructureNode age = (IScalarStructureNode)root.Children.Single(node => node.Name == "age");
        IScalarStructureNode active = (IScalarStructureNode)root.Children.Single(node => node.Name == "active");
        IScalarStructureNode missing = (IScalarStructureNode)root.Children.Single(node => node.Name == "missing");
        IScalarStructureNode firstOrder = (IScalarStructureNode)orders.Children.First();

        Assert.Equal("$root", root.Name);
        Assert.Equal(StructureNodeKind.Object, root.Kind);
        Assert.Equal(StructureNodeKind.Array, orders.Kind);
        Assert.Equal("0", firstOrder.Name);
        Assert.Equal("String", name.Value.DataType);
        Assert.Equal("Ada", name.Value.RawValue);
        Assert.Equal("Number", age.Value.DataType);
        Assert.Equal("37", age.Value.RawValue);
        Assert.Equal("Boolean", active.Value.DataType);
        Assert.Equal("true", active.Value.RawValue);
        Assert.True(missing.Value.IsNull);
        Assert.Equal("10", firstOrder.Value.RawValue);
        Assert.Equal(8, graph.Nodes.Count);
    }

    /// <summary>
    /// Confirms that scalar root JSON is supported.
    /// </summary>
    [Fact]
    public void ReadSupportsScalarRootJson()
    {
        JsonReader reader = new();
        StructureInput input = new()
        {
            Format = string.Empty,
            Content = "\"hello\""
        };

        IScalarStructureNode root = (IScalarStructureNode)reader.Read(input).Root;

        Assert.Equal("$root", root.Name);
        Assert.Equal(StructureNodeKind.Scalar, root.Kind);
        Assert.Equal("String", root.Value.DataType);
        Assert.Equal("hello", root.Value.RawValue);
    }

    /// <summary>
    /// Confirms that array root JSON is supported.
    /// </summary>
    [Fact]
    public void ReadSupportsArrayRootJson()
    {
        JsonReader reader = new();
        StructureInput input = new()
        {
            Format = "JSON",
            Content = "[1,2]"
        };

        IStructureNode root = reader.Read(input).Root;

        Assert.Equal("$root", root.Name);
        Assert.Equal(StructureNodeKind.Array, root.Kind);
        Assert.Equal(["0", "1"], root.Children.Select(node => node.Name));
    }

    /// <summary>
    /// Confirms that invalid JSON is rejected.
    /// </summary>
    [Fact]
    public void ReadRejectsInvalidJson()
    {
        JsonReader reader = new();
        StructureInput input = new()
        {
            Format = "json",
            Content = "{"
        };

        Assert.ThrowsAny<JsonException>(() => reader.Read(input));
    }

    /// <summary>
    /// Confirms that non-json input formats are rejected.
    /// </summary>
    [Fact]
    public void ReadRejectsNonJsonFormat()
    {
        JsonReader reader = new();
        StructureInput input = new()
        {
            Format = "xml",
            Content = "{}"
        };

        Assert.Throws<InvalidOperationException>(() => reader.Read(input));
    }
}
