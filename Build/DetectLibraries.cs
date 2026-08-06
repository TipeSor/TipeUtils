using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

public sealed class DetectLibraries : Microsoft.Build.Utilities.Task
{
    private static readonly ModuleConfig[] ModuleConfigs =
        new ModuleCollection()
            .AddModule(b => b
                .IncludeSource("Console"))
            .AddModule("Rl", b => b
                .IncludeSource("Rl")
                .RequirePackage("Raylib_cs"))
            .AddModule("GUI", b => b
                .IncludeSource("GUI")
                .RequireModule("Rl")
                .RequireFeature("GUI"))
            .Build();

    public ITaskItem[]? PackageReferences { get; set; }
    public ITaskItem[]? References { get; set; }
    public ITaskItem[]? ReferencePaths { get; set; }
    public ITaskItem[]? ExistingCompileItems { get; set; }
    public string? Packages { get; set; }
    public string? Features { get; set; }
    public string? Modules { get; set; }
    public string? SourceRoot { get; set; }

    [Output]
    public ITaskItem[] SourceFiles { get; set; } = [];

    [Output]
    public ITaskItem[] DefineConstants { get; set; } = [];

    public override bool Execute()
    {
        string sourceRoot = Path.GetFullPath(SourceRoot ?? Path.Combine(BuildEngine.ProjectFileOfTaskNode, "..", ".."));
        var packageNames = GetActivePackageNames();
        var featureFlags = GetEnabledFeatureFlags();
        ModuleResolution modules = ResolveModules(packageNames, featureFlags);
        var alreadyCompiledFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string? path in ExistingCompileItems.EmptyIfNull().Select(GetFullPath))
        {
            if (path is not null)
                alreadyCompiledFiles.Add(path);
        }

        var sourceFiles = new List<ITaskItem>();
        var addedSourceFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (ModuleConfig module in modules.ActiveModules)
            AddSourceFiles(sourceRoot, module.SourceDirectories, alreadyCompiledFiles, addedSourceFiles, sourceFiles);

        foreach (StubConfig stub in modules.ActiveStubs)
            AddSourceFiles(sourceRoot, stub.SourceDirectories, alreadyCompiledFiles, addedSourceFiles, sourceFiles);

        SourceFiles = [.. sourceFiles];
        DefineConstants = GetDefineConstants(featureFlags, modules.ModuleNames);
        return true;
    }

    private static void AddSourceFiles(
        string sourceRoot,
        IEnumerable<string> sourceDirectories,
        HashSet<string> alreadyCompiledFiles,
        HashSet<string> addedSourceFiles,
        List<ITaskItem> sourceFiles)
    {
        foreach (string relativeFile in Files(sourceRoot, sourceDirectories))
        {
            string fullPath = Path.GetFullPath(Path.Combine(sourceRoot, relativeFile));

            if (alreadyCompiledFiles.Contains(fullPath) || !addedSourceFiles.Add(fullPath))
                continue;

            var item = new TaskItem(fullPath);
            item.SetMetadata("Link", Path.Combine("TipeUtils", relativeFile));
            item.SetMetadata("Visible", "false");
            sourceFiles.Add(item);
        }
    }

    private static ITaskItem[] GetDefineConstants(HashSet<string> featureFlags, HashSet<string> moduleNames)
    {
        var constants = new List<ITaskItem>();
        var addedConstants = new HashSet<string>(StringComparer.Ordinal);

        foreach (string feature in featureFlags)
            AddDefineConstant(constants, addedConstants, "TipeUtilsFeature", feature);

        foreach (string module in moduleNames)
            AddDefineConstant(constants, addedConstants, "TipeUtilsModule", module);

        return [.. constants];
    }

    private static void AddDefineConstant(List<ITaskItem> constants, HashSet<string> addedConstants, string prefix, string name)
    {
        string symbol = GetDefineConstant(prefix, name);

        if (symbol.Length > 0 && addedConstants.Add(symbol))
            constants.Add(new TaskItem(symbol));
    }

    private static string GetDefineConstant(string prefix, string name)
    {
        string symbol = prefix + "_" + name.Replace('-', '_').Replace('.', '_');
        var chars = symbol.ToCharArray();

        for (int i = 0; i < chars.Length; i++)
        {
            if (!IsDefineConstantChar(chars[i]))
                chars[i] = '_';
        }

        return new string(chars);
    }

    private static bool IsDefineConstantChar(char c)
        => c == '_' || char.IsLetterOrDigit(c);

    private HashSet<string> GetActivePackageNames()
    {
        var packages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AddPackageList(packages, Packages);
        AddItems(packages, PackageReferences, useIdentityMetadata: true);
        AddItems(packages, References, useIdentityMetadata: false);
        AddItems(packages, ReferencePaths, useIdentityMetadata: false);

        return packages;
    }

    private HashSet<string> GetEnabledFeatureFlags()
    {
        var features = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AddList(features, Features);

        return features;
    }

    private ModuleResolution ResolveModules(HashSet<string> packageNames, HashSet<string> featureFlags)
    {
        var modules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var activeModules = new List<ModuleConfig>();
        var activeStubs = new Dictionary<string, StubConfig>(StringComparer.OrdinalIgnoreCase);

        AddList(modules, Modules);

        bool changed;
        do
        {
            changed = false;

            foreach (ModuleConfig module in ModuleConfigs)
            {
                if (activeModules.Contains(module))
                    continue;

                if (!CanActivate(module, packageNames, featureFlags, modules))
                    continue;

                activeModules.Add(module);

                string? moduleName = module.Name;
                if (!string.IsNullOrWhiteSpace(moduleName))
                {
                    activeStubs.Remove(moduleName!);
                    changed |= AddName(modules, moduleName);
                }
            }

            foreach (ModuleConfig module in ModuleConfigs)
            {
                string? moduleName = module.Name;
                if (string.IsNullOrWhiteSpace(moduleName) || module.Stub is null)
                    continue;

                if (modules.Contains(moduleName!))
                    continue;

                if (!IsStubNeeded(moduleName!, packageNames, featureFlags, modules))
                    continue;

                if (!CanActivate(module.Stub, packageNames, featureFlags, modules))
                    continue;

                activeStubs[moduleName!] = module.Stub;
                changed |= AddName(modules, moduleName);
            }
        }
        while (changed);

        return new(modules, activeModules, activeStubs.Values);
    }

    private static bool CanActivate(ModuleConfig module, HashSet<string> packageNames, HashSet<string> featureFlags, HashSet<string> moduleNames)
        => CanActivate(module.RequiredPackages, module.RequiredFeatures, module.RequiredModules, packageNames, featureFlags, moduleNames);

    private static bool CanActivate(StubConfig stub, HashSet<string> packageNames, HashSet<string> featureFlags, HashSet<string> moduleNames)
        => CanActivate(stub.RequiredPackages, stub.RequiredFeatures, stub.RequiredModules, packageNames, featureFlags, moduleNames);

    private static bool CanActivate(
        IEnumerable<string> requiredPackages,
        IEnumerable<string> requiredFeatures,
        IEnumerable<string> requiredModules,
        HashSet<string> packageNames,
        HashSet<string> featureFlags,
        HashSet<string> moduleNames)
    {
        return requiredPackages.All(packageNames.Contains) &&
            requiredFeatures.All(featureFlags.Contains) &&
            requiredModules.All(moduleNames.Contains);
    }

    private static bool IsStubNeeded(
        string moduleName,
        HashSet<string> packageNames,
        HashSet<string> featureFlags,
        HashSet<string> moduleNames)
    {
        foreach (ModuleConfig module in ModuleConfigs)
        {
            if (!module.RequiredModules.Contains(moduleName, StringComparer.OrdinalIgnoreCase))
                continue;

            if (!module.RequiredPackages.All(packageNames.Contains))
                continue;

            if (!module.RequiredFeatures.All(featureFlags.Contains))
                continue;

            if (module.RequiredModules
                .Where(requiredModule => !string.Equals(requiredModule, moduleName, StringComparison.OrdinalIgnoreCase))
                .All(moduleNames.Contains))
            {
                return true;
            }
        }

        return false;
    }

    private static void AddList(HashSet<string> items, string? list)
    {
        if (string.IsNullOrWhiteSpace(list))
            return;

        foreach (string item in list!.Split([';', ','], StringSplitOptions.RemoveEmptyEntries))
        {
            string trimmedItem = item.Trim();

            AddName(items, trimmedItem);
        }
    }

    private static void AddPackageList(HashSet<string> packages, string? packageList)
        => AddList(packages, packageList);

    private static void AddItems(HashSet<string> packages, ITaskItem[]? items, bool useIdentityMetadata)
    {
        foreach (ITaskItem item in items.EmptyIfNull())
        {
            string package = useIdentityMetadata ? item.GetMetadata("Identity") : Path.GetFileNameWithoutExtension(item.ItemSpec);

            AddName(packages, package);
        }
    }

    private static bool AddName(HashSet<string> names, string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        bool added = names.Add(name!);
        added |= names.Add(name!.Replace('-', '_'));
        return added;
    }

    private static string? GetFullPath(ITaskItem item)
    {
        string path = item.GetMetadata("FullPath");
        return string.IsNullOrWhiteSpace(path) ? null : Path.GetFullPath(path);
    }

    private static IEnumerable<string> Files(string sourceRoot, IEnumerable<string> directories)
    {
        foreach (string directory in directories)
        {
            string fullDirectory = Path.Combine(sourceRoot, directory);

            if (!Directory.Exists(fullDirectory))
                continue;

            foreach (string file in Directory.EnumerateFiles(fullDirectory, "*.cs", SearchOption.AllDirectories))
            {
                string relativeFile = GetRelativePath(sourceRoot, file);

                if (!IsBuildOutput(relativeFile))
                    yield return relativeFile;
            }
        }
    }

    private static bool IsBuildOutput(string relativePath)
    {
        string[] parts = relativePath.Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]);
        return parts.Any(part =>
            string.Equals(part, "bin", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(part, "obj", StringComparison.OrdinalIgnoreCase));
    }

    private static string GetRelativePath(string root, string path)
    {
        string normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        string normalizedPath = Path.GetFullPath(path);

        if (normalizedPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            return normalizedPath.Substring(normalizedRoot.Length);

        return normalizedPath;
    }
}

internal sealed class ModuleConfig(
    string? name,
    string[] sourceDirectories,
    string[] requiredPackages,
    string[] requiredFeatures,
    string[] requiredModules,
    StubConfig? stub)
{
    public ModuleConfig(string[] sourceDirectories)
        : this(null, sourceDirectories, [], [], [], null) { }

    public ModuleConfig(string[] sourceDirectories, string[] requiredPackages)
        : this(null, sourceDirectories, requiredPackages, [], [], null) { }

    public ModuleConfig(
        string? name,
        IEnumerable<string> sourceDirectories,
        IEnumerable<string> requiredPackages,
        IEnumerable<string> requiredFeatures,
        IEnumerable<string> requiredModules,
        StubConfig? stub)
        : this(
            name,
            [.. sourceDirectories],
            [.. requiredPackages],
            [.. requiredFeatures],
            [.. requiredModules],
            stub) { }

    public string? Name { get; } = name;
    public string[] SourceDirectories { get; } = sourceDirectories;
    public string[] RequiredPackages { get; } = requiredPackages;
    public string[] RequiredFeatures { get; } = requiredFeatures;
    public string[] RequiredModules { get; } = requiredModules;
    public StubConfig? Stub { get; } = stub;
}

internal sealed class ModuleBuilder
{
    readonly List<string> sourceDirectories = [];
    readonly List<string> requiredPackages = [];
    readonly List<string> requiredFeatures = [];
    readonly List<string> requiredModules = [];
    readonly string? name;
    StubConfig? stub;

    public ModuleBuilder(string? name = null)
    {
        this.name = name;
    }

    public ModuleBuilder IncludeSource(string directory)
    {
        sourceDirectories.Add(directory);
        return this;
    }

    public ModuleBuilder RequirePackage(string package)
    {
        requiredPackages.Add(package);
        return this;
    }

    public ModuleBuilder RequireFeature(string feature)
    {
        requiredFeatures.Add(feature);
        return this;
    }

    public ModuleBuilder RequireModule(string module)
    {
        requiredModules.Add(module);
        return this;
    }

    public ModuleBuilder IncludeSources(params string[] directories)
    {
        sourceDirectories.AddRange(directories);
        return this;
    }

    public ModuleBuilder RequirePackages(params string[] packages)
    {
        requiredPackages.AddRange(packages);
        return this;
    }

    public ModuleBuilder RequireFeatures(params string[] features)
    {
        requiredFeatures.AddRange(features);
        return this;
    }

    public ModuleBuilder RequireModules(params string[] modules)
    {
        requiredModules.AddRange(modules);
        return this;
    }

    public ModuleBuilder ProvideStub(Func<StubBuilder, StubBuilder> setup)
    {
        stub = setup(new()).Build();
        return this;
    }

    public ModuleConfig Build()
        => new(name, sourceDirectories, requiredPackages, requiredFeatures, requiredModules, stub);
}

internal sealed class StubConfig(
    string[] sourceDirectories,
    string[] requiredPackages,
    string[] requiredFeatures,
    string[] requiredModules)
{
    public StubConfig(
        IEnumerable<string> sourceDirectories,
        IEnumerable<string> requiredPackages,
        IEnumerable<string> requiredFeatures,
        IEnumerable<string> requiredModules)
        : this(
            [.. sourceDirectories],
            [.. requiredPackages],
            [.. requiredFeatures],
            [.. requiredModules]) { }

    public string[] SourceDirectories { get; } = sourceDirectories;
    public string[] RequiredPackages { get; } = requiredPackages;
    public string[] RequiredFeatures { get; } = requiredFeatures;
    public string[] RequiredModules { get; } = requiredModules;
}

internal sealed class StubBuilder
{
    readonly List<string> sourceDirectories = [];
    readonly List<string> requiredPackages = [];
    readonly List<string> requiredFeatures = [];
    readonly List<string> requiredModules = [];

    public StubBuilder IncludeSource(string directory)
    {
        sourceDirectories.Add(directory);
        return this;
    }

    public StubBuilder RequirePackage(string package)
    {
        requiredPackages.Add(package);
        return this;
    }

    public StubBuilder RequireFeature(string feature)
    {
        requiredFeatures.Add(feature);
        return this;
    }

    public StubBuilder RequireModule(string module)
    {
        requiredModules.Add(module);
        return this;
    }

    public StubBuilder IncludeSources(params string[] directories)
    {
        sourceDirectories.AddRange(directories);
        return this;
    }

    public StubBuilder RequirePackages(params string[] packages)
    {
        requiredPackages.AddRange(packages);
        return this;
    }

    public StubBuilder RequireFeatures(params string[] features)
    {
        requiredFeatures.AddRange(features);
        return this;
    }

    public StubBuilder RequireModules(params string[] modules)
    {
        requiredModules.AddRange(modules);
        return this;
    }

    public StubConfig Build()
        => new(sourceDirectories, requiredPackages, requiredFeatures, requiredModules);
}

internal sealed class ModuleResolution(
    HashSet<string> moduleNames,
    IEnumerable<ModuleConfig> activeModules,
    IEnumerable<StubConfig> activeStubs)
{
    public HashSet<string> ModuleNames { get; } = moduleNames;
    public ModuleConfig[] ActiveModules { get; } = [.. activeModules];
    public StubConfig[] ActiveStubs { get; } = [.. activeStubs];
}

internal sealed class ModuleCollection()
{
    readonly List<ModuleConfig> modules = [];

    public ModuleCollection AddModule(string name, Func<ModuleBuilder, ModuleBuilder> setup)
    {
        modules.Add(setup(new(name)).Build());
        return this;
    }

    public ModuleCollection AddModule(Func<ModuleBuilder, ModuleBuilder> setup)
    {
        modules.Add(setup(new()).Build());
        return this;
    }

    public ModuleConfig[] Build() => [.. modules];
}

internal static class ItemExtensions
{
    public static IEnumerable<ITaskItem> EmptyIfNull(this IEnumerable<ITaskItem>? items)
        => items ?? [];
}
