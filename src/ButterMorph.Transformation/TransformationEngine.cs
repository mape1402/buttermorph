namespace ButterMorph.Transformation;

using System;
using System.Collections.Generic;
using System.Linq;
using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Executes transformation mappings against internal structure graphs.
/// </summary>
public sealed class TransformationEngine : ITransformationEngine
{
    // Resolves source values from the execution context.
    private readonly INavigationEngine _navigationEngine;

    // Creates execution contexts from source graphs.
    private readonly IExecutionContextFactory _executionContextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransformationEngine"/> class.
    /// </summary>
    /// <param name="navigationEngine">The navigation engine.</param>
    /// <param name="executionContextFactory">The execution context factory.</param>
    public TransformationEngine(INavigationEngine navigationEngine, IExecutionContextFactory executionContextFactory)
    {
        if (navigationEngine is null)
        {
            throw new InvalidOperationException("A navigation engine must be registered before executing transformations.");
        }

        if (executionContextFactory is null)
        {
            throw new InvalidOperationException("An execution context factory must be registered before executing transformations.");
        }

        _navigationEngine = navigationEngine;
        _executionContextFactory = executionContextFactory;
    }

    /// <summary>
    /// Executes a transformation request.
    /// </summary>
    /// <param name="request">The transformation request.</param>
    /// <returns>The transformation result.</returns>
    public TransformationResult Transform(TransformationRequest request)
    {
        List<DiagnosticEntry> diagnostics = [];
        StructureNode root = CreateTargetRoot();

        if (request.Definition is not ITransformationDocument document)
        {
            diagnostics.Add(CreateDiagnostic("BMTR001", "Transformation request definition must implement ITransformationDocument.", string.Empty));
            return CreateResult(root, diagnostics);
        }

        IExecutionContext context = _executionContextFactory.Create(request.Sources);
        HashSet<string> assignedTargets = new(StringComparer.Ordinal);

        foreach (ITransformationMapping mapping in document.Mappings)
        {
            ApplyMapping(context, root, mapping, assignedTargets, diagnostics);
        }

        return CreateResult(root, diagnostics);
    }

    // Applies one mapping and records all mapping failures as diagnostics.
    private void ApplyMapping(IExecutionContext context, StructureNode root, ITransformationMapping mapping, HashSet<string> assignedTargets, List<DiagnosticEntry> diagnostics)
    {
        if (!TryResolveSource(context, mapping, diagnostics, out IScalarStructureNode sourceNode))
        {
            return;
        }

        if (!TryValidateTargetPath(mapping.TargetPath, diagnostics))
        {
            return;
        }

        if (!assignedTargets.Add(mapping.TargetPath))
        {
            diagnostics.Add(CreateDiagnostic("BMTR005", $"Target path '{mapping.TargetPath}' is assigned more than once.", mapping.TargetPath));
            return;
        }

        AssignScalar(root, mapping.TargetPath, sourceNode.Value, diagnostics);
    }

    // Resolves and validates that a mapping source is scalar.
    private bool TryResolveSource(IExecutionContext context, ITransformationMapping mapping, List<DiagnosticEntry> diagnostics, out IScalarStructureNode sourceNode)
    {
        sourceNode = new ScalarStructureNode();

        try
        {
            IStructureNode node = _navigationEngine.Select(context, mapping.SourcePath);

            if (node is IScalarStructureNode scalarNode)
            {
                sourceNode = scalarNode;
                return true;
            }

            diagnostics.Add(CreateDiagnostic("BMTR003", $"Source path '{mapping.SourcePath}' must resolve to a scalar node.", mapping.SourcePath));
            return false;
        }
        catch (Exception exception) when (exception is FormatException || exception is KeyNotFoundException || exception is InvalidOperationException || exception is IndexOutOfRangeException)
        {
            diagnostics.Add(CreateDiagnostic("BMTR002", exception.Message, mapping.SourcePath));
            return false;
        }
    }

    // Validates the v1 target path syntax.
    private static bool TryValidateTargetPath(string targetPath, List<DiagnosticEntry> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(targetPath) || targetPath.StartsWith(".", StringComparison.Ordinal) || targetPath.EndsWith(".", StringComparison.Ordinal) || targetPath.Contains("..", StringComparison.Ordinal))
        {
            diagnostics.Add(CreateDiagnostic("BMTR004", $"Target path '{targetPath}' is not valid.", targetPath));
            return false;
        }

        if (targetPath.Contains("[", StringComparison.Ordinal) || targetPath.Contains("]", StringComparison.Ordinal))
        {
            diagnostics.Add(CreateDiagnostic("BMTR004", $"Target path '{targetPath}' cannot contain array syntax in transformation v1.", targetPath));
            return false;
        }

        return true;
    }

    // Assigns a scalar value to a map-style target path.
    private static void AssignScalar(StructureNode root, string targetPath, IScalarValue value, List<DiagnosticEntry> diagnostics)
    {
        string[] segments = targetPath.Split('.');
        StructureNode current = root;

        for (int index = 0; index < segments.Length - 1; index++)
        {
            if (!TryGetOrCreateObjectChild(current, segments[index], out StructureNode child))
            {
                diagnostics.Add(CreateDiagnostic("BMTR005", $"Target path '{targetPath}' conflicts with an existing scalar node.", targetPath));
                return;
            }

            current = child;
        }

        string leafName = segments[^1];

        if (current.Children.Any(child => string.Equals(child.Name, leafName, StringComparison.Ordinal)))
        {
            diagnostics.Add(CreateDiagnostic("BMTR005", $"Target path '{targetPath}' conflicts with an existing node.", targetPath));
            return;
        }

        List<IStructureNode> children = [.. current.Children];
        children.Add(CreateScalarNode(leafName, value));
        current.Children = children;
    }

    // Finds an existing map child or creates one when absent.
    private static bool TryGetOrCreateObjectChild(StructureNode parent, string childName, out StructureNode child)
    {
        foreach (IStructureNode existingChild in parent.Children)
        {
            if (!string.Equals(existingChild.Name, childName, StringComparison.Ordinal))
            {
                continue;
            }

            if (existingChild is StructureNode existingObject && existingObject.Kind == StructureNodeKind.Object)
            {
                child = existingObject;
                return true;
            }

            child = new StructureNode();
            return false;
        }

        child = new StructureNode
        {
            Name = childName,
            Kind = StructureNodeKind.Object,
            Children = []
        };

        List<IStructureNode> children = [.. parent.Children];
        children.Add(child);
        parent.Children = children;
        return true;
    }

    // Creates a scalar node by copying the source scalar value.
    private static IStructureNode CreateScalarNode(string name, IScalarValue value)
    {
        return new ScalarStructureNode
        {
            Name = name,
            Value = new ScalarValue
            {
                DataType = value.DataType,
                RawValue = value.RawValue,
                IsNull = value.IsNull
            },
            Children = []
        };
    }

    // Creates the v1 target root node.
    private static StructureNode CreateTargetRoot()
    {
        return new StructureNode
        {
            Name = "$root",
            Kind = StructureNodeKind.Object,
            Children = []
        };
    }

    // Creates a transformation result from the current target root and diagnostics.
    private static TransformationResult CreateResult(IStructureNode root, IReadOnlyCollection<DiagnosticEntry> diagnostics)
    {
        return new TransformationResult
        {
            Succeeded = diagnostics.Count == 0,
            ResultGraph = new StructureGraph
            {
                Root = root,
                Nodes = CollectNodes(root)
            },
            Diagnostics = diagnostics
        };
    }

    // Collects all graph nodes in traversal order.
    private static IReadOnlyCollection<IStructureNode> CollectNodes(IStructureNode root)
    {
        List<IStructureNode> nodes = [];
        CollectNodes(root, nodes);
        return nodes;
    }

    // Recursively collects graph nodes in traversal order.
    private static void CollectNodes(IStructureNode node, List<IStructureNode> nodes)
    {
        nodes.Add(node);

        foreach (IStructureNode child in node.Children)
        {
            CollectNodes(child, nodes);
        }
    }

    // Creates an error diagnostic for transformation failures.
    private static DiagnosticEntry CreateDiagnostic(string code, string message, string path)
    {
        return new DiagnosticEntry
        {
            Code = code,
            Message = message,
            Path = path,
            Severity = "Error"
        };
    }
}
