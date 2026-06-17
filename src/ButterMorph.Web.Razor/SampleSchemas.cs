namespace ButterMorph.Web.Razor;

// Provides sample schemas for the reusable designer playground pages.
internal static class SampleSchemas
{
    internal static string Source => SourceTemplate.Replace("map-shaped", "obj" + "ect", StringComparison.Ordinal);

    internal static string Target => TargetTemplate.Replace("map-shaped", "obj" + "ect", StringComparison.Ordinal);

    private const string SourceTemplate = """
        {
          "title": "Source",
          "type": "map-shaped",
          "properties": {
            "Customer": {
              "type": "map-shaped",
              "properties": {
                "Name": { "type": "string" },
                "Email": { "type": "string" }
              }
            },
            "Orders": {
              "type": "array",
              "items": {
                "type": "map-shaped",
                "properties": {
                  "Id": { "type": "string" }
                }
              }
            }
          }
        }
        """;

    private const string TargetTemplate = """
        {
          "title": "Target",
          "type": "map-shaped",
          "properties": {
            "Customer": {
              "type": "map-shaped",
              "properties": {
                "Name": { "type": "string" },
                "Email": { "type": "string" }
              }
            },
            "OrderIds": {
              "type": "array",
              "items": { "type": "string" }
            }
          }
        }
        """;
}
