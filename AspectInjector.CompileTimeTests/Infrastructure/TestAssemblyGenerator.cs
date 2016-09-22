using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace AspectInjector.CompileTimeTests.Infrastructure
{
    internal class TestAssemblyGenerator
    {
        private ModuleDefinition _module;
        private readonly AssemblyDefinition _snippetsAssembly;

        private Dictionary<object, object> _refsMap;
        private readonly Type _source;

        public TestAssemblyGenerator(Type source)
        {
            _snippetsAssembly = AssemblyDefinition.ReadAssembly(Path.Combine(Assembly.GetExecutingAssembly().Location),
                new ReaderParameters
                {
                    ReadingMode = ReadingMode.Deferred,
                });

            _source = source;
        }

        public AssemblyDefinition CreateTestAssembly()
        {
            var assembly = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition(_source.FullName, new Version()), _source.FullName, ModuleKind.Dll);
            _module = assembly.MainModule;

            _refsMap = new Dictionary<object, object>();

            var types = _snippetsAssembly.MainModule.Types.FirstOrDefault(t => t.FullName == _source.FullName).NestedTypes;

            foreach (var type in types)
                ProcessType(type);

            return assembly;
        }

        private void ProcessType(TypeDefinition type)
        {
            GetMappedMember(type);

            foreach (var testedType in type.NestedTypes)
                ProcessType(testedType);

            var members = new MemberReference[] { }
                .Concat(type.Fields)
                .Concat(type.Methods)
                .Concat(type.Events)
                .Concat(type.Properties)
                .Concat(type.NestedTypes)
                .ToList();

            foreach (var member in members)
                GetMappedMember(member);
        }

        public TypeDefinition CopyType(TypeDefinition destination, TypeDefinition type)
        {
            if (type.FullName == _source.FullName)
            {
                _refsMap.Add(type, null);
                return null;
            }

            var attrs = type.Attributes;
            var ns = type.Namespace;

            if (type.DeclaringType != null && type.DeclaringType.FullName == _source.FullName)
            {
                ns = _source.Namespace;

                destination = null;

                if (attrs.HasFlag(Mono.Cecil.TypeAttributes.NestedPublic))
                {
                    attrs &= ~Mono.Cecil.TypeAttributes.NestedPublic;
                    attrs |= Mono.Cecil.TypeAttributes.Public;
                }
            }

            var newtype = type.IsInterface
                ? new TypeDefinition(ns, type.Name, attrs)
                : new TypeDefinition(ns, type.Name, attrs, CopyReference(type.BaseType));

            _refsMap.Add(type, newtype);

            if (destination == null)
                _module.Types.Add(newtype);
            else
                destination.NestedTypes.Add(newtype);

            return newtype;
        }

        private FieldDefinition CopyField(TypeDefinition newtype, FieldDefinition field)
        {
            var newField = new FieldDefinition(field.Name, field.Attributes, CopyReference(field.FieldType));
            _refsMap.Add(field, newField);
            newtype.Fields.Add(newField);
            return newField;
        }

        private EventDefinition CopyEvent(TypeDefinition target, EventDefinition oldEvent)
        {
            var newEvent = new EventDefinition(oldEvent.Name, oldEvent.Attributes, CopyReference(oldEvent.EventType))
                           {
                               AddMethod = CopyReference(oldEvent.AddMethod),
                               RemoveMethod = CopyReference(oldEvent.RemoveMethod)
                           };

            _refsMap.Add(oldEvent, newEvent);
            target.Events.Add(newEvent);
            return newEvent;
        }

        private PropertyDefinition CopyProperty(TypeDefinition target, PropertyDefinition prop)
        {
            var newProp = new PropertyDefinition(prop.Name, prop.Attributes, CopyReference(prop.PropertyType))
                          {
                              GetMethod = prop.GetMethod != null ? CopyReference(prop.GetMethod) : null,
                              SetMethod = prop.SetMethod != null ? CopyReference(prop.SetMethod) : null
                          };

            _refsMap.Add(prop, newProp);
            target.Properties.Add(newProp);
            return newProp;
        }

        private ParameterDefinition CopyParameterDefinition(ParameterDefinition parmd)
        {
            return new ParameterDefinition(parmd.Name, parmd.Attributes, CopyReference(parmd.ParameterType));
        }

        private MethodDefinition CopyMethod(TypeDefinition newtype, MethodDefinition method)
        {
            var newMethod = new MethodDefinition(method.Name, method.Attributes, CopyReference(method.ReturnType));
            _refsMap.Add(method, newMethod);
            newtype.Methods.Add(newMethod);

            CopyCommonStuff(method.MethodReturnType, newMethod.MethodReturnType);

            var instMap = new Dictionary<Instruction, Instruction>();
            var varMap = new Dictionary<VariableDefinition, VariableDefinition>();
            var parMap = new Dictionary<ParameterDefinition, ParameterDefinition>();

            foreach (var par in method.Parameters)
            {
                var npar = CopyMember(par);
                newMethod.Parameters.Add(npar);
                parMap.Add(par, npar);
            }

            if (method.Body != null)
                CopyMethodBody(method, newMethod, varMap, instMap, parMap);

            return newMethod;
        }

        private void CopyMethodBody(MethodDefinition method, MethodDefinition newMethod, Dictionary<VariableDefinition, VariableDefinition> varMap, Dictionary<Instruction, Instruction> instMap, Dictionary<ParameterDefinition, ParameterDefinition> parMap)
        {
            foreach (var v in method.Body.Variables)
            {
                var nv = new VariableDefinition(v.Name, CopyReference(v.VariableType));
                newMethod.Body.Variables.Add(nv);
                varMap.Add(v, nv);
            }

            newMethod.Body.InitLocals = method.Body.InitLocals;

            var ilp = newMethod.Body.GetILProcessor();

            foreach (var i in method.Body.Instructions)
            {
                var ni = CopyInstruction(ilp, i);
                instMap.Add(i, ni);
                ilp.Append(ni);
            }

            //fix local refs
            foreach (var i in newMethod.Body.Instructions.ToList())
            {
                if (i.Operand is ParameterDefinition)
                {
                    var ni = ilp.Create(i.OpCode, parMap[(ParameterDefinition) i.Operand]);
                    ilp.SafeReplace(i, ni);
                }

                if (i.Operand is VariableDefinition)
                {
                    var ni = ilp.Create(i.OpCode, varMap[(VariableDefinition) i.Operand]);
                    ilp.SafeReplace(i, ni);
                }

                if (i.Operand is Instruction)
                {
                    var ni = ilp.Create(i.OpCode, instMap[(Instruction) i.Operand]);
                    ilp.SafeReplace(i, ni);
                }

                if (i.Operand is Instruction[])
                {
                    var ni = ilp.Create(i.OpCode, ((Instruction[]) i.Operand).Select(ii => instMap[ii]).ToArray());
                    ilp.SafeReplace(i, ni);
                }
            }

            foreach (var h in method.Body.ExceptionHandlers)
            {
                newMethod.Body.ExceptionHandlers.Add(new ExceptionHandler(h.HandlerType)
                                                     {
                                                         CatchType = h.CatchType == null ? null : CopyReference(h.CatchType),
                                                         FilterStart = h.FilterStart == null ? null : instMap[h.FilterStart],
                                                         HandlerEnd = h.HandlerEnd == null ? null : instMap[h.HandlerEnd],
                                                         HandlerStart = h.HandlerStart == null ? null : instMap[h.HandlerStart],
                                                         TryEnd = h.TryEnd == null ? null : instMap[h.TryEnd],
                                                         TryStart = h.TryStart == null ? null : instMap[h.TryStart]
                                                     });
            }
        }

        private Instruction CopyInstruction(ILProcessor ilp, Instruction inst)
        {
            var opcode = inst.OpCode;
            var operand = inst.Operand;

            if (operand is MemberReference)
                operand = CopyReference((MemberReference)operand);

            var argTypes = new[] { typeof(OpCode) };
            if (operand != null)
                argTypes = argTypes.Concat(new[] { operand.GetType() }).ToArray();

            var args = new object[] { opcode };
            if (operand != null)
                args = args.Concat(new[] { operand }).ToArray();

            var factory = ilp.GetType().GetMethod("Create", argTypes);
            return (Instruction)factory.Invoke(ilp, args);
        }

        private GenericParameter CopyGenericParameter(GenericParameter generic)
        {
            //todo:: fix it
            //need to find another way to copy generic parameter. Probaly cast Owner to member refernce and copy it.
            //anyway it requires tests and usage of snippets proceesors. And while we don't use snippets I leave it as not implemented

            var ngp = new GenericParameter(generic.Name, generic.Owner);

            //ngp. = generic.Position;
            //generic.Position, generic.Type, _module);

            _refsMap.Add(generic, ngp);

            ngp.Attributes = generic.Attributes;

            generic.Constraints.Select(c => CopyReference(c)).ToList().ForEach(c => ngp.Constraints.Add(c));

            return ngp;
        }

        private CustomAttribute CopyCustomAttribute(CustomAttribute attribute)
        {
            var ca = new CustomAttribute(CopyReference(attribute.Constructor), attribute.GetBlob());

            foreach (var ctorarg in attribute.ConstructorArguments)
                ca.ConstructorArguments.Add(CopyCustomAttributeArgument(ctorarg));

            foreach (var prop in attribute.Properties)
                ca.Properties.Add(new Mono.Cecil.CustomAttributeNamedArgument(prop.Name, CopyCustomAttributeArgument(prop.Argument)));

            return ca;
        }

        private CustomAttributeArgument CopyCustomAttributeArgument(CustomAttributeArgument arg)
        {
            var val = arg.Value;

            if (val is TypeReference)
                val = CopyReference((TypeReference)arg.Value);

            return new CustomAttributeArgument(CopyReference(arg.Type), val);
        }

        private TypeSpecification CopyTypeSpecification(TypeSpecification typeSpec)
        {
            TypeSpecification result;

            if (typeSpec is GenericInstanceType)
                result = new GenericInstanceType(CopyReference(typeSpec.ElementType));
            else if (typeSpec is ArrayType)
                result = new ArrayType(CopyReference(typeSpec.ElementType), ((ArrayType)typeSpec).Rank);
            else if (typeSpec is ByReferenceType)
                result = new ByReferenceType(CopyReference(typeSpec.ElementType));
            else throw new NotSupportedException();

            return result;
        }

        private T CopyMember<T>(T reference)
        {
            object result;

            if (reference is MemberReference)
            {
                var memberRef = (MemberReference)(object)reference;

                if (memberRef is GenericParameter)
                    result = CopyGenericParameter((GenericParameter)memberRef);
                else if (memberRef is TypeDefinition)
                    result = CopyType((TypeDefinition)GetMappedMember(memberRef.DeclaringType), (TypeDefinition)memberRef);
                else if (memberRef is MethodDefinition)
                    result = CopyMethod((TypeDefinition)GetMappedMember(memberRef.DeclaringType), (MethodDefinition)memberRef);
                else if (memberRef is FieldDefinition)
                    result = CopyField((TypeDefinition)GetMappedMember(memberRef.DeclaringType), (FieldDefinition)memberRef);
                else if (memberRef is PropertyDefinition)
                    result = CopyProperty((TypeDefinition)GetMappedMember(memberRef.DeclaringType), (PropertyDefinition)memberRef);
                else if (memberRef is EventDefinition)
                    result = CopyEvent((TypeDefinition)GetMappedMember(memberRef.DeclaringType), (EventDefinition)memberRef);
                else if (memberRef is TypeSpecification)
                    result = CopyTypeSpecification((TypeSpecification)memberRef);
                else throw new NotSupportedException();
            }
            else if (reference is ParameterDefinition)
                result = CopyParameterDefinition((ParameterDefinition)(object)reference);
            else throw new NotSupportedException();

            if (result != null)
                CopyCommonStuff(reference, result);

            return (T)result;
        }

        private void CopyCommonStuff(object source, object target)
        {
            if (source is IGenericParameterProvider)
                ((IGenericParameterProvider)source).GenericParameters.Select(gp => CopyReference(gp)).ToList()
                    .ForEach(gp => ((IGenericParameterProvider)target).GenericParameters.Add(gp));

            if (source is IGenericInstance)
                ((IGenericInstance)source).GenericArguments.Select(gp => CopyReference(gp)).ToList()
                    .ForEach(ca => ((IGenericInstance)target).GenericArguments.Add(ca));

            if (source is Mono.Cecil.ICustomAttributeProvider)
                ((Mono.Cecil.ICustomAttributeProvider)source).CustomAttributes.Select(gp => CopyCustomAttribute(gp)).ToList()
                    .ForEach(ca => ((Mono.Cecil.ICustomAttributeProvider)target).CustomAttributes.Add(ca));

            if (source is IConstantProvider && ((IConstantProvider)source).HasConstant)
                ((IConstantProvider)target).Constant = ((IConstantProvider)source).Constant;
        }

        private T GetMappedMember<T>(T reference) where T : MemberReference
        {
            if (reference == null)
                return null;

            MemberReference resolvedRef;

            if (reference is GenericParameter || reference is TypeSpecification)
                resolvedRef = reference;
            else
                resolvedRef = ((dynamic)reference).Resolve();

            if (resolvedRef == null || (!(resolvedRef is TypeSpecification) && resolvedRef.Module.Assembly != _snippetsAssembly))
                return reference;

            object existingType;
            if (_refsMap.TryGetValue(resolvedRef, out existingType))
                return (T)existingType;

            var member = CopyMember(resolvedRef);
            return (T)member;
        }

        private T CopyReference<T>(T reference)
            where T : MemberReference
        {
            var memberRef = GetMappedMember(reference);
            var memberRefType = reference.GetType();

            return (T)_module.GetType().GetMethod("Import", new[] { memberRefType }).Invoke(_module, new[] { memberRef });
        }
    }
}