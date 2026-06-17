namespace ButterMorph.UnitTests;

using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Design;
using ButterMorph.Functions;

/// <summary>
/// Verifies design explorers.
/// </summary>
public sealed class DesignExplorerTests
{
    /// <summary>
    /// Confirms that schema explorer creates UI-ready paths.
    /// </summary>
    [Fact]
    public void SchemaExplorerCreatesNavigableTree()
    {
        SchemaExplorer explorer = new();

        ISchemaTreeNode root = explorer.Explore(CreateSchema());

        Assert.Equal("$root", root.Path);
        Assert.Contains(root.Children, child => child.Path == "Orders");
        ISchemaTreeNode orders = root.Children.First(child => child.Path == "Orders");
        Assert.Contains(orders.Children, child => child.Path == "Orders[0]");
    }

    /// <summary>
    /// Confirms that capability explorer lists descriptors.
    /// </summary>
    [Fact]
    public void CapabilityExplorerListsDescriptors()
    {
        FunctionRegistry functions = new();
        ValidationRuleRegistry validations = new();
        functions.Register("concat", new CapturingFunction(new ScalarFunctionResult
        {
            Value = new ScalarValue()
        }), new FunctionDescriptor
        {
            Key = "concat",
            DisplayName = "Concat",
            Description = "Concat",
            ValueKind = FunctionValueKind.Scalar
        });
        validations.Register("required", new PassingValidationRuleHandler(), new ValidationRuleDescriptor
        {
            Key = "required",
            DisplayName = "Required",
            Description = "Required",
            ValueKind = FunctionValueKind.Scalar
        });
        CapabilityExplorer explorer = new(functions, validations);

        Assert.Single(explorer.ListFunctions());
        Assert.Single(explorer.ListValidationRules());
    }

    // Creates a schema with array item paths.
    private static IStructureSchema CreateSchema()
    {
        return new StructureSchema
        {
            Name = "Schema",
            Root = new SchemaNode
            {
                Name = "$root",
                Kind = SchemaNodeKind.Object,
                Children =
                [
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
