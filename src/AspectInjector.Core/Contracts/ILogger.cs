using AspectInjector.Core.Models;

namespace AspectInjector.Core.Contracts
{
    public interface ILogger
    {
        bool IsErrorThrown { get; }

        void LogError(CompilationMessage message);

        void LogInfo(string message);

        void LogWarning(CompilationMessage message);
    }
}