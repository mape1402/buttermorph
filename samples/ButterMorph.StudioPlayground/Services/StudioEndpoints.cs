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
        app.MapPost("/api/state/hydrate", async (StudioStore store, HttpRequest request) =>
        {
            StudioStateSnapshot snapshot = await JsonSerializer.DeserializeAsync<StudioStateSnapshot>(request.Body, JsonOptions) ?? new StudioStateSnapshot();
            store.ReplaceFromSnapshot(snapshot);
            return CreateState(store);
        });
        app.MapGet("/api/{kind}/{id}", (string kind, string id, StudioStore store) => GetItem(kind, id, store));
        app.MapPost("/api/{kind}", async (string kind, StudioStore store, HttpRequest request) =>
        {
            JsonElement body = await JsonSerializer.DeserializeAsync<JsonElement>(request.Body, JsonOptions);
            string id = body.TryGetProperty("id", out JsonElement keyElement) ? keyElement.GetString() : string.Empty;
            string name = body.TryGetProperty("name", out JsonElement nameElement) ? nameElement.GetString() : string.Empty;
            id = string.IsNullOrWhiteSpace(id) ? CreateId(kind) : id;
            name = string.IsNullOrWhiteSpace(name) ? CreateDisplayName(kind) : name;

            return Results.Json(new { id, name });
        });
        app.MapDelete("/api/{kind}/{id}", (string kind, string id, StudioStore store) =>
        {
            bool removed = store.Delete(kind, id);
            return Results.Json(new { removed });
        });
        app.MapPost("/api/schemas/{id}/injection", async (string id, StudioStore store, HttpRequest request) =>
        {
            StudioInjectionRequest body = await JsonSerializer.DeserializeAsync<StudioInjectionRequest>(request.Body, JsonOptions) ?? new StudioInjectionRequest();
            if (!store.TryGetSchema(id, out StudioSchema schema))
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
                schema.Id,
                schema.Key,
                schema.Name,
                schema.InjectedCustomTypeKeys,
                schema.InjectedCustomFieldKeys
            });
        });
        app.MapPost("/api/mappings/{id}/settings", async (string id, StudioStore store, HttpRequest request) =>
        {
            StudioMappingSettingsRequest body = await JsonSerializer.DeserializeAsync<StudioMappingSettingsRequest>(request.Body, JsonOptions) ?? new StudioMappingSettingsRequest();
            if (!store.TryGetMapping(id, out StudioMapping mapping))
            {
                mapping = new StudioMapping { Id = id, Name = body.Name };
            }

            mapping.Name = string.IsNullOrWhiteSpace(body.Name) ? mapping.Name : body.Name;
            mapping.TargetSchemaId = body.TargetSchemaId;
            mapping.ShowSchemaActions = body.ShowSchemaActions;
            mapping.SourceSchemaIds.Clear();
            foreach (KeyValuePair<string, string> source in body.SourceSchemaIds)
            {
                if (!string.IsNullOrWhiteSpace(source.Key) && !string.IsNullOrWhiteSpace(source.Value))
                {
                    mapping.SourceSchemaIds[source.Key] = source.Value;
                }
            }

            if (mapping.Document == null)
            {
                mapping.Document = new TransformationDocument();
            }

            store.SaveMapping(mapping);
            return Results.Json(mapping);
        });
        app.MapPost("/api/mappings/{id}/setup", async (string id, StudioStore store, HttpRequest request) =>
        {
            StudioMappingSettingsRequest body = await JsonSerializer.DeserializeAsync<StudioMappingSettingsRequest>(request.Body, JsonOptions) ?? new StudioMappingSettingsRequest();
            StudioMappingSetup setup = new()
            {
                Id = id,
                Name = body.Name,
                TargetSchemaId = body.TargetSchemaId,
                ShowSchemaActions = body.ShowSchemaActions
            };

            foreach (KeyValuePair<string, string> source in body.SourceSchemaIds)
            {
                if (!string.IsNullOrWhiteSpace(source.Key) && !string.IsNullOrWhiteSpace(source.Value))
                {
                    setup.SourceSchemaIds[source.Key] = source.Value;
                }
            }

            store.SaveMappingSetup(setup);
            return Results.Json(new { setup.Id, setup.Name, setup.TargetSchemaId, setup.SourceSchemaIds, setup.ShowSchemaActions });
        });
        app.MapPost("/api/mappings/{id}/execute", async (
            string id,
            StudioStore store,
            StudioButterMorphHost host,
            IButterMorphEngine engine,
            HttpRequest request) =>
        {
            StudioExecutionRequest body = await JsonSerializer.DeserializeAsync<StudioExecutionRequest>(request.Body, JsonOptions) ?? new StudioExecutionRequest();
            if (!store.TryGetMapping(id, out StudioMapping mapping))
            {
                return Results.NotFound();
            }

            foreach (KeyValuePair<string, string> source in body.Sources)
            {
                mapping.SourceSamples[source.Key] = source.Value;
            }

            mapping.Document = host.ResolveMappingDocument(mapping);
            store.SaveMapping(mapping);

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
                mapping.Id,
                mapping.Name,
                mapping.TargetSchemaId,
                mapping.SourceSchemaIds,
                mapping.SourceSamples,
                mapping.DslContent,
                mapping.ShowSchemaActions,
                mapping.SavedAt
            })
        });
    }

    private static IResult GetItem(string kind, string id, StudioStore store)
    {
        if (kind == "customTypes" && store.TryGetCustomType(id, out StudioCustomType customType))
        {
            return Results.Json(customType);
        }

        if (kind == "customFields" && store.TryGetCustomField(id, out StudioCustomField customField))
        {
            return Results.Json(customField);
        }

        if (kind == "schemas" && store.TryGetSchema(id, out StudioSchema schema))
        {
            return Results.Json(schema);
        }

        if (kind == "mappings" && store.TryGetMapping(id, out StudioMapping mapping))
        {
            return Results.Json(mapping);
        }

        return Results.NotFound();
    }

    private static StudioExecutionView Execute(StudioMapping mapping, StudioExecutionRequest request, StudioButterMorphHost host, IButterMorphEngine engine)
    {
        Dictionary<string, IStructureGraph> graphs = new(StringComparer.OrdinalIgnoreCase);
        JsonReader reader = new();
        JsonWriter writer = new();

        foreach (KeyValuePair<string, string> source in mapping.SourceSchemaIds)
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

    private static string CreateId(string kind)
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

