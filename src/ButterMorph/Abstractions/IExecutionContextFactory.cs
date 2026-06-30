using System.Collections.Generic;

namespace ButterMorph.Abstractions;

/// <summary>
/// Defines creation behavior for execution contexts.
/// </summary>
public interface IExecutionContextFactory
{
    /// <summary>
    /// Creates an execution context from source graphs.
    /// </summary>
    /// <param name="sources">The source graphs.</param>
    /// <returns>The execution context.</returns>
    IExecutionContext Create(IReadOnlyDictionary<string, IStructureGraph> sources);
}
