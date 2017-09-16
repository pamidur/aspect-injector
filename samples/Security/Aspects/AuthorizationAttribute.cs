using System;

namespace AspectInjector.Samples.Security.Aspects
{
    public class AuthorizationAttribute : Attribute
    {
        public string Permission { get; }

        public AuthorizationAttribute(string permission)
        {
            Permission = permission;
        }
    }
}
