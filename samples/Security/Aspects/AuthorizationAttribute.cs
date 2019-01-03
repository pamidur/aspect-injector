using AspectInjector.Broker;
using System;

namespace AspectInjector.Samples.Security.Aspects
{
    [Injection(typeof(AuthorizationAspect))]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class AuthorizationAttribute : Attribute
    {
        public string Permission { get; }

        public AuthorizationAttribute(string permission)
        {
            Permission = permission;
        }
    }
}
