using AspectInjector.Core.Models;

namespace AspectInjector.Core.Contracts
{
    public interface ILogger
    {
        void LogMessage(string message);

        void LogInformation(string message);

        void LogWarning(string message);

        void LogError(string message);

        void LogError(CompilationError error);

        bool IsErrorThrown { get; }
    }
}