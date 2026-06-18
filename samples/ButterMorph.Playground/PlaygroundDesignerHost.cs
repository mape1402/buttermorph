using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Web.Razor;

/// <summary>
/// Provides contextual playground data for the embedded designer.
/// </summary>
internal sealed class PlaygroundDesignerHost : IButterMorphDesignerHost
{
    // Context value for the customer order scenario.
    private const string ComplexContext = "complex";

    // Context value for the invoice payment scenario.
    private const string InvoiceContext = "invoice";

    // Context value for the support case scenario.
    private const string SupportContext = "support";

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
                Map("$invoice.Header.IssuedOn", "Document.Date"),
                Map("$invoice.Header.Currency", "Document.Currency"),
                Map("$invoice.BillTo.CustomerCode", "Party.Code"),
                Map("$invoice.BillTo.LegalName", "Party.Name"),
                Map("$invoice.BillTo.TaxId", "Party.TaxId"),
                Map("$payment.Payment.Reference", "Settlement.Reference"),
                Map("$payment.Payment.Method", "Settlement.Method"),
                Map("$payment.Payment.Amount", "Settlement.PaidAmount"),
                Map("$invoice.Header.Subtotal", "Totals.Subtotal"),
                Map("$invoice.Header.Tax", "Totals.Tax"),
                Map("$invoice.Header.Total", "Totals.GrandTotal")
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
                Map("$ticket.Ticket.Subject", "Case.Title"),
                Map("$ticket.Ticket.Priority", "Case.Severity"),
                Map("$ticket.Ticket.CreatedAt", "Case.OpenedAt"),
                Map("$ticket.Ticket.Channel", "Case.SourceChannel"),
                Map("$ticket.Requester.Name", "Contact.DisplayName"),
                Map("$ticket.Requester.Email", "Contact.EmailAddress"),
                Map("$ticket.Requester.Phone", "Contact.PhoneNumber"),
                Map("$profile.Customer.CustomerId", "Customer.Id"),
                Map("$profile.Customer.Segment", "Customer.Segment"),
                Map("$profile.Customer.RiskLevel", "Customer.Risk"),
                Map("$asset.Device.SerialNumber", "Asset.Serial"),
                Map("$asset.Device.Model", "Asset.Model"),
                Map("$asset.Device.Firmware", "Asset.FirmwareVersion"),
                Map("$asset.Warranty.Status", "Asset.WarrantyStatus")
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
