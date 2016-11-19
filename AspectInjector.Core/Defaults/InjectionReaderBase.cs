using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Defaults
{
    public abstract class InjectionReaderBase<T> : IInjectionReader
        where T : Injection
    {
        protected ILogger Log { get; private set; }

        protected abstract IEnumerable<T> ReadInjections(ModuleDefinition module);

        public void Init(ProcessingContext context)
        {
            Log = context.Services.Log;
        }

        IEnumerable<Injection> IInjectionReader.ReadInjections(ModuleDefinition module)
        {
            return ReadInjections(module).Cast<Injection>();
        }

        public void Cleanup(ModuleDefinition module)
        {
            throw new NotImplementedException();
        }
    }
}