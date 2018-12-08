using AspectInjector.Rules;
using Mono.Cecil.Cil;

namespace AspectInjector.Core.Contracts
{
    public interface ILogger
    {
        bool IsErrorThrown { get; }
        void Log(Rule rule, SequencePoint sp, params string[] messages);
    }
}