using System;

namespace AspectInjector.SampleApps.Freezable
{
    class Program
    {
        static void Main(string[] args)
        {
            var sample = new SampleClass();
            sample.Text = "Hello!"; //works
            sample.Value = 1; //works

            ((IFreezable)sample).Freeze();

            sample.Text = "World!"; //throws an exception
        }
    }
}