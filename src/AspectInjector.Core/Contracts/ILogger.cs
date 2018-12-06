using Microsoft.CodeAnalysis;
using Mono.Cecil.Cil;

namespace AspectInjector.Core.Contracts
{
    public interface ILogger
    {
        bool IsErrorThrown { get; }
        void Log(DiagnosticDescriptor descriptor, SequencePoint sp, params string[] messages);
    }
}