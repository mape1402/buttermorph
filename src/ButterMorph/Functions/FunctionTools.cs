namespace ButterMorph.Functions;

using System.Globalization;
using ButterMorph.Abstractions;
using ButterMorph.Core;

internal sealed class FunctionTools
{
    // Converts structure values to text when scalar-only conversion is not enough.
    private readonly JsonFunctionConverter _json = new();

    internal void Require(FunctionExecutionContext context, string key, int minimum, int maximum)
    {
        int count = context.Arguments.Count;

        if (count < minimum)
        {
            throw new InvalidOperationException($"Function '{key}' expects at least {minimum} arguments, got {count}.");
        }

        if (maximum >= 0 && count > maximum)
        {
            throw new InvalidOperationException($"Function '{key}' expects at most {maximum} arguments, got {count}.");
        }
    }

    internal IFunctionArgument Argument(FunctionExecutionContext context, string key, int index)
    {
        if (index < 0 || index >= context.Arguments.Count)
        {
            throw new InvalidOperationException($"Function '{key}' missing argument {index}.");
        }

        return context.Arguments[index];
    }

    internal string Text(IFunctionArgument argument)
    {
        if (argument is IScalarFunctionArgument scalarArgument)
        {
            return Text(scalarArgument.Value);
        }

        if (argument is IScalarCollectionFunctionArgument scalarCollectionArgument)
        {
            return string.Join(",", scalarCollectionArgument.Values.Select(Text));
        }

        if (argument is IStructureNodeFunctionArgument nodeArgument)
        {
            return _json.WriteNode(nodeArgument.Node);
        }

        if (argument is IStructureNodeCollectionFunctionArgument nodeCollectionArgument)
        {
            return string.Join(",", nodeCollectionArgument.Nodes.Select(_json.WriteNode));
        }

        throw new InvalidOperationException("Unsupported function argument kind.");
    }

    internal string Text(IScalarValue value)
    {
        if (value.IsNull)
        {
            return string.Empty;
        }

        return value.RawValue;
    }

    internal double Number(IFunctionArgument argument)
    {
        string text = Text(argument);

        if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
        {
            return value;
        }

        throw new InvalidOperationException($"Value '{text}' is not numeric.");
    }

    internal bool Truthy(IFunctionArgument argument)
    {
        if (argument is IScalarFunctionArgument scalarArgument)
        {
            return Truthy(scalarArgument.Value);
        }

        if (argument is IScalarCollectionFunctionArgument scalarCollectionArgument)
        {
            return scalarCollectionArgument.Values.Count > 0;
        }

        if (argument is IStructureNodeFunctionArgument nodeArgument)
        {
            return nodeArgument.Node.Children.Count > 0 || nodeArgument.Node is IScalarStructureNode;
        }

        if (argument is IStructureNodeCollectionFunctionArgument nodeCollectionArgument)
        {
            return nodeCollectionArgument.Nodes.Count > 0;
        }

        return false;
    }

    internal bool Truthy(IScalarValue value)
    {
        if (value.IsNull)
        {
            return false;
        }

        string text = value.RawValue;

        if (bool.TryParse(text, out bool boolean))
        {
            return boolean;
        }

        if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double number))
        {
            return Math.Abs(number) > double.Epsilon;
        }

        return !string.IsNullOrWhiteSpace(text);
    }

    internal bool IsNull(IFunctionArgument argument)
    {
        if (argument is IScalarFunctionArgument scalarArgument)
        {
            return scalarArgument.Value.IsNull;
        }

        return false;
    }

    internal bool IsEmpty(IFunctionArgument argument)
    {
        if (IsNull(argument))
        {
            return true;
        }

        if (argument is IScalarFunctionArgument scalarArgument)
        {
            return string.IsNullOrWhiteSpace(Text(scalarArgument.Value));
        }

        if (argument is IScalarCollectionFunctionArgument scalarCollectionArgument)
        {
            return scalarCollectionArgument.Values.Count == 0;
        }

        if (argument is IStructureNodeFunctionArgument nodeArgument)
        {
            return nodeArgument.Node.Children.Count == 0;
        }

        if (argument is IStructureNodeCollectionFunctionArgument nodeCollectionArgument)
        {
            return nodeCollectionArgument.Nodes.Count == 0;
        }

        return true;
    }

    internal bool Same(IFunctionArgument left, IFunctionArgument right)
    {
        if (left.Kind != right.Kind)
        {
            return false;
        }

        if (left is IScalarFunctionArgument leftScalar && right is IScalarFunctionArgument rightScalar)
        {
            if (leftScalar.Value.IsNull && rightScalar.Value.IsNull)
            {
                return true;
            }

            if (double.TryParse(leftScalar.Value.RawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double leftNumber) && double.TryParse(rightScalar.Value.RawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double rightNumber))
            {
                return Math.Abs(leftNumber - rightNumber) < 0.000000000001d;
            }

            return string.Equals(leftScalar.Value.RawValue, rightScalar.Value.RawValue, StringComparison.Ordinal);
        }

        return string.Equals(Text(left), Text(right), StringComparison.Ordinal);
    }

    internal IReadOnlyCollection<IScalarValue> ScalarValues(IFunctionArgument argument)
    {
        if (argument is IScalarCollectionFunctionArgument scalarCollectionArgument)
        {
            return scalarCollectionArgument.Values.Select(CloneScalar).ToList();
        }

        if (argument is IScalarFunctionArgument scalarArgument)
        {
            return [CloneScalar(scalarArgument.Value)];
        }

        throw new InvalidOperationException("Function argument is not a scalar collection.");
    }

    internal IReadOnlyCollection<IStructureNode> Nodes(IFunctionArgument argument)
    {
        if (argument is IStructureNodeCollectionFunctionArgument nodeCollectionArgument)
        {
            return nodeCollectionArgument.Nodes.Select(CloneNode).ToList();
        }

        if (argument is IStructureNodeFunctionArgument nodeArgument)
        {
            return [CloneNode(nodeArgument.Node)];
        }

        throw new InvalidOperationException("Function argument is not a structure node collection.");
    }

    internal IFunctionResult CloneArgument(IFunctionArgument argument)
    {
        if (argument is IScalarFunctionArgument scalarArgument)
        {
            return ScalarResult(CloneScalar(scalarArgument.Value));
        }

        if (argument is IScalarCollectionFunctionArgument scalarCollectionArgument)
        {
            return new ScalarCollectionFunctionResult
            {
                Values = scalarCollectionArgument.Values.Select(CloneScalar).ToList()
            };
        }

        if (argument is IStructureNodeFunctionArgument nodeArgument)
        {
            return new StructureNodeFunctionResult
            {
                Node = CloneNode(nodeArgument.Node)
            };
        }

        if (argument is IStructureNodeCollectionFunctionArgument nodeCollectionArgument)
        {
            return new StructureNodeCollectionFunctionResult
            {
                Nodes = nodeCollectionArgument.Nodes.Select(CloneNode).ToList()
            };
        }

        throw new InvalidOperationException("Unsupported function argument kind.");
    }

    internal IScalarValue CloneScalar(IScalarValue value)
    {
        return new ScalarValue
        {
            DataType = value.DataType,
            RawValue = value.RawValue,
            IsNull = value.IsNull
        };
    }

    internal IStructureNode CloneNode(IStructureNode node)
    {
        if (node is IScalarStructureNode scalarNode)
        {
            return new ScalarStructureNode
            {
                Name = node.Name,
                Value = CloneScalar(scalarNode.Value)
            };
        }

        return new StructureNode
        {
            Name = node.Name,
            Kind = node.Kind,
            Children = node.Children.Select(CloneNode).ToList()
        };
    }

    internal IFunctionResult ScalarResult(IScalarValue value)
    {
        return new ScalarFunctionResult
        {
            Value = value
        };
    }

    internal IFunctionResult StringResult(string value)
    {
        return ScalarResult(StringValue(value));
    }

    internal IFunctionResult NumberResult(double value)
    {
        return ScalarResult(new ScalarValue
        {
            DataType = "Number",
            RawValue = value.ToString("G17", CultureInfo.InvariantCulture),
            IsNull = false
        });
    }

    internal IFunctionResult BooleanResult(bool value)
    {
        string text = "false";

        if (value)
        {
            text = "true";
        }

        return ScalarResult(new ScalarValue
        {
            DataType = "Boolean",
            RawValue = text,
            IsNull = false
        });
    }

    internal IFunctionResult NullResult()
    {
        return ScalarResult(new ScalarValue
        {
            DataType = "Null",
            RawValue = string.Empty,
            IsNull = true
        });
    }

    internal IScalarValue StringValue(string value)
    {
        return new ScalarValue
        {
            DataType = "String",
            RawValue = value,
            IsNull = false
        };
    }

    internal IFunctionResult ScalarCollectionResult(IReadOnlyCollection<IScalarValue> values)
    {
        return new ScalarCollectionFunctionResult
        {
            Values = values.Select(CloneScalar).ToList()
        };
    }

    internal IFunctionResult NodeCollectionResult(IReadOnlyCollection<IStructureNode> nodes)
    {
        return new StructureNodeCollectionFunctionResult
        {
            Nodes = nodes.Select(CloneNode).ToList()
        };
    }
}
