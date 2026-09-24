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

    /// <summary>
    /// Confirms that transformation documents preserve schemas and metadata.
    /// </summary>
    [Fact]
    public void TransformationDocumentPreservesDesignMetadata()
    {
        StructureSchema sourceSchema = new()
        {
            Name = "Source",
            Root = new SchemaNode
            {
                Name = "$root",
                Kind = SchemaNodeKind.Object
            }
        };
        StructureSchema targetSchema = new()
        {
            Name = "Target",
            Root = new SchemaNode
            {
                Name = "$root",
                Kind = SchemaNodeKind.Object
            }
        };
        TransformationDocument document = new()
        {
            SourceSchemas = new Dictionary<string, IStructureSchema>
            {
                ["source"] = sourceSchema
            },
            TargetSchema = targetSchema,
            Metadata = new Dictionary<string, string>
            {
                ["owner"] = "ui"
            }
        };

        Assert.Same(sourceSchema, document.SourceSchemas["source"]);
        Assert.Same(targetSchema, document.TargetSchema);
        Assert.Equal("ui", document.Metadata["owner"]);
    }

    /// <summary>
    /// Confirms that validation documents preserve validation assertions.
    /// </summary>
    [Fact]
    public void ValidationDocumentPreservesAssertions()
    {
        ValidationAssertion assertion = new()
        {
            Expression = new ScalarLiteralExpression
            {
                Value = new ScalarValue
                {
                    DataType = "Boolean",
                    RawValue = "true"
                }
            },
            Message = "Must be true.",
            Path = "$source.Name"
        };
        ValidationDocument document = new()
        {
            Assertions =
            [
                assertion
            ]
        };

        Assert.Same(assertion, Assert.Single(document.Assertions));
    }
}
