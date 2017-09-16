using System;
namespace AspectInjector.Samples.Security.Services
{
    public interface ISecurityService
    {
        bool HasPermission(Guid userId, string permission);
    }
}
