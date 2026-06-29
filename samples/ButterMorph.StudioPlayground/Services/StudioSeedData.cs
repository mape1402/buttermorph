namespace ButterMorph.StudioPlayground.Services;

using System.Text;
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
    // JSON text used for map-shaped schema values.
    private const string MapType = "obj" + "ect";

    /// <summary>
    /// Seeds the in-memory store.
    /// </summary>
    /// <param name="store">The target store.</param>
    public static void Seed(StudioStore store)
    {
        SchemaTypeSchemaBuilder typeBuilder = new();
        PayloadSchemaBuilder payloadBuilder = new();
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

        StudioCustomType contact = AddType(customTypes, typeBuilder, new SchemaTypeDesignInput
        {
            Key = "70b0e156-0b7e-4a9e-9a36-271dc8ff7424",
            Name = "Contact",
            Description = "Contact",
            VersionNumber = "1.0.0",
            BaseType = MapType,
            PayloadSchemaJson = CreateContactTypeSchema(email, phoneNumber),
            Comment = "Initial version."
        }, "a42053f2-2789-4f2f-a351-36b548a869af");

        StudioCustomType contactList = AddType(customTypes, typeBuilder, new SchemaTypeDesignInput
        {
            Key = "29bef8df-5c75-41b7-ac9e-7a38478ba064",
            Name = "ContactList",
            Description = "Contact List",
            VersionNumber = "1.0.0",
            BaseType = "array",
            ArrayItemTypeVersionId = contact.ContextKey,
            MinItems = "1",
            Comment = "Initial version."
        }, "bb023fcb-101e-437f-81cf-4b3a407e4048");

        StudioCustomField topic = new()
        {
            ContextKey = "field-topic",
            Key = "topic",
            Name = "Topic",
            Description = "Queue or topic name used by the host.",
            DataType = "string",
            AppliesToJson = "[\"Schema\"]",
            IsRequired = true,
            IsActive = true,
            ValidationJson = "{\"minLength\":3}"
        };

        StudioCustomField securityClassification = new()
        {
            ContextKey = "field-security-classification",
            Key = "SecurityClasification",
            Name = "Security Clasification",
            Description = "Security classification metadata for schemas and fields.",
            DataType = "string",
            AppliesToJson = "[\"Field\"]",
            IsRequired = false,
            IsActive = true,
            ValidationJson = "{\"enum\":[\"Public\",\"Private\",\"Confidential\"]}"
        };

        foreach (StudioCustomType customType in customTypes)
        {
            store.SaveCustomType(customType);
        }

        store.SaveCustomField(topic);
        store.SaveCustomField(securityClassification);

        IReadOnlyCollection<SchemaTypeCatalogItem> typeCatalog = StudioButterMorphHost.CreateTypeCatalog(customTypes);
        IReadOnlyCollection<FieldMetadataCatalogItem> fieldCatalog = StudioButterMorphHost.CreateFieldCatalog([topic, securityClassification]);

        StudioSchema customerProfile = CreateSchema(payloadBuilder, typeCatalog, fieldCatalog, new PayloadSchemaDesignInput
        {
            Key = "customer-profile",
            Name = "Customer Profile",
            Description = "Atlas-style customer profile schema.",
            Version = "1.0.0",
            VersionComment = "Seed version.",
            JsonSchema = CreateCustomerProfileSchema(uniqueIdentifier, rfc, contactList, contact, email, phoneNumber, typeCatalog)
        }, "schema-customer-profile");
        customerProfile.InjectedCustomTypeKeys.AddRange(customTypes.Select(item => item.ContextKey));
        customerProfile.InjectedCustomFieldKeys.Add(topic.ContextKey);
        customerProfile.InjectedCustomFieldKeys.Add(securityClassification.ContextKey);

        StudioSchema customerSummary = CreateSchema(payloadBuilder, typeCatalog, fieldCatalog, new PayloadSchemaDesignInput
        {
            Key = "customer-summary",
            Name = "Customer Summary",
            Description = "Atlas-style customer summary output schema.",
            Version = "1.0.0",
            VersionComment = "Seed version.",
            JsonSchema = CreateCustomerSummarySchema(uniqueIdentifier, rfc, email, phoneNumber, typeCatalog)
        }, "schema-customer-summary");
        customerSummary.InjectedCustomTypeKeys.AddRange(customTypes.Select(item => item.ContextKey));
        customerSummary.InjectedCustomFieldKeys.Add(topic.ContextKey);
        customerSummary.InjectedCustomFieldKeys.Add(securityClassification.ContextKey);

        store.SaveSchema(customerProfile);
        store.SaveSchema(customerSummary);
        store.SaveMapping(CreateCustomerProfileToSummaryMapping(customerProfile, customerSummary));
    }

    // Builds and stores one custom type using the ButterMorph schema type builder.
    private static StudioCustomType AddType(List<StudioCustomType> customTypes, SchemaTypeSchemaBuilder builder, SchemaTypeDesignInput input, string contextKey)
    {
        SchemaTypeDesignResult result = builder.Build(input, StudioButterMorphHost.CreateTypeCatalog(customTypes));
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Seed custom type generation failed for " + input.Name + ".");
        }

        StudioCustomType customType = new()
        {
            ContextKey = contextKey,
            Key = result.Key,
            Name = result.Name,
            Description = result.Description,
            Version = result.VersionNumber,
            BaseType = result.BaseType,
            Comment = result.Comment,
            JsonSchema = result.JsonSchema
        };
        customTypes.Add(customType);
        return customType;
    }

    // Builds one payload schema through the ButterMorph payload schema builder.
    private static StudioSchema CreateSchema(PayloadSchemaBuilder builder, IReadOnlyCollection<SchemaTypeCatalogItem> schemaTypes, IReadOnlyCollection<FieldMetadataCatalogItem> metadataFields, PayloadSchemaDesignInput input, string contextKey)
    {
        PayloadSchemaDesignResult result = builder.Build(input, schemaTypes, metadataFields);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Seed payload schema generation failed for " + input.Name + ".");
        }

        return new StudioSchema
        {
            ContextKey = contextKey,
            Key = result.Key,
            Name = result.Name,
            Description = result.Description,
            Version = result.Version,
            VersionComment = result.VersionComment,
            JsonSchema = result.JsonSchema
        };
    }

    // Creates the structured body for the Contact custom type.
    private static string CreateContactTypeSchema(StudioCustomType email, StudioCustomType phoneNumber)
    {
        return WriteSchema(writer =>
        {
            writer.WriteString("type", MapType);
            writer.WritePropertyName("properties");
            writer.WriteStartObject();
            WriteScalarProperty(writer, "Key", "string", "Identifier Key", true, string.Empty);
            WriteCustomProperty(writer, "Email", email, false, string.Empty);
            WriteCustomProperty(writer, "Phone", phoneNumber, true, string.Empty);
            writer.WriteEndObject();
        });
    }

    // Creates the structured body for the source schema seed.
    private static string CreateCustomerProfileSchema(StudioCustomType uniqueIdentifier, StudioCustomType rfc, StudioCustomType contactList, StudioCustomType contact, StudioCustomType email, StudioCustomType phoneNumber, IReadOnlyCollection<SchemaTypeCatalogItem> catalog)
    {
        return WriteSchema(writer =>
        {
            writer.WriteString("type", MapType);
            writer.WritePropertyName("properties");
            writer.WriteStartObject();
            WriteScalarProperty(writer, "Name", "string", string.Empty, true, "Confidential", minLength: 3, maxLength: 60);
            WriteCustomProperty(writer, "Id", uniqueIdentifier, true, "Public");
            WriteCustomProperty(writer, "RFC", rfc, true, "Private");
            WriteCustomProperty(writer, "Contacts", contactList, true, "Private");
            WriteScalarProperty(writer, "Status", "string", string.Empty, true, "Public", allowedValues: ["Active", "Inactive"]);
            writer.WriteEndObject();
            WriteDefinitions(writer, catalog, uniqueIdentifier, rfc, contactList, contact, email, phoneNumber);
        });
    }

    // Creates the structured body for the target schema seed.
    private static string CreateCustomerSummarySchema(StudioCustomType uniqueIdentifier, StudioCustomType rfc, StudioCustomType email, StudioCustomType phoneNumber, IReadOnlyCollection<SchemaTypeCatalogItem> catalog)
    {
        return WriteSchema(writer =>
        {
            writer.WriteString("type", MapType);
            writer.WritePropertyName("properties");
            writer.WriteStartObject();
            WriteCustomProperty(writer, "CustomerId", uniqueIdentifier, true, "Public");
            WriteScalarProperty(writer, "DisplayName", "string", string.Empty, true, "Confidential");
            WriteCustomProperty(writer, "TaxIdentifier", rfc, false, "Private");
            WriteScalarProperty(writer, "Status", "string", string.Empty, true, "Public");
            writer.WritePropertyName("PrimaryContact");
            writer.WriteStartObject();
            writer.WriteString("type", MapType);
            writer.WritePropertyName("properties");
            writer.WriteStartObject();
            WriteCustomProperty(writer, "Email", email, false, string.Empty);
            WriteCustomProperty(writer, "Phone", phoneNumber, false, string.Empty);
            writer.WriteEndObject();
            writer.WriteEndObject();
            writer.WriteEndObject();
            WriteDefinitions(writer, catalog, uniqueIdentifier, rfc, email, phoneNumber);
        });
    }

    // Creates compact JSON through a writer callback.
    private static string WriteSchema(Action<Utf8JsonWriter> writeBody)
    {
        using MemoryStream stream = new();
        using Utf8JsonWriter writer = new(stream, new JsonWriterOptions { Indented = false });
        writer.WriteStartObject();
        writeBody(writer);
        writer.WriteEndObject();
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    // Writes a scalar property compatible with Atlas conventions.
    private static void WriteScalarProperty(Utf8JsonWriter writer, string name, string type, string description, bool required, string classification, int minLength = 0, int maxLength = 0, IReadOnlyCollection<string> allowedValues = null)
    {
        writer.WritePropertyName(name);
        writer.WriteStartObject();
        writer.WriteString("type", type);
        if (!string.IsNullOrWhiteSpace(description))
        {
            writer.WriteString("description", description);
        }

        if (required)
        {
            writer.WriteBoolean("required", true);
        }

        WriteClassification(writer, classification);
        if (minLength > 0)
        {
            writer.WriteNumber("minLength", minLength);
        }

        if (maxLength > 0)
        {
            writer.WriteNumber("maxLength", maxLength);
        }

        if (allowedValues != null && allowedValues.Count > 0)
        {
            writer.WritePropertyName("enum");
            writer.WriteStartArray();
            foreach (string value in allowedValues)
            {
                writer.WriteStringValue(value);
            }

            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }

    // Writes a custom type property compatible with Atlas conventions.
    private static void WriteCustomProperty(Utf8JsonWriter writer, string name, StudioCustomType customType, bool required, string classification)
    {
        writer.WritePropertyName(name);
        writer.WriteStartObject();
        writer.WriteString("$ref", "#/$defs/" + GetDefinitionKey(customType));
        writer.WriteString("typeId", customType.Key);
        writer.WriteString("typeVersionId", customType.ContextKey);
        if (!string.IsNullOrWhiteSpace(customType.Description))
        {
            writer.WriteString("description", customType.Description);
        }

        if (required)
        {
            writer.WriteBoolean("required", true);
        }

        WriteClassification(writer, classification);
        writer.WriteEndObject();
    }

    // Writes field metadata in the same shape Atlas uses.
    private static void WriteClassification(Utf8JsonWriter writer, string classification)
    {
        if (string.IsNullOrWhiteSpace(classification))
        {
            return;
        }

        writer.WritePropertyName("metadata");
        writer.WriteStartObject();
        writer.WriteString("SecurityClasification", classification);
        writer.WriteEndObject();
    }

    // Writes $defs for the referenced custom types.
    private static void WriteDefinitions(Utf8JsonWriter writer, IReadOnlyCollection<SchemaTypeCatalogItem> catalog, params StudioCustomType[] customTypes)
    {
        writer.WritePropertyName("$defs");
        writer.WriteStartObject();
        foreach (StudioCustomType customType in customTypes)
        {
            SchemaTypeCatalogItem item = catalog.First(catalogItem => string.Equals(catalogItem.TypeVersionId, customType.ContextKey, StringComparison.OrdinalIgnoreCase));
            writer.WritePropertyName(GetDefinitionKey(customType));
            using JsonDocument document = JsonDocument.Parse(item.JsonSchema);
            WriteDefinitionBody(writer, document.RootElement);
        }

        writer.WriteEndObject();
    }

    // Writes one definition body without carrying nested definition bags into the parent schema.
    private static void WriteDefinitionBody(Utf8JsonWriter writer, JsonElement definition)
    {
        writer.WriteStartObject();
        foreach (JsonProperty property in definition.EnumerateObject())
        {
            if (string.Equals(property.Name, "$defs", StringComparison.Ordinal))
            {
                continue;
            }

            property.WriteTo(writer);
        }

        writer.WriteEndObject();
    }

    // Resolves Atlas-style definition keys.
    private static string GetDefinitionKey(StudioCustomType customType)
    {
        return customType.Name + "@" + customType.Version;
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
            ContextKey = "mapping-customer-profile-to-summary",
            Name = "Customer Profile to Summary",
            TargetSchemaKey = targetSchema.ContextKey,
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

        mapping.SourceSchemaKeys["customer"] = sourceSchema.ContextKey;
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
