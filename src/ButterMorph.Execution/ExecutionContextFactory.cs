namespace ButterMorph.Execution;

using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Creates execution contexts for pipeline runs.
/// </summary>
public sealed class ExecutionContextFactory : IExecutionContextFactory
{
    /// <summary>
    /// Creates an execution context from source graphs.
    /// </summary>
    /// <param name="sources">The source graphs.</param>
    /// <returns>The execution context.</returns>
    public IExecutionContext Create(IReadOnlyDictionary<string, IStructureGraph> sources)
    {
        return new ExecutionContext
        {
            Sources = sources,
            Diagnostics = new DiagnosticCollection()
        };
    }
}
