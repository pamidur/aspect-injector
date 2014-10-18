using AspectInjector.Broker;
using AspectInjector.BuildTask.Common;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using System;
using System.Collections.Generic;

namespace AspectInjector.BuildTask.Processors
{
    internal static class ProcessingUtils
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
                else if (source == AdviceArgumentSource.TargetReturnValue)
                {
                    if (!argument.ParameterType.IsTypeOf(typeof(object)))
                        throw new CompilationException("Argument should be of type System.Object to inject AdviceArgumentSource.ReturningValue", adviceMethod);
                }
                else if (source == AdviceArgumentSource.TargetException)
                {
                    if (!argument.ParameterType.IsTypeOf(adviceMethod.Module.TypeSystem.ResolveType(typeof(Exception))))
                        throw new CompilationException("Argument should be of type System.Object to inject AdviceArgumentSource.Exception", adviceMethod);
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