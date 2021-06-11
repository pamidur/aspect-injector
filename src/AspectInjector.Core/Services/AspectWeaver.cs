using AspectInjector.Core.Contracts;
using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using FluentIL;
using FluentIL.Logging;
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
            var singletonField = aspect.Host.Fields.FirstOrDefault(m => m.Name == Constants.AspectGlobalField);

            if (singletonField == null)
            {
                singletonField = new FieldDefinition(Constants.AspectGlobalField, FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly, aspect.Host);
                aspect.Host.Fields.Add(singletonField);

                var cctor = aspect.Host.Methods.FirstOrDefault(c => c.IsConstructor && c.IsStatic);

                if (cctor == null)
                {
                    cctor = new MethodDefinition(".cctor",
                        MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                            aspect.Host.Module.TypeSystem.Void);

                    aspect.Host.Methods.Add(cctor);

                    cctor.Body.Instead((in Cut i) => i.Return());
                }

                cctor.Body.AfterEntry((in Cut i) => i.Store(singletonField, (in Cut val) => val.CreateAspectInstance(aspect)));
            }

            aspect.Host.IsBeforeFieldInit = false;
        }
    }
}