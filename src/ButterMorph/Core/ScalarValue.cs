using ButterMorph.Abstractions;

namespace ButterMorph.Core;

/// <summary>
/// Represents a canonical scalar data value.
/// </summary>
public sealed class ScalarValue : IScalarValue
{
    /// <summary>
    /// Gets or sets a value indicating whether the scalar value represents null.
    /// </summary>
    public bool IsNull { get; set; }

    /// <summary>
    /// Gets or sets the logical scalar type name.
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw serialized scalar value.
    /// </summary>
    public string RawValue { get; set; } = string.Empty;
}
