using AspectInjector.Broker;
using AspectInjector.Samples.Security.Aspects;
using System;

namespace AspectInjector.Samples.Security.Services
{
    [Inject(typeof(AuthorizationAspect))]
    public class SampleService
    {
        [Authorization(Permissions.FullAccess)]
        public void UpdateGlobalSettings(Guid userId)
        {
            Console.WriteLine("Updating global settings");
        }

        public void UpdateUserSettings(Guid userId)
        {
            Console.WriteLine("Updating user settings");
        }
    }
}
