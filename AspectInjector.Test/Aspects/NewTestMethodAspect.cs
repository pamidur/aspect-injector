using System;

namespace AspectInjector.Test
{
    internal class NewTestMethodAspect
    {
        public NewTestMethodAspect()
        {
            Console.WriteLine("Constructor");
        }

        [MethodInjection(MethodPoint.Ending)]
        public void After([ArgumentInjection(ArgumentValue.Instance)] object target)
        {
            Console.WriteLine("We've just called " + target.ToString());
        }

        [MethodInjection(MethodPoint.Beginning)]
        public void ICallItBefore([ArgumentInjection(ArgumentValue.MemberName)] string propertyName)
        {
            Console.WriteLine("We are gonna call " + propertyName);
        }
    }
}