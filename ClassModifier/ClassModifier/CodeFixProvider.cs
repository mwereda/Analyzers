using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ClassModifier
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ClassModifierCodeFixProvider)), Shared]
    public class ClassModifierCodeFixProvider : CodeFixProvider
    {
        private const string publicTitle = "Make public";
        private const string internalTitle = "Make internal";
        private const string privateTitle = "Make private";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(ClassModifierAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: publicTitle,
                    createChangedDocument: c => AddModifier(context.Document, declaration, SyntaxKind.PublicKeyword, c),
                    equivalenceKey: publicTitle),
                diagnostic);
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: internalTitle,
                    createChangedDocument: c => AddModifier(context.Document, declaration, SyntaxKind.InternalKeyword, c),
                    equivalenceKey: internalTitle),
                diagnostic);
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: privateTitle,
                    createChangedDocument: c => AddModifier(context.Document, declaration, SyntaxKind.PrivateKeyword, c),
                    equivalenceKey: privateTitle),
                diagnostic);
        }

        private async Task<Document> AddModifier(Document document, TypeDeclarationSyntax typeDecl, SyntaxKind modifier, CancellationToken cancellationToken)
        {
            var classDeclaration = typeDecl as ClassDeclarationSyntax;
            if (classDeclaration == null)
            {
                return document;
            }

            var newClassDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(modifier));
            var root = await document.GetSyntaxRootAsync();
            var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);

            return document.WithSyntaxRoot(newRoot);         
        }
    }
}