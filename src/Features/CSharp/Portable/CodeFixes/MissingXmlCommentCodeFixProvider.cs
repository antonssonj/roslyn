using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Composition;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CodeActions;
using System.Threading;

namespace Microsoft.CodeAnalysis.CSharp.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = "MissingXmlCommentCodeFixProvider"), Shared]
    internal class MissingXmlCommentCodeFixProvider : CodeFixProvider
    {
        private const string CS1591 = nameof(CS1591);

        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(CS1591);
            }
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

           
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

            
            context.RegisterCodeFix(
                CodeAction.Create("Add Inheritdoc comment", c => InsertInheritDocComment(context.Document, declaration, c)),
                diagnostic);
            
        }

        private async Task<Document> InsertInheritDocComment(Document document, TypeDeclarationSyntax propDecl, CancellationToken cancellationToken)
        {


            var leadingtrivia = propDecl.GetLeadingTrivia();

            

            var t = propDecl
                    .WithoutLeadingTrivia()
                    .WithLeadingTrivia(SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, "/// <inheritdoc />"))
                    .NormalizeWhitespace();
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(propDecl, t);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}
