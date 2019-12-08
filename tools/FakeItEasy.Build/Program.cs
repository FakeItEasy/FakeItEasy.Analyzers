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
                () => Run("dotnet", "build FakeItEasy.sln -c Release /maxcpucount /nr:false /verbosity:minimal /nologo /bl:artifacts/logs/build.binlog"));

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
                action: project => Run("dotnet", $"pack {project.Path} --configuration Release --no-build --nologo --output {Path.GetFullPath("artifacts/output")}"));

            Target(
                "initialize-user-properties",
                () =>
                    {
                        if (!File.Exists("FakeItEasy.user.props"))
                        {
                            var defaultUserProps = @"
<Project>
  <PropertyGroup>
    <BuildProfile></BuildProfile>
  </PropertyGroup>
</Project>".Trim();
                            File.WriteAllText("FakeItEasy.user.props", defaultUserProps, Encoding.UTF8);
                        }
                    });

            foreach (var profile in Directory.EnumerateFiles("profiles", "*.props").Select(Path.GetFileNameWithoutExtension))
            {
                Target(
                    "use-profile-" + profile,
                    DependsOn("initialize-user-properties"),
                    () =>
                        {
                            var xmlDoc = XDocument.Load("FakeItEasy.user.props");

                            var buildProfileElement = xmlDoc.Root.Elements("PropertyGroup").Elements("BuildProfile").FirstOrDefault();
                            if (buildProfileElement is null)
                            {
                                var propertyGroupElement = xmlDoc.Root.Element("PropertyGroup");
                                if (propertyGroupElement is null)
                                {
                                    propertyGroupElement = new XElement("PropertyGroup");
                                    xmlDoc.Root.Add(propertyGroupElement);
                                }

                                buildProfileElement = new XElement("BuildProfile");
                                propertyGroupElement.Add(buildProfileElement);
                            }

                            if (buildProfileElement.Value != profile)
                            {
                                buildProfileElement.Value = profile;
                                xmlDoc.Save("FakeItEasy.user.props");
                            }
                        });
            }

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
