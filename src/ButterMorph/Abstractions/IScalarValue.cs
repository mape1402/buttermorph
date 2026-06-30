namespace ButterMorph.Abstractions;

/// <summary>
/// Represents a format-independent scalar data value.
/// </summary>
public interface IScalarValue
{
    /// <summary>
    /// Gets a value indicating whether the scalar value represents null.
    /// </summary>
    bool IsNull { get; }

    /// <summary>
    /// Gets the logical scalar type name.
    /// </summary>
    string DataType { get; }

    /// <summary>
    /// Gets the raw serialized scalar value.
    /// </summary>
    string RawValue { get; }
}
