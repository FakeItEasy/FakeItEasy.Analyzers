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

RunTargetsAndExit(args, messageOnly: ex => ex is NonZeroExitCodeException);

file sealed record Project(string Path)
{
    public static implicit operator Project(string path) => new Project(path);

    public override string ToString() => this.Path.Split('/').Last();
}
