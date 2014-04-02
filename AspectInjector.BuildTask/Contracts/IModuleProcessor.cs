using Mono.Cecil;

namespace AspectInjector.BuildTask.Contracts
{
  public interface IModuleProcessor
  {
    void ProcessModule(ModuleDefinition module);
  }
}