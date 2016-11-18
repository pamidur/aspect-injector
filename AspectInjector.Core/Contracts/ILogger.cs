using System;

namespace AspectInjector.Core.Contracts
{
    public interface ILogger
    {
        void LogMessage(string message);

        void LogInformation(string message);

        void LogWarning(string message);

        void LogError(string message);

        void LogException(Exception exception);

        bool IsErrorThrown { get; }
    }
}