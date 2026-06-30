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
    // Evaluates source expressions before target assignment.
    private readonly ITransformationExpressionEvaluator _expressionEvaluator;

    // Creates execution contexts from source graphs.
    private readonly IExecutionContextFactory _executionContextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransformationEngine"/> class.
    /// </summary>
    /// <param name="expressionEvaluator">The transformation expression evaluator.</param>
    /// <param name="executionContextFactory">The execution context factory.</param>
    public TransformationEngine(ITransformationExpressionEvaluator expressionEvaluator, IExecutionContextFactory executionContextFactory)
    {
        if (expressionEvaluator is null)
        {
            throw new InvalidOperationException("A transformation expression evaluator must be registered before executing transformations.");
        }

        if (executionContextFactory is null)
        {
            throw new InvalidOperationException("An execution context factory must be registered before executing transformations.");
        }

        _expressionEvaluator = expressionEvaluator;
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
        ITransformationExpressionEvaluationResult evaluationResult = _expressionEvaluator.Evaluate(new TransformationExpressionEvaluationContext
        {
            ExecutionContext = context,
            Expression = mapping.SourceExpression,
            Aliases = new Dictionary<string, IStructureNode>()
        });

        if (!evaluationResult.Succeeded)
        {
            diagnostics.Add(CreateDiagnostic("BMTR002", $"Source expression for target '{mapping.TargetPath}' failed.", mapping.TargetPath));
            diagnostics.AddRange(evaluationResult.Diagnostics);
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

        AssignResult(root, mapping.TargetPath, evaluationResult.Result, diagnostics);
    }

    // Validates the target path syntax.
    private static bool TryValidateTargetPath(string targetPath, List<DiagnosticEntry> diagnostics)
    {
        if (string.IsNullOrWhiteSpace(targetPath) || targetPath.StartsWith(".", StringComparison.Ordinal) || targetPath.EndsWith(".", StringComparison.Ordinal) || targetPath.Contains("..", StringComparison.Ordinal))
        {
            diagnostics.Add(CreateDiagnostic("BMTR003", $"Target path '{targetPath}' is not valid.", targetPath));
            return false;
        }

        string[] segments = targetPath.Split('.');

        foreach (string segment in segments)
        {
            if (!TryParseSegment(segment, out string name, out int index, out bool hasIndex))
            {
                diagnostics.Add(CreateDiagnostic("BMTR003", $"Target path segment '{segment}' is not valid.", targetPath));
                return false;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                diagnostics.Add(CreateDiagnostic("BMTR003", $"Target path segment '{segment}' is not valid.", targetPath));
                return false;
            }

            if (hasIndex && index < 0)
            {
                diagnostics.Add(CreateDiagnostic("BMTR003", $"Target path segment '{segment}' is not valid.", targetPath));
                return false;
            }
        }

        return true;
    }

    // Assigns an evaluated source result to the target graph.
    private static void AssignResult(StructureNode root, string targetPath, IFunctionResult result, List<DiagnosticEntry> diagnostics)
    {
        string[] segments = targetPath.Split('.');
        StructureNode parent = root;

        for (int segmentIndex = 0; segmentIndex < segments.Length - 1; segmentIndex++)
        {
            if (!TryGetOrCreateParent(parent, segments[segmentIndex], targetPath, diagnostics, out StructureNode nextParent))
            {
                return;
            }

            parent = nextParent;
        }

        string finalSegment = segments[^1];

        if (result is IScalarFunctionResult scalarResult)
        {
            AssignNode(parent, finalSegment, CreateScalarNode(string.Empty, scalarResult.Value), targetPath, diagnostics);
            return;
        }

        if (result is IScalarCollectionFunctionResult scalarCollectionResult)
        {
            if (FinalSegmentHasIndex(finalSegment))
            {
                diagnostics.Add(CreateDiagnostic("BMTR006", $"Result kind '{result.Kind}' cannot be assigned to indexed target '{targetPath}'.", targetPath));
                return;
            }

            AssignNode(parent, finalSegment, CreateScalarArrayNode(string.Empty, scalarCollectionResult.Values), targetPath, diagnostics);
            return;
        }

        if (result is IStructureNodeFunctionResult nodeResult)
        {
            AssignNode(parent, finalSegment, CloneNode(nodeResult.Node, string.Empty), targetPath, diagnostics);
            return;
        }

        if (result is IStructureNodeCollectionFunctionResult nodeCollectionResult)
        {
            if (FinalSegmentHasIndex(finalSegment))
            {
                diagnostics.Add(CreateDiagnostic("BMTR006", $"Result kind '{result.Kind}' cannot be assigned to indexed target '{targetPath}'.", targetPath));
                return;
            }

            AssignNode(parent, finalSegment, CreateNodeArray(string.Empty, nodeCollectionResult.Nodes), targetPath, diagnostics);
            return;
        }

        diagnostics.Add(CreateDiagnostic("BMTR006", $"Result kind '{result.Kind}' cannot be assigned to target '{targetPath}'.", targetPath));
    }

    // Gets or creates an intermediate target parent node.
    private static bool TryGetOrCreateParent(StructureNode current, string segment, string targetPath, List<DiagnosticEntry> diagnostics, out StructureNode parent)
    {
        parent = current;

        TryParseSegment(segment, out string name, out int index, out bool hasIndex);

        if (!hasIndex)
        {
            return TryGetOrCreateObjectChild(current, name, targetPath, diagnostics, out parent);
        }

        if (!TryGetOrCreateArrayChild(current, name, targetPath, diagnostics, out StructureNode arrayNode))
        {
            return false;
        }

        return TryGetOrCreateArrayItem(arrayNode, index, targetPath, diagnostics, out parent);
    }

    // Assigns a node to a final target segment.
    private static void AssignNode(StructureNode parent, string segment, IStructureNode node, string targetPath, List<DiagnosticEntry> diagnostics)
    {
        TryParseSegment(segment, out string name, out int index, out bool hasIndex);

        if (!hasIndex)
        {
            SetNamedChild(parent, CloneNode(node, name), targetPath, diagnostics);
            return;
        }

        if (!TryGetOrCreateArrayChild(parent, name, targetPath, diagnostics, out StructureNode arrayNode))
        {
            return;
        }

        SetArrayItem(arrayNode, index, CloneNode(node, index.ToString(System.Globalization.CultureInfo.InvariantCulture)), targetPath, diagnostics);
    }

    // Finds an existing map child or creates one when absent.
    private static bool TryGetOrCreateObjectChild(StructureNode parent, string childName, string targetPath, List<DiagnosticEntry> diagnostics, out StructureNode child)
    {
        foreach (IStructureNode existingChild in parent.Children)
        {
            if (!string.Equals(existingChild.Name, childName, StringComparison.Ordinal))
            {
                continue;
            }

            if (existingChild is StructureNode existingMap && existingMap.Kind == StructureNodeKind.Object)
            {
                child = existingMap;
                return true;
            }

            child = new StructureNode();
            diagnostics.Add(CreateDiagnostic("BMTR004", $"Target path '{targetPath}' conflicts with existing node '{childName}'.", targetPath));
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

    // Finds an existing array child or creates one when absent.
    private static bool TryGetOrCreateArrayChild(StructureNode parent, string childName, string targetPath, List<DiagnosticEntry> diagnostics, out StructureNode child)
    {
        foreach (IStructureNode existingChild in parent.Children)
        {
            if (!string.Equals(existingChild.Name, childName, StringComparison.Ordinal))
            {
                continue;
            }

            if (existingChild is StructureNode existingArray && existingArray.Kind == StructureNodeKind.Array)
            {
                child = existingArray;
                return true;
            }

            child = new StructureNode();
            diagnostics.Add(CreateDiagnostic("BMTR004", $"Target path '{targetPath}' conflicts with existing node '{childName}'.", targetPath));
            return false;
        }

        child = new StructureNode
        {
            Name = childName,
            Kind = StructureNodeKind.Array,
            Children = []
        };

        List<IStructureNode> children = [.. parent.Children];
        children.Add(child);
        parent.Children = children;
        return true;
    }

    // Gets or creates an indexed map node for intermediate traversal.
    private static bool TryGetOrCreateArrayItem(StructureNode arrayNode, int index, string targetPath, List<DiagnosticEntry> diagnostics, out StructureNode item)
    {
        List<IStructureNode> children = [.. arrayNode.Children];

        while (children.Count <= index)
        {
            children.Add(new StructureNode
            {
                Name = children.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Kind = StructureNodeKind.Object,
                Children = []
            });
        }

        IStructureNode existingItem = children[index];

        if (existingItem is StructureNode existingMap && existingMap.Kind == StructureNodeKind.Object)
        {
            arrayNode.Children = children;
            item = existingMap;
            return true;
        }

        item = new StructureNode();
        diagnostics.Add(CreateDiagnostic("BMTR004", $"Target path '{targetPath}' conflicts with existing array item '{index}'.", targetPath));
        return false;
    }

    // Sets a named child and reports conflicts.
    private static void SetNamedChild(StructureNode parent, IStructureNode node, string targetPath, List<DiagnosticEntry> diagnostics)
    {
        if (parent.Children.Any(child => string.Equals(child.Name, node.Name, StringComparison.Ordinal)))
        {
            diagnostics.Add(CreateDiagnostic("BMTR004", $"Target path '{targetPath}' conflicts with an existing node.", targetPath));
            return;
        }

        List<IStructureNode> children = [.. parent.Children];
        children.Add(node);
        parent.Children = children;
    }

    // Sets an indexed child and reports conflicts.
    private static void SetArrayItem(StructureNode arrayNode, int index, IStructureNode node, string targetPath, List<DiagnosticEntry> diagnostics)
    {
        List<IStructureNode> children = [.. arrayNode.Children];

        while (children.Count < index)
        {
            children.Add(new StructureNode
            {
                Name = children.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Kind = StructureNodeKind.Object,
                Children = []
            });
        }

        if (children.Count == index)
        {
            children.Add(node);
            arrayNode.Children = children;
            return;
        }

        diagnostics.Add(CreateDiagnostic("BMTR004", $"Target path '{targetPath}' conflicts with an existing array item.", targetPath));
    }

    // Parses a target path segment.
    private static bool TryParseSegment(string segment, out string name, out int index, out bool hasIndex)
    {
        name = segment;
        index = -1;
        hasIndex = false;

        int openIndex = segment.IndexOf('[', StringComparison.Ordinal);
        int closeIndex = segment.IndexOf(']', StringComparison.Ordinal);

        if (openIndex < 0 && closeIndex < 0)
        {
            return !string.IsNullOrWhiteSpace(name);
        }

        if (openIndex <= 0 || closeIndex != segment.Length - 1 || closeIndex <= openIndex + 1)
        {
            return false;
        }

        name = segment[..openIndex];
        string indexText = segment[(openIndex + 1)..closeIndex];

        if (!int.TryParse(indexText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out index))
        {
            return false;
        }

        hasIndex = true;
        return index >= 0;
    }

    // Determines whether the final target segment includes an index.
    private static bool FinalSegmentHasIndex(string segment)
    {
        TryParseSegment(segment, out string name, out int index, out bool hasIndex);
        return hasIndex;
    }

    // Creates an array node from scalar values.
    private static IStructureNode CreateScalarArrayNode(string name, IReadOnlyCollection<IScalarValue> values)
    {
        List<IStructureNode> children = [];
        int index = 0;

        foreach (IScalarValue value in values)
        {
            children.Add(CreateScalarNode(index.ToString(System.Globalization.CultureInfo.InvariantCulture), value));
            index++;
        }

        return new StructureNode
        {
            Name = name,
            Kind = StructureNodeKind.Array,
            Children = children
        };
    }

    // Creates an array node from structure nodes.
    private static IStructureNode CreateNodeArray(string name, IReadOnlyCollection<IStructureNode> nodes)
    {
        List<IStructureNode> children = [];
        int index = 0;

        foreach (IStructureNode node in nodes)
        {
            children.Add(CloneNode(node, index.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            index++;
        }

        return new StructureNode
        {
            Name = name,
            Kind = StructureNodeKind.Array,
            Children = children
        };
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

    // Clones a structure node with a new name.
    private static IStructureNode CloneNode(IStructureNode node, string name)
    {
        if (node is IScalarStructureNode scalarNode)
        {
            return CreateScalarNode(name, scalarNode.Value);
        }

        List<IStructureNode> children = [];

        foreach (IStructureNode child in node.Children)
        {
            children.Add(CloneNode(child, child.Name));
        }

        return new StructureNode
        {
            Name = name,
            Kind = node.Kind,
            Children = children
        };
    }

    // Creates the target root node.
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
