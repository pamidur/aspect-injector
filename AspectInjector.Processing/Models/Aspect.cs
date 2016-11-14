using AspectInjector.Contracts;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Models
{
    public class Aspect<TTarget> : Aspect
    {
        public TTarget Target { get; set; }
    }

    public abstract class Aspect
    {
        public TypeReference AdviceHost { get; set; }
    }
}