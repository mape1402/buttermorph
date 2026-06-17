namespace ButterMorph.UnitTests;

using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Verifies structure schema model containers.
/// </summary>
public sealed class SchemaModelTests
{
    /// <summary>
    /// Confirms that schema nodes preserve structure, types, required state, and metadata.
    /// </summary>
    [Fact]
    public void StructureSchemaPreservesNodeDetails()
    {
        SchemaNode name = new()
        {
            Name = "Name",
            Kind = SchemaNodeKind.Scalar,
            DataType = "String",
            IsRequired = true,
            Metadata = new Dictionary<string, string>
            {
                ["label"] = "Customer name"
            }
        };
        SchemaNode root = new()
        {
            Name = "$root",
            Kind = SchemaNodeKind.Object,
            Children =
            [
                name
            ]
        };
        StructureSchema schema = new()
        {
            Name = "Customer",
            Root = root,
            Metadata = new Dictionary<string, string>
            {
                ["source"] = "crm"
            }
        };

        Assert.Equal("Customer", schema.Name);
        Assert.Equal("$root", schema.Root.Name);
        Assert.Equal(SchemaNodeKind.Object, schema.Root.Kind);
        ISchemaNode child = Assert.Single(schema.Root.Children);
        Assert.Equal("Name", child.Name);
        Assert.Equal(SchemaNodeKind.Scalar, child.Kind);
        Assert.Equal("String", child.DataType);
        Assert.True(child.IsRequired);
        Assert.Equal("Customer name", child.Metadata["label"]);
        Assert.Equal("crm", schema.Metadata["source"]);
    }
}
