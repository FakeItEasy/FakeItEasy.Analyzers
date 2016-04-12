﻿namespace FakeItEasy.Analyzer.Tests.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Formatting;
    using NUnit.Framework;

    /// <summary>
    /// Superclass of all Unit tests made for diagnostics with code fixes.
    /// Contains methods used to verify correctness of code fixes.
    /// </summary>
    public abstract partial class CodeFixVerifier : DiagnosticVerifier
    {
        /// <summary>
        /// Returns the code fix being tested (C#) - to be implemented in non-abstract class.
        /// </summary>
        /// <returns>The CodeFixProvider to be used for CSharp code.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "It's not appropriate here")]
        protected virtual CodeFixProvider GetCSharpCodeFixProvider()
        {
            return null;
        }

        /// <summary>
        /// Returns the code fix being tested (VB) - to be implemented in non-abstract class.
        /// </summary>
        /// <returns>The CodeFixProvider to be used for VisualBasic code.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "It's not appropriate here")]
        protected virtual CodeFixProvider GetBasicCodeFixProvider()
        {
            return null;
        }

        /// <summary>
        /// Called to test a C# code fix when applied on the inputted string as a source.
        /// </summary>
        /// <param name="oldSource">A class in the form of a string before the code fix was applied to it.</param>
        /// <param name="newSource">A class in the form of a string after the code fix was applied to it.</param>
        /// <param name="codeFixIndex">Index determining which code fix to apply if there are multiple.</param>
        /// <param name="allowNewCompilerDiagnostics">A boolean controlling whether or not the test will fail if the code fix introduces other warnings after being applied.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "It's not intended to be a public API, so it doesn't matter")]
        protected void VerifyCSharpFix(string oldSource, string newSource, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false)
        {
            this.VerifyFix(LanguageNames.CSharp, this.GetCSharpDiagnosticAnalyzer(), this.GetCSharpCodeFixProvider(), oldSource, newSource, codeFixIndex, allowNewCompilerDiagnostics);
        }

        /// <summary>
        /// Called to test a VB code fix when applied on the inputted string as a source.
        /// </summary>
        /// <param name="oldSource">A class in the form of a string before the code fix was applied to it.</param>
        /// <param name="newSource">A class in the form of a string after the code fix was applied to it.</param>
        /// <param name="codeFixIndex">Index determining which code fix to apply if there are multiple.</param>
        /// <param name="allowNewCompilerDiagnostics">A boolean controlling whether or not the test will fail if the code fix introduces other warnings after being applied.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed", Justification = "It's not intended to be a public API, so it doesn't matter")]
        protected void VerifyBasicFix(string oldSource, string newSource, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false)
        {
            this.VerifyFix(LanguageNames.VisualBasic, this.GetBasicDiagnosticAnalyzer(), this.GetBasicCodeFixProvider(), oldSource, newSource, codeFixIndex, allowNewCompilerDiagnostics);
        }

        /// <summary>
        /// General verifier for code fixes.
        /// Creates a Document from the source string, then gets diagnostics on it and applies the relevant code fixes.
        /// Then gets the string after the code fix is applied and compares it with the expected result.
        /// Note: If any code fix causes new diagnostics to show up, the test fails unless allowNewCompilerDiagnostics is set to true.
        /// </summary>
        /// <param name="language">The language the source code is in.</param>
        /// <param name="analyzer">The analyzer to be applied to the source code.</param>
        /// <param name="codeFixProvider">The code fix to be applied to the code wherever the relevant Diagnostic is found.</param>
        /// <param name="oldSource">A class in the form of a string before the code fix was applied to it.</param>
        /// <param name="newSource">A class in the form of a string after the code fix was applied to it.</param>
        /// <param name="codeFixIndex">Index determining which code fix to apply if there are multiple.</param>
        /// <param name="allowNewCompilerDiagnostics">A boolean controlling whether or not the test will fail if the code fix introduces other warnings after being applied.</param>
        private void VerifyFix(string language, DiagnosticAnalyzer analyzer, CodeFixProvider codeFixProvider, string oldSource, string newSource, int? codeFixIndex, bool allowNewCompilerDiagnostics)
        {
            var document = CreateDocument(oldSource, language);
            var analyzerDiagnostics = GetSortedDiagnosticsFromDocuments(analyzer, new[] { document });
            var compilerDiagnostics = GetCompilerDiagnostics(document);
            var attempts = analyzerDiagnostics.Length;

            for (int i = 0; i < attempts; ++i)
            {
                var actions = new List<CodeAction>();
                var context = new CodeFixContext(document, analyzerDiagnostics[0], (a, d) => actions.Add(a), CancellationToken.None);
                codeFixProvider.RegisterCodeFixesAsync(context).Wait();

                if (!actions.Any())
                {
                    break;
                }

                if (codeFixIndex != null)
                {
                    document = ApplyFix(document, actions.ElementAt((int)codeFixIndex));
                    break;
                }

                document = ApplyFix(document, actions.ElementAt(0));
                analyzerDiagnostics = CodeFixVerifier.GetSortedDiagnosticsFromDocuments(analyzer, new[] { document });

                var newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, GetCompilerDiagnostics(document));

                // Check if applying the code fix introduced any new compiler diagnostics
                if (!allowNewCompilerDiagnostics && newCompilerDiagnostics.Any())
                {
                    // Format and get the compiler diagnostics again so that the locations make sense in the output
                    document = document.WithSyntaxRoot(Formatter.Format(document.GetSyntaxRootAsync().Result, Formatter.Annotation, document.Project.Solution.Workspace));
                    newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, GetCompilerDiagnostics(document));

                    string message =
                        FormattableString.Invariant(
                            $@"Fix introduced new compiler diagnostics:
{string.Join("\r\n", newCompilerDiagnostics.Select(d => d.ToString()))}

New document:
{document.GetSyntaxRootAsync().Result.ToFullString()}
");
                    Assert.IsTrue(false, message);
                }

                // Check if there are analyzer diagnostics left after the code fix
                if (!analyzerDiagnostics.Any())
                {
                    break;
                }
            }

            // After applying all of the code fixes, compare the resulting string to the inputted one
            var actual = GetStringFromDocument(document);
            Assert.AreEqual(newSource, actual);
        }
    }
}
