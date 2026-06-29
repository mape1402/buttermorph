# ButterMorph Studio Playground

This sample demonstrates a plug-and-play host integration for ButterMorph.

## Host Setup

Register ButterMorph and the reusable Razor designer:

```csharp
builder.Services.AddButterMorph();
builder.Services.AddButterMorphJsonSchema();
builder.Services.AddButterMorphRazorDesigner();
builder.Services.AddSingleton<IButterMorphDesignerHost, YourMappingHost>();
builder.Services.AddSingleton<IButterMorphSchemaTypeDesignerHost, YourSchemaTypeHost>();
builder.Services.AddSingleton<IButterMorphFieldMetadataDesignerHost, YourMetadataFieldHost>();
builder.Services.AddSingleton<IButterMorphPayloadSchemaDesignerHost, YourPayloadSchemaHost>();

app.MapButterMorphDesigner("/buttermorph");
```

Load the helper script in the host page:

```html
<script src="/_content/ButterMorph.Web.Razor/buttermorph/buttermorph-host.js"></script>
```

Open designers as a blocking iframe modal:

```javascript
window.ButterMorphHost.openFrame("/buttermorph/designer?context=mapping-1&popup=true", {
  title: "ButterMorph Mapping Designer",
  width: 1420,
  height: 900
});
```

## Ownership Model

- ButterMorph owns the reusable designers, schema conversion, mapping runtime and engine.
- The host owns persistence, catalog availability, business context and execution samples.
- The host decides which custom types and custom fields are injected into each schema designer session.
- The host decides which schemas are source and target schemas for each mapping designer session.
