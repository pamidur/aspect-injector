using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Test
{
    internal class NewTestMethodAspect
    {
        public NewTestMethodAspect()
        {
            Console.WriteLine("Constructor");
        }

        [MethodInjection(Point = MethodPoint.Begining)]
        public void ICallItBefore([InjectArgument(Argument = InjectArgument.MemberName)] string propertyName)
        {
            Console.WriteLine("We are gonna call " + propertyName);
        }

        [MethodInjection(Point = MethodPoint.Ending)]
        public void After([InjectArgument(Argument = InjectArgument.Instanse)] object target)
        {
            Console.WriteLine("We've just called " + target.ToString());
        }
    }
}