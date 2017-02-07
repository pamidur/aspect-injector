using AspectInjector.Core.Contracts;
using System;

namespace AspectInjector.Core.Mixin
{
    public static class MixinConfiguration
    {
        public static void Configure(Action<Type, Type> register)
        {
            register(typeof(IEffectExtractor), typeof(MixinExtractor));
            register(typeof(IEffectWeaver), typeof(MixinWeaver));
        }
    }
}