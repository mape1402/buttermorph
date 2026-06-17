namespace ButterMorph.Dsl;

// Represents one named member inside an inline map-shaped expression.
internal sealed class PropertyExpressionNode : AstNode
{
    internal string Name { get; set; } = string.Empty;

    internal AstNode Expression { get; set; } = new PathNode();
}
