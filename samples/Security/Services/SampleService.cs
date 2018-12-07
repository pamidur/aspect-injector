using AspectInjector.Samples.Security.Aspects;
using System;

namespace AspectInjector.Samples.Security.Services
{
    public class SampleService
    {
        [Authorization(Permissions.FullAccess)]
        public void UpdateGlobalSettings()
        {
            Console.WriteLine("Updating global settings");
        }

        public void UpdateUserSettings()
        {
            Console.WriteLine("Updating user settings");
        }
    }
}
