using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using System;

namespace AspectInjector.Core.Services
{
    public class AspectWeaver : IAspectWeaver
    {
        private readonly ILogger _log;

        public AspectWeaver(ILogger logger)
        {
            _log = logger;
        }

        public void WeaveGlobalAssests(AspectDefinition target)
        {
            new NotImplementedException();
        }
    }
}