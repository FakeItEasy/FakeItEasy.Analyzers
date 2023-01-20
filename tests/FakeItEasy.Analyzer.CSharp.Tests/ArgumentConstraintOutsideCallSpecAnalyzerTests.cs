namespace FakeItEasy.Analyzer.CSharp.Tests
{
    using FakeItEasy.Analyzer.Tests.Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Xunit;

    public class ArgumentConstraintOutsideCallSpecAnalyzerTests : DiagnosticVerifier
    {
        public static TheoryData<string> Constraints =>
            ArgumentConstraintTestCases.From(
                "A<int>._",
                "A<int>.Ignored",
                "A<int>.That.IsEqualTo(42)",
                "A<int>.That.Not.IsEqualTo(42)");

        public static TheoryData<string> EvenIncompleteConstraints =>
            ArgumentConstraintTestCases.From(
                "A<int>._",
                "A<int>.Ignored",
                "A<int>.That",
                "A<int>.That.Not",
                "A<int>.That.IsEqualTo(42)",
                "A<int>.That.Not.IsEqualTo(42)");

        public static TheoryData<string> ErroneousConstraints =>
            ArgumentConstraintTestCases.From(
                "A<int>.That.Matches(x => Test(x))");

        [Theory]
        [MemberData(nameof(EvenIncompleteConstraints))]
        public void Diagnostic_should_be_triggered_for_constraint_assigned_to_variable(string constraint)
        {
            string code = $@"using FakeItEasy;
namespace TheNamespace
{{
    class TheClass
    {{
        void Test()
        {{
            var x = {constraint};
        }}
    }}
}}
";

            this.VerifyCSharpDiagnostic(
                code,
                new DiagnosticResult
                {
                    Id = "FakeItEasy0003",
                    Message = $"Argument constraint '{constraint}' is not valid outside a call specification.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 8, 21) }
                });
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_be_triggered_in_A_CallToSet_To_Value(string constraint)
        {
            string code = $@"using FakeItEasy;
namespace TheNamespace
{{
    class TheClass
    {{
        void Test()
        {{
            var foo = A.Fake<IFoo>();
            A.CallToSet(() => foo.Bar).To({constraint}).DoesNothing();
        }}
    }}

    interface IFoo {{ int Bar {{ get; set; }} }}
}}
";

            this.VerifyCSharpDiagnostic(
                code,
                new DiagnosticResult
                {
                    Id = "FakeItEasy0003",
                    Message = $"Argument constraint '{constraint}' is not valid outside a call specification.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 9, 43) }
                });
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_A_CallTo_Func(string constraint)
        {
            string code = $@"using FakeItEasy;
namespace TheNamespace
{{
    class TheClass
    {{
        void Test()
        {{
            var foo = A.Fake<IFoo>();
            A.CallTo(() => foo.Bar({constraint})).Returns(42);
        }}
    }}

    interface IFoo {{ int Bar(int x); }}
}}
";

            this.VerifyCSharpDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_A_CallTo_Action(string constraint)
        {
            string code = $@"using FakeItEasy;
namespace TheNamespace
{{
    class TheClass
    {{
        void Test()
        {{
            var foo = A.Fake<IFoo>();
            A.CallTo(() => foo.Bar({constraint})).DoesNothing();
        }}
    }}

    interface IFoo {{ void Bar(int x); }}
}}
";

            this.VerifyCSharpDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_A_CallTo_Indexer(string constraint)
        {
            string code = $@"using FakeItEasy;
namespace TheNamespace
{{
    class TheClass
    {{
        void Test()
        {{
            var foo = A.Fake<IFoo>();
            A.CallTo(() => foo[{constraint}]).Returns(42);
        }}
    }}

    interface IFoo {{ int this[int key] {{ get; set; }} }}
}}
";

            this.VerifyCSharpDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_A_CallToSet_To_Expression(string constraint)
        {
            string code = $@"using FakeItEasy;
namespace TheNamespace
{{
    class TheClass
    {{
        void Test()
        {{
            var foo = A.Fake<IFoo>();
            A.CallToSet(() => foo.Bar).To(() => {constraint}).DoesNothing();
        }}
    }}

    interface IFoo {{ int Bar {{ get; set; }} }}
}}
";

            this.VerifyCSharpDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_A_CallToSet_Indexer(string constraint)
        {
            string code = $@"using FakeItEasy;
namespace TheNamespace
{{
    class TheClass
    {{
        void Test()
        {{
            var foo = A.Fake<IFoo>();
            A.CallToSet(() => foo[{constraint}]).DoesNothing();
        }}
    }}

    interface IFoo {{ int this[int key] {{ get; set; }} }}
}}
";

            this.VerifyCSharpDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_Fake_CallsTo_Func(string constraint)
        {
            string code = $@"using FakeItEasy;
namespace TheNamespace
{{
    class TheClass
    {{
        void Test()
        {{
            var fake = new Fake<IFoo>();
            fake.CallsTo(foo => foo.Bar({constraint})).Returns(42);
        }}
    }}

    interface IFoo {{ int Bar(int x); }}
}}
";

            this.VerifyCSharpDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_Fake_CallsTo_Action(string constraint)
        {
            string code = $@"using FakeItEasy;
namespace TheNamespace
{{
    class TheClass
    {{
        void Test()
        {{
            var fake = new Fake<IFoo>();
            fake.CallsTo(foo => foo.Bar({constraint})).DoesNothing();
        }}
    }}

    interface IFoo {{ void Bar(int x); }}
}}
";

            this.VerifyCSharpDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_Fake_CallsTo_Indexer(string constraint)
        {
            string code = $@"using FakeItEasy;
namespace TheNamespace
{{
    class TheClass
    {{
        void Test()
        {{
            var fake = new Fake<IFoo>();
            fake.CallsTo(foo => foo[{constraint}]).Returns(42);
        }}
    }}

    interface IFoo {{ int this[int key] {{ get; set; }} }}
}}
";

            this.VerifyCSharpDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_Fake_CallsToSet_To_Expression(string constraint)
        {
            string code = $@"using FakeItEasy;
namespace TheNamespace
{{
    class TheClass
    {{
        void Test()
        {{
            var fake = new Fake<IFoo>();
            fake.CallsToSet(foo => foo.Bar).To(() => {constraint}).DoesNothing();
        }}
    }}

    interface IFoo {{ int Bar {{ get; set; }} }}
}}
";

            this.VerifyCSharpDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_Fake_CallsToSet_Indexer(string constraint)
        {
            string code = $@"using FakeItEasy;
namespace TheNamespace
{{
    class TheClass
    {{
        void Test()
        {{
            var fake = new Fake<IFoo>();
            fake.CallsToSet(foo => foo[{constraint}]).DoesNothing();
        }}
    }}

    interface IFoo {{ int this[int key] {{ get; set; }} }}
}}
";

            this.VerifyCSharpDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(ErroneousConstraints))]
        public void Diagnostic_should_not_be_triggered_if_constraint_inside_call_spec_contains_error(string constraint)
        {
            string code = $@"using FakeItEasy;
namespace TheNamespace
{{
    class TheClass
    {{
        void Test()
        {{
            var foo = A.Fake<IFoo>();
            A.CallTo(() => foo.Bar({constraint})).Returns(42);
        }}

        bool Test(byte x) => true;
    }}

    interface IFoo {{ void Bar(int x); }}
}}
";

            this.VerifyCSharpDiagnosticWithCompilationErrors(code);
        }

        [Theory]
        [MemberData(nameof(ErroneousConstraints))]
        public void Diagnostic_should_be_triggered_if_constraint_outside_call_spec_contains_error(string constraint)
        {
            string code = $@"using FakeItEasy;
namespace TheNamespace
{{
    class TheClass
    {{
        void Test()
        {{
            var c = {constraint};
        }}

        bool Test(byte x) => true;
    }}

    interface IFoo {{ void Bar(int x); }}
}}
";

            this.VerifyCSharpDiagnosticWithCompilationErrors(
                code,
                new DiagnosticResult
                {
                    Id = "FakeItEasy0003",
                    Message = $"Argument constraint '{constraint}' is not valid outside a call specification.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.cs", 8, 21) }
                });
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
            new ArgumentConstraintOutsideCallSpecAnalyzer();
    }
}
