
namespace AspectInjector.BuildTask.Contracts
{
    public interface IAspectInjector<in TContext>
        where TContext : class, IInjectionContext
    {
        void Inject(TContext context);
    }
}