// Copyright © 2014 Aspect Injector Team
// Author: Alexander Guly
// Licensed under the Apache License, Version 2.0

using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.BuildTask.Contexts
{
    internal static class TypeContextFactory
    {
        private static readonly Dictionary<TypeDefinition, TargetTypeContext> _contexts = new Dictionary<TypeDefinition, TargetTypeContext>();

        public static TargetTypeContext GetOrCreateContext(TypeDefinition td)
        {
            lock (_contexts)
            {
                TargetTypeContext result;

                if (!_contexts.TryGetValue(td, out result))
                {
                    var ctors = td.Methods.Where(m => m.IsConstructor).Select(c => MethodContextFactory.GetOrCreateContext(c)).ToArray();
                    result = new TargetTypeContext(td, ctors);
                    _contexts.Add(td, result);
                }

                return result;
            }
        }
    }
}