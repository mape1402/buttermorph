# ButterMorph

ButterMorph is a modular .NET toolkit for designing, storing, and executing shape-neutral data transformations.

It provides the core transformation model, schema definition services, JSON adapters, a textual DSL, and reusable Razor designers that can be embedded into any host application. The host owns persistence and business rules; ButterMorph owns the modeling, schema/mapping designers, rehydration, DSL import/export, and runtime execution.

## Big Picture

ButterMorph is built around a clear host integration boundary.

A host application, such as an internal platform, admin portal, or business system, is responsible for:

- storing custom types, custom fields, schemas, mappings, and source samples;
- deciding which custom types and custom fields are available in each schema designer session;
- deciding which schemas are available as mapping sources and targets;
- launching ButterMorph designers;
- saving returned definitions or mappings;
- providing authentication, authorization, navigation, and business workflows.

ButterMorph is responsible for:

- creating clean custom type definitions;
- creating versioned custom field definitions;
- creating payload schemas with `key`, `name`, `description`, `version`, metadata, properties, `$defs`, and `$metadataDefs`;
- rehydrating designers from saved ButterMorph definitions;
- building transformation documents;
- importing/exporting DSL;
- analyzing mappings before execution;
- executing mappings over typed structure graphs;
- exposing reusable Razor UI for mapping and schema tooling.

The intended flow is:

```text
Host -> opens ButterMorph designer -> user edits -> ButterMorph returns definition/document -> Host saves it
```

ButterMorph does not persist host data. That is intentional: the same NuGet packages can be embedded into different products with different database models and rules.

## Packages

| Package | Purpose |
| --- | --- |
| `ButterMorph` | Core runtime, typed model, expressions, native functions, validation, semantics, DSL, modeling builders, and dependency injection. |
| `ButterMorph.Json` | JSON graph reader/writer adapters for runtime input/output. |
| `ButterMorph.Json.Schema` | JSON Schema import/export compatibility for ButterMorph schemas. |
| `ButterMorph.SchemaDesign` | Headless schema design services, custom type definitions, custom field definitions, payload schema definitions, and hydrators. |
| `ButterMorph.Design` | Headless mapping design sessions, schema exploration, capability exploration, diagnostics, and DSL integration. |
| `ButterMorph.Web.Razor` | Reusable Razor Pages UI for mapping, custom types, custom fields, and schema designers. |

## Install / Reference

Reference only the packages your host needs.

Runtime-only mapping execution:

```xml
<PackageReference Include="ButterMorph" Version="1.0.0" />
<PackageReference Include="ButterMorph.Json" Version="1.0.0" />
```

JSON Schema import/export:

```xml
<PackageReference Include="ButterMorph.Json.Schema" Version="1.0.0" />
```

Full reusable Razor designer experience:

```xml
<PackageReference Include="ButterMorph.Web.Razor" Version="1.0.0" />
```

`ButterMorph.Web.Razor` depends on the design packages it needs.

## Getting Started: Razor Host

Register ButterMorph services in your host application:

```csharp
using ButterMorph.DependencyInjection;
using ButterMorph.Design;
using ButterMorph.Json.Schema;
using ButterMorph.SchemaDesign;
using ButterMorph.Web.Razor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddButterMorph();
builder.Services.AddButterMorphJsonSchema();
builder.Services.AddButterMorphSchemaDesign();
builder.Services.AddButterMorphDesign();
builder.Services.AddButterMorphRazorDesigner();

var app = builder.Build();

app.MapRazorPages();
app.MapButterMorphDesigner("/buttermorph");

app.Run();
```

Then implement the host interfaces you need:

- `IButterMorphSchemaTypeDesignerHost` for custom data types.
- `IButterMorphFieldMetadataDesignerHost` for custom metadata fields.
- `IButterMorphPayloadSchemaDesignerHost` for schemas.
- `IButterMorphDesignerHost` for mappings.

The host interfaces are the persistence boundary. ButterMorph calls `Load(...)` when a designer opens and `Save(...)` when the user saves.

## Designer Routes

After calling `app.MapButterMorphDesigner("/buttermorph")`, the reusable UI is mounted under the configured prefix.

| Route | Purpose |
| --- | --- |
| `/buttermorph/designer` | Mapping designer. |
| `/buttermorph/schema-types/designer` | Custom data type designer. |
| `/buttermorph/metadata-fields/designer` | Custom metadata field designer. |
| `/buttermorph/payload-schema/designer` | Payload schema designer. |

Each designer receives a technical `context` query parameter from the host. The `context` value is only a correlation key for the host session. It is not part of the ButterMorph schema or mapping payload.

Example:

```text
/buttermorph/payload-schema/designer?context=schema-123&mode=edit&popup=true
```

## Host Save Flow

A typical save flow looks like this:

1. The host opens a ButterMorph designer in an iframe/modal or popup.
2. ButterMorph loads initial state from the host using the matching host interface.
3. The user edits the definition or mapping.
4. ButterMorph validates the input.
5. On success, ButterMorph calls the host `Save(...)` method with a clean payload.
6. The host persists the payload in its own store.
7. ButterMorph notifies the opener and closes the designer surface.

Operational values such as `Succeeded`, diagnostics, browser popup context, and host IDs are not part of the clean payloads.

## Schema Design Model

ButterMorph schema design has three reusable concepts.

### Custom Data Types

Custom data types define reusable type constraints and structures. For example, a `CustomerCode` type can define string length and pattern constraints. An `Address` type can define an object structure.

The designer returns a clean `SchemaTypeDefinition`:

```json
{
  "key": "customer-code",
  "name": "Customer Code",
  "description": "Canonical customer code.",
  "version": "1.0.0",
  "baseType": "string",
  "comment": "Initial version.",
  "schema": {
    "type": "string",
    "minLength": 5,
    "maxLength": 20
  }
}
```

The host stores this definition and later decides where it is available.

### Custom Fields

Custom fields define metadata fields that can be captured at schema level or field level. They are versioned so saved schemas can keep a stable reference to the custom field definition that was used.

The designer returns a clean `CustomFieldDefinition`:

```json
{
  "key": "security-classification",
  "name": "Security Classification",
  "description": "Data classification for governance.",
  "version": "1.0.0",
  "versionComment": "Initial version.",
  "dataType": "string",
  "appliesTo": ["Schema", "Field"],
  "isRequired": true,
  "isActive": true,
  "validation": {
    "allowedValues": ["Public", "Private", "Confidential"]
  }
}
```

The host injects selected custom fields into the schema designer.

### Payload Schemas

Payload schemas are the schemas used by business payloads and mapping sessions. They contain identity, versioning, metadata values, properties, custom type definitions, and metadata field definitions.

The schema designer returns a clean `PayloadSchemaDefinition`:

```json
{
  "key": "customer-profile",
  "name": "Customer Profile",
  "description": "Customer profile payload.",
  "version": "1.0.0",
  "versionComment": "Initial version.",
  "metadata": {
    "security-classification": {
      "type": "string",
      "value": "Private",
      "definition": "security-classification@1.0.0"
    }
  },
  "type": "object",
  "properties": {
    "Name": {
      "type": "string",
      "required": true
    }
  },
  "$defs": {},
  "$metadataDefs": {
    "security-classification@1.0.0": {
      "key": "security-classification",
      "name": "Security Classification",
      "version": "1.0.0",
      "dataType": "string",
      "allowedValues": ["Public", "Private", "Confidential"]
    }
  }
}
```

The schema is not stored as a wrapper. The host may keep its own database ID, but the visible ButterMorph payload stays clean.

## Injecting Catalogs Into the Schema Designer

The host controls what is available in each schema session.

When implementing `IButterMorphPayloadSchemaDesignerHost.Load(...)`, return the selected catalog items:

```csharp
public Task<ButterMorphPayloadSchemaDesignerLoadResult> Load(ButterMorphPayloadSchemaDesignerLoadRequest request)
{
    return Task.FromResult(new ButterMorphPayloadSchemaDesignerLoadResult
    {
        Definition = savedSchemaDefinition,
        SchemaTypes = selectedCustomTypes,
        MetadataFields = selectedCustomFields,
        ShowManualActions = false
    });
}
```

This allows the host to enforce business rules. For example, a sales schema can receive a `customer-code` custom type while a support schema does not.

## Mapping Design

The mapping designer builds an `ITransformationDocument` and exports a DSL representation that can be stored by the host.

The host provides:

- source schemas keyed by source alias;
- a target schema;
- an optional initial transformation document;
- or an optional initial DSL string.

Example host load:

```csharp
public Task<ButterMorphDesignerLoadResult> Load(ButterMorphDesignerLoadRequest request)
{
    return Task.FromResult(new ButterMorphDesignerLoadResult
    {
        SourceSchemas = new Dictionary<string, IStructureSchema>
        {
            ["invoice"] = invoiceSchema,
            ["vendor"] = vendorSchema
        },
        TargetSchema = accountingSchema,
        InitialDslContent = savedDsl,
        ShowSchemaActions = false
    });
}
```

Example save:

```csharp
public Task<ButterMorphDesignerSaveResult> Save(ButterMorphDesignerSaveRequest request)
{
    SaveDsl(request.ContextKey, request.DslContent);

    return Task.FromResult(new ButterMorphDesignerSaveResult
    {
        Succeeded = true,
        Message = "Mapping saved."
    });
}
```

The host should store the DSL for portability. ButterMorph can parse it back into a transformation document when the designer is reopened.

## DSL Example

```text
metadata {
  owner: "finance"
}

target {
  Invoice {
    Number: $invoice.Header.InvoiceNumber
    Date: formatDate($invoice.Header.IssuedOn, "yyyy-MM-dd")
    VendorName: $vendor.Name
    Lines: project $invoice.Lines as line => {
      Code: line.Sku,
      Quantity: line.Quantity,
      Amount: line.Amount
    }
  }
}
```

## Programmatic Modeling

You can build transformation documents without using the UI:

```csharp
using ButterMorph.Modeling;

var expressions = ButterMorphModel.Expressions;

var document = ButterMorphModel.CreateDocument()
    .Map(expressions.Path("$source.Customer.Name"), "Customer.Name")
    .Map(expressions.Function("ToUpper", [expressions.Path("$source.Customer.Email")]), "Customer.Email")
    .Build();
```

Programmatic modeling is useful for tests, generators, importers, or a future custom DSL parser.

## Executing a Mapping

Use `ButterMorph.Json` to convert JSON into ButterMorph structure graphs, execute the engine, and write JSON output.

```csharp
using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Json;

var reader = new JsonReader();
var writer = new JsonWriter();

var sourceGraph = reader.Read(new StructureInput
{
    Format = "json",
    Content = """
    {
      "Customer": {
        "Name": "Ada Lovelace"
      }
    }
    """
});

var result = engine.Transform(new TransformationRequest
{
    Definition = transformationDocument,
    Sources = new Dictionary<string, IStructureGraph>
    {
        ["source"] = sourceGraph
    }
});

if (!result.Succeeded)
{
    foreach (var diagnostic in result.Diagnostics)
    {
        Console.WriteLine($"{diagnostic.Code}: {diagnostic.Message}");
    }

    return;
}

var output = writer.Write(result.ResultGraph);
Console.WriteLine(output.Content);
```

## JSON Schema Compatibility

`ButterMorph.Json.Schema` converts between JSON Schema text and `IStructureSchema` for mapping/runtime workflows.

Use it when a host already stores schemas as JSON Schema but wants ButterMorph to read, display, map, or export them.

```csharp
builder.Services.AddButterMorphJsonSchema();
```

The schema designer itself returns ButterMorph schema definitions. JSON Schema compatibility is an adapter layer, not the owner of host persistence.

## Reusable Host Modal

`ButterMorph.Web.Razor` includes a browser helper for opening designers in a reusable iframe/modal host flow. This is the recommended approach for applications that want a modal designer without exposing a separate browser popup UX.

The host opens the designer URL, listens for the save message, refreshes its own state, and stores the returned definition through its backend.

## Samples

| Sample | Description |
| --- | --- |
| `samples/ButterMorph.Playground` | Small host-flow playground. |
| `samples/ButterMorph.StudioPlayground` | Full host simulation with custom types, custom fields, schemas, mappings, catalog injection, local browser persistence, and execution. |

Run the Studio Playground:

```bash
dotnet run --project samples/ButterMorph.StudioPlayground/ButterMorph.StudioPlayground.csproj --urls http://127.0.0.1:5080
```

Open:

```text
http://127.0.0.1:5080/
```

## Build and Test

```bash
dotnet restore ButterMorph.sln
dotnet build ButterMorph.sln
dotnet test ButterMorph.sln --no-build
```

Create packages locally:

```bash
dotnet pack src/ButterMorph/ButterMorph.csproj --configuration Release --output ./nupkgs
dotnet pack src/ButterMorph.Json/ButterMorph.Json.csproj --configuration Release --output ./nupkgs
dotnet pack src/ButterMorph.Json.Schema/ButterMorph.Json.Schema.csproj --configuration Release --output ./nupkgs
dotnet pack src/ButterMorph.SchemaDesign/ButterMorph.SchemaDesign.csproj --configuration Release --output ./nupkgs
dotnet pack src/ButterMorph.Design/ButterMorph.Design.csproj --configuration Release --output ./nupkgs
dotnet pack src/ButterMorph.Web.Razor/ButterMorph.Web.Razor.csproj --configuration Release --output ./nupkgs
```

## Design Principles

- Keep ButterMorph modular and NuGet-friendly.
- Keep hosts responsible for persistence and business rules.
- Keep playgrounds as examples, not hidden core implementations.
- Keep clean definitions separate from operational validation results.
- Store host IDs outside ButterMorph payloads.
- Prefer DSL for mapping portability.
- Preserve schema history through versioned custom types and custom fields.
