namespace ButterMorph.Dsl;

// Represents a path expression in the internal syntax tree.
internal sealed class PathNode : AstNode
{
    internal string Path { get; set; } = string.Empty;
}
