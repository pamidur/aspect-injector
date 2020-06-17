using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AspectInjector.Analyzer.Refactorings
{
    public static partial class Samples
    {
        public static class Advices
        {
            public static readonly (MethodDeclarationSyntax Method, UsingDirectiveSyntax[] Usings) Before = CreateAdviceSample(
                Broker.Kind.Before, "Before", PredefinedType(Token(SyntaxKind.VoidKeyword)),
                new[] { Parameters.Name, Parameters.Arguments, Parameters.Type },
@"{
    //Don't forget to remove unused parameters as it improves performance!
    //Alt+Enter or Ctrl+. on Method name or Advice attribute to add more Arguments

    Console.WriteLine($""Calling {name} from {hostType.Name} with args: {string.Join(',', args.Select(a => ToString()))}"");
}",
                new[] { SyntaxBasicBlocks.Namespaces.SystemLinq, SyntaxBasicBlocks.Namespaces.System });


            public static readonly (MethodDeclarationSyntax Method, UsingDirectiveSyntax[] Usings) After = CreateAdviceSample(
                Broker.Kind.After, "After", PredefinedType(Token(SyntaxKind.VoidKeyword)),
                new[] { Parameters.Name, Parameters.ReturnValue, Parameters.Type },
@"{
    //Don't forget to remove unused parameters as it improves performance!
    //Alt+Enter or Ctrl+. on Method name or Advice attribute to add more Arguments

    Console.WriteLine($""Finished {name} from {hostType.Name} with result: {retValue}"");
}",
                new[] { SyntaxBasicBlocks.Namespaces.System });

            public static readonly (MethodDeclarationSyntax Method, UsingDirectiveSyntax[] Usings) Around = CreateAdviceSample(
                Broker.Kind.Around, "Around", PredefinedType(Token(SyntaxKind.ObjectKeyword)),
                new[] { Parameters.Name, Parameters.Arguments, Parameters.Type, Parameters.Target },
@"{
    //Don't forget to remove unused parameters as it improves performance!
    //Alt+Enter or Ctrl+. on Method name or Advice attribute to add more Arguments

    Console.WriteLine($""Entering {name} from {hostType.Name}"");
    var result = target(args);
    Console.WriteLine($""Leaving {name} from {hostType.Name}"");
    return result;
}",
                new[] { SyntaxBasicBlocks.Namespaces.System });

            private static (MethodDeclarationSyntax Method, UsingDirectiveSyntax[] Usings) CreateAdviceSample(Broker.Kind kind, string name, TypeSyntax returnType, IReadOnlyList<ParameterSample> parameters, string body, UsingDirectiveSyntax[] usings = null)
            {
                usings = usings ?? new UsingDirectiveSyntax[0];
                usings = usings.Concat(parameters.Select(p => p.UsingSyntax)).Where(s => s != null).ToArray();

                var method = MethodDeclaration(returnType, Identifier(name))
                    .WithAdviceAttribute(kind)
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithParameterList(ParameterList(SeparatedList(parameters.Select(p => p.ParameterSyntax))))
                    .WithBody(ParseCompilationUnit(body, options: new CSharpParseOptions(kind: SourceCodeKind.Script)).ChildNodes().First().ChildNodes().OfType<BlockSyntax>().First())                   
                    .WithAdditionalAnnotations(Formatter.Annotation);

                return (method, usings);
            }
        }
    }
}