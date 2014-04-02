using AspectInjector.BuildTask.Contracts;
using AspectInjector.BuildTask.Injectors;
using AspectInjector.BuildTask.Processors.AspectProcessors;
using AspectInjector.BuildTask.Processors.ModuleProcessors;
using System.Collections.Generic;

namespace AspectInjector.BuildTask
{
  internal class Configuration
  {
    private static List<IModuleProcessor> _processorsTree;

    public static List<IModuleProcessor> GetProcessorsTree()
    {
      if (_processorsTree == null)
      {
        _processorsTree = new List<IModuleProcessor>
                {
                    new InjectionProcessor(new List<IAspectProcessor>
                    {
                        new AdviceProcessor(new AdviceInjector())
                    }),

                    new Janitor()
                };
      }

      return _processorsTree;
    }
  }
}