using AspectInjector.BuildTask.Contexts;

namespace AspectInjector.BuildTask.Contracts
{
  public interface IAspectInjector
  {
    void Inject(InjectionContext context);
  }
}