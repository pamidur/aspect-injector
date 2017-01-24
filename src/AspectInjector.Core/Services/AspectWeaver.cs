using System;
using System.Collections.Generic;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;

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
            throw new NotImplementedException();
        }
    }
}