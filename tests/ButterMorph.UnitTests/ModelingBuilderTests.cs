namespace ButterMorph.UnitTests;

using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Execution;
using ButterMorph.Functions;
using ButterMorph.Modeling;
using ButterMorph.Navigation;
using ButterMorph.Transformation;

/// <summary>
/// Verifies ButterMorph modeling builders.
/// </summary>
public sealed class ModelingBuilderTests
{
    /// <summary>
    /// Confirms that document builder preserves modeling data.
    /// </summary>
    [Fact]
    public void DocumentBuilderPreservesSchemasMappingsValidationsAndMetadata()
    {
        IStructureSchema sourceSchema = CreateCustomerSchema();
        IStructureSchema targetSchema = ButterMorphModel.CreateSchema("Target").Build();
        ValidationRule validation = new()
        {
            Path = "Customer.Name",
            RuleKey = "required"
        };

        ITransformationDocument document = ButterMorphModel.CreateDocument()
            .WithSourceSchema("source", sourceSchema)
            .WithTargetSchema(targetSchema)
            .MapPath("$source.Customer.Name", "Customer.Name")
            .WithValidation(validation)
            .WithMetadata("owner", "ui")
            .Build();

        ITransformationMapping mapping = Assert.Single(document.Mappings);

        Assert.Same(sourceSchema, document.SourceSchemas["source"]);
        Assert.Same(targetSchema, document.TargetSchema);
        Assert.Same(validation, Assert.Single(document.Validations));
        Assert.Equal("ui", document.Metadata["owner"]);
        Assert.IsAssignableFrom<IPathExpression>(mapping.SourceExpression);
        Assert.Equal("Customer.Name", mapping.TargetPath);
    }

    /// <summary>
    /// Confirms that mapping builder creates mappings.
    /// </summary>
    [Fact]
    public void MappingBuilderCreatesMapping()
    {
        ITransformationMapping mapping = new TransformationMappingBuilder()
            .FromPath("$source.Customer.Name")
            .To("Customer.Name")
            .Build();

        IPathExpression expression = Assert.IsAssignableFrom<IPathExpression>(mapping.SourceExpression);

        Assert.Equal("$source.Customer.Name", expression.Path);
        Assert.Equal("Customer.Name", mapping.TargetPath);
    }

    /// <summary>
    /// Confirms that schema builder preserves node details.
    /// </summary>
    [Fact]
    public void SchemaBuilderPreservesStructure()
    {
        ISchemaNode name = ButterMorphModel.CreateNode()
            .Scalar("Name", "String")
            .Required()
            .WithMetadata("label", "Customer name")
            .Build();
        ISchemaNode item = ButterMorphModel.CreateNode()
            .Object("$item")
            .WithChild(name)
            .Build();
        ISchemaNode orders = ButterMorphModel.CreateNode()
            .Array("Orders", item)
            .Build();
        ISchemaNode root = ButterMorphModel.CreateNode()
            .Object("$root")
            .WithChild(orders)
            .Build();

        IStructureSchema schema = ButterMorphModel.CreateSchema("Customer")
            .WithRoot(root)
            .WithMetadata("source", "crm")
            .Build();

        ISchemaNode arrayNode = Assert.Single(schema.Root.Children);
        ISchemaNode itemNode = Assert.Single(arrayNode.Children);
        ISchemaNode nameNode = Assert.Single(itemNode.Children);

        Assert.Equal("Customer", schema.Name);
        Assert.Equal("customer", schema.Key);
        Assert.Equal(SchemaNodeKind.Array, arrayNode.Kind);
        Assert.Equal("String", nameNode.DataType);
        Assert.True(nameNode.IsRequired);
        Assert.Equal("Customer name", nameNode.Metadata["label"]);
        Assert.Equal("crm", schema.Metadata["source"]);
    }

    /// <summary>
    /// Confirms that expression builder creates all expression shapes.
    /// </summary>
    [Fact]
    public void ExpressionBuilderCreatesExpressionShapes()
    {
        IExpressionBuilder expressions = ButterMorphModel.Expressions;
        IPathExpression path = expressions.Path("$source.Name");
        IScalarLiteralExpression scalar = expressions.Scalar("String", "Ada");
        IScalarLiteralExpression nullScalar = expressions.NullScalar();
        IScalarLiteralExpression boolean = expressions.Boolean(true);
        IScalarLiteralExpression number = expressions.Number("42");
        IScalarCollectionLiteralExpression collection = expressions.ScalarCollection(
        [
            scalar.Value
        ]);
        IFunctionCallExpression function = expressions.Function("concat", [path, scalar]);
        IConditionalExpression conditional = expressions.When(boolean, scalar, nullScalar);
        ICollectionProjectionExpression projection = expressions.Project(path, "item", scalar);
        IObjectPropertyExpression property = expressions.Property("Name", scalar);
        IObjectExpression map = expressions.Object([property]);
        IArrayExpression array = expressions.Array([number]);

        Assert.Equal(TransformationExpressionKind.Path, path.Kind);
        Assert.Equal("Ada", scalar.Value.RawValue);
        Assert.True(nullScalar.Value.IsNull);
        Assert.Equal("true", boolean.Value.RawValue);
        Assert.Equal("42", number.Value.RawValue);
        Assert.Single(collection.Values);
        Assert.Equal("concat", function.FunctionKey);
        Assert.Same(scalar, conditional.ThenExpression);
        Assert.Equal("item", projection.ItemAlias);
        Assert.Equal("Name", property.Name);
        Assert.Single(map.Properties);
        Assert.Single(array.Items);
    }

    /// <summary>
    /// Confirms that builder output can execute through the transformation engine.
    /// </summary>
    [Fact]
    public void BuilderDocumentExecutesThroughTransformationEngine()
    {
        ITransformationDocument document = ButterMorphModel.CreateDocument()
            .WithSourceSchema("source", CreateCustomerSchema())
            .WithTargetSchema(ButterMorphModel.CreateSchema("Target").Build())
            .MapPath("$source.Customer.Name", "Customer.Name")
            .Map(
                ButterMorphModel.Expressions.Project(
                    ButterMorphModel.Expressions.Path("$source.Orders"),
                    "order",
                    ButterMorphModel.Expressions.Path("order.Id")),
                "OrderIds")
            .Build();
        TransformationEngine engine = CreateEngine();

        TransformationResult result = engine.Transform(new TransformationRequest
        {
            Sources = new Dictionary<string, IStructureGraph>
            {
                ["source"] = NavigationTestGraphFactory.CreateCustomerGraph()
            },
            Definition = document
        });
        PathResolver resolver = new();
        IScalarStructureNode name = Assert.IsAssignableFrom<IScalarStructureNode>(resolver.Resolve(result.ResultGraph.Root, "Customer.Name"));
        IScalarStructureNode orderId = Assert.IsAssignableFrom<IScalarStructureNode>(resolver.Resolve(result.ResultGraph.Root, "OrderIds[0]"));

        Assert.True(result.Succeeded);
        Assert.Equal("Ada", name.Value.RawValue);
        Assert.Equal("A1", orderId.Value.RawValue);
    }

    /// <summary>
    /// Confirms that obvious invalid builder inputs throw clear exceptions.
    /// </summary>
    [Fact]
    public void BuildersRejectInvalidInputs()
    {
        Assert.Throws<ArgumentException>(() => ButterMorphModel.CreateSchema(string.Empty));
        Assert.Throws<ArgumentException>(() => ButterMorphModel.CreateSchema("Customer").WithKey(string.Empty).Build());
        Assert.Throws<ArgumentException>(() => ButterMorphModel.CreateNode().Scalar(string.Empty, "String"));
        Assert.Throws<ArgumentException>(() => ButterMorphModel.Expressions.Path(string.Empty));
        Assert.Throws<ArgumentException>(() => ButterMorphModel.Expressions.Function(string.Empty, []));
        Assert.Throws<ArgumentException>(() => ButterMorphModel.Expressions.Project(ButterMorphModel.Expressions.Path("$source.Items"), string.Empty, ButterMorphModel.Expressions.Path("item.Id")));
        Assert.Throws<ArgumentException>(() => ButterMorphModel.CreateDocument().MapPath("$source.Name", string.Empty));
    }

    // Creates a test customer schema.
    private static IStructureSchema CreateCustomerSchema()
    {
        ISchemaNode name = ButterMorphModel.CreateNode()
            .Scalar("Name", "String")
            .Build();
        ISchemaNode customer = ButterMorphModel.CreateNode()
            .Object("Customer")
            .WithChild(name)
            .Build();
        ISchemaNode root = ButterMorphModel.CreateNode()
            .Object("$root")
            .WithChild(customer)
            .Build();

        return ButterMorphModel.CreateSchema("Customer")
            .WithRoot(root)
            .Build();
    }

    // Creates a transformation engine with modeling-compatible services.
    private static TransformationEngine CreateEngine()
    {
        PathResolver pathResolver = new();
        NavigationEngine navigationEngine = new(pathResolver);
        TransformationExpressionEvaluator evaluator = new(navigationEngine, pathResolver, new FunctionRegistry());
        return new TransformationEngine(evaluator, new ExecutionContextFactory());
    }
}
