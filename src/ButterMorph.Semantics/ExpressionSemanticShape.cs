namespace ButterMorph.Semantics;

using ButterMorph.Abstractions;
using ButterMorph.Core;

// Carries an inferred expression value kind and optional schema node.
internal sealed class ExpressionSemanticShape
{
    internal FunctionValueKind ValueKind { get; set; }

    internal bool HasSchema { get; set; }

    internal ISchemaNode SchemaNode { get; set; } = new SchemaNode();
}
