namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

internal sealed class FunctionDescriptorFactory
{
    internal IFunctionDescriptor Create(string key, IFunction function, FunctionValueKind valueKind, string category, int minimum, int maximum)
    {
        return Create(key, function, valueKind, category, minimum, maximum, []);
    }

    internal IFunctionDescriptor Create(
        string key,
        IFunction function,
        FunctionValueKind valueKind,
        string category,
        int minimum,
        int maximum,
        IReadOnlyList<FunctionValueKind> parameterKinds)
    {
        return new FunctionDescriptor
        {
            Key = key,
            DisplayName = key,
            Description = function.Description,
            ValueKind = valueKind,
            Parameters = CreateParameters(minimum, maximum, parameterKinds),
            Metadata = new Dictionary<string, string>
            {
                ["category"] = category,
                ["minArgs"] = minimum.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["maxArgs"] = maximum.ToString(System.Globalization.CultureInfo.InvariantCulture)
            }
        };
    }

    // Creates scalar-compatible design-time parameters for semantic validation.
    private IReadOnlyCollection<IFunctionParameterDescriptor> CreateParameters(int minimum, int maximum, IReadOnlyList<FunctionValueKind> parameterKinds)
    {
        List<IFunctionParameterDescriptor> parameters = [];
        int count = maximum;

        if (count < 0)
        {
            count = minimum;
        }

        for (int index = 0; index < count; index++)
        {
            parameters.Add(new FunctionParameterDescriptor
            {
                Key = "argument" + index.ToString(System.Globalization.CultureInfo.InvariantCulture),
                DisplayName = "Argument " + index.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Description = "Function argument.",
                ValueKind = ResolveParameterKind(parameterKinds, index),
                IsRequired = index < minimum
            });
        }

        return parameters;
    }

    // Resolves a parameter value kind, repeating the last provided kind for vararg descriptors.
    private static FunctionValueKind ResolveParameterKind(IReadOnlyList<FunctionValueKind> parameterKinds, int index)
    {
        if (parameterKinds.Count == 0)
        {
            return FunctionValueKind.Scalar;
        }

        if (index < parameterKinds.Count)
        {
            return parameterKinds[index];
        }

        return parameterKinds[^1];
    }
}
