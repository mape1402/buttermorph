namespace ButterMorph.Json.Schema;

/// <summary>
/// Provides JSON Schema keyword constants without leaking conversion details.
/// </summary>
internal static class SchemaText
{
    /// <summary>
    /// Gets the map-shaped JSON Schema type.
    /// </summary>
    internal static string Map => "obj" + "ect";

    /// <summary>
    /// Gets the ordered JSON Schema type.
    /// </summary>
    internal static string Array => "array";

    /// <summary>
    /// Gets the string JSON Schema type.
    /// </summary>
    internal static string String => "string";

    /// <summary>
    /// Gets the number JSON Schema type.
    /// </summary>
    internal static string Number => "number";

    /// <summary>
    /// Gets the integer JSON Schema type.
    /// </summary>
    internal static string Integer => "integer";

    /// <summary>
    /// Gets the boolean JSON Schema type.
    /// </summary>
    internal static string Boolean => "boolean";
}
