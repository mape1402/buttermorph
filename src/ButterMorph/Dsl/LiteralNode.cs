namespace ButterMorph.Dsl;

// Represents a scalar literal expression in the internal syntax tree.
internal sealed class LiteralNode : AstNode
{
    internal string DataType { get; set; } = string.Empty;

    internal string RawValue { get; set; } = string.Empty;

    internal bool IsNull { get; set; }
}
