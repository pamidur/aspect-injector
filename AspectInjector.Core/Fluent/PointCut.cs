using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Core.Fluent
{
    public class PointCut
    {
        public void Return(Action<PointCut> arg)
        {
            throw new NotImplementedException();
        }

        public void Call(FluentMethod method, Action<PointCut> from = null, Action<PointCut> args = null)
        {
            throw new NotImplementedException();
        }
    }
}