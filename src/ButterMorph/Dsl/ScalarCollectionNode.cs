namespace ButterMorph.Dsl;

// Represents a scalar collection literal expression in the internal syntax tree.
internal sealed class ScalarCollectionNode : AstNode
{
    internal List<LiteralNode> Values { get; } = [];
}
