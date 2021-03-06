namespace FakeItEasy.Analyzer
{
    using System.Collections.Immutable;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
#if CSHARP
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
#elif VISUAL_BASIC
    using Microsoft.CodeAnalysis.VisualBasic;
    using Microsoft.CodeAnalysis.VisualBasic.Syntax;
#endif
    using static FakeItEasy.Analyzer.SyntaxHelpers;

#if CSHARP
    [ExportCodeFixProvider(LanguageNames.CSharp)]
#elif VISUAL_BASIC
    [ExportCodeFixProvider(LanguageNames.VisualBasic)]
#endif
    public class ArgumentConstraintTypeMismatchCodeFixProvider : CodeFixProvider
    {
        private static readonly Task CompletedTask = Task.FromResult(false);

        public override ImmutableArray<string> FixableDiagnosticIds { get; } =
            ImmutableArray.Create(DiagnosticDefinitions.ArgumentConstraintNullabilityMismatch.Id, DiagnosticDefinitions.ArgumentConstraintTypeMismatch.Id);

        private static string MakeConstraintNullableCodeFixTitle =>
            DiagnosticDefinitions.ResourceManager.GetString(nameof(MakeConstraintNullableCodeFixTitle), CultureInfo.CurrentUICulture);

        private static string MakeNotNullConstraintCodeFixTitle =>
            DiagnosticDefinitions.ResourceManager.GetString(nameof(MakeNotNullConstraintCodeFixTitle), CultureInfo.CurrentUICulture);

        private static string ChangeConstraintTypeCodeFixTitle =>
            DiagnosticDefinitions.ResourceManager.GetString(nameof(ChangeConstraintTypeCodeFixTitle), CultureInfo.CurrentUICulture);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.FirstOrDefault();
            if (diagnostic is null)
            {
                return CompletedTask;
            }

            if (diagnostic.Descriptor.Id == DiagnosticDefinitions.ArgumentConstraintNullabilityMismatch.Id)
            {
                context.RegisterCodeFix(
                  CodeAction.Create(
                      MakeConstraintNullableCodeFixTitle,
                      ct => MakeConstraintNullableAsync(context, diagnostic, ct),
                      equivalenceKey: "MakeConstraintNullable"),
                  diagnostic);

                context.RegisterCodeFix(
                    CodeAction.Create(
                        MakeNotNullConstraintCodeFixTitle,
                        ct => MakeNotNullConstraintAsync(context, diagnostic, ct),
                        equivalenceKey: "MakeNotNullConstraint"),
                    diagnostic);
            }
            else if (diagnostic.Descriptor.Id == DiagnosticDefinitions.ArgumentConstraintTypeMismatch.Id)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        ChangeConstraintTypeCodeFixTitle,
                        ct => ChangeConstraintTypeAsync(context, diagnostic, ct),
                        equivalenceKey: "ChangeConstraintType"),
                    diagnostic);
            }

            return CompletedTask;
        }

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        private static SyntaxNode GetConstraintNode(Diagnostic diagnostic, SyntaxNode root)
        {
            // getInnermostNodeForTie: true to disambiguate between ArgumentSyntax and
            // MemberAccessExpressionSyntax, which have the same source span. We want the
            // MemberAccessExpressionSyntax node, which is the innermost one.
            return root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
        }

        private static async Task<Document> MakeConstraintNullableAsync(CodeFixContext context, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var constraintNode = GetConstraintNode(diagnostic, root);

            // The A<T> type
            var aType = (constraintNode as MemberAccessExpressionSyntax)?.Expression as GenericNameSyntax;

            // The T type
            var constraintType = aType?.TypeArgumentList.Arguments.FirstOrDefault();
            if (constraintType is object)
            {
                // The T? type
                var nullableConstraintType = SyntaxFactory.NullableType(constraintType);

                // Replace T node with T? and return updated document
                var newRoot = root.ReplaceNode(constraintType, nullableConstraintType);
                return context.Document.WithSyntaxRoot(newRoot);
            }

            return context.Document;
        }

        private static async Task<Document> MakeNotNullConstraintAsync(CodeFixContext context, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            var constraintNode = GetConstraintNode(diagnostic, root);

            // The A<T> type
            var aType = (GenericNameSyntax)((MemberAccessExpressionSyntax)constraintNode).Expression;

            // The T type
            var constraintType = aType.TypeArgumentList.Arguments.FirstOrDefault();

            // The T? type
            var nullableConstraintType = SyntaxFactory.NullableType(constraintType);

            // The A<T?> type
            var nullableAType = aType.ReplaceNode(constraintType, nullableConstraintType);

            // A<T?>.That
            var thatNode =
                SimpleMemberAccess(
                    nullableAType,
                    SyntaxFactory.IdentifierName("That"));

            // The new A<T?>.That.IsNotNull() constraint
            var newConstraintNode =
                SyntaxFactory.InvocationExpression(
                    SimpleMemberAccess(
                        thatNode,
                        SyntaxFactory.IdentifierName("IsNotNull")),
                    SyntaxFactory.ArgumentList());

            // Replace node and return updated document
            var newRoot = root.ReplaceNode(constraintNode, newConstraintNode);
            return context.Document.WithSyntaxRoot(newRoot);
        }

        private static async Task<Document> ChangeConstraintTypeAsync(CodeFixContext context, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var root = await context.Document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            // The T type
            var constraintType = GetConstraintType(diagnostic, root);
            var parameterTypeName = diagnostic.Properties[ArgumentConstraintTypeMismatchAnalyzer.ParameterTypeKey];
            var parameterType = SyntaxFactory.ParseName(parameterTypeName);
            var newRoot = root.ReplaceNode(constraintType, parameterType);
            return context.Document.WithSyntaxRoot(newRoot);
        }

        private static TypeSyntax GetConstraintType(Diagnostic diagnostic, SyntaxNode root)
        {
            var constraintNode = GetConstraintNode(diagnostic, root);
            if (constraintNode is MemberAccessExpressionSyntax memberAccessNode)
            {
                var expressionNode = (GenericNameSyntax)memberAccessNode.Expression;
                return expressionNode.TypeArgumentList.Arguments.First();
            }

            InvocationExpressionSyntax invocationNode = (InvocationExpressionSyntax)constraintNode;
            var aTThatMatches = (MemberAccessExpressionSyntax)invocationNode.Expression;
            var aTthat = (MemberAccessExpressionSyntax)aTThatMatches.Expression;
            var aT = (GenericNameSyntax)aTthat.Expression;
            return aT.TypeArgumentList.Arguments.First();
        }
    }
}
