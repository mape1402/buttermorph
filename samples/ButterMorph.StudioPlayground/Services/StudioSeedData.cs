namespace ButterMorph.StudioPlayground.Services;

using System.Text.Json;
using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.SchemaDesign;
using ButterMorph.StudioPlayground.Models;

/// <summary>
/// Seeds the Studio Playground with realistic host-owned data.
/// </summary>
internal static class StudioSeedData
{
    private static readonly JsonSerializerOptions ResultJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>
    /// Seeds the in-memory store.
    /// </summary>
    /// <param name="store">The target store.</param>
    public static void Seed(StudioStore store)
    {
        SchemaTypeSchemaBuilder typeBuilder = new();
        PayloadSchemaBuilder payloadBuilder = new();
        PayloadSchemaDefinitionBuilder schemaBuilder = new(payloadBuilder);
        List<StudioCustomType> customTypes = [];

        StudioCustomType uniqueIdentifier = AddType(customTypes, typeBuilder, new SchemaTypeDesignInput
        {
            Key = "1af5b754-142d-4136-8640-71ba70bec3c9",
            Name = "UniqueIdentifier",
            Description = "Unique Identifier",
            VersionNumber = "1.0.0",
            BaseType = "string",
            MinLength = "26",
            MaxLength = "26",
            Pattern = "^[0-7][0-9A-HJKMNP-TV-Z]{25}$",
            Comment = "Initial version."
        }, "3d56346a-934c-414c-8659-8bc203e021c4");

        StudioCustomType rfc = AddType(customTypes, typeBuilder, new SchemaTypeDesignInput
        {
            Key = "c2c0955c-acd1-43bc-b618-25001b71997e",
            Name = "RFC",
            Description = "RFC Mexico",
            VersionNumber = "1.0.0",
            BaseType = "string",
            MinLength = "12",
            MaxLength = "13",
            Pattern = "^(?:[A-ZÑ&]{3}|[A-ZÑ&]{4})\\d{2}(0[1-9]|1[0-2])(0[1-9]|[12]\\d|3[01])[A-Z0-9]{3}$",
            Comment = "Initial version."
        }, "595eeb50-f2d6-4ec3-8534-5e8e1ac7552c");

        StudioCustomType email = AddType(customTypes, typeBuilder, new SchemaTypeDesignInput
        {
            Key = "add0b828-03d8-4070-8746-ecdd9df02d3b",
            Name = "Email",
            Description = "Email Address",
            VersionNumber = "1.0.0",
            BaseType = "string",
            MinLength = "5",
            MaxLength = "60",
            Pattern = "^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}$",
            Comment = "Initial version."
        }, "fb29b46d-e368-4b70-ab07-37e678b376fb");

        StudioCustomType phoneNumber = AddType(customTypes, typeBuilder, new SchemaTypeDesignInput
        {
            Key = "6e737c82-02d9-4857-b0a3-ac93bbb754c3",
            Name = "PhoneNumber",
            Description = "Phone Number",
            VersionNumber = "1.0.0",
            BaseType = "string",
            MinLength = "10",
            MaxLength = "10",
            Pattern = "^\\d{10}$",
            Comment = "Initial version."
        }, "de2ec2c8-efff-4f96-b2d7-5604549f88a2");

        StudioCustomField topic = new()
        {
            Id = "field-topic",
            Key = "topic",
            Name = "Topic",
            Description = "Queue or topic name used by the host.",
            DataType = "string",
            AppliesToJson = "[\"Schema\"]",
            IsRequired = true,
            IsActive = true,
            ValidationJson = "{\"minLength\":3}"
        };
        topic.ButterMorphResultJson = SerializeButterMorphDefinition(CreateFieldDefinition(topic));

        StudioCustomField securityClassification = new()
        {
            Id = "field-security-classification",
            Key = "SecurityClasification",
            Name = "Security Clasification",
            Description = "Security classification metadata for schemas and fields.",
            DataType = "string",
            AppliesToJson = "[\"Field\"]",
            IsRequired = false,
            IsActive = true,
            ValidationJson = "{\"allowedValues\":[\"Public\",\"Private\",\"Confidential\"]}"
        };
        securityClassification.ButterMorphResultJson = SerializeButterMorphDefinition(CreateFieldDefinition(securityClassification));

        foreach (StudioCustomType customType in customTypes)
        {
            store.SaveCustomType(customType);
        }

        store.SaveCustomField(topic);
        store.SaveCustomField(securityClassification);

        IReadOnlyCollection<SchemaTypeCatalogItem> typeCatalog = StudioButterMorphHost.CreateTypeCatalog(customTypes);
        IReadOnlyCollection<FieldMetadataCatalogItem> fieldCatalog = StudioButterMorphHost.CreateFieldCatalog([topic, securityClassification]);

        StudioSchema customerProfile = CreateSchema(schemaBuilder, typeCatalog, fieldCatalog, new PayloadSchemaDesignInput
        {
            Key = "customer-profile",
            Name = "Customer Profile",
            Description = "Atlas-style customer profile schema.",
            Version = "1.0.0",
            VersionComment = "Seed version."
        }, CreateCustomerProfileFields(uniqueIdentifier, rfc, email, phoneNumber), "schema-customer-profile");
        customerProfile.InjectedCustomTypeKeys.AddRange(customTypes.Select(item => item.Id));
        customerProfile.InjectedCustomFieldKeys.Add(topic.Id);
        customerProfile.InjectedCustomFieldKeys.Add(securityClassification.Id);

        StudioSchema customerSummary = CreateSchema(schemaBuilder, typeCatalog, fieldCatalog, new PayloadSchemaDesignInput
        {
            Key = "customer-summary",
            Name = "Customer Summary",
            Description = "Atlas-style customer summary output schema.",
            Version = "1.0.0",
            VersionComment = "Seed version."
        }, CreateCustomerSummaryFields(uniqueIdentifier, rfc, email, phoneNumber), "schema-customer-summary");
        customerSummary.InjectedCustomTypeKeys.AddRange(customTypes.Select(item => item.Id));
        customerSummary.InjectedCustomFieldKeys.Add(topic.Id);
        customerSummary.InjectedCustomFieldKeys.Add(securityClassification.Id);

        store.SaveSchema(customerProfile);
        store.SaveSchema(customerSummary);
        store.SaveMapping(CreateCustomerProfileToSummaryMapping(customerProfile, customerSummary));
    }

    // Builds and stores one custom type using the ButterMorph schema type builder.
    private static StudioCustomType AddType(List<StudioCustomType> customTypes, SchemaTypeSchemaBuilder builder, SchemaTypeDesignInput input, string id)
    {
        SchemaTypeDesignResult result = builder.Build(input, StudioButterMorphHost.CreateTypeCatalog(customTypes));
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Seed custom type generation failed for " + input.Name + ".");
        }

        StudioCustomType customType = new()
        {
            Id = id,
            Key = result.Key,
            Name = result.Name,
            Description = result.Description,
            Version = result.VersionNumber,
            BaseType = result.BaseType,
            Comment = result.Comment,
            JsonSchema = result.JsonSchema,
            ButterMorphResultJson = SerializeButterMorphDefinition(result.Definition)
        };
        customTypes.Add(customType);
        return customType;
    }

    // Builds one payload schema through the ButterMorph payload schema builder.
    private static StudioSchema CreateSchema(PayloadSchemaDefinitionBuilder builder, IReadOnlyCollection<SchemaTypeCatalogItem> schemaTypes, IReadOnlyCollection<FieldMetadataCatalogItem> metadataFields, PayloadSchemaDesignInput input, IReadOnlyCollection<PayloadSchemaField> fields, string id)
    {
        PayloadSchemaDesignResult result = builder.Build(input, fields, schemaTypes, metadataFields);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Seed payload schema generation failed for " + input.Name + ".");
        }

        return new StudioSchema
        {
            Id = id,
            Key = result.Key,
            Name = result.Name,
            Description = result.Description,
            Version = result.Version,
            VersionComment = result.VersionComment,
            JsonSchema = result.JsonSchema,
            ButterMorphResultJson = SerializeButterMorphDefinition(result.Definition)
        };
    }

    // Creates a field metadata result from a seeded host item.
    private static CustomFieldDefinition CreateFieldDefinition(StudioCustomField field)
    {
        return new CustomFieldDefinition
        {
            Key = field.Key,
            Name = field.Name,
            Description = field.Description,
            DataType = field.DataType,
            AppliesToJson = field.AppliesToJson,
            IsRequired = field.IsRequired,
            IsActive = field.IsActive,
            ValidationJson = field.ValidationJson,
            ChildrenDefinitionJson = field.ChildrenDefinitionJson,
            ArrayItemDataType = field.ArrayItemDataType,
            ArrayItemDefinitionJson = field.ArrayItemDefinitionJson
        };
    }

    // Serializes the exact ButterMorph result object held by the host.
    private static string SerializeButterMorphDefinition<T>(T definition)
    {
        return JsonSerializer.Serialize(definition, ResultJsonOptions);
    }

    // Creates source schema fields.
    private static IReadOnlyCollection<PayloadSchemaField> CreateCustomerProfileFields(StudioCustomType uniqueIdentifier, StudioCustomType rfc, StudioCustomType email, StudioCustomType phoneNumber)
    {
        return
        [
            Scalar("Name", "string", true, "Confidential", new Dictionary<string, string> { ["minLength"] = "3", ["maxLength"] = "60" }),
            Custom("Id", uniqueIdentifier, true, "Public"),
            Custom("RFC", rfc, true, "Private"),
            new PayloadSchemaField
            {
                Name = "Contacts",
                DataType = "array",
                IsRequired = true,
                Metadata = Classification("Private"),
                ArrayItem = new PayloadSchemaField
                {
                    DataType = "object",
                    Children =
                    [
                        new PayloadSchemaField { Name = "Key", DataType = "string", Description = "Identifier Key", IsRequired = true },
                        Custom("Email", email, false, string.Empty),
                        Custom("Phone", phoneNumber, true, string.Empty)
                    ]
                }
            },
            Scalar("Status", "string", true, "Public", new Dictionary<string, string> { ["enum"] = "[\"Active\",\"Inactive\"]" })
        ];
    }

    // Creates target schema fields.
    private static IReadOnlyCollection<PayloadSchemaField> CreateCustomerSummaryFields(StudioCustomType uniqueIdentifier, StudioCustomType rfc, StudioCustomType email, StudioCustomType phoneNumber)
    {
        return
        [
            Custom("CustomerId", uniqueIdentifier, true, "Public"),
            Scalar("DisplayName", "string", true, "Confidential", new Dictionary<string, string>()),
            Custom("TaxIdentifier", rfc, false, "Private"),
            Scalar("Status", "string", true, "Public", new Dictionary<string, string>()),
            new PayloadSchemaField
            {
                Name = "PrimaryContact",
                DataType = "object",
                Children =
                [
                    Custom("Email", email, false, string.Empty),
                    Custom("Phone", phoneNumber, false, string.Empty)
                ]
            }
        ];
    }

    // Creates a scalar field.
    private static PayloadSchemaField Scalar(string name, string dataType, bool required, string classification, IReadOnlyDictionary<string, string> validation)
    {
        return new PayloadSchemaField
        {
            Name = name,
            DataType = dataType,
            IsRequired = required,
            Metadata = Classification(classification),
            Validation = validation
        };
    }

    // Creates a custom type field.
    private static PayloadSchemaField Custom(string name, StudioCustomType customType, bool required, string classification)
    {
        return new PayloadSchemaField
        {
            Name = name,
            DataType = customType.BaseType,
            Description = customType.Description,
            IsRequired = required,
            CustomTypeVersionId = customType.Id,
            Metadata = Classification(classification)
        };
    }

    // Creates field metadata.
    private static IReadOnlyDictionary<string, string> Classification(string classification)
    {
        if (string.IsNullOrWhiteSpace(classification))
        {
            return new Dictionary<string, string>();
        }

        return new Dictionary<string, string> { ["SecurityClasification"] = classification };
    }

    // Creates the seeded mapping document and sample source data.
    private static StudioMapping CreateCustomerProfileToSummaryMapping(StudioSchema sourceSchema, StudioSchema targetSchema)
    {
        List<ITransformationMapping> mappings =
        [
            new TransformationMapping { TargetPath = "CustomerId", SourceExpression = new PathExpression { Path = "$customer.Id" } },
            new TransformationMapping { TargetPath = "DisplayName", SourceExpression = new PathExpression { Path = "$customer.Name" } },
            new TransformationMapping { TargetPath = "TaxIdentifier", SourceExpression = new PathExpression { Path = "$customer.RFC" } },
            new TransformationMapping { TargetPath = "Status", SourceExpression = new PathExpression { Path = "$customer.Status" } },
            new TransformationMapping { TargetPath = "PrimaryContact.Email", SourceExpression = new PathExpression { Path = "$customer.Contacts[0].Email" } },
            new TransformationMapping { TargetPath = "PrimaryContact.Phone", SourceExpression = new PathExpression { Path = "$customer.Contacts[0].Phone" } }
        ];

        StudioMapping mapping = new()
        {
            Id = "mapping-customer-profile-to-summary",
            Name = "Customer Profile to Summary",
            TargetSchemaId = targetSchema.Id,
            Document = new TransformationDocument
            {
                Mappings = mappings,
                Metadata = new Dictionary<string, string> { ["name"] = "Customer Profile to Summary" }
            },
            DslContent = """
                metadata {
                  name: "Customer Profile to Summary"
                }
                target {
                  CustomerId: $customer.Id
                  DisplayName: $customer.Name
                  TaxIdentifier: $customer.RFC
                  Status: $customer.Status
                  PrimaryContact {
                    Email: $customer.Contacts[0].Email
                    Phone: $customer.Contacts[0].Phone
                  }
                }
                """
        };

        mapping.SourceSchemaIds["customer"] = sourceSchema.Id;
        mapping.SourceSamples["customer"] = """
            {
              "Name": "Northwind Trading",
              "Id": "01J4Z7Z4Z4Z4Z4Z4Z4Z4Z4Z4Z4",
              "RFC": "NTR990101ABC",
              "Contacts": [
                {
                  "Key": "main",
                  "Email": "billing@northwind.example",
                  "Phone": "5512345678"
                }
              ],
              "Status": "Active"
            }
            """;

        return mapping;
    }
}
