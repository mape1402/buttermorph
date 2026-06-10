namespace ButterMorph.UnitTests;

using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Creates structure graphs used by navigation tests.
/// </summary>
internal static class NavigationTestGraphFactory
{
    /// <summary>
    /// Creates a graph with customer and orders nodes.
    /// </summary>
    /// <returns>The test structure graph.</returns>
    public static IStructureGraph CreateCustomerGraph()
    {
        IStructureNode name = CreateScalar("Name", "Ada");
        IStructureNode id = CreateScalar("Id", "A1");
        IStructureNode customer = new StructureNode
        {
            Name = "Customer",
            Kind = StructureNodeKind.Object,
            Children =
            [
                name
            ]
        };
        IStructureNode firstOrder = new StructureNode
        {
            Name = "0",
            Kind = StructureNodeKind.Object,
            Children =
            [
                id
            ]
        };
        IStructureNode orders = new StructureNode
        {
            Name = "Orders",
            Kind = StructureNodeKind.Array,
            Children =
            [
                firstOrder
            ]
        };
        IStructureNode root = new StructureNode
        {
            Name = "$root",
            Kind = StructureNodeKind.Object,
            Children =
            [
                customer,
                orders
            ]
        };

        return new StructureGraph
        {
            Root = root,
            Nodes =
            [
                root,
                customer,
                name,
                orders,
                firstOrder,
                id
            ]
        };
    }

    /// <summary>
    /// Creates a graph with a status node.
    /// </summary>
    /// <returns>The test structure graph.</returns>
    public static IStructureGraph CreateStatusGraph()
    {
        IStructureNode status = CreateScalar("Status", "Active");
        IStructureNode root = new StructureNode
        {
            Name = "$root",
            Kind = StructureNodeKind.Object,
            Children =
            [
                status
            ]
        };

        return new StructureGraph
        {
            Root = root,
            Nodes =
            [
                root,
                status
            ]
        };
    }

    // Creates a string scalar node for navigation tests.
    private static IStructureNode CreateScalar(string name, string rawValue)
    {
        return new ScalarStructureNode
        {
            Name = name,
            Value = new ScalarValue
            {
                DataType = "String",
                RawValue = rawValue,
                IsNull = false
            }
        };
    }
}
