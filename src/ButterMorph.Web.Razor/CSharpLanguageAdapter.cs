namespace ButterMorph.Web.Razor;

using System.Text;
using System.Text.Json;
using ButterMorph.SchemaDesign;

/// <summary>
/// Generates C# DTO snippets from ButterMorph payload schemas.
/// </summary>
public sealed class CSharpLanguageAdapter : IButterMorphLanguageAdapter, IButterMorphPrimitiveTypeResolver
{
    private static readonly IReadOnlyCollection<ButterMorphLanguageType> LanguageTypes =
    [
        Type("string"),
        Type("bool"),
        Type("int"),
        Type("long"),
        Type("decimal"),
        Type("double"),
        Type("DateOnly"),
        Type("DateTime"),
        Type("DateTimeOffset"),
        Type("TimeOnly"),
        Type("TimeSpan"),
        Type("object")
    ];

    /// <inheritdoc />
    public string Key => "csharp";

    /// <inheritdoc />
    public string DisplayName => "C#";

    /// <inheritdoc />
    public string FileExtension => ".cs";

    /// <inheritdoc />
    public string LanguageKey => Key;

    /// <inheritdoc />
    public IReadOnlyCollection<ButterMorphLanguageType> GetLanguageTypes()
    {
        return LanguageTypes;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<ButterMorphLanguageType> ResolvePrimitiveTypes()
    {
        return GetLanguageTypes();
    }

    /// <inheritdoc />
    public ButterMorphLanguageMappingProfile CreateDefaultProfile(PayloadSchemaDefinition definition, string rawJson)
    {
        string rootName = SanitizeTypeName(ResolveValue(definition?.Name, ReadRootString(rawJson, "name"), "Schema"));
        ButterMorphLanguageMappingProfile profile = new()
        {
            Id = "csharp-default",
            Key = "csharp-default",
            Name = "C# Defaults",
            Description = "Default C# type mappings.",
            LanguageKey = Key,
            Language = Key,
            Namespace = string.Empty,
            RootTypeName = rootName,
            EmitAttributes = true,
            UseNullableReferences = true,
            UseRequiredMembers = true,
            CollectionType = "List"
        };

        profile.TypeMappings.AddRange(
        [
            Mapping("string", "builtin", "string", "builtin"),
            Mapping("number", "builtin", "decimal", "builtin"),
            Mapping("boolean", "builtin", "bool", "builtin"),
            Mapping("date", "builtin", "DateOnly", "builtin"),
            Mapping("datetime", "builtin", "DateTimeOffset", "builtin"),
            Mapping("time", "builtin", "TimeOnly", "builtin"),
            Mapping("timespan", "builtin", "TimeSpan", "builtin"),
            Mapping("object", "builtin", "object", "builtin")
        ]);

        foreach (KeyValuePair<string, string> customType in ReadCustomTypeMappings(rawJson))
        {
            if (!profile.TypeMappings.Any(mapping => string.Equals(mapping.ButterMorphType, customType.Key, StringComparison.OrdinalIgnoreCase)))
            {
                profile.TypeMappings.Add(Mapping(customType.Key, string.Empty, SanitizeTypeName(customType.Value), string.Empty));
            }
        }

        return profile;
    }

    /// <inheritdoc />
    public ButterMorphCodeGenerationResult GenerateCode(PayloadSchemaDefinition definition, string rawJson, ButterMorphLanguageMappingProfile profile)
    {
        List<string> diagnostics = [];
        SchemaViewerDisplayModel schema = SchemaViewerDisplayBuilder.Build(definition, rawJson);
        ButterMorphLanguageMappingProfile resolvedProfile = NormalizeProfile(profile, definition, rawJson);
        string rootTypeName = SanitizeTypeName(ResolveValue(resolvedProfile.RootTypeName, schema.Name, "Schema"));
        TypeMappingLookup typeMappings = TypeMappingLookup.Create(resolvedProfile.TypeMappings);
        Dictionary<string, string> memberMappings = resolvedProfile.MemberMappings
            .Where(mapping => !string.IsNullOrWhiteSpace(mapping.FieldPath) && !string.IsNullOrWhiteSpace(mapping.MemberName))
            .GroupBy(mapping => mapping.FieldPath, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First().MemberName, StringComparer.OrdinalIgnoreCase);
        HashSet<string> referencedNamespaces = new(StringComparer.Ordinal);

        List<GeneratedClass> classes = [];
        classes.Add(CreateClass(rootTypeName, schema.Root.Children, string.Empty, resolvedProfile, typeMappings, memberMappings, diagnostics, referencedNamespaces));
        bool usesFieldAttribute = classes
            .SelectMany(item => item.Properties)
            .Any(property => property.EmitAttribute);

        StringBuilder builder = new();
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Generic;");
        if (usesFieldAttribute)
        {
            builder.AppendLine("using ButterMorph.Contracts;");
        }

        foreach (string referencedNamespace in referencedNamespaces
            .Where(item => !IsBuiltInUsing(item) && !string.IsNullOrWhiteSpace(item))
            .OrderBy(item => item, StringComparer.Ordinal))
        {
            builder.Append("using ");
            builder.Append(referencedNamespace);
            builder.AppendLine(";");
        }

        foreach (GeneratedClass generatedClass in classes)
        {
            builder.AppendLine();
            WriteClass(builder, generatedClass);
        }

        return new ButterMorphCodeGenerationResult
        {
            Succeeded = diagnostics.Count == 0,
            Code = builder.ToString().TrimEnd(),
            Diagnostics = diagnostics
        };

        GeneratedClass CreateClass(
            string className,
            IReadOnlyCollection<SchemaViewerNodeDisplayModel> fields,
            string parentPath,
            ButterMorphLanguageMappingProfile activeProfile,
            TypeMappingLookup mappings,
            IReadOnlyDictionary<string, string> aliases,
            List<string> generationDiagnostics,
            ISet<string> referencedTypeNamespaces)
        {
            GeneratedClass generatedClass = new()
            {
                Name = className
            };
            HashSet<string> propertyNames = new(StringComparer.Ordinal);

            foreach (SchemaViewerNodeDisplayModel field in fields)
            {
                string fieldPath = string.IsNullOrWhiteSpace(parentPath) ? field.Name : parentPath + "." + field.Name;
                string propertyName = aliases.TryGetValue(fieldPath, out string alias)
                    ? SanitizeMemberName(alias)
                    : SanitizeMemberName(field.Name);

                if (!propertyNames.Add(propertyName))
                {
                    generationDiagnostics.Add($"Property name collision at '{fieldPath}' resolved as '{propertyName}'.");
                }

                string languageType = ResolveLanguageType(field, fieldPath, className, activeProfile, mappings, generationDiagnostics, referencedTypeNamespaces);
                generatedClass.Properties.Add(new GeneratedProperty
                {
                    SchemaName = field.Name,
                    Path = fieldPath,
                    Name = propertyName,
                    TypeName = ApplyNullability(languageType, field, activeProfile),
                    IsRequired = activeProfile.UseRequiredMembers && field.IsRequired,
                    EmitAttribute = activeProfile.EmitAttributes && !string.Equals(propertyName, field.Name, StringComparison.Ordinal),
                    IsCollection = IsArray(field)
                });
            }

            return generatedClass;
        }

        string ResolveLanguageType(
            SchemaViewerNodeDisplayModel field,
            string fieldPath,
            string containingClassName,
            ButterMorphLanguageMappingProfile activeProfile,
            TypeMappingLookup mappings,
            List<string> generationDiagnostics,
            ISet<string> referencedTypeNamespaces)
        {
            if (IsArray(field))
            {
                string itemType = field.Children.Count > 0
                    ? CreateNestedClass(field, fieldPath, containingClassName, activeProfile, mappings, memberMappings, generationDiagnostics, referencedTypeNamespaces)
                    : ResolveMappedType(ResolveArrayItemTypeKey(field), ResolveArrayItemTypeVersion(field), mappings, generationDiagnostics, referencedTypeNamespaces);
                return activeProfile.CollectionType == "Array"
                    ? itemType + "[]"
                    : ResolveCollectionType(activeProfile.CollectionType) + "<" + itemType + ">";
            }

            if (field.Children.Count > 0 || string.Equals(field.Kind, "object", StringComparison.OrdinalIgnoreCase))
            {
                return CreateNestedClass(field, fieldPath, containingClassName, activeProfile, mappings, memberMappings, generationDiagnostics, referencedTypeNamespaces);
            }

            return ResolveMappedType(ResolveTypeKey(field), ResolveTypeVersion(field), mappings, generationDiagnostics, referencedTypeNamespaces);
        }

        string CreateNestedClass(
            SchemaViewerNodeDisplayModel field,
            string fieldPath,
            string containingClassName,
            ButterMorphLanguageMappingProfile activeProfile,
            TypeMappingLookup mappings,
            IReadOnlyDictionary<string, string> aliases,
            List<string> generationDiagnostics,
            ISet<string> referencedTypeNamespaces)
        {
            string nestedClassName = SanitizeTypeName(containingClassName + field.Name);
            if (classes.Any(item => string.Equals(item.Name, nestedClassName, StringComparison.Ordinal)))
            {
                return nestedClassName;
            }

            classes.Add(CreateClass(nestedClassName, field.Children, fieldPath, activeProfile, mappings, aliases, generationDiagnostics, referencedTypeNamespaces));
            return nestedClassName;
        }
    }

    private static ButterMorphLanguageMappingProfile NormalizeProfile(ButterMorphLanguageMappingProfile profile, PayloadSchemaDefinition definition, string rawJson)
    {
        if (profile == null || string.IsNullOrWhiteSpace(profile.LanguageKey))
        {
            return new CSharpLanguageAdapter().CreateDefaultProfile(definition, rawJson);
        }

        return profile;
    }

    private static void WriteClass(StringBuilder builder, GeneratedClass generatedClass)
    {
        builder.Append("public sealed class ");
        builder.AppendLine(generatedClass.Name);
        builder.AppendLine("{");

        foreach (GeneratedProperty property in generatedClass.Properties)
        {
            if (property.EmitAttribute)
            {
                builder.Append("    [ButterMorphField(\"");
                builder.Append(Escape(property.SchemaName));
                builder.AppendLine("\")]");
            }

            builder.Append("    public ");
            if (property.IsRequired && !IsNullable(property.TypeName) && !IsValueType(property.TypeName))
            {
                builder.Append("required ");
            }

            builder.Append(property.TypeName);
            builder.Append(' ');
            builder.Append(property.Name);
            builder.Append(" { get; init; }");
            if (property.IsCollection)
            {
                builder.Append(" = [];");
            }

            builder.AppendLine();
        }

        builder.AppendLine("}");
    }

    private static string ApplyNullability(string typeName, SchemaViewerNodeDisplayModel field, ButterMorphLanguageMappingProfile profile)
    {
        if (field.IsRequired || !profile.UseNullableReferences || typeName.EndsWith("[]", StringComparison.Ordinal) || typeName.StartsWith("List<", StringComparison.Ordinal) || typeName.StartsWith("IReadOnlyList<", StringComparison.Ordinal))
        {
            return typeName;
        }

        return typeName.EndsWith("?", StringComparison.Ordinal) ? typeName : typeName + "?";
    }

    private static string ResolveMappedType(
        string typeKey,
        string typeVersion,
        TypeMappingLookup mappings,
        List<string> diagnostics,
        ISet<string> referencedNamespaces)
    {
        if (mappings.TryResolve(typeKey, typeVersion, out CSharpMappedType mappedType))
        {
            if (!string.IsNullOrWhiteSpace(mappedType.Namespace))
            {
                referencedNamespaces.Add(mappedType.Namespace);
            }

            return mappedType.TypeName;
        }

        diagnostics.Add($"Type '{typeKey}'{FormatVersion(typeVersion)} does not have a C# mapping. Falling back to object.");
        return "object";
    }

    private static bool IsBuiltInUsing(string value)
    {
        return string.Equals(value, "System", StringComparison.Ordinal)
            || string.Equals(value, "System.Collections.Generic", StringComparison.Ordinal)
            || string.Equals(value, "ButterMorph.Contracts", StringComparison.Ordinal);
    }

    private static string ResolveCollectionType(string collectionType)
    {
        return collectionType switch
        {
            "IReadOnlyList" => "IReadOnlyList",
            _ => "List"
        };
    }

    private static string ResolveTypeKey(SchemaViewerNodeDisplayModel field)
    {
        string referenceKey = ReadReferenceKey(field.Reference);
        if (!string.IsNullOrWhiteSpace(referenceKey))
        {
            return referenceKey;
        }

        string type = field.DataType ?? string.Empty;
        if (string.Equals(type, "custom type", StringComparison.OrdinalIgnoreCase))
        {
            return string.IsNullOrWhiteSpace(field.Name) ? "object" : field.Name;
        }

        return NormalizeButterMorphType(type);
    }

    private static string ResolveArrayItemTypeKey(SchemaViewerNodeDisplayModel field)
    {
        string referenceKey = ReadReferenceKey(field.ItemReference);
        if (!string.IsNullOrWhiteSpace(referenceKey))
        {
            return referenceKey;
        }

        string dataType = field.DataType ?? string.Empty;
        const string prefix = "array of ";
        if (dataType.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return NormalizeButterMorphType(dataType[prefix.Length..]);
        }

        return "object";
    }

    private static string ResolveTypeVersion(SchemaViewerNodeDisplayModel field)
    {
        return ReadReferenceVersion(field.Reference);
    }

    private static string ResolveArrayItemTypeVersion(SchemaViewerNodeDisplayModel field)
    {
        return ReadReferenceVersion(field.ItemReference);
    }

    private static string NormalizeButterMorphType(string type)
    {
        string normalized = (type ?? string.Empty).Trim();
        if (string.Equals(normalized, "object", StringComparison.OrdinalIgnoreCase))
        {
            return "object";
        }

        if (string.Equals(normalized, "custom type", StringComparison.OrdinalIgnoreCase))
        {
            return "object";
        }

        return normalized.ToLowerInvariant();
    }

    private static bool IsArray(SchemaViewerNodeDisplayModel field)
    {
        return string.Equals(field.Kind, "array", StringComparison.OrdinalIgnoreCase) ||
            (field.DataType ?? string.Empty).StartsWith("array of ", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNullable(string typeName)
    {
        return typeName.EndsWith("?", StringComparison.Ordinal);
    }

    private static bool IsValueType(string typeName)
    {
        string baseType = typeName.TrimEnd('?');
        return baseType is "bool" or "int" or "long" or "decimal" or "double" or "DateOnly" or "DateTime" or "DateTimeOffset" or "TimeOnly" or "TimeSpan";
    }

    private static ButterMorphLanguageType Type(string key)
    {
        return new ButterMorphLanguageType
        {
            Key = key,
            DisplayName = key,
            QualifiedName = key,
            Version = "builtin",
            Source = "primitive"
        };
    }

    private static ButterMorphLanguageTypeMapping Mapping(string butterMorphType, string butterMorphTypeVersion, string languageType, string languageTypeVersion)
    {
        return new ButterMorphLanguageTypeMapping
        {
            ButterMorphType = butterMorphType,
            ButterMorphTypeVersion = butterMorphTypeVersion,
            LanguageType = languageType,
            LanguageTypeVersion = languageTypeVersion
        };
    }

    private static IReadOnlyDictionary<string, string> ReadCustomTypeMappings(string rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(rawJson);
            HashSet<string> keys = new(StringComparer.OrdinalIgnoreCase);
            if (document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.TryGetProperty("properties", out JsonElement properties))
            {
                ReadReferences(properties, keys);
            }

            IReadOnlyDictionary<string, string> displayNames = ReadDefinitionDisplayNames(document.RootElement);
            return keys
                .OrderBy(key => key, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    key => key,
                    key => displayNames.TryGetValue(key, out string displayName) ? displayName : key,
                    StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static IReadOnlyDictionary<string, string> ReadDefinitionDisplayNames(JsonElement root)
    {
        Dictionary<string, string> displayNames = new(StringComparer.OrdinalIgnoreCase);
        if (root.ValueKind != JsonValueKind.Object ||
            !root.TryGetProperty("$defs", out JsonElement definitions) ||
            definitions.ValueKind != JsonValueKind.Object)
        {
            return displayNames;
        }

        foreach (JsonProperty definition in definitions.EnumerateObject())
        {
            string key = ReadReferenceKey(definition.Name);
            string displayName = ReadJsonString(definition.Value, "name");
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = ReadJsonString(definition.Value, "title");
            }

            displayNames[key] = string.IsNullOrWhiteSpace(displayName) ? key : displayName;
        }

        return displayNames;
    }

    private static void ReadReferences(JsonElement element, HashSet<string> keys)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            if (element.TryGetProperty("$ref", out JsonElement reference) && reference.ValueKind == JsonValueKind.String)
            {
                string key = ReadReferenceKey(reference.GetString() ?? string.Empty);
                if (!string.IsNullOrWhiteSpace(key))
                {
                    keys.Add(key);
                }
            }

            foreach (JsonProperty property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, "metadata", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(property.Name, "$metadataDefs", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                ReadReferences(property.Value, keys);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in element.EnumerateArray())
            {
                ReadReferences(item, keys);
            }
        }
    }

    private static string ReadReferenceKey(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            return string.Empty;
        }

        string key = reference.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;
        int versionIndex = key.IndexOf('@', StringComparison.Ordinal);
        return versionIndex >= 0 ? key[..versionIndex] : key;
    }

    private static string ReadReferenceVersion(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            return string.Empty;
        }

        string key = reference.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;
        int versionIndex = key.IndexOf('@', StringComparison.Ordinal);
        return versionIndex >= 0 && versionIndex < key.Length - 1 ? key[(versionIndex + 1)..] : string.Empty;
    }

    private static string FormatVersion(string version)
    {
        return string.IsNullOrWhiteSpace(version) ? string.Empty : "@" + version;
    }

    private static string ReadRootString(string rawJson, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return string.Empty;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(rawJson);
            return document.RootElement.ValueKind == JsonValueKind.Object &&
                document.RootElement.TryGetProperty(propertyName, out JsonElement property) &&
                property.ValueKind == JsonValueKind.String
                    ? property.GetString() ?? string.Empty
                    : string.Empty;
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    private static string ReadJsonString(JsonElement element, string propertyName)
    {
        return element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.String
                ? property.GetString() ?? string.Empty
                : string.Empty;
    }

    private static string SanitizeTypeName(string value)
    {
        string identifier = SanitizeIdentifier(value);
        return string.IsNullOrWhiteSpace(identifier) ? "GeneratedType" : char.ToUpperInvariant(identifier[0]) + identifier[1..];
    }

    private static string SanitizeMemberName(string value)
    {
        string identifier = SanitizeIdentifier(value);
        return string.IsNullOrWhiteSpace(identifier) ? "Value" : char.ToUpperInvariant(identifier[0]) + identifier[1..];
    }

    private static string SanitizeIdentifier(string value)
    {
        StringBuilder builder = new();
        bool capitalizeNext = true;
        foreach (char character in value ?? string.Empty)
        {
            if (char.IsLetterOrDigit(character) || character == '_')
            {
                builder.Append(capitalizeNext ? char.ToUpperInvariant(character) : character);
                capitalizeNext = false;
                continue;
            }

            capitalizeNext = true;
        }

        string identifier = builder.ToString();
        if (identifier.Length == 0)
        {
            return string.Empty;
        }

        if (char.IsDigit(identifier[0]))
        {
            identifier = "_" + identifier;
        }

        return identifier;
    }

    private static string ResolveValue(params string[] values)
    {
        foreach (string value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    private static string Escape(string value)
    {
        return (value ?? string.Empty).Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
    }

    private sealed class GeneratedClass
    {
        public string Name { get; set; } = string.Empty;

        public List<GeneratedProperty> Properties { get; } = [];
    }

    private sealed class GeneratedProperty
    {
        public string SchemaName { get; set; } = string.Empty;

        public string Path { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string TypeName { get; set; } = string.Empty;

        public bool IsRequired { get; set; }

        public bool EmitAttribute { get; set; }

        public bool IsCollection { get; set; }
    }

    private sealed class CSharpMappedType
    {
        public string TypeName { get; set; } = string.Empty;

        public string Namespace { get; set; } = string.Empty;
    }

    private sealed class TypeMappingLookup
    {
        private readonly Dictionary<string, CSharpMappedType> exactMappings;
        private readonly Dictionary<string, CSharpMappedType> typeMappings;

        private TypeMappingLookup(Dictionary<string, CSharpMappedType> exactMappings, Dictionary<string, CSharpMappedType> typeMappings)
        {
            this.exactMappings = exactMappings;
            this.typeMappings = typeMappings;
        }

        public static TypeMappingLookup Create(IEnumerable<ButterMorphLanguageTypeMapping> mappings)
        {
            Dictionary<string, CSharpMappedType> exactMappings = new(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, CSharpMappedType> typeMappings = new(StringComparer.OrdinalIgnoreCase);
            foreach (ButterMorphLanguageTypeMapping mapping in mappings ?? [])
            {
                if (string.IsNullOrWhiteSpace(mapping.ButterMorphType) || string.IsNullOrWhiteSpace(mapping.LanguageType))
                {
                    continue;
                }

                string version = mapping.ButterMorphTypeVersion ?? string.Empty;
                CSharpMappedType mappedType = CreateMappedType(mapping.LanguageType);
                exactMappings[CreateKey(mapping.ButterMorphType, version)] = mappedType;
                if (!typeMappings.ContainsKey(mapping.ButterMorphType))
                {
                    typeMappings[mapping.ButterMorphType] = mappedType;
                }
            }

            return new TypeMappingLookup(exactMappings, typeMappings);
        }

        public bool TryResolve(string typeKey, string typeVersion, out CSharpMappedType languageType)
        {
            languageType = new CSharpMappedType();
            if (string.IsNullOrWhiteSpace(typeKey))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(typeVersion) &&
                exactMappings.TryGetValue(CreateKey(typeKey, typeVersion), out languageType))
            {
                return true;
            }

            if (exactMappings.TryGetValue(CreateKey(typeKey, string.Empty), out languageType) ||
                exactMappings.TryGetValue(CreateKey(typeKey, "builtin"), out languageType))
            {
                return true;
            }

            return typeMappings.TryGetValue(typeKey, out languageType);
        }

        private static string CreateKey(string typeKey, string version)
        {
            return (typeKey ?? string.Empty) + "@" + (version ?? string.Empty);
        }

        private static CSharpMappedType CreateMappedType(string value)
        {
            string typeName = (value ?? string.Empty).Trim();
            string suffix = string.Empty;
            while (typeName.EndsWith("[]", StringComparison.Ordinal))
            {
                suffix += "[]";
                typeName = typeName[..^2];
            }

            if (typeName.EndsWith("?", StringComparison.Ordinal))
            {
                suffix = "?" + suffix;
                typeName = typeName[..^1];
            }

            int lastDotIndex = typeName.LastIndexOf('.');
            if (lastDotIndex <= 0 || lastDotIndex >= typeName.Length - 1 || typeName.Contains('<', StringComparison.Ordinal))
            {
                return new CSharpMappedType
                {
                    TypeName = typeName + suffix
                };
            }

            return new CSharpMappedType
            {
                Namespace = typeName[..lastDotIndex],
                TypeName = typeName[(lastDotIndex + 1)..] + suffix
            };
        }
    }
}
