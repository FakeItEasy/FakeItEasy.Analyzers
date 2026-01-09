using System.Text.RegularExpressions;
using SimpleExec;
using static Bullseye.Targets;
using static SimpleExec.Command;

Project[] projectsToPack =
{
    "src/FakeItEasy.Analyzer.CSharp/FakeItEasy.Analyzer.CSharp.csproj",
    "src/FakeItEasy.Analyzer.VisualBasic/FakeItEasy.Analyzer.VisualBasic.csproj"
};

var testSuites = new Dictionary<string, string[]>
{
    ["unit"] = new[]
    {
        "tests/FakeItEasy.Analyzer.CSharp.Tests",
        "tests/FakeItEasy.Analyzer.VisualBasic.Tests",
    }
};

Target("default", DependsOn("unit", "pack"));

Target(
    "build",
    () => Run("dotnet", "build FakeItEasy.Analyzers.sln -c Release /maxcpucount /nr:false /verbosity:minimal /nologo /bl:artifacts/logs/build.binlog"));

foreach (var testSuite in testSuites)
{
    Target(
        testSuite.Key,
        DependsOn("build"),
        forEach: testSuite.Value,
        action: testDirectory => Run("dotnet", "test --configuration Release --no-build --nologo -- RunConfiguration.NoAutoReporters=true", testDirectory));
}

Target(
    "pack",
    DependsOn("build"),
    forEach: projectsToPack,
    action: project => Run("dotnet", $"pack {project.Path} --configuration Release --no-build --nologo --output \"{Path.GetFullPath("artifacts/output")}\""));

Target("docs", DependsOn("generate-docs"));

Target(
    "generate-docs",
    DependsOn("create-python-version-file"),
    () => Run("uv", "run mkdocs build --clean --site-dir ../artifacts/docs --config-file mkdocs.yml --strict", "docs"));

// uv really likes there to be a .python-version file to specify which version of python it should use.
// Rather than maintain a duplicate of the version in pyproject.toml, we'll generate the .python-version file from the latter.
Target(
    "create-python-version-file",
    () =>
    {
        // expect the line to look something like
        // requires-python = "~=x.yz"
        // . We want the x.yz part.
        const string versionSourceFile = "docs/pyproject.toml";
        const string versionPattern = @"requires-python = ""~=(?<version>[^""]+)""";
        var pythonVersion = File.ReadLines(versionSourceFile)
            .Select(line => Regex.Match(line, versionPattern))
            .Where(m => m.Success)
            .Select(m => m.Groups["version"].Value)
            .FirstOrDefault() ?? throw new InvalidOperationException($"Could not find required Python version in {versionSourceFile}");
        File.WriteAllText("docs/.python-version", pythonVersion);
    });

RunTargetsAndExit(args, messageOnly: ex => ex is NonZeroExitCodeException);

file sealed record Project(string Path)
{
    public static implicit operator Project(string path) => new Project(path);

    public override string ToString() => this.Path.Split('/').Last();
}
