using AspectInjector.Core.Models;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Core.Models
{
    public class PointCut
    {
        public void Return(Action<PointCut> arg)
        {
            throw new NotImplementedException();
        }

        public PointCut Call(MethodReference method, Action<PointCut> args = null)
        {
            throw new NotImplementedException();
        }

        public PointCut This { get; }

        public PointCut Field(Aspect aspect)
        {
            throw new NotImplementedException();
        }

        public PointCut Field(FieldReference field)
        {
            throw new NotImplementedException();
        }

        public PointCut Parameter(ParameterReference par)
        {
            throw new NotImplementedException();
        }
    }
}