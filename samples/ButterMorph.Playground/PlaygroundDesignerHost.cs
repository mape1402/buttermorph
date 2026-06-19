using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Web.Razor;

/// <summary>
/// Provides contextual playground data for the embedded designer.
/// </summary>
internal sealed class PlaygroundDesignerHost : IButterMorphDesignerHost
{
    // Stores mapping saves for the playground shell.
    private readonly PlaygroundMappingStore _mappingStore;

    // Context value for the customer order scenario.
    private const string ComplexContext = "complex";

    // Context value for the invoice payment scenario.
    private const string InvoiceContext = "invoice";

    // Context value for the support case scenario.
    private const string SupportContext = "support";

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaygroundDesignerHost"/> class.
    /// </summary>
    /// <param name="mappingStore">The mapping save store.</param>
    public PlaygroundDesignerHost(PlaygroundMappingStore mappingStore)
    {
        _mappingStore = mappingStore;
    }

    /// <summary>
    /// Loads schemas and an initial mapping document for a known playground context.
    /// </summary>
    /// <param name="request">The load request sent by the designer.</param>
    /// <returns>The designer load result.</returns>
    public Task<ButterMorphDesignerLoadResult> Load(ButterMorphDesignerLoadRequest request)
    {
        if (string.Equals(request.ContextKey, ComplexContext, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(CreateCustomerOrderLoadResult());
        }

        if (string.Equals(request.ContextKey, InvoiceContext, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(CreateInvoiceLoadResult());
        }

        if (string.Equals(request.ContextKey, SupportContext, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(CreateSupportLoadResult());
        }

        return Task.FromResult(new ButterMorphDesignerLoadResult());
    }

    // Creates the customer order scenario.
    private static ButterMorphDesignerLoadResult CreateCustomerOrderLoadResult()
    {
        IStructureSchema customerSchema = CreateCustomerSchema();
        IStructureSchema orderSchema = CreateOrderSchema();
        IStructureSchema targetSchema = CreateCustomerOrderTargetSchema();
        ITransformationDocument document = CreateCustomerOrderDocument(customerSchema, orderSchema, targetSchema);

        return new ButterMorphDesignerLoadResult
        {
            SourceSchemas = new Dictionary<string, IStructureSchema>
            {
                ["customer"] = customerSchema,
                ["orders"] = orderSchema
            },
            TargetSchema = targetSchema,
            InitialDocument = document,
            ShowSchemaActions = false
        };
    }

    // Creates the invoice payment scenario.
    private static ButterMorphDesignerLoadResult CreateInvoiceLoadResult()
    {
        IStructureSchema invoiceSchema = CreateInvoiceSchema();
        IStructureSchema paymentSchema = CreatePaymentSchema();
        IStructureSchema vendorSchema = CreateVendorSchema();
        IStructureSchema targetSchema = CreateAccountingEntryTargetSchema();
        ITransformationDocument document = CreateInvoiceDocument(invoiceSchema, paymentSchema, vendorSchema, targetSchema);

        return new ButterMorphDesignerLoadResult
        {
            SourceSchemas = new Dictionary<string, IStructureSchema>
            {
                ["invoice"] = invoiceSchema,
                ["payment"] = paymentSchema,
                ["vendor"] = vendorSchema
            },
            TargetSchema = targetSchema,
            InitialDocument = document,
            ShowSchemaActions = false
        };
    }

    // Creates the support case scenario.
    private static ButterMorphDesignerLoadResult CreateSupportLoadResult()
    {
        IStructureSchema ticketSchema = CreateTicketSchema();
        IStructureSchema profileSchema = CreateProfileSchema();
        IStructureSchema assetSchema = CreateAssetSchema();
        IStructureSchema targetSchema = CreateCasePacketTargetSchema();
        ITransformationDocument document = CreateSupportDocument(ticketSchema, profileSchema, assetSchema, targetSchema);

        return new ButterMorphDesignerLoadResult
        {
            SourceSchemas = new Dictionary<string, IStructureSchema>
            {
                ["ticket"] = ticketSchema,
                ["profile"] = profileSchema,
                ["asset"] = assetSchema
            },
            TargetSchema = targetSchema,
            InitialDocument = document,
            ShowSchemaActions = false
        };
    }

    /// <summary>
    /// Accepts the saved document from the designer host flow.
    /// </summary>
    /// <param name="request">The save request sent by the designer.</param>
    /// <returns>The designer save result.</returns>
    public Task<ButterMorphDesignerSaveResult> Save(ButterMorphDesignerSaveRequest request)
    {
        if (!IsPreparedScenario(request.ContextKey))
        {
            return Task.FromResult(new ButterMorphDesignerSaveResult
            {
                Succeeded = true,
                Message = "Mappings saved."
            });
        }

        _mappingStore.Save(new PlaygroundMappingSave
        {
            ContextKey = request.ContextKey,
            DslContent = request.DslContent,
            SavedAt = DateTimeOffset.UtcNow.ToString("O"),
            MappingCount = request.Document.Mappings.Count
        });

        return Task.FromResult(new ButterMorphDesignerSaveResult
        {
            Succeeded = true,
            Message = "Mapping received by playground host for " + request.ContextKey + "."
        });
    }

    // Determines whether a context is owned by the playground host.
    private static bool IsPreparedScenario(string contextKey)
    {
        if (string.Equals(contextKey, ComplexContext, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(contextKey, InvoiceContext, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return string.Equals(contextKey, SupportContext, StringComparison.OrdinalIgnoreCase);
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

    // Creates the prepared customer order target schema.
    private static IStructureSchema CreateCustomerOrderTargetSchema()
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

    // Creates the prepared customer order mapping document.
    private static ITransformationDocument CreateCustomerOrderDocument(
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
                Function("upper", "Customer.FullName", Path("$customer.Identity.Name")),
                Function("lower", "Customer.EmailAddress", Path("$customer.Identity.Email")),
                Function("defaultEmpty", "ShippingAddress.Street", Path("$customer.Address.Line1"), Text("NO STREET")),
                Map("$customer.Address.City", "ShippingAddress.City"),
                Map("$customer.Address.State", "ShippingAddress.Region"),
                Map("$customer.Address.PostalCode", "ShippingAddress.ZipCode"),
                Map("$customer.Address.Country", "ShippingAddress.CountryCode"),
                Map("$customer.Preferences.Language", "Summary.PreferredLanguage"),
                Map("$orders.Orders[0].Total", "Summary.LatestOrderTotal"),
                Map("$customer.Preferences.NewsletterEnabled", "Summary.Newsletter"),
                Project(
                    "$orders.Orders[0].Items",
                    "item",
                    "OrderLines",
                    Object(
                        Property("Sku", Path("item.Sku")),
                        Property("Name", Path("item.Description")),
                        Property("Units", Path("item.Quantity")),
                        Property("Price", Path("item.UnitPrice"))))
            ],
            Metadata = new Dictionary<string, string>
            {
                ["playground-context"] = ComplexContext
            }
        };
    }

    // Creates the prepared invoice source schema.
    private static IStructureSchema CreateInvoiceSchema()
    {
        return new StructureSchema
        {
            Name = "Invoice source",
            Root = Node("$root", SchemaNodeKind.Object, string.Empty,
            [
                Node("Header", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("InvoiceNumber", "string"),
                    Scalar("IssuedOn", "string"),
                    Scalar("Currency", "string"),
                    Scalar("Subtotal", "number"),
                    Scalar("Tax", "number"),
                    Scalar("Total", "number")
                ]),
                Node("BillTo", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("CustomerCode", "string"),
                    Scalar("LegalName", "string"),
                    Scalar("TaxId", "string")
                ]),
                Node("Lines", SchemaNodeKind.Array, string.Empty,
                [
                    Node("$item", SchemaNodeKind.Object, string.Empty,
                    [
                        Scalar("Sku", "string"),
                        Scalar("Description", "string"),
                        Scalar("Quantity", "integer"),
                        Scalar("Amount", "number")
                    ])
                ])
            ])
        };
    }

    // Creates the prepared payment source schema.
    private static IStructureSchema CreatePaymentSchema()
    {
        return new StructureSchema
        {
            Name = "Payment source",
            Root = Node("$root", SchemaNodeKind.Object, string.Empty,
            [
                Node("Payment", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("Reference", "string"),
                    Scalar("PaidOn", "string"),
                    Scalar("Method", "string"),
                    Scalar("Amount", "number")
                ]),
                Node("Bank", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("Account", "string"),
                    Scalar("AuthorizationCode", "string")
                ])
            ])
        };
    }

    // Creates the prepared vendor source schema.
    private static IStructureSchema CreateVendorSchema()
    {
        return new StructureSchema
        {
            Name = "Vendor source",
            Root = Node("$root", SchemaNodeKind.Object, string.Empty,
            [
                Node("Vendor", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("VendorId", "string"),
                    Scalar("Name", "string"),
                    Scalar("TaxRegime", "string")
                ]),
                Node("Ledger", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("AccountCode", "string"),
                    Scalar("CostCenter", "string")
                ])
            ])
        };
    }

    // Creates the prepared accounting entry target schema.
    private static IStructureSchema CreateAccountingEntryTargetSchema()
    {
        return new StructureSchema
        {
            Name = "Accounting entry",
            Root = Node("$root", SchemaNodeKind.Object, string.Empty,
            [
                Node("Document", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("Number", "string"),
                    Scalar("Date", "string"),
                    Scalar("Currency", "string")
                ]),
                Node("Party", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("Code", "string"),
                    Scalar("Name", "string"),
                    Scalar("TaxId", "string")
                ]),
                Node("Settlement", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("Reference", "string"),
                    Scalar("Method", "string"),
                    Scalar("PaidAmount", "number")
                ]),
                Node("Totals", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("Subtotal", "number"),
                    Scalar("Tax", "number"),
                    Scalar("GrandTotal", "number")
                ]),
                Node("Lines", SchemaNodeKind.Array, string.Empty,
                [
                    Node("$item", SchemaNodeKind.Object, string.Empty,
                    [
                        Scalar("Code", "string"),
                        Scalar("Text", "string"),
                        Scalar("Units", "integer"),
                        Scalar("LineAmount", "number")
                    ])
                ])
            ])
        };
    }

    // Creates the prepared invoice mapping document.
    private static ITransformationDocument CreateInvoiceDocument(
        IStructureSchema invoiceSchema,
        IStructureSchema paymentSchema,
        IStructureSchema vendorSchema,
        IStructureSchema targetSchema)
    {
        return new TransformationDocument
        {
            SourceSchemas = new Dictionary<string, IStructureSchema>
            {
                ["invoice"] = invoiceSchema,
                ["payment"] = paymentSchema,
                ["vendor"] = vendorSchema
            },
            TargetSchema = targetSchema,
            Mappings =
            [
                Map("$invoice.Header.InvoiceNumber", "Document.Number"),
                Function("formatDate", "Document.Date", Path("$invoice.Header.IssuedOn"), Text("yyyy-MM-dd")),
                Map("$invoice.Header.Currency", "Document.Currency"),
                Map("$invoice.BillTo.CustomerCode", "Party.Code"),
                Function("concat", "Party.Name", Path("$vendor.Vendor.Name"), Text(" / "), Path("$invoice.BillTo.LegalName")),
                Map("$invoice.BillTo.TaxId", "Party.TaxId"),
                Map("$payment.Payment.Reference", "Settlement.Reference"),
                Map("$payment.Payment.Method", "Settlement.Method"),
                Map("$payment.Payment.Amount", "Settlement.PaidAmount"),
                Map("$invoice.Header.Subtotal", "Totals.Subtotal"),
                Map("$invoice.Header.Tax", "Totals.Tax"),
                Function("add", "Totals.GrandTotal", Path("$invoice.Header.Subtotal"), Path("$invoice.Header.Tax")),
                Project(
                    "$invoice.Lines",
                    "line",
                    "Lines",
                    Object(
                        Property("Code", Path("line.Sku")),
                        Property("Text", Path("line.Description")),
                        Property("Units", Path("line.Quantity")),
                        Property("LineAmount", Path("line.Amount"))))
            ],
            Metadata = new Dictionary<string, string>
            {
                ["playground-context"] = InvoiceContext
            }
        };
    }

    // Creates the prepared ticket source schema.
    private static IStructureSchema CreateTicketSchema()
    {
        return new StructureSchema
        {
            Name = "Ticket source",
            Root = Node("$root", SchemaNodeKind.Object, string.Empty,
            [
                Node("Ticket", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("TicketId", "string"),
                    Scalar("Subject", "string"),
                    Scalar("Priority", "string"),
                    Scalar("CreatedAt", "string"),
                    Scalar("Channel", "string")
                ]),
                Node("Requester", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("Name", "string"),
                    Scalar("Email", "string"),
                    Scalar("Phone", "string")
                ]),
                Node("Conversation", SchemaNodeKind.Array, string.Empty,
                [
                    Node("$item", SchemaNodeKind.Object, string.Empty,
                    [
                        Scalar("Author", "string"),
                        Scalar("Message", "string"),
                        Scalar("CreatedAt", "string")
                    ])
                ])
            ])
        };
    }

    // Creates the prepared profile source schema.
    private static IStructureSchema CreateProfileSchema()
    {
        return new StructureSchema
        {
            Name = "Profile source",
            Root = Node("$root", SchemaNodeKind.Object, string.Empty,
            [
                Node("Customer", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("CustomerId", "string"),
                    Scalar("Segment", "string"),
                    Scalar("RiskLevel", "string")
                ]),
                Node("Entitlements", SchemaNodeKind.Array, string.Empty,
                [
                    Node("$item", SchemaNodeKind.Object, string.Empty,
                    [
                        Scalar("ProductCode", "string"),
                        Scalar("Plan", "string"),
                        Scalar("ExpiresOn", "string")
                    ])
                ])
            ])
        };
    }

    // Creates the prepared asset source schema.
    private static IStructureSchema CreateAssetSchema()
    {
        return new StructureSchema
        {
            Name = "Asset source",
            Root = Node("$root", SchemaNodeKind.Object, string.Empty,
            [
                Node("Device", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("SerialNumber", "string"),
                    Scalar("Model", "string"),
                    Scalar("Firmware", "string")
                ]),
                Node("Warranty", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("Status", "string"),
                    Scalar("ExpiresOn", "string")
                ])
            ])
        };
    }

    // Creates the prepared case packet target schema.
    private static IStructureSchema CreateCasePacketTargetSchema()
    {
        return new StructureSchema
        {
            Name = "Case packet",
            Root = Node("$root", SchemaNodeKind.Object, string.Empty,
            [
                Node("Case", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("Id", "string"),
                    Scalar("Title", "string"),
                    Scalar("Severity", "string"),
                    Scalar("OpenedAt", "string"),
                    Scalar("SourceChannel", "string")
                ]),
                Node("Contact", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("DisplayName", "string"),
                    Scalar("EmailAddress", "string"),
                    Scalar("PhoneNumber", "string")
                ]),
                Node("Customer", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("Id", "string"),
                    Scalar("Segment", "string"),
                    Scalar("Risk", "string")
                ]),
                Node("Asset", SchemaNodeKind.Object, string.Empty,
                [
                    Scalar("Serial", "string"),
                    Scalar("Model", "string"),
                    Scalar("FirmwareVersion", "string"),
                    Scalar("WarrantyStatus", "string")
                ]),
                Node("Messages", SchemaNodeKind.Array, string.Empty,
                [
                    Node("$item", SchemaNodeKind.Object, string.Empty,
                    [
                        Scalar("From", "string"),
                        Scalar("Body", "string"),
                        Scalar("At", "string")
                    ])
                ])
            ])
        };
    }

    // Creates the prepared support mapping document.
    private static ITransformationDocument CreateSupportDocument(
        IStructureSchema ticketSchema,
        IStructureSchema profileSchema,
        IStructureSchema assetSchema,
        IStructureSchema targetSchema)
    {
        return new TransformationDocument
        {
            SourceSchemas = new Dictionary<string, IStructureSchema>
            {
                ["ticket"] = ticketSchema,
                ["profile"] = profileSchema,
                ["asset"] = assetSchema
            },
            TargetSchema = targetSchema,
            Mappings =
            [
                Map("$ticket.Ticket.TicketId", "Case.Id"),
                Function("replace", "Case.Title", Path("$ticket.Ticket.Subject"), Text("Issue"), Text("Case")),
                Function("upper", "Case.Severity", Path("$ticket.Ticket.Priority")),
                Function("parseDate", "Case.OpenedAt", Path("$ticket.Ticket.CreatedAt")),
                Map("$ticket.Ticket.Channel", "Case.SourceChannel"),
                Function("defaultEmpty", "Contact.DisplayName", Path("$ticket.Requester.Name"), Text("Unknown requester")),
                Function("lower", "Contact.EmailAddress", Path("$ticket.Requester.Email")),
                Map("$ticket.Requester.Phone", "Contact.PhoneNumber"),
                Map("$profile.Customer.CustomerId", "Customer.Id"),
                Map("$profile.Customer.Segment", "Customer.Segment"),
                Function("if", "Customer.Risk", Path("$profile.Customer.RiskLevel"), Text("REVIEW"), Text("NORMAL")),
                Map("$asset.Device.SerialNumber", "Asset.Serial"),
                Map("$asset.Device.Model", "Asset.Model"),
                Map("$asset.Device.Firmware", "Asset.FirmwareVersion"),
                Map("$asset.Warranty.Status", "Asset.WarrantyStatus"),
                Project(
                    "$ticket.Conversation",
                    "message",
                    "Messages",
                    Object(
                        Property("From", Path("message.Author")),
                        Property("Body", Path("message.Message")),
                        Property("At", Path("message.CreatedAt"))))
            ],
            Metadata = new Dictionary<string, string>
            {
                ["playground-context"] = SupportContext
            }
        };
    }

    // Creates a mapping from a source path to a target path.
    private static ITransformationMapping Map(string sourcePath, string targetPath)
    {
        return new TransformationMapping
        {
            SourceExpression = Path(sourcePath),
            TargetPath = targetPath
        };
    }

    // Creates a mapping from a function expression to a target path.
    private static ITransformationMapping Function(string functionKey, string targetPath, params ITransformationExpression[] arguments)
    {
        return new TransformationMapping
        {
            SourceExpression = new FunctionCallExpression
            {
                FunctionKey = functionKey,
                Arguments = arguments
            },
            TargetPath = targetPath
        };
    }

    // Creates a mapping from a projection expression to a target array path.
    private static ITransformationMapping Project(string sourcePath, string alias, string targetPath, ITransformationExpression body)
    {
        return new TransformationMapping
        {
            SourceExpression = new CollectionProjectionExpression
            {
                SourceExpression = Path(sourcePath),
                ItemAlias = alias,
                BodyExpression = body
            },
            TargetPath = targetPath
        };
    }

    // Creates a map-shaped expression.
    private static ITransformationExpression Object(params IObjectPropertyExpression[] properties)
    {
        return new ObjectExpression
        {
            Properties = properties
        };
    }

    // Creates a map-shaped property expression.
    private static IObjectPropertyExpression Property(string name, ITransformationExpression expression)
    {
        return new ObjectPropertyExpression
        {
            Name = name,
            Expression = expression
        };
    }

    // Creates a path expression.
    private static ITransformationExpression Path(string sourcePath)
    {
        return new PathExpression
        {
            Path = sourcePath
        };
    }

    // Creates a text literal expression.
    private static ITransformationExpression Text(string value)
    {
        return new ScalarLiteralExpression
        {
            Value = new ScalarValue
            {
                DataType = "String",
                RawValue = value,
                IsNull = false
            }
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
