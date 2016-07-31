// Copyright © 2014 Aspect Injector Team
// Author: Alexander Guly
// Licensed under the Apache License, Version 2.0

using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace AspectInjector.BuildTask.Contexts
{
    internal static class ILProcessorFactory
    {
        private static readonly Dictionary<MethodBody, ILProcessor> Processors = new Dictionary<MethodBody, ILProcessor>();

        public static ILProcessor GetOrCreateProcessor(MethodBody mb)
        {
            lock (Processors)
            {
                ILProcessor result;

                if (!Processors.TryGetValue(mb, out result))
                {
                    result = mb.GetILProcessor();
                    Processors.Add(mb, result);
                }

                return result;
            }
        }
    }
}