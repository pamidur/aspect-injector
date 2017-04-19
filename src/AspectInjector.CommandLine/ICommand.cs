using System.Collections.Generic;

namespace AspectInjector.CLI
{
    interface ICommand
    {
        string Description { get; }
        int Execute(IReadOnlyList<string> args);
    }
}
