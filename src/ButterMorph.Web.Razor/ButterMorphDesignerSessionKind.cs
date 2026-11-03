namespace ButterMorph.Web.Razor;

/// <summary>
/// Identifies a designer whose interactive state is retained by ButterMorph.
/// </summary>
public enum ButterMorphDesignerSessionKind
{
    /// <summary>Mapping and transformation designer.</summary>
    Mapping,

    /// <summary>Validation and condition designer.</summary>
    Validation,

    /// <summary>Generic structure schema designer.</summary>
    Schema
}
