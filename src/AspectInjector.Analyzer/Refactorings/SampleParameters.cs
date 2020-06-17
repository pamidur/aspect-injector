using AspectInjector.Broker;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AspectInjector.Analyzer.Refactorings
{
    public readonly struct ParameterSample
    {
        public readonly string Source;
        public readonly string Name;
        public readonly ParameterSyntax ParameterSyntax;
        public readonly UsingDirectiveSyntax UsingSyntax;

        public ParameterSample(string source, string name, ParameterSyntax parameterSyntax, UsingDirectiveSyntax usingSyntax)
        {
            Source = source;
            Name = name;
            ParameterSyntax = parameterSyntax;
            UsingSyntax = usingSyntax;
        }
    }

    public static partial class Samples
    {
        public static class Parameters
        {
            public static readonly ParameterSample Arguments =
                GetAdviceParameter(nameof(Source.Arguments), "args", SyntaxBasicBlocks.Types.ObjectArray);

            public static readonly ParameterSample Triggers =
                GetAdviceParameter(nameof(Source.Triggers), "triggers", SyntaxBasicBlocks.Types.AttributeArray, SyntaxBasicBlocks.Namespaces.System);

            public static readonly ParameterSample Instance =
                GetAdviceParameter(nameof(Source.Instance), "instance", PredefinedType(Token(SyntaxKind.ObjectKeyword)));

            public static readonly ParameterSample ReturnValue =
                GetAdviceParameter(nameof(Source.ReturnValue), "retValue", PredefinedType(Token(SyntaxKind.ObjectKeyword)));

            public static readonly ParameterSample Metadata =
                 GetAdviceParameter(nameof(Source.Metadata), "metadata", IdentifierName(nameof(MethodBase)), SyntaxBasicBlocks.Namespaces.SystemReflection);

            public static readonly ParameterSample Name =
                GetAdviceParameter(nameof(Source.Name), "name", PredefinedType(Token(SyntaxKind.StringKeyword)));

            public static readonly ParameterSample ReturnType =
                GetAdviceParameter(nameof(Source.ReturnType), "retType", IdentifierName(nameof(Type)), SyntaxBasicBlocks.Namespaces.System);

            public static readonly ParameterSample Type =
                 GetAdviceParameter(nameof(Source.Type), "hostType", IdentifierName(nameof(Type)), SyntaxBasicBlocks.Namespaces.System);

            public static readonly ParameterSample Target =
                GetAdviceParameter(nameof(Source.Target), "target", IdentifierName("Func<object[],object>"), SyntaxBasicBlocks.Namespaces.System);

            private static ParameterSample GetAdviceParameter(string source, string name, TypeSyntax type, UsingDirectiveSyntax @using = null)
            {
                return new ParameterSample(source, name, Parameter(Identifier(name))
                .WithAttributeLists(SingletonList(AttributeList(SingletonSeparatedList(
                    Attribute(IdentifierName(nameof(Broker.Argument)))
                    .AddArgumentListArguments(AttributeArgument(MemberAccessExpression(
                                                                        SyntaxKind.SimpleMemberAccessExpression,
                                                                        IdentifierName(nameof(Source)),
                                                                        IdentifierName(source))))))))
                .WithType(type), @using);
            }
        }
    }
}
