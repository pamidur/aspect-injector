using AspectInjector.Broker;
using System;
using AspectInjector.Samples.Security.Services;
using System.Reflection;
using System.Linq;

namespace AspectInjector.Samples.Security.Aspects
{
    [Aspect(Scope.Global, Factory = typeof(AuthorizationAspectFactory))]
    public class AuthorizationAspect
    {
        private readonly ISecurityService _securityService;

        public AuthorizationAspect(ISecurityService securityService)
        {
            _securityService = securityService;
        }

        // This advice assumes that user ID is know to security service
        // It also takes permission name from AuthorizationAttribute set on the target method
        // and if the attribute is present, calls security service to make sure that the user has the specified permission
        [Advice(Kind.Before, Targets = Target.Method)]
        public void CheckAccess(
            [Argument(Source.Method)] MethodBase method,
            [Argument(Source.Injections)] Attribute[] triggers
            )
        {
            foreach (var trigger in triggers.OfType<AuthorizationAttribute>())
            {
                if(!_securityService.CurrentUserHasPermission(trigger.Permission))
                    throw new Exception($"Current user doesn't have permission {trigger.Permission} to execute method {method.Name}");                
            }
        }
    }
}
