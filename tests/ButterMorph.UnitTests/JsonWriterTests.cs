namespace ButterMorph.UnitTests;

using System.Collections.Generic;
using System.Text.Json;
using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Json;

/// <summary>
/// Verifies JSON writer behavior.
/// </summary>
public sealed class JsonWriterTests
{
    /// <summary>
    /// Confirms that graph maps are written as JSON maps.
    /// </summary>
    [Fact]
    public void WriteEmitsJsonForObjectGraph()
    {
        StructureGraph graph = new()
        {
            Root = new StructureNode
            {
                Name = "$root",
                Kind = StructureNodeKind.Object,
                Children =
                [
                    CreateScalarNode("name", "String", "Ada", false),
                    CreateScalarNode("age", "Number", "37", false),
                    CreateScalarNode("active", "Boolean", "true", false),
                    CreateScalarNode("missing", "Null", string.Empty, true),
                    new StructureNode
                    {
                        Name = "scores",
                        Kind = StructureNodeKind.Array,
                        Children =
                        [
                            CreateScalarNode("0", "Number", "10", false),
                            CreateScalarNode("1", "Number", "20", false)
                        ]
                    }
                ]
            }
        };
        JsonWriter writer = new();

        IStructureOutput output = writer.Write(graph);

        Assert.Equal("json", output.Format);
        AssertJsonEquivalent("""{"name":"Ada","age":37,"active":true,"missing":null,"scores":[10,20]}""", output.Content);
    }

    /// <summary>
    /// Confirms that scalar root graphs are written as JSON scalars.
    /// </summary>
    [Fact]
    public void WriteEmitsJsonForScalarRootGraph()
    {
        StructureGraph graph = new()
        {
            Root = CreateScalarNode("$root", "String", "hello", false)
        };
        JsonWriter writer = new();

        IStructureOutput output = writer.Write(graph);

        AssertJsonEquivalent("\"hello\"", output.Content);
    }

    /// <summary>
    /// Confirms that array root graphs are written as JSON arrays.
    /// </summary>
    [Fact]
    public void WriteEmitsJsonForArrayRootGraph()
    {
        StructureGraph graph = new()
        {
            Root = new StructureNode
            {
                Name = "$root",
                Kind = StructureNodeKind.Array,
                Children =
                [
                    CreateScalarNode("0", "Boolean", "false", false),
                    CreateScalarNode("1", "Null", string.Empty, true)
                ]
            }
        };
        JsonWriter writer = new();

        IStructureOutput output = writer.Write(graph);

        AssertJsonEquivalent("[false,null]", output.Content);
    }

    /// <summary>
    /// Confirms that read-write preserves equivalent JSON semantics.
    /// </summary>
    [Fact]
    public void ReadWriteRoundTripPreservesJsonSemantics()
    {
        string json = """{"customer":{"name":"Ada"},"items":[{"sku":"A1","qty":2}],"enabled":false}""";
        JsonReader reader = new();
        JsonWriter writer = new();
        StructureInput input = new()
        {
            Format = "json",
            Content = json
        };

        IStructureOutput output = writer.Write(reader.Read(input));

        AssertJsonEquivalent(json, output.Content);
    }

    // Creates a scalar node for writer-focused unit tests.
    private static IStructureNode CreateScalarNode(string name, string dataType, string rawValue, bool isNull)
    {
        return new ScalarStructureNode
        {
            Name = name,
            Value = new ScalarValue
            {
                DataType = dataType,
                RawValue = rawValue,
                IsNull = isNull
            }
        };
    }

    // Compares JSON by document semantics instead of raw formatting.
    private static void AssertJsonEquivalent(string expected, string actual)
    {
        using JsonDocument expectedDocument = JsonDocument.Parse(expected);
        using JsonDocument actualDocument = JsonDocument.Parse(actual);

        Assert.Equal(expectedDocument.RootElement.ToString(), actualDocument.RootElement.ToString());
    }
}
