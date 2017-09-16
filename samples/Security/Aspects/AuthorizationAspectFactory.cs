using AspectInjector.Samples.Security.Services;
using System;

namespace AspectInjector.Samples.Security.Aspects
{
    public static class AuthorizationAspectFactory
    {
        public static ISecurityService SecurityService { get; set; }

        public static object GetInstance(Type type)
        {
            if (type != typeof(AuthorizationAspect))
            {
                throw new ArgumentException($"{nameof(AuthorizationAspectFactory)} can create instances only of type {nameof(AuthorizationAspect)}");
            }

            if (SecurityService == null)
            {
                throw new ArgumentException($"{nameof(SecurityService)} is null");
            }

            return new AuthorizationAspect(SecurityService);
        }
    }
}
