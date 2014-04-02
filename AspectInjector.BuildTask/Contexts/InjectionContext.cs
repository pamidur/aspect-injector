using AspectInjector.Broker;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.BuildTask.Contexts
{
  public class InjectionContext : IComparable<InjectionContext>
  {
    public InjectionContext()
    {
    }

    public InjectionContext(InjectionContext other)
    {
      AspectType = other.AspectType;
      AspectFactory = other.AspectFactory;
      AspectFactoryArgumentsSources = other.AspectFactoryArgumentsSources;
      AspectCustomData = other.AspectCustomData;

      AdviceMethod = other.AdviceMethod;
      AdviceArgumentsSources = other.AdviceArgumentsSources;

      TargetType = other.TargetType;
      TargetMethodContext = other.TargetMethodContext;
      TargetName = other.TargetName;

      InjectionPoint = other.InjectionPoint;
    }

    public List<AdviceArgumentSource> AdviceArgumentsSources { get; set; }
    public MethodDefinition AdviceMethod { get; set; }
    public object[] AspectCustomData { get; set; }
    public MethodDefinition AspectFactory { get; set; }
    public List<AdviceArgumentSource> AspectFactoryArgumentsSources { get; set; }
    public TypeDefinition AspectType { get; set; }
    public InjectionPoints InjectionPoint { get; set; }
    public bool IsAbortable
    {
      get { return AdviceArgumentsSources != null && AdviceArgumentsSources.Any(s => s == AdviceArgumentSource.AbortFlag); }
    }
    public TargetMethodContext TargetMethodContext { get; set; }
    public string TargetName { get; set; }
    public TypeDefinition TargetType { get; set; }

    public int CompareTo(InjectionContext other)
    {
      if (object.Equals(TargetMethodContext, other.TargetMethodContext))
      {
        if (IsAbortable) return -1;
        if (other.IsAbortable) return 1;
        return 0;
      }
      return TargetMethodContext.TargetMethod.FullName.CompareTo(other.TargetMethodContext.TargetMethod.FullName);
    }
  }
}