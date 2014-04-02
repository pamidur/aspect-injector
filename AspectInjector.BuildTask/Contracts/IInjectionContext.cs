using AspectInjector.BuildTask.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.BuildTask.Contracts
{
    public interface IInjectionContext
    {
        AspectContext AspectContext { get; }
    }
}
