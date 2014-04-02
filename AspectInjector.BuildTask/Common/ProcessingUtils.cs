using AspectInjector.Broker;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using System.Collections.Generic;

namespace AspectInjector.BuildTask.Common
{
  internal class ProcessingUtils
  {
    public static IEnumerable<AdviceArgumentSource> GetAdviceArgumentsSources(MethodDefinition adviceMethod)
    {
      foreach (var argument in adviceMethod.Parameters)
      {
        var argumentAttribute = argument.CustomAttributes.GetAttributeOfType<AdviceArgumentAttribute>();
        if (argumentAttribute == null)
          throw new CompilationException("Unbound advice arguments are not supported", adviceMethod);

        var source = (AdviceArgumentSource)argumentAttribute.ConstructorArguments[0].Value;
        if (source == AdviceArgumentSource.Instance)
        {
          if (!argument.ParameterType.IsTypeOf(typeof(object)))
            throw new CompilationException("Argument should be of type System.Object to inject AdviceArgumentSource.Instance", adviceMethod);
        }
        else if (source == AdviceArgumentSource.TargetArguments)
        {
          if (!argument.ParameterType.IsTypeOf(new ArrayType(adviceMethod.Module.TypeSystem.Object)))
            throw new CompilationException("Argument should be of type System.Array<System.Object> to inject AdviceArgumentSource.TargetArguments", adviceMethod);
        }
        else if (source == AdviceArgumentSource.TargetName)
        {
          if (!argument.ParameterType.IsTypeOf(typeof(string)))
            throw new CompilationException("Argument should be of type System.String to inject AdviceArgumentSource.TargetName", adviceMethod);
        }
        else if (source == AdviceArgumentSource.AbortFlag)
        {
          if (!argument.ParameterType.IsTypeOf(new ByReferenceType(adviceMethod.Module.TypeSystem.Boolean)))
            throw new CompilationException("Argument should be of type ref System.Boolean to inject AdviceArgumentSource.AbortFlag", adviceMethod);
        }
        else if (source == AdviceArgumentSource.ReturningValue)
        {
          throw new CompilationException("AdviceArgumentSource.ReturningValue is not supported yet. Reserved to inspect returning value on after methods", adviceMethod);
        }
        else if (source == AdviceArgumentSource.CustomData)
        {
          if (!argument.ParameterType.IsTypeOf(new ArrayType(adviceMethod.Module.TypeSystem.Object)))
            throw new CompilationException("Argument should be of type System.Array<System.Object> to inject AdviceArgumentSource.CustomData", adviceMethod);
        }

        yield return source;
      }
    }
  }
}