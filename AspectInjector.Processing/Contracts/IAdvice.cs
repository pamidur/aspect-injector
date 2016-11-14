using AspectInjector.Models;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Contracts
{
    public interface IAdvice
    {
        TypeReference HostType { get; set; }

        bool IsApplicableFor(Aspect aspect);
    }
}