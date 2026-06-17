namespace ButterMorph.UnitTests;

using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Semantics;

/// <summary>
/// Verifies schema path resolution behavior.
/// </summary>
public sealed class SchemaPathResolverTests
{
    /// <summary>
    /// Confirms that simple scalar properties resolve.
    /// </summary>
    [Fact]
    public void ResolveReturnsScalarProperty()
    {
        ISchemaNode root = CreateRootSchema().Root;
        SchemaPathResolver resolver = new();

        ISchemaNode node = resolver.Resolve(root, "Customer.Name");

        Assert.Equal("Name", node.Name);
        Assert.Equal(SchemaNodeKind.Scalar, node.Kind);
    }

    /// <summary>
    /// Confirms that array item paths resolve through item schema.
    /// </summary>
    [Fact]
    public void ResolveReturnsArrayItemProperty()
    {
        ISchemaNode root = CreateRootSchema().Root;
        SchemaPathResolver resolver = new();

        ISchemaNode node = resolver.Resolve(root, "Orders[0].Id");

        Assert.Equal("Id", node.Name);
        Assert.Equal(SchemaNodeKind.Scalar, node.Kind);
    }

    /// <summary>
    /// Confirms that missing paths fail with a key lookup error.
    /// </summary>
    [Fact]
    public void ResolveThrowsWhenPathIsMissing()
    {
        ISchemaNode root = CreateRootSchema().Root;
        SchemaPathResolver resolver = new();

        Assert.Throws<KeyNotFoundException>(() => resolver.Resolve(root, "Customer.Unknown"));
    }

    /// <summary>
    /// Confirms that indexing non-array schema nodes fails.
    /// </summary>
    [Fact]
    public void ResolveThrowsWhenIndexingNonArray()
    {
        ISchemaNode root = CreateRootSchema().Root;
        SchemaPathResolver resolver = new();

        Assert.Throws<InvalidOperationException>(() => resolver.Resolve(root, "Customer[0]"));
    }

    /// <summary>
    /// Confirms that invalid index syntax fails.
    /// </summary>
    [Fact]
    public void ResolveThrowsWhenIndexSyntaxIsInvalid()
    {
        ISchemaNode root = CreateRootSchema().Root;
        SchemaPathResolver resolver = new();

        Assert.Throws<FormatException>(() => resolver.Resolve(root, "Orders[x]"));
    }

    // Creates the schema used by resolver tests.
    private static IStructureSchema CreateRootSchema()
    {
        return new StructureSchema
        {
            Name = "Test",
            Root = new SchemaNode
            {
                Name = "$root",
                Kind = SchemaNodeKind.Object,
                Children =
                [
                    new SchemaNode
                    {
                        Name = "Customer",
                        Kind = SchemaNodeKind.Object,
                        Children =
                        [
                            new SchemaNode
                            {
                                Name = "Name",
                                Kind = SchemaNodeKind.Scalar,
                                DataType = "String"
                            }
                        ]
                    },
                    new SchemaNode
                    {
                        Name = "Orders",
                        Kind = SchemaNodeKind.Array,
                        Children =
                        [
                            new SchemaNode
                            {
                                Name = "$item",
                                Kind = SchemaNodeKind.Object,
                                Children =
                                [
                                    new SchemaNode
                                    {
                                        Name = "Id",
                                        Kind = SchemaNodeKind.Scalar,
                                        DataType = "String"
                                    }
                                ]
                            }
                        ]
                    }
                ]
            }
        };
    }
}
