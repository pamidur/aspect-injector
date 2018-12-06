using AspectInjector.Broker;
using AspectInjector.Core.Contracts;
using AspectInjector.Core.Models;
using System.Linq;

namespace AspectInjector.Core.Advice.Effects
{
    internal class AfterAdviceEffect : BeforeAdviceEffect
    {
        public override Kind Kind => Kind.After;

        protected override void ValidateSupportedArguments(AspectDefinition aspectDefinition, ILogger log)
        {
            var wrongArgs = Arguments.Where(a => a.Source == Source.Target).ToArray();
            LogWrongArgs(wrongArgs, aspectDefinition, log);
        }
    }
}