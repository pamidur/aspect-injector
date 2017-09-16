using AspectInjector.Broker;
using System;
using AspectInjector.Samples.Security.Services;
using System.Reflection;

namespace AspectInjector.Samples.Security.Aspects
{
    [Aspect(Aspect.Scope.Global, Factory = typeof(AuthorizationAspectFactory))]
    public class AuthorizationAspect
    {
        private readonly ISecurityService _securityService;

        public AuthorizationAspect(ISecurityService securityService)
        {
            _securityService = securityService;
        }

        // This advice assumes that each target method has user ID as the first parameter
        // It also takes permission name from AuthorizationAttribute set on the target method
        // and if the attribute is present, calls security service to make sure that the user has the specified permission
        [Advice(Advice.Type.Before, Advice.Target.Method)]
        public void CheckAccess([Advice.Argument(Advice.Argument.Source.Method)] MethodInfo method,
            [Advice.Argument(Advice.Argument.Source.Arguments)] object[] arguments)
        {
            if (arguments.Length == 0 || !(arguments[0] is Guid))
            {
                throw new ArgumentException($"{nameof(AuthorizationAspect)} expects every target method to have Guid as the first parameter");
            }

            var userId = (Guid)arguments[0];
            var authorizationAttr = method.GetCustomAttribute<AuthorizationAttribute>();

            if (authorizationAttr != null && !_securityService.HasPermission(userId, authorizationAttr.Permission))
            {
                throw new Exception($"User {userId} doesn't have permission {authorizationAttr.Permission} to execute method {method.Name}");
            }
        }
    }
}
