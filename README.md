# ButterMorph

ButterMorph is a modular .NET mapping and schema design toolkit for building typed transformation documents, reusable schema definitions, and embeddable Razor designer experiences.

It is designed so host applications can provide schemas, catalogs, and persistence while ButterMorph owns the mapping/schema designer runtime and UI.

## Packages

- `ButterMorph` — core runtime, transformation model, expressions, functions, validation, semantics, DSL, and dependency injection.
- `ButterMorph.Json` — JSON graph reader and writer adapters.
- `ButterMorph.Json.Schema` — JSON Schema import/export compatibility for ButterMorph schemas.
- `ButterMorph.SchemaDesign` — schema type, custom field, payload schema, and metadata design services.
- `ButterMorph.Design` — headless mapping design sessions, schema exploration, capabilities, and diagnostics.
- `ButterMorph.Web.Razor` — reusable Razor Pages designers for mapping and schema tooling.

## Host Integration Model

ButterMorph is intended to be consumed by a host application.

The host owns:

- database persistence;
- business routing and authorization;
- which custom types and custom fields are available;
- launching the designers;
- saving returned definitions or mappings.

ButterMorph owns:

- schema and mapping designer UI;
- schema/mapping rehydration;
- transformation document generation;
- DSL import/export;
- schema definition generation;
- runtime execution over typed graphs.

## Minimal Razor Host Setup

```csharp
builder.Services.AddButterMorph();
builder.Services.AddButterMorphJsonSchema();
builder.Services.AddButterMorphSchemaDesign();
builder.Services.AddButterMorphDesign();
builder.Services.AddButterMorphRazorDesigner();

app.MapButterMorphDesigner("/buttermorph");
```

Host applications can implement the provided designer host interfaces to preload state and receive save results.

## Samples

- `samples/ButterMorph.Playground` — simple host flow demo.
- `samples/ButterMorph.StudioPlayground` — structured host simulation with custom types, custom fields, schemas, mappings, and execution.

## Build

```bash
dotnet build ButterMorph.sln
dotnet test ButterMorph.sln --no-build
```

