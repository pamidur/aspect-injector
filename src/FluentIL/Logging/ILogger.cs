using FluentIL.Common;
using Mono.Cecil.Cil;

namespace FluentIL.Logging
{
    public interface ILogger
    {
        bool IsErrorThrown { get; }
        void Log(Rule rule, SequencePoint sp, params string[] messages);
    }
}