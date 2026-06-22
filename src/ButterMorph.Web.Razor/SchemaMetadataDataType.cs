namespace ButterMorph.Web.Razor;

/// <summary>
/// Defines supported schema metadata value types.
/// </summary>
public enum SchemaMetadataDataType
{
    /// <summary>
    /// Represents text metadata.
    /// </summary>
    String,

    /// <summary>
    /// Represents decimal metadata.
    /// </summary>
    Number,

    /// <summary>
    /// Represents integer metadata.
    /// </summary>
    Integer,

    /// <summary>
    /// Represents boolean metadata.
    /// </summary>
    Boolean,

    /// <summary>
    /// Represents date metadata.
    /// </summary>
    Date,

    /// <summary>
    /// Represents date-time metadata.
    /// </summary>
    DateTime,

    /// <summary>
    /// Represents array metadata.
    /// </summary>
    Array,

    /// <summary>
    /// Represents structured metadata.
    /// </summary>
    Object
}
