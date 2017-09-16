using System;
using AspectInjector.Samples.Security.Aspects;
using AspectInjector.Samples.Security.Services;

namespace AspectInjector.Samples.Security
{
    class Program
    {
        static void Main(string[] args)
        {
            // Providing service instance to the aspect factory to allow passing service to the aspect constructor 
            AuthorizationAspectFactory.SecurityService = new SecurityService();

            try
            {
                var service = new SampleService();
                service.UpdateUserSettings(Guid.NewGuid());
                service.UpdateGlobalSettings(Guid.NewGuid());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
