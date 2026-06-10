namespace ButterMorph.UnitTests;

using System.Collections.Generic;
using ButterMorph.Abstractions;
using ButterMorph.Core;
using ButterMorph.Execution;

/// <summary>
/// Verifies execution context factory behavior.
/// </summary>
public sealed class ExecutionContextFactoryTests
{
    /// <summary>
    /// Confirms that source graphs are preserved and diagnostics are created.
    /// </summary>
    [Fact]
    public void CreatePreservesSourcesAndCreatesDiagnostics()
    {
        StructureGraph graph = new();
        IReadOnlyDictionary<string, IStructureGraph> sources = new Dictionary<string, IStructureGraph>
        {
            ["source"] = graph
        };
        IExecutionContextFactory factory = new ExecutionContextFactory();

        IExecutionContext context = factory.Create(sources);

        Assert.Same(sources, context.Sources);
        Assert.NotNull(context.Diagnostics);
        Assert.Empty(context.Diagnostics.Entries);
    }
}
