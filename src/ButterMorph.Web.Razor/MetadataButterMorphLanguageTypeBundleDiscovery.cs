namespace ButterMorph.Web.Razor;

using System.Reflection;
using ButterMorph.Contracts;

/// <summary>
/// Discovers ButterMorph language type bundles through metadata-only inspection.
/// </summary>
public sealed class MetadataButterMorphLanguageTypeBundleDiscovery : IButterMorphLanguageTypeBundleDiscovery
{
    private const string DefaultVersion = "1.0.0";
    private static readonly string BundleAttributeName = typeof(ButterMorphLanguageTypeBundleAttribute).FullName;
    private static readonly string LanguageTypeAttributeName = typeof(ButterMorphLanguageTypeAttribute).FullName;

    /// <inheritdoc />
    public ButterMorphLanguageTypeBundleDiscoveryResult Discover(Stream assemblyStream, string fileName)
    {
        if (assemblyStream == null || assemblyStream == Stream.Null)
        {
            return Failed("Assembly file is required.");
        }

        if (!string.Equals(Path.GetExtension(fileName), ".dll", StringComparison.OrdinalIgnoreCase))
        {
            return Failed("Only .dll assemblies can be scanned.");
        }

        string directory = Path.Combine(Path.GetTempPath(), "buttermorph-" + Guid.NewGuid().ToString("N"));
        string path = Path.Combine(directory, ResolveFileName(fileName));
        try
        {
            Directory.CreateDirectory(directory);
            using (FileStream file = File.Create(path))
            {
                assemblyStream.CopyTo(file);
            }

            return Discover(path);
        }
        catch (Exception exception) when (exception is BadImageFormatException or FileLoadException or FileNotFoundException or ReflectionTypeLoadException or IOException)
        {
            return Failed(exception.Message);
        }
        finally
        {
            TryDeleteDirectory(directory);
        }
    }

    private static ButterMorphLanguageTypeBundleDiscoveryResult Discover(string assemblyPath)
    {
        PathAssemblyResolver resolver = new(CreateResolverPaths(assemblyPath));
        using MetadataLoadContext metadataContext = new(resolver, typeof(object).Assembly.GetName().Name);
        Assembly assembly = metadataContext.LoadFromAssemblyPath(assemblyPath);
        List<string> diagnostics = [];

        CustomAttributeData bundleAttribute = assembly.CustomAttributes.FirstOrDefault(IsBundleAttribute);

        if (bundleAttribute == null)
        {
            return Failed("Assembly must declare ButterMorphLanguageTypeBundleAttribute. Runtime bundle providers are no longer supported.");
        }

        string bundleKey = ReadConstructorString(bundleAttribute, 0);
        string language = ReadConstructorString(bundleAttribute, 1);
        string version = ReadNamedString(bundleAttribute, nameof(ButterMorphLanguageTypeBundleAttribute.Version), DefaultVersion);
        string description = ReadNamedString(bundleAttribute, nameof(ButterMorphLanguageTypeBundleAttribute.Description), string.Empty);

        if (string.IsNullOrWhiteSpace(bundleKey))
        {
            diagnostics.Add("Bundle key is required.");
        }

        if (string.IsNullOrWhiteSpace(language))
        {
            diagnostics.Add("Bundle language is required.");
        }

        List<ButterMorphLanguageType> discoveredTypes = [];
        foreach (Type type in GetLoadableTypes(assembly, diagnostics))
        {
            CustomAttributeData attribute = type.CustomAttributes.FirstOrDefault(IsLanguageTypeAttribute);
            if (attribute == null)
            {
                continue;
            }

            string typeKey = ReadConstructorString(attribute, 0);
            if (string.IsNullOrWhiteSpace(typeKey))
            {
                diagnostics.Add("Type '" + type.FullName + "' has an empty ButterMorph language type key.");
                continue;
            }

            discoveredTypes.Add(new ButterMorphLanguageType
            {
                Key = typeKey,
                DisplayName = ReadNamedString(attribute, nameof(ButterMorphLanguageTypeAttribute.Name), type.Name),
                QualifiedName = type.FullName ?? type.Name,
                Version = ReadNamedString(attribute, nameof(ButterMorphLanguageTypeAttribute.Version), DefaultVersion),
                Description = ReadNamedString(attribute, nameof(ButterMorphLanguageTypeAttribute.Description), string.Empty),
                Source = "bundle",
                BundleKey = bundleKey
            });
        }

        if (discoveredTypes.Count == 0)
        {
            diagnostics.Add("Assembly does not contain types marked with ButterMorphLanguageTypeAttribute.");
        }

        return new ButterMorphLanguageTypeBundleDiscoveryResult
        {
            Succeeded = diagnostics.Count == 0,
            Diagnostics = diagnostics,
            Bundle = new ButterMorphLanguageTypeBundle
            {
                AssemblyId = CreateAssemblyId(assembly),
                AssemblyKey = bundleKey,
                AssemblyDescription = description,
                AssemblyVersion = version,
                LanguageKey = language,
                Types = discoveredTypes
                    .OrderBy(type => type.DisplayName, StringComparer.OrdinalIgnoreCase)
                    .ToList()
            }
        };
    }

    private static IReadOnlyCollection<string> CreateResolverPaths(string assemblyPath)
    {
        HashSet<string> paths = new(StringComparer.OrdinalIgnoreCase);
        string trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? string.Empty;
        foreach (string path in trustedPlatformAssemblies.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            paths.Add(path);
        }

        paths.Add(typeof(ButterMorphLanguageTypeAttribute).Assembly.Location);
        paths.Add(assemblyPath);

        string directory = Path.GetDirectoryName(assemblyPath) ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
        {
            foreach (string path in Directory.EnumerateFiles(directory, "*.dll"))
            {
                paths.Add(path);
            }
        }

        return paths;
    }

    private static Type[] GetLoadableTypes(Assembly assembly, List<string> diagnostics)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            diagnostics.AddRange(exception.LoaderExceptions
                .Where(item => item != null)
                .Select(item => item.Message));
            return exception.Types.Where(type => type != null).ToArray();
        }
    }

    private static string CreateAssemblyId(Assembly assembly)
    {
        Module module = assembly.ManifestModule;
        return (assembly.GetName().Name ?? "assembly") + ":" + module.ModuleVersionId.ToString("N");
    }

    private static bool IsBundleAttribute(CustomAttributeData attribute)
    {
        return string.Equals(attribute.AttributeType.FullName, BundleAttributeName, StringComparison.Ordinal);
    }

    private static bool IsLanguageTypeAttribute(CustomAttributeData attribute)
    {
        return string.Equals(attribute.AttributeType.FullName, LanguageTypeAttributeName, StringComparison.Ordinal);
    }

    private static string ReadConstructorString(CustomAttributeData attribute, int index)
    {
        if (attribute.ConstructorArguments.Count <= index)
        {
            return string.Empty;
        }

        return attribute.ConstructorArguments[index].Value as string ?? string.Empty;
    }

    private static string ReadNamedString(CustomAttributeData attribute, string name, string fallback)
    {
        CustomAttributeNamedArgument argument = attribute.NamedArguments.FirstOrDefault(item =>
            string.Equals(item.MemberName, name, StringComparison.Ordinal));
        string value = argument.TypedValue.Value as string;
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static ButterMorphLanguageTypeBundleDiscoveryResult Failed(string diagnostic)
    {
        return new ButterMorphLanguageTypeBundleDiscoveryResult
        {
            Succeeded = false,
            Diagnostics = [diagnostic]
        };
    }

    private static string ResolveFileName(string fileName)
    {
        string resolved = Path.GetFileName(fileName);
        return string.IsNullOrWhiteSpace(resolved) ? "bundle.dll" : resolved;
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
