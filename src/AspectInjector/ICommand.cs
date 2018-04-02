using System.Collections.Generic;

namespace AspectInjector
{
    interface ICommand
    {
        string Description { get; }
        int Execute(IReadOnlyList<string> args);
    }
}
