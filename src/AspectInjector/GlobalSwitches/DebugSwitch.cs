using System;
using System.Diagnostics;

namespace AspectInjector.GlobalSwitches
{
    class DebugSwitch : ISwitch
    {
        public string Description => "Launches debugger.";

        public int Enable()
        {
            Debugger.Launch();
            return 0;
        }
    }
}
