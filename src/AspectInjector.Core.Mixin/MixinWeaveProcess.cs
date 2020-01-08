using AspectInjector.Core.Extensions;
using AspectInjector.Core.Models;
using AspectInjector.Rules;
using FluentIL;
using FluentIL.Extensions;
using FluentIL.Logging;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Mixin
{
    internal class MixinWeaveProcess
    {
        private readonly TypeDefinition _target;
        private readonly MixinEffect _effect;
        private readonly AspectDefinition _aspect;

        public MixinWeaveProcess(ILogger log, IMemberDefinition target, AspectDefinition aspect, MixinEffect effect)
        {
            switch (target)
            {
                case TypeDefinition td: _target = td; break;
                case MethodDefinition md: _target = md.DeclaringType; break;
                case PropertyDefinition pd: _target = pd.DeclaringType; break;
                case EventDefinition ed: _target = ed.DeclaringType; break;
                default: log.Log(GeneralRules.UnexpectedCompilerBehaviour, _target, $"Unexpected mixin target '{target.ToString()}'"); break;
            }

            _aspect = aspect;
            _effect = effect;
        }

        public void Execute()
        {
            var ifaceTree = GetInterfacesTree(_effect.InterfaceType);

            foreach (var iface in ifaceTree)
                if (_target.Interfaces.All(i => !i.InterfaceType.Match(iface)))
                {
                    var ifaceRef = _target.Module.ImportReference(iface.Clone(_target));

                    _target.Interfaces.Add(new InterfaceImplementation(ifaceRef));

                    var ifaceDefinition = iface.Resolve();

                    foreach (var method in ifaceDefinition.Methods.Where(m => m.IsNormalMethod() || m.IsConstructor))
                        CreateMethodProxy(method, ifaceRef);

                    foreach (var @event in ifaceDefinition.Events)
                        CreateEventProxy(@event, ifaceRef);

                    foreach (var property in ifaceDefinition.Properties)
                        CreatePropertyProxy(property, ifaceRef);
                }
        }

        protected MethodDefinition CreateMethodProxy(MethodDefinition originalMethod, TypeReference @interface)
        {
            var method = _target.Module.ImportReference(originalMethod).MakeReference(@interface);

            //if (@interface.IsGenericInstance)
            //    method = _target.Module.ImportReference(originalMethod.MakeGenericReference(@interface));

            return GetOrCreateMethodProxy(method);
        }

        protected void CreateEventProxy(EventDefinition originalEvent, TypeReference @interface)
        {
            var eventName = $"{@interface.FullName}.{originalEvent.Name}";

            var proxyAdd = originalEvent.AddMethod == null ? null : CreateMethodProxy(originalEvent.AddMethod, @interface);
            var proxyRemove = originalEvent.RemoveMethod == null ? null : CreateMethodProxy(originalEvent.RemoveMethod, @interface);

            var eventType = (proxyAdd ?? proxyRemove).Parameters[0].ParameterType;
            var ed = new EventDefinition(eventName, EventAttributes.None, eventType)
            {
                RemoveMethod = proxyRemove,
                AddMethod = proxyAdd
            };

            _target.Events.Add(ed);
        }

        protected void CreatePropertyProxy(PropertyDefinition originalProperty, TypeReference @interface)
        {
            var propertyName = $"{@interface.FullName}.{originalProperty.Name}";

            var proxyGet = originalProperty.GetMethod == null ? null : CreateMethodProxy(originalProperty.GetMethod, @interface);
            var proxySet = originalProperty.SetMethod == null ? null : CreateMethodProxy(originalProperty.SetMethod, @interface);

            var propertyType = proxyGet?.ReturnType ?? proxySet.Parameters[0].ParameterType;
            var pd = new PropertyDefinition(propertyName, PropertyAttributes.None, propertyType)
            {
                GetMethod = proxyGet,
                SetMethod = proxySet
            };

            _target.Properties.Add(pd);
        }

        private IEnumerable<TypeReference> GetInterfacesTree(TypeReference typeReference)
        {
            var definition = typeReference.Resolve();
            if (!definition.IsInterface)
                throw new NotSupportedException(typeReference.Name + " should be an interface");

            var nestedIfaces = definition.Interfaces.Select(i => i.InterfaceType.IsGenericInstance ? ParametrizeSubInterface((GenericInstanceType)i.InterfaceType, typeReference) : i.InterfaceType).ToArray();

            return new[] { typeReference }.Concat(nestedIfaces);
        }

        private TypeReference ParametrizeSubInterface(GenericInstanceType interfaceType, TypeReference typeReference)
        {
            TypeReference LookupType(TypeReference tr)
            {
                if (tr is GenericParameter gp)
                    return ((GenericInstanceType)typeReference).GenericArguments[gp.Position];

                return tr;
            }

            var gparams = interfaceType.GenericArguments.Select(LookupType).ToArray();

            return interfaceType.Resolve().MakeGenericInstanceType(gparams);
        }

        protected MethodDefinition GetOrCreateMethodProxy(MethodReference ifaceMethod)
        {
            var proxy = _target.Methods.FirstOrDefault(m => m.IsExplicitImplementationOf(ifaceMethod));
            if (proxy == null)
            {
                proxy = Implement(ifaceMethod, _target);
                proxy.Mark(WellKnownTypes.DebuggerHiddenAttribute);


                if (proxy.HasGenericParameters)
                    ifaceMethod = ifaceMethod.MakeGenericInstanceMethod(proxy.GenericParameters.ToArray());

                proxy.Body.Instead(
                    e => e
                    .LoadAspect(_aspect)
                    .Call(ifaceMethod, args =>
                    {
                        foreach (var pp in proxy.Parameters)
                            args = args.Load(pp);
                        return args;
                    })
                    .Return()
                    );
            }

            return proxy;
        }

        private static MethodDefinition Implement(MethodReference origin, TypeDefinition target)
        {
            var methodName = $"{origin.DeclaringType.FullName}.{origin.Name}";
            var originDef = origin.Resolve();
            var method = new MethodDefinition(methodName, MethodAttributes.Private | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, origin.ReturnType);

            target.Methods.Add(method);



            foreach (var gparam in origin.GenericParameters)
                method.GenericParameters.Add(gparam.Clone(method));

            method.ReturnType = ResolveType(origin.ReturnType, origin, method.GenericParameters.ToArray());

            if (originDef.IsSpecialName)
                method.IsSpecialName = true;

            foreach (var parameter in origin.Parameters)
            {
                var resolvedType = ResolveType(parameter.ParameterType, origin, method.GenericParameters.ToArray());
                method.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, target.Module.ImportReference(resolvedType)));
            }

            //foreach (var parameter in origin.Parameters)
            //{
            //    var resolvedType = ResolveType(parameter.ParameterType, origin, method.GenericParameters.ToArray());
            //    parameter.ParameterType = resolvedType;
            //    //method.Parameters.Add(new ParameterDefinition(parameter.Name, parameter.Attributes, target.Module.ImportReference(resolvedType)));
            //}

            //origin.ReturnType = ResolveType(origin.ReturnType, origin, method.GenericParameters.ToArray());

            method.Overrides.Add(origin);

            return method;
        }

        private static TypeReference ResolveType(TypeReference tr, MethodReference reference, IList<GenericParameter> genericParameters)
        {
            var newtr = tr;
            if (!newtr.ContainsGenericParameter)
                return reference.Module.ImportReference(newtr);

            var definition = reference.Resolve();

            if (newtr is GenericParameter gpp)
            {
                // pick Type from method implementation
                if (gpp.Owner == definition)
                    newtr = genericParameters[gpp.Position];
                //pick Type from interface implementation
                else if (gpp.Owner == definition.DeclaringType)
                    newtr = ((IGenericInstance)reference.DeclaringType).GenericArguments[gpp.Position];
            }
            else if (tr is ByReferenceType brt)
                newtr = new ByReferenceType(ResolveType(brt.ElementType, reference, genericParameters));
            else if (tr is GenericInstanceType git)
            {
                var newgit = new GenericInstanceType(ResolveType(git.ElementType, reference, genericParameters));

                foreach (var ga in git.GenericArguments)
                    newgit.GenericArguments.Add(ResolveType(ga, reference, genericParameters));

                newtr = newgit;
            }
            else
                newtr = new TypeReference(tr.Namespace, tr.Name, reference.Module, tr.Scope, tr.IsValueType)
                {
                    DeclaringType = tr.DeclaringType
                };

            foreach (var subgp in tr.GenericParameters)
            {
                newtr.GenericParameters.Add(genericParameters[subgp.Position]);
            }


            return reference.Module.ImportReference(newtr);
        }
    }
}