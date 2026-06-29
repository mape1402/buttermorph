namespace ButterMorph.StudioPlayground.Services;

using System.Text.Json;
using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Json;
using ButterMorph.StudioPlayground.Models;
using Microsoft.AspNetCore.Http.HttpResults;

/// <summary>
/// Maps Studio Playground endpoints.
/// </summary>
internal static class StudioEndpoints
{
    // Accepts browser camelCase payloads and C# PascalCase payloads.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Maps all Studio API endpoints.
    /// </summary>
    /// <param name="app">The web application.</param>
    public static void MapStudioEndpoints(this WebApplication app)
    {
        app.MapGet("/api/state", (StudioStore store) => CreateState(store));
        app.MapGet("/api/{kind}/{contextKey}", (string kind, string contextKey, StudioStore store) => GetItem(kind, contextKey, store));
        app.MapPost("/api/{kind}", async (string kind, StudioStore store, HttpRequest request) =>
        {
            JsonElement body = await JsonSerializer.DeserializeAsync<JsonElement>(request.Body, JsonOptions);
            string contextKey = body.TryGetProperty("contextKey", out JsonElement keyElement) ? keyElement.GetString() : string.Empty;
            string name = body.TryGetProperty("name", out JsonElement nameElement) ? nameElement.GetString() : string.Empty;
            contextKey = string.IsNullOrWhiteSpace(contextKey) ? CreateContextKey(kind) : contextKey;
            name = string.IsNullOrWhiteSpace(name) ? CreateDisplayName(kind) : name;

            CreateDraft(kind, contextKey, name, store);

            return Results.Json(new { contextKey, name });
        });
        app.MapDelete("/api/{kind}/{contextKey}", (string kind, string contextKey, StudioStore store) =>
        {
            bool removed = store.Delete(kind, contextKey);
            return Results.Json(new { removed });
        });
        app.MapPost("/api/schemas/{contextKey}/injection", async (string contextKey, StudioStore store, HttpRequest request) =>
        {
            StudioInjectionRequest body = await JsonSerializer.DeserializeAsync<StudioInjectionRequest>(request.Body, JsonOptions) ?? new StudioInjectionRequest();
            if (!store.TryGetSchema(contextKey, out StudioSchema schema))
            {
                return Results.NotFound();
            }

            schema.InjectedCustomTypeKeys.Clear();
            schema.InjectedCustomTypeKeys.AddRange(body.CustomTypeKeys);
            schema.InjectedCustomFieldKeys.Clear();
            schema.InjectedCustomFieldKeys.AddRange(body.CustomFieldKeys);
            store.SaveSchema(schema);

            return Results.Json(new
            {
                schema.ContextKey,
                schema.Key,
                schema.Name,
                schema.InjectedCustomTypeKeys,
                schema.InjectedCustomFieldKeys
            });
        });
        app.MapPost("/api/mappings/{contextKey}/settings", async (string contextKey, StudioStore store, HttpRequest request) =>
        {
            StudioMappingSettingsRequest body = await JsonSerializer.DeserializeAsync<StudioMappingSettingsRequest>(request.Body, JsonOptions) ?? new StudioMappingSettingsRequest();
            if (!store.TryGetMapping(contextKey, out StudioMapping mapping))
            {
                mapping = new StudioMapping { ContextKey = contextKey, Name = body.Name };
            }

            mapping.Name = string.IsNullOrWhiteSpace(body.Name) ? mapping.Name : body.Name;
            mapping.TargetSchemaKey = body.TargetSchemaKey;
            mapping.SourceSchemaKeys.Clear();
            foreach (KeyValuePair<string, string> source in body.SourceSchemaKeys)
            {
                if (!string.IsNullOrWhiteSpace(source.Key) && !string.IsNullOrWhiteSpace(source.Value))
                {
                    mapping.SourceSchemaKeys[source.Key] = source.Value;
                }
            }

            if (mapping.Document == null)
            {
                mapping.Document = new TransformationDocument();
            }

            store.SaveMapping(mapping);
            return Results.Json(mapping);
        });
        app.MapPost("/api/mappings/{contextKey}/execute", async (
            string contextKey,
            StudioStore store,
            StudioButterMorphHost host,
            IButterMorphEngine engine,
            HttpRequest request) =>
        {
            StudioExecutionRequest body = await JsonSerializer.DeserializeAsync<StudioExecutionRequest>(request.Body, JsonOptions) ?? new StudioExecutionRequest();
            if (!store.TryGetMapping(contextKey, out StudioMapping mapping))
            {
                return Results.NotFound();
            }

            return Results.Json(Execute(mapping, body, host, engine));
        });
    }

    private static IResult CreateState(StudioStore store)
    {
        return Results.Json(new
        {
            customTypes = store.CustomTypes,
            customFields = store.CustomFields,
            schemas = store.Schemas,
            mappings = store.Mappings.Select(mapping => new
            {
                mapping.ContextKey,
                mapping.Name,
                mapping.TargetSchemaKey,
                mapping.SourceSchemaKeys,
                mapping.SourceSamples,
                mapping.DslContent,
                mapping.SavedAt
            })
        });
    }

    private static IResult GetItem(string kind, string contextKey, StudioStore store)
    {
        if (kind == "customTypes" && store.TryGetCustomType(contextKey, out StudioCustomType customType))
        {
            return Results.Json(customType);
        }

        if (kind == "customFields" && store.TryGetCustomField(contextKey, out StudioCustomField customField))
        {
            return Results.Json(customField);
        }

        if (kind == "schemas" && store.TryGetSchema(contextKey, out StudioSchema schema))
        {
            return Results.Json(schema);
        }

        if (kind == "mappings" && store.TryGetMapping(contextKey, out StudioMapping mapping))
        {
            return Results.Json(mapping);
        }

        return Results.NotFound();
    }

    private static void CreateDraft(string kind, string contextKey, string name, StudioStore store)
    {
        if (kind == "customTypes")
        {
            store.SaveCustomType(new StudioCustomType { ContextKey = contextKey, Name = name, Key = Slug(name), Version = "1.0.0", BaseType = "string" });
            return;
        }

        if (kind == "customFields")
        {
            store.SaveCustomField(new StudioCustomField { ContextKey = contextKey, Name = name, Key = Slug(name), AppliesToJson = "[\"Schema\",\"Field\"]", IsActive = true });
            return;
        }

        if (kind == "schemas")
        {
            store.SaveSchema(new StudioSchema { ContextKey = contextKey, Name = name, Key = Slug(name), Version = "1.0.0", JsonSchema = CreateEmptySchemaJson(Slug(name), name) });
            return;
        }

        if (kind == "mappings")
        {
            store.SaveMapping(new StudioMapping { ContextKey = contextKey, Name = name, Document = new TransformationDocument() });
        }
    }

    private static StudioExecutionView Execute(StudioMapping mapping, StudioExecutionRequest request, StudioButterMorphHost host, IButterMorphEngine engine)
    {
        Dictionary<string, IStructureGraph> graphs = new(StringComparer.OrdinalIgnoreCase);
        JsonReader reader = new();
        JsonWriter writer = new();

        foreach (KeyValuePair<string, string> source in mapping.SourceSchemaKeys)
        {
            string json = request.Sources.TryGetValue(source.Key, out string postedJson)
                ? postedJson
                : mapping.SourceSamples.GetValueOrDefault(source.Key, "{}");
            graphs[source.Key] = reader.Read(new StructureInput { Content = json, Format = "json" });
        }

        TransformationResult result = engine.Transform(new TransformationRequest
        {
            Sources = graphs,
            Definition = mapping.Document
        });

        string output = result.ResultGraph == null
            ? string.Empty
            : writer.Write(result.ResultGraph).Content;

        return new StudioExecutionView
        {
            Succeeded = result.Succeeded,
            OutputJson = PrettyJson(output),
            Diagnostics = result.Diagnostics.Select(item => item.Code + ": " + item.Message).ToArray()
        };
    }

    private static string CreateContextKey(string kind)
    {
        return kind.TrimEnd('s') + "-" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    private static string CreateDisplayName(string kind)
    {
        return kind switch
        {
            "customTypes" => "New Custom Type",
            "customFields" => "New Custom Field",
            "schemas" => "New Schema",
            "mappings" => "New Mapping",
            _ => "New Item"
        };
    }

    private static string Slug(string value)
    {
        string lowered = value.Trim().ToLowerInvariant();
        return string.Join("-", lowered.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string CreateEmptySchemaJson(string key, string name)
    {
        return "{\"key\":\"" + key + "\",\"name\":\"" + name + "\",\"version\":\"1.0.0\",\"type\":\"object\",\"properties\":{}}";
    }

    private static string PrettyJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return string.Empty;
        }

        using JsonDocument document = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions { WriteIndented = true });
    }
}
