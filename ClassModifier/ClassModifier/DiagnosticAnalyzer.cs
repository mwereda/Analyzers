using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;

namespace ClassModifier
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ClassModifierAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ClassModifier";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Modifiers";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {           
            context.RegisterSemanticModelAction(AnalyzeClass);
        }        

        private static void AnalyzeClass(SemanticModelAnalysisContext context)
        {
            var syntaxTree = context.SemanticModel.SyntaxTree;
            var classSyntax = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classSyntax == null)
            {
                return;
            }
            
            if (!classSyntax.Modifiers.Any(x => x != SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
            {
                var diagnostic = Diagnostic.Create(Rule, Location.Create(syntaxTree, classSyntax.Span));
                context.ReportDiagnostic(diagnostic);
            }          
        }      
    }
}
