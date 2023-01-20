namespace FakeItEasy.Analyzer.VisualBasic.Tests
{
    using FakeItEasy.Analyzer.Tests.Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Xunit;

    public class ArgumentConstraintOutsideCallSpecAnalyzerTests : DiagnosticVerifier
    {
        public static TheoryData<string> Constraints =>
            ArgumentConstraintTestCases.From(
                "A(Of Integer).Ignored",
                "A(Of Integer).That.IsEqualTo(42)",
                "A(Of Integer).That.Not.IsEqualTo(42)");

        public static TheoryData<string> EvenIncompleteConstraints =>
            ArgumentConstraintTestCases.From(
                "A(Of Integer).Ignored",
                "A(Of Integer).That",
                "A(Of Integer).That.Not",
                "A(Of Integer).That.IsEqualTo(42)",
                "A(Of Integer).That.Not.IsEqualTo(42)");

        public static TheoryData<string> ErroneousConstraints =>
            ArgumentConstraintTestCases.From(
                "A(Of Integer)");

        [Theory]
        [MemberData(nameof(EvenIncompleteConstraints))]
        public void Diagnostic_should_be_triggered_for_constraint_assigned_to_variable(string constraint)
        {
            string code = $@"Imports FakeItEasy
Namespace TheNamespace
    Class TheClass
        Sub Test()
            Dim x = {constraint}
        End Sub
    End Class
End Namespace
";

            this.VerifyVisualBasicDiagnostic(
                code,
                new DiagnosticResult
                {
                    Id = "FakeItEasy0003",
                    Message = $"Argument constraint '{constraint}' is not valid outside a call specification.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.vb", 5, 21) }
                });
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_be_triggered_in_A_CallToSet_To_Value(string constraint)
        {
            string code = $@"Imports FakeItEasy
Namespace TheNamespace
    Class TheClass
        Sub Test()
            Dim foo = A.Fake(Of IFoo)()
            A.CallToSet(Function() foo.Bar).To({constraint}).DoesNothing()
        End Sub
    End Class

    Interface IFoo
        Property Bar As Integer
    End Interface
End Namespace
";

            this.VerifyVisualBasicDiagnostic(
                code,
                new DiagnosticResult
                {
                    Id = "FakeItEasy0003",
                    Message = $"Argument constraint '{constraint}' is not valid outside a call specification.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.vb", 6, 48) }
                });
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_A_CallTo_Func(string constraint)
        {
            string code = $@"Imports FakeItEasy
Namespace TheNamespace
    Class TheClass
        Sub Test()
            Dim foo = A.Fake(Of IFoo)()
            A.CallTo(Function() foo.Bar({constraint})).Returns(42)
        End Sub
    End Class

    Interface IFoo
        Function Bar(ByVal x As Integer) As Integer
    End Interface
End Namespace
";

            this.VerifyVisualBasicDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_A_CallTo_Action(string constraint)
        {
            string code = $@"Imports FakeItEasy
Namespace TheNamespace
    Class TheClass
        Sub Test()
            Dim foo = A.Fake(Of IFoo)()
            A.CallTo(Sub() foo.Bar({constraint})).DoesNothing()
        End Sub
    End Class

    Interface IFoo
        Sub Bar(ByVal x As Integer)
    End Interface
End Namespace
";

            this.VerifyVisualBasicDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_A_CallTo_Indexer(string constraint)
        {
            string code = $@"Imports FakeItEasy
Namespace TheNamespace
    Class TheClass
        Sub Test()
            Dim foo = A.Fake(Of IFoo)()
            A.CallTo(Function() foo.Item({constraint})).Returns(42)
        End Sub
    End Class

    Interface IFoo
        Property Item(ByVal key as Integer) As Integer
    End Interface
End Namespace
";

            this.VerifyVisualBasicDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_A_CallTo_Default_Indexer(string constraint)
        {
            string code = $@"Imports FakeItEasy
Namespace TheNamespace
    Class TheClass
        Sub Test()
            Dim foo = A.Fake(Of IFoo)()
            A.CallTo(Function() foo({constraint})).Returns(42)
        End Sub
    End Class

    Interface IFoo
        Default Property Item(ByVal key as Integer) As Integer
    End Interface
End Namespace
";

            this.VerifyVisualBasicDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_A_CallToSet_To_Expression(string constraint)
        {
            string code = $@"Imports FakeItEasy
Namespace TheNamespace
    Class TheClass
        Sub Test()
            Dim foo = A.Fake(Of IFoo)()
            A.CallToSet(Function() foo.Bar).To(Function() {constraint}).DoesNothing()
        End Sub
    End Class

    Interface IFoo
        Property Bar As Integer
    End Interface
End Namespace
";

            this.VerifyVisualBasicDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_A_CallToSet_Indexer(string constraint)
        {
            string code = $@"Imports FakeItEasy
Namespace TheNamespace
    Class TheClass
        Sub Test()
            Dim foo = A.Fake(Of IFoo)()
            A.CallToSet(Function() foo.Item({constraint})).DoesNothing()
        End Sub
    End Class

    Interface IFoo
        Property Item(ByVal key as Integer) As Integer
    End Interface
End Namespace
";

            this.VerifyVisualBasicDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_A_CallToSet_Default_Indexer(string constraint)
        {
            string code = $@"Imports FakeItEasy
Namespace TheNamespace
    Class TheClass
        Sub Test()
            Dim foo = A.Fake(Of IFoo)()
            A.CallToSet(Function() foo({constraint})).DoesNothing()
        End Sub
    End Class

    Interface IFoo
        Default Property Item(ByVal key as Integer) As Integer
    End Interface
End Namespace
";

            this.VerifyVisualBasicDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_Fake_CallsTo_Func(string constraint)
        {
            string code = $@"Imports FakeItEasy
Namespace TheNamespace
    Class TheClass
        Sub Test()
            Dim fake = new Fake(Of IFoo)()
            fake.CallsTo(Function(foo) foo.Bar({constraint})).Returns(42)
        End Sub
    End Class

    Interface IFoo
        Function Bar(ByVal x As Integer) As Integer
    End Interface
End Namespace
";

            this.VerifyVisualBasicDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_Fake_CallsTo_Action(string constraint)
        {
            string code = $@"Imports FakeItEasy
Namespace TheNamespace
    Class TheClass
        Sub Test()
            Dim fake = new Fake(Of IFoo)()
            fake.CallsTo(Sub(foo) foo.Bar({constraint})).DoesNothing()
        End Sub
    End Class

    Interface IFoo
        Sub Bar(ByVal x As Integer)
    End Interface
End Namespace
";

            this.VerifyVisualBasicDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_Fake_CallsTo_Indexer(string constraint)
        {
            string code = $@"Imports FakeItEasy
Namespace TheNamespace
    Class TheClass
        Sub Test()
            Dim fake = new Fake(Of IFoo)()
            fake.CallsTo(Function(foo) foo.Item({constraint})).Returns(42)
        End Sub
    End Class

    Interface IFoo
        Property Item(ByVal key as Integer) As Integer
    End Interface
End Namespace
";

            this.VerifyVisualBasicDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_Fake_CallsTo_Default_Indexer(string constraint)
        {
            string code = $@"Imports FakeItEasy
Namespace TheNamespace
    Class TheClass
        Sub Test()
            Dim fake = new Fake(Of IFoo)()
            fake.CallsTo(Function(foo) foo({constraint})).Returns(42)
        End Sub
    End Class

    Interface IFoo
        Default Property Item(ByVal key as Integer) As Integer
    End Interface
End Namespace
";

            this.VerifyVisualBasicDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_Fake_CallsToSet_To_Expression(string constraint)
        {
            string code = $@"Imports FakeItEasy
Namespace TheNamespace
    Class TheClass
        Sub Test()
            Dim fake = new Fake(Of IFoo)()
            fake.CallsToSet(Function(foo) foo.Bar).To(Function() {constraint}).DoesNothing()
        End Sub
    End Class

    Interface IFoo
        Property Bar As Integer
    End Interface
End Namespace
";

            this.VerifyVisualBasicDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_Fake_CallsToSet_Indexer(string constraint)
        {
            string code = $@"Imports FakeItEasy
Namespace TheNamespace
    Class TheClass
        Sub Test()
            Dim fake = new Fake(Of IFoo)()
            fake.CallsToSet(Function(foo) foo.Item({constraint})).DoesNothing()
        End Sub
    End Class

    Interface IFoo
        Property Item(ByVal key as Integer) As Integer
    End Interface
End Namespace
";

            this.VerifyVisualBasicDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(Constraints))]
        public void Diagnostic_should_not_be_triggered_in_Fake_CallsToSet_Default_Indexer(string constraint)
        {
            string code = $@"Imports FakeItEasy
Namespace TheNamespace
    Class TheClass
        Sub Test()
            Dim fake = new Fake(Of IFoo)()
            fake.CallsToSet(Function(foo) foo({constraint})).DoesNothing()
        End Sub
    End Class

    Interface IFoo
        Default Property Item(ByVal key as Integer) As Integer
    End Interface
End Namespace
";

            this.VerifyVisualBasicDiagnostic(code);
        }

        [Theory]
        [MemberData(nameof(ErroneousConstraints))]
        public void Diagnostic_should_not_be_triggered_if_constraint_inside_call_spec_contains_error(string constraint)
        {
            string code = $@"Imports FakeItEasy
Namespace TheNamespace
    Class TheClass
        Sub Test()
            Dim foo = A.Fake(Of IFoo)()
            A.CallTo(Function() foo.Bar({constraint}.That.Matches(Function(x) Test(x)))).Returns(42)
        End Sub

        Function Test(ByVal x As Byte) As Boolean
            Return True
        End Function
    End Class

    Interface IFoo
        Function Bar(ByVal x As Integer) As Integer
    End Interface
End Namespace
";

            this.VerifyVisualBasicDiagnosticWithCompilationErrors(code);
        }

        [Theory]
        [MemberData(nameof(ErroneousConstraints))]
        public void Diagnostic_should_be_triggered_if_constraint_outside_call_spec_contains_error(string constraint)
        {
            string code = $@"Imports FakeItEasy
Namespace TheNamespace
    Class TheClass
        Sub Test()
            Dim c = {constraint}.That.Matches(Function(x) Test(x))
        End Sub

        Function Test(ByVal x As Byte) As Boolean
            Return True
        End Function
    End Class

    Interface IFoo
        Function Bar(ByVal x As Integer) As Integer
    End Interface
End Namespace
";

            this.VerifyVisualBasicDiagnosticWithCompilationErrors(
                code,
                new DiagnosticResult
                {
                    Id = "FakeItEasy0003",
                    Message = $"Argument constraint '{constraint}.That.Matches(Function(x) Test(x))' is not valid outside a call specification.",
                    Severity = DiagnosticSeverity.Warning,
                    Locations = new[] { new DiagnosticResultLocation("Test0.vb", 5, 21) }
                });
        }

        protected override DiagnosticAnalyzer GetVisualBasicDiagnosticAnalyzer() =>
            new ArgumentConstraintOutsideCallSpecAnalyzer();
    }
}
