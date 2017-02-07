using AspectInjector.Core.Advice.Weavers;
using AspectInjector.Core.Contracts;
using System;

namespace AspectInjector.Core.Advice
{
    public static class AdviceConfiguration
    {
        public static void Configure(Action<Type, Type> register)
        {
            register(typeof(IEffectExtractor), typeof(AdviceExtractor));

            register(typeof(IEffectWeaver), typeof(AdviceAfterWeaver));
            register(typeof(IEffectWeaver), typeof(AdviceBeforeWeaver));
        }
    }
}