namespace ButterMorph.Transformation;

using System;
using System.Collections.Generic;
using ButterMorph.Abstractions;
using ButterMorph.Core;

/// <summary>
/// Evaluates transformation expressions against execution context graphs.
/// </summary>
public sealed class TransformationExpressionEvaluator : ITransformationExpressionEvaluator
{
    // Resolves absolute paths from execution sources.
    private readonly INavigationEngine _navigationEngine;

    // Resolves paths relative to alias nodes.
    private readonly IPathResolver _pathResolver;

    // Resolves function calls by key.
    private readonly IFunctionRegistry _functionRegistry;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransformationExpressionEvaluator"/> class.
    /// </summary>
    /// <param name="navigationEngine">The navigation engine.</param>
    /// <param name="pathResolver">The path resolver.</param>
    /// <param name="functionRegistry">The function registry.</param>
    public TransformationExpressionEvaluator(INavigationEngine navigationEngine, IPathResolver pathResolver, IFunctionRegistry functionRegistry)
    {
        if (navigationEngine is null)
        {
            throw new InvalidOperationException("A navigation engine must be registered before evaluating expressions.");
        }

        if (pathResolver is null)
        {
            throw new InvalidOperationException("A path resolver must be registered before evaluating expressions.");
        }

        if (functionRegistry is null)
        {
            throw new InvalidOperationException("A function registry must be registered before evaluating expressions.");
        }

        _navigationEngine = navigationEngine;
        _pathResolver = pathResolver;
        _functionRegistry = functionRegistry;
    }

    /// <summary>
    /// Evaluates a transformation expression.
    /// </summary>
    /// <param name="context">The expression evaluation context.</param>
    /// <returns>The expression evaluation result.</returns>
    public ITransformationExpressionEvaluationResult Evaluate(TransformationExpressionEvaluationContext context)
    {
        if (context.Expression is IPathExpression pathExpression)
        {
            return EvaluatePath(context, pathExpression);
        }

        if (context.Expression is IScalarLiteralExpression scalarLiteralExpression)
        {
            return CreateSuccess(new ScalarFunctionResult
            {
                Value = scalarLiteralExpression.Value
            });
        }

        if (context.Expression is IScalarCollectionLiteralExpression scalarCollectionLiteralExpression)
        {
            return CreateSuccess(new ScalarCollectionFunctionResult
            {
                Values = scalarCollectionLiteralExpression.Values
            });
        }

        if (context.Expression is IFunctionCallExpression functionCallExpression)
        {
            return EvaluateFunctionCall(context, functionCallExpression);
        }

        if (context.Expression is IConditionalExpression conditionalExpression)
        {
            return EvaluateConditional(context, conditionalExpression);
        }

        if (context.Expression is ICollectionProjectionExpression projectionExpression)
        {
            return EvaluateProjection(context, projectionExpression);
        }

        if (context.Expression is IObjectExpression objectExpression)
        {
            return EvaluateObject(context, objectExpression);
        }

        if (context.Expression is IArrayExpression arrayExpression)
        {
            return EvaluateArray(context, arrayExpression);
        }

        return CreateFailure(CreateDiagnostic("BMEX002", $"Expression kind '{context.Expression.Kind}' is not supported.", string.Empty));
    }

    // Evaluates an absolute source path or alias-relative path.
    private ITransformationExpressionEvaluationResult EvaluatePath(TransformationExpressionEvaluationContext context, IPathExpression expression)
    {
        try
        {
            IStructureNode node = ResolvePath(context, expression.Path);
            return CreateSuccess(ConvertNodeToResult(node));
        }
        catch (KeyNotFoundException exception)
        {
            return CreateFailure(CreateDiagnostic("BMEX001", exception.Message, expression.Path));
        }
        catch (FormatException exception)
        {
            return CreateFailure(CreateDiagnostic("BMEX001", exception.Message, expression.Path));
        }
        catch (InvalidOperationException exception)
        {
            return CreateFailure(CreateDiagnostic("BMEX001", exception.Message, expression.Path));
        }
        catch (IndexOutOfRangeException exception)
        {
            return CreateFailure(CreateDiagnostic("BMEX001", exception.Message, expression.Path));
        }
    }

    // Evaluates a registered function call with recursively evaluated arguments.
    private ITransformationExpressionEvaluationResult EvaluateFunctionCall(TransformationExpressionEvaluationContext context, IFunctionCallExpression expression)
    {
        List<DiagnosticEntry> diagnostics = [];
        List<IFunctionArgument> arguments = [];

        foreach (ITransformationExpression argumentExpression in expression.Arguments)
        {
            ITransformationExpressionEvaluationResult argumentResult = Evaluate(CreateChildContext(context, argumentExpression));

            if (!argumentResult.Succeeded)
            {
                diagnostics.Add(CreateDiagnostic("BMEX004", $"Function argument for '{expression.FunctionKey}' failed.", string.Empty));
                diagnostics.AddRange(argumentResult.Diagnostics);
                continue;
            }

            arguments.Add(ConvertResultToArgument(argumentResult.Result));
        }

        if (diagnostics.Count > 0)
        {
            return CreateFailure(diagnostics);
        }

        IFunction function;

        try
        {
            function = _functionRegistry.Resolve(expression.FunctionKey);
        }
        catch (KeyNotFoundException exception)
        {
            return CreateFailure(CreateDiagnostic("BMEX003", exception.Message, expression.FunctionKey));
        }

        IFunctionResult result = function.Execute(new FunctionExecutionContext
        {
            ExecutionContext = context.ExecutionContext,
            Arguments = arguments
        });

        if (result is null)
        {
            return CreateFailure(CreateDiagnostic("BMEX005", $"Function '{expression.FunctionKey}' did not return a result.", expression.FunctionKey));
        }

        return CreateSuccess(result);
    }

    // Evaluates a conditional expression by interpreting a boolean scalar condition.
    private ITransformationExpressionEvaluationResult EvaluateConditional(TransformationExpressionEvaluationContext context, IConditionalExpression expression)
    {
        ITransformationExpressionEvaluationResult conditionResult = Evaluate(CreateChildContext(context, expression.Condition));

        if (!conditionResult.Succeeded)
        {
            return conditionResult;
        }

        if (conditionResult.Result is not IScalarFunctionResult scalarResult)
        {
            return CreateFailure(CreateDiagnostic("BMEX005", "Conditional expression requires a scalar condition result.", string.Empty));
        }

        ITransformationExpression branchExpression = expression.ElseExpression;

        if (!scalarResult.Value.IsNull && string.Equals(scalarResult.Value.RawValue, "true", StringComparison.OrdinalIgnoreCase))
        {
            branchExpression = expression.ThenExpression;
        }

        return Evaluate(CreateChildContext(context, branchExpression));
    }

    // Evaluates a source collection and projects each item through the body expression.
    private ITransformationExpressionEvaluationResult EvaluateProjection(TransformationExpressionEvaluationContext context, ICollectionProjectionExpression expression)
    {
        ITransformationExpressionEvaluationResult sourceResult = Evaluate(CreateChildContext(context, expression.SourceExpression));

        if (!sourceResult.Succeeded)
        {
            return sourceResult;
        }

        if (sourceResult.Result is not IStructureNodeCollectionFunctionResult collectionResult)
        {
            return CreateFailure(CreateDiagnostic("BMEX006", "Projection source must evaluate to a structure node collection.", expression.ItemAlias));
        }

        List<DiagnosticEntry> diagnostics = [];
        List<IStructureNode> nodes = [];
        int index = 0;

        foreach (IStructureNode sourceNode in collectionResult.Nodes)
        {
            Dictionary<string, IStructureNode> aliases = new(context.Aliases, StringComparer.Ordinal)
            {
                [expression.ItemAlias] = sourceNode
            };

            ITransformationExpressionEvaluationResult bodyResult = Evaluate(new TransformationExpressionEvaluationContext
            {
                ExecutionContext = context.ExecutionContext,
                Expression = expression.BodyExpression,
                Aliases = aliases
            });

            if (!bodyResult.Succeeded)
            {
                diagnostics.AddRange(bodyResult.Diagnostics);
                continue;
            }

            AppendResultNodes(nodes, bodyResult.Result, ref index);
        }

        if (diagnostics.Count > 0)
        {
            return CreateFailure(diagnostics);
        }

        return CreateSuccess(new StructureNodeCollectionFunctionResult
        {
            Nodes = nodes
        });
    }

    // Evaluates a map-shaped expression into a structure node result.
    private ITransformationExpressionEvaluationResult EvaluateObject(TransformationExpressionEvaluationContext context, IObjectExpression expression)
    {
        List<DiagnosticEntry> diagnostics = [];
        List<IStructureNode> children = [];

        foreach (IObjectPropertyExpression property in expression.Properties)
        {
            ITransformationExpressionEvaluationResult propertyResult = Evaluate(CreateChildContext(context, property.Expression));

            if (!propertyResult.Succeeded)
            {
                diagnostics.AddRange(propertyResult.Diagnostics);
                continue;
            }

            children.Add(ConvertResultToNode(property.Name, propertyResult.Result));
        }

        if (diagnostics.Count > 0)
        {
            return CreateFailure(diagnostics);
        }

        return CreateSuccess(new StructureNodeFunctionResult
        {
            Node = new StructureNode
            {
                Name = "$expression",
                Kind = StructureNodeKind.Object,
                Children = children
            }
        });
    }

    // Evaluates an ordered expression into an array-shaped structure node result.
    private ITransformationExpressionEvaluationResult EvaluateArray(TransformationExpressionEvaluationContext context, IArrayExpression expression)
    {
        List<DiagnosticEntry> diagnostics = [];
        List<IStructureNode> children = [];
        int index = 0;

        foreach (ITransformationExpression itemExpression in expression.Items)
        {
            ITransformationExpressionEvaluationResult itemResult = Evaluate(CreateChildContext(context, itemExpression));

            if (!itemResult.Succeeded)
            {
                diagnostics.AddRange(itemResult.Diagnostics);
                continue;
            }

            AppendResultNodes(children, itemResult.Result, ref index);
        }

        if (diagnostics.Count > 0)
        {
            return CreateFailure(diagnostics);
        }

        return CreateSuccess(new StructureNodeFunctionResult
        {
            Node = new StructureNode
            {
                Name = "$expression",
                Kind = StructureNodeKind.Array,
                Children = children
            }
        });
    }

    // Resolves an absolute source path or an alias-relative path.
    private IStructureNode ResolvePath(TransformationExpressionEvaluationContext context, string path)
    {
        if (path.StartsWith("$", StringComparison.Ordinal))
        {
            return _navigationEngine.Select(context.ExecutionContext, path);
        }

        string alias = path;
        string remainder = string.Empty;
        int separatorIndex = path.IndexOf('.', StringComparison.Ordinal);

        if (separatorIndex >= 0)
        {
            alias = path[..separatorIndex];
            remainder = path[(separatorIndex + 1)..];
        }

        if (!context.Aliases.TryGetValue(alias, out IStructureNode aliasNode))
        {
            throw new KeyNotFoundException($"Alias '{alias}' is not available in the current expression scope.");
        }

        if (string.IsNullOrWhiteSpace(remainder))
        {
            return aliasNode;
        }

        return _pathResolver.Resolve(aliasNode, remainder);
    }

    // Converts a resolved structure node into a function-shaped expression result.
    private static IFunctionResult ConvertNodeToResult(IStructureNode node)
    {
        if (node is IScalarStructureNode scalarNode)
        {
            return new ScalarFunctionResult
            {
                Value = scalarNode.Value
            };
        }

        if (node.Kind == StructureNodeKind.Array)
        {
            return new StructureNodeCollectionFunctionResult
            {
                Nodes = node.Children
            };
        }

        return new StructureNodeFunctionResult
        {
            Node = node
        };
    }

    // Converts a function-shaped result into a function argument.
    private static IFunctionArgument ConvertResultToArgument(IFunctionResult result)
    {
        if (result is IScalarFunctionResult scalarResult)
        {
            return new ScalarFunctionArgument
            {
                Value = scalarResult.Value
            };
        }

        if (result is IScalarCollectionFunctionResult scalarCollectionResult)
        {
            return new ScalarCollectionFunctionArgument
            {
                Values = scalarCollectionResult.Values
            };
        }

        if (result is IStructureNodeFunctionResult nodeResult)
        {
            return new StructureNodeFunctionArgument
            {
                Node = nodeResult.Node
            };
        }

        IStructureNodeCollectionFunctionResult nodeCollectionResult = (IStructureNodeCollectionFunctionResult)result;

        return new StructureNodeCollectionFunctionArgument
        {
            Nodes = nodeCollectionResult.Nodes
        };
    }

    // Converts an expression result into a structure node with a requested name.
    private static IStructureNode ConvertResultToNode(string name, IFunctionResult result)
    {
        if (result is IScalarFunctionResult scalarResult)
        {
            return CreateScalarNode(name, scalarResult.Value);
        }

        if (result is IScalarCollectionFunctionResult scalarCollectionResult)
        {
            return CreateScalarArrayNode(name, scalarCollectionResult.Values);
        }

        if (result is IStructureNodeFunctionResult nodeResult)
        {
            return CloneNode(nodeResult.Node, name);
        }

        IStructureNodeCollectionFunctionResult collectionResult = (IStructureNodeCollectionFunctionResult)result;
        return CreateNodeArray(name, collectionResult.Nodes);
    }

    // Appends one expression result to an ordered node collection.
    private static void AppendResultNodes(List<IStructureNode> nodes, IFunctionResult result, ref int index)
    {
        if (result is IScalarFunctionResult scalarResult)
        {
            nodes.Add(CreateScalarNode(index.ToString(System.Globalization.CultureInfo.InvariantCulture), scalarResult.Value));
            index++;
            return;
        }

        if (result is IScalarCollectionFunctionResult scalarCollectionResult)
        {
            foreach (IScalarValue value in scalarCollectionResult.Values)
            {
                nodes.Add(CreateScalarNode(index.ToString(System.Globalization.CultureInfo.InvariantCulture), value));
                index++;
            }

            return;
        }

        if (result is IStructureNodeFunctionResult nodeResult)
        {
            nodes.Add(CloneNode(nodeResult.Node, index.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            index++;
            return;
        }

        IStructureNodeCollectionFunctionResult collectionResult = (IStructureNodeCollectionFunctionResult)result;

        foreach (IStructureNode node in collectionResult.Nodes)
        {
            nodes.Add(CloneNode(node, index.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            index++;
        }
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

    // Creates a scalar node from a scalar value.
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

    // Creates a child context for recursive evaluation.
    private static TransformationExpressionEvaluationContext CreateChildContext(TransformationExpressionEvaluationContext context, ITransformationExpression expression)
    {
        return new TransformationExpressionEvaluationContext
        {
            ExecutionContext = context.ExecutionContext,
            Expression = expression,
            Aliases = context.Aliases
        };
    }

    // Creates a successful expression evaluation result.
    private static ITransformationExpressionEvaluationResult CreateSuccess(IFunctionResult result)
    {
        return new TransformationExpressionEvaluationResult
        {
            Succeeded = true,
            Result = result,
            Diagnostics = []
        };
    }

    // Creates a failed expression evaluation result from one diagnostic.
    private static ITransformationExpressionEvaluationResult CreateFailure(DiagnosticEntry diagnostic)
    {
        return CreateFailure([diagnostic]);
    }

    // Creates a failed expression evaluation result from diagnostics.
    private static ITransformationExpressionEvaluationResult CreateFailure(IReadOnlyCollection<DiagnosticEntry> diagnostics)
    {
        return new TransformationExpressionEvaluationResult
        {
            Succeeded = false,
            Result = new ScalarFunctionResult
            {
                Value = new ScalarValue()
            },
            Diagnostics = diagnostics
        };
    }

    // Creates an error diagnostic for expression failures.
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
