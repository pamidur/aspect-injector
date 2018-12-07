namespace AspectInjector.Samples.Security.Services
{
    public interface ISecurityService
    {
        bool CurrentUserHasPermission(string permission);
    }
}
