namespace ButterMorph.Functions;

using ButterMorph.Abstractions;

internal sealed class FunctionDescriptorFactory
{
    internal IFunctionDescriptor Create(string key, FunctionValueKind valueKind, string category, int minimum, int maximum)
    {
        return new FunctionDescriptor
        {
            Key = key,
            DisplayName = key,
            Description = "Native ButterMorph function " + key + ".",
            ValueKind = valueKind,
            Parameters = CreateParameters(minimum, maximum),
            Metadata = new Dictionary<string, string>
            {
                ["category"] = category,
                ["minArgs"] = minimum.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["maxArgs"] = maximum.ToString(System.Globalization.CultureInfo.InvariantCulture)
            }
        };
    }

    // Creates scalar-compatible design-time parameters for semantic validation.
    private IReadOnlyCollection<IFunctionParameterDescriptor> CreateParameters(int minimum, int maximum)
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
                ValueKind = FunctionValueKind.Scalar,
                IsRequired = index < minimum
            });
        }

        return parameters;
    }
}
