namespace AspectInjector.Samples.Security.Services
{
    public class SecurityService: ISecurityService
    {
        public bool CurrentUserHasPermission(string permission)
        {
            // Just to simulate that user doesn't have access to all permissions
            if (permission == Permissions.FullAccess)
            {
                return false;
            }

            return true;
        }
    }
}
