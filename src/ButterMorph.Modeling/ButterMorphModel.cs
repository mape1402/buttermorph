namespace ButterMorph.Modeling;

/// <summary>
/// Provides entry points for ButterMorph modeling builders.
/// </summary>
public static class ButterMorphModel
{
    // Reuses the stateless expression builder entry point.
    private static readonly IExpressionBuilder ExpressionBuilder = new ExpressionBuilder();

    /// <summary>
    /// Gets the expression builder.
    /// </summary>
    public static IExpressionBuilder Expressions => ExpressionBuilder;

    /// <summary>
    /// Creates a transformation document builder.
    /// </summary>
    /// <returns>The transformation document builder.</returns>
    public static ITransformationDocumentBuilder CreateDocument()
    {
        return new TransformationDocumentBuilder();
    }

    /// <summary>
    /// Creates a structure schema builder.
    /// </summary>
    /// <param name="name">The schema name.</param>
    /// <returns>The structure schema builder.</returns>
    public static IStructureSchemaBuilder CreateSchema(string name)
    {
        return new StructureSchemaBuilder(name);
    }

    /// <summary>
    /// Creates a schema node builder.
    /// </summary>
    /// <returns>The schema node builder.</returns>
    public static ISchemaNodeBuilder CreateNode()
    {
        return new SchemaNodeBuilder();
    }
}
