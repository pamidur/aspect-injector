using AspectInjector.Core.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Core.Contracts
{
    public interface IInitializable
    {
        void Init(ProcessingContext context);
    }
}