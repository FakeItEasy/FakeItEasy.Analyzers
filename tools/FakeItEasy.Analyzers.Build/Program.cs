namespace FakeItEasy.Build
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using SimpleExec;
    using static Bullseye.Targets;
    using static SimpleExec.Command;

    public class Program
    {
        private static readonly Project[] ProjectsToPack =
        {
            "src/FakeItEasy.Analyzer.CSharp/FakeItEasy.Analyzer.CSharp.csproj",
            "src/FakeItEasy.Analyzer.VisualBasic/FakeItEasy.Analyzer.VisualBasic.csproj"
        };

        private static readonly IReadOnlyDictionary<string, string[]> TestSuites = new Dictionary<string, string[]>
        {
            ["unit"] = new[]
            {
                "tests/FakeItEasy.Analyzer.CSharp.Tests",
                "tests/FakeItEasy.Analyzer.VisualBasic.Tests",
            }
        };

        public static void Main(string[] args)
        {
            Target("default", DependsOn("unit", "pack"));

            Target(
                "build",
                () => Run("dotnet", "build FakeItEasy.Analyzers.sln -c Release /maxcpucount /nr:false /verbosity:minimal /nologo /bl:artifacts/logs/build.binlog"));

            foreach (var testSuite in TestSuites)
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
                forEach: ProjectsToPack,
                action: project => Run("dotnet", $"pack {project.Path} --configuration Release --no-build --nologo --output \"{Path.GetFullPath("artifacts/output")}\""));

            RunTargetsAndExit(args, messageOnly: ex => ex is NonZeroExitCodeException);
        }

        private class Project
        {
            public Project(string path) => this.Path = path;

            public string Path { get; set; }

            public static implicit operator Project(string path) => new Project(path);

            public override string ToString() => this.Path.Split('/').Last();
        }
    }
}
