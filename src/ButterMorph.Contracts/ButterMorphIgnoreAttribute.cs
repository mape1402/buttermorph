namespace ButterMorph.Contracts;

/// <summary>
/// Excludes a field or property from ButterMorph schema discovery.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class ButterMorphIgnoreAttribute : Attribute
{
}
