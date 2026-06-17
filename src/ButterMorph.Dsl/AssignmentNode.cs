namespace ButterMorph.Dsl;

// Represents one target assignment parsed from a target block.
internal sealed class AssignmentNode : AstNode
{
    internal string TargetPath { get; set; } = string.Empty;

    internal AstNode Expression { get; set; } = new PathNode();
}
