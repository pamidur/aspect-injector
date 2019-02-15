using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using FluentIL;
using Mono.Cecil;
using System.Linq;

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
            EnsureSingletonField(target);
        }

        private void EnsureSingletonField(AspectDefinition aspect)
        {
            var ts = aspect.Host.Module.GetTypeSystem();

            var singletonField = aspect.Host.Fields.FirstOrDefault(m => m.Name == Constants.AspectGlobalField);

            if (singletonField == null)
            {
                singletonField = new FieldDefinition(Constants.AspectGlobalField, FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly, ts.Import(aspect.Host));
                aspect.Host.Fields.Add(singletonField);

                var cctor = aspect.Host.Methods.FirstOrDefault(c => c.IsConstructor && c.IsStatic);

                if (cctor == null)
                {
                    cctor = new MethodDefinition(".cctor",
                        MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                            ts.Void);

                    aspect.Host.Methods.Add(cctor);

                    cctor.Body.Instead(i => i.Return());
                }

                cctor.Body.AfterEntry(i => i.Store(singletonField, val => val.CreateAspectInstance(aspect)));
            }

            aspect.Host.IsBeforeFieldInit = false;
        }
    }
}