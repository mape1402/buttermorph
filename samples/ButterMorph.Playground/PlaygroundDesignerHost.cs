using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Web.Razor;

/// <summary>
/// Provides contextual playground data for the embedded designer.
/// </summary>
internal sealed class PlaygroundDesignerHost : IButterMorphDesignerHost
{
    // Context value that activates the prepared playground preload.
    private const string ComplexContext = "complex";

    /// <summary>
    /// Loads schemas and an initial mapping document for a known playground context.
    /// </summary>
    /// <param name="request">The load request sent by the designer.</param>
    /// <returns>The designer load result.</returns>
    public Task<ButterMorphDesignerLoadResult> Load(ButterMorphDesignerLoadRequest request)
    {
        if (!string.Equals(request.ContextKey, ComplexContext, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new ButterMorphDesignerLoadResult());
        }

        IStructureSchema customerSchema = CreateCustomerSchema();
        IStructureSchema orderSchema = CreateOrderSchema();
        IStructureSchema targetSchema = CreateTargetSchema();
        ITransformationDocument document = CreateDocument(customerSchema, orderSchema, targetSchema);

        return Task.FromResult(new ButterMorphDesignerLoadResult
        {
            SourceSchemas = new Dictionary<string, IStructureSchema>
            {
                ["customer"] = customerSchema,
                ["orders"] = orderSchema
            },
            TargetSchema = targetSchema,
            InitialDocument = document,
            ShowSchemaActions = false
        });
    }

    /// <summary>
    /// Accepts the saved document from the designer host flow.
    /// </summary>
    /// <param name="request">The save request sent by the designer.</param>
    /// <returns>The designer save result.</returns>
    public Task<ButterMorphDesignerSaveResult> Save(ButterMorphDesignerSaveRequest request)
    {
        if (!string.Equals(request.ContextKey, ComplexContext, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new ButterMorphDesignerSaveResult
            {
                Succeeded = true,
                Message = "Mappings saved."
            });
        }

        return Task.FromResult(new ButterMorphDesignerSaveResult
        {
            Succeeded = true,
            Message = "Mapping received by playground host."
        });
    }

    // Creates the prepared customer source schema.
    private static IStructureSchema CreateCustomerSchema()
    {
        return new StructureSchema
        {
            Name = "Customer source",
            Root = Node("$root", SchemaNodeKind.Object, string.Empty,
            [
                Node("Identity", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("Id", "string"),
                    Scalar("Name", "string"),
                    Scalar("Email", "string"),
                    Scalar("BirthDate", "string")
                ]),
                Node("Address", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("Line1", "string"),
                    Scalar("Line2", "string"),
                    Scalar("City", "string"),
                    Scalar("State", "string"),
                    Scalar("PostalCode", "string"),
                    Scalar("Country", "string")
                ]),
                Node("Preferences", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("Language", "string"),
                    Scalar("NewsletterEnabled", "boolean"),
                    Scalar("LoyaltyLevel", "string")
                ])
            ])
        };
    }

    // Creates the prepared orders source schema.
    private static IStructureSchema CreateOrderSchema()
    {
        return new StructureSchema
        {
            Name = "Orders source",
            Root = Node("$root", SchemaNodeKind.Object, string.Empty,
            [
                Node("Orders", SchemaNodeKind.Array, string.Empty,
                [
                    Node("$item", SchemaNodeKind.Object, string.Empty,
                    [
                        Scalar("OrderId", "string"),
                        Scalar("CreatedAt", "string"),
                        Scalar("Status", "string"),
                        Scalar("Total", "number"),
                        Node("Items", SchemaNodeKind.Array, string.Empty,
                        [
                            Node("$item", SchemaNodeKind.Object, string.Empty,
                            [
                                Scalar("Sku", "string"),
                                Scalar("Description", "string"),
                                Scalar("Quantity", "integer"),
                                Scalar("UnitPrice", "number")
                            ])
                        ])
                    ])
                ])
            ])
        };
    }

    // Creates the prepared target schema.
    private static IStructureSchema CreateTargetSchema()
    {
        return new StructureSchema
        {
            Name = "Customer order summary",
            Root = Node("$root", SchemaNodeKind.Object, string.Empty,
            [
                Node("Customer", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("CustomerId", "string"),
                    Scalar("FullName", "string"),
                    Scalar("EmailAddress", "string")
                ]),
                Node("ShippingAddress", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("Street", "string"),
                    Scalar("City", "string"),
                    Scalar("Region", "string"),
                    Scalar("ZipCode", "string"),
                    Scalar("CountryCode", "string")
                ]),
                Node("OrderLines", SchemaNodeKind.Array, string.Empty,
                [
                    Node("$item", SchemaNodeKind.Object, string.Empty,
                    [
                        Scalar("Sku", "string"),
                        Scalar("Name", "string"),
                        Scalar("Units", "integer"),
                        Scalar("Price", "number")
                    ])
                ]),
                Node("Summary", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("PreferredLanguage", "string"),
                    Scalar("LatestOrderTotal", "number"),
                    Scalar("Newsletter", "boolean")
                ])
            ])
        };
    }

    // Creates the prepared initial mapping document.
    private static ITransformationDocument CreateDocument(
        IStructureSchema customerSchema,
        IStructureSchema orderSchema,
        IStructureSchema targetSchema)
    {
        return new TransformationDocument
        {
            SourceSchemas = new Dictionary<string, IStructureSchema>
            {
                ["customer"] = customerSchema,
                ["orders"] = orderSchema
            },
            TargetSchema = targetSchema,
            Mappings =
            [
                Map("$customer.Identity.Id", "Customer.CustomerId"),
                Map("$customer.Identity.Name", "Customer.FullName"),
                Map("$customer.Identity.Email", "Customer.EmailAddress"),
                Map("$customer.Address.Line1", "ShippingAddress.Street"),
                Map("$customer.Address.City", "ShippingAddress.City"),
                Map("$customer.Address.State", "ShippingAddress.Region"),
                Map("$customer.Address.PostalCode", "ShippingAddress.ZipCode"),
                Map("$customer.Address.Country", "ShippingAddress.CountryCode"),
                Map("$customer.Preferences.Language", "Summary.PreferredLanguage"),
                Map("$orders.Orders[0].Total", "Summary.LatestOrderTotal"),
                Map("$customer.Preferences.NewsletterEnabled", "Summary.Newsletter")
            ],
            Metadata = new Dictionary<string, string>
            {
                ["playground-context"] = ComplexContext
            }
        };
    }

    // Creates a mapping from a source path to a target path.
    private static ITransformationMapping Map(string sourcePath, string targetPath)
    {
        return new TransformationMapping
        {
            SourceExpression = new PathExpression
            {
                Path = sourcePath
            },
            TargetPath = targetPath
        };
    }

    // Creates a scalar schema node.
    private static ISchemaNode Scalar(string name, string dataType)
    {
        return Node(name, SchemaNodeKind.Scalar, dataType, []);
    }

    // Creates a schema node.
    private static ISchemaNode Node(
        string name,
        SchemaNodeKind kind,
        string dataType,
        IReadOnlyCollection<ISchemaNode> children)
    {
        return new SchemaNode
        {
            Name = name,
            Kind = kind,
            DataType = dataType,
            Children = children
        };
    }
}
