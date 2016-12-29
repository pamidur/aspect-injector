using AspectInjector.Core.Contexts;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Defaults
{
    public abstract class EffectReaderBase<T> : IEffectReader
        where T : Effect
    {
        protected ILogger Log { get; private set; }

        protected abstract IEnumerable<T> ReadEffects(TypeDefinition aspectHost);

        public void Init(ProcessingContext context)
        {
            Log = context.Services.Log;
        }

        IEnumerable<Effect> IEffectReader.ReadEffects(TypeDefinition aspectHost)
        {
            return ReadEffects(aspectHost).Cast<Effect>();
        }

        public void Cleanup(ModuleDefinition module)
        {
            throw new NotImplementedException();
        }
    }
}