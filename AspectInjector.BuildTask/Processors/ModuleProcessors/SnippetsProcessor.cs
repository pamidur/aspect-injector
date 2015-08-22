using AspectInjector.BuildTask.Contracts;
using AspectInjector.BuildTask.Extensions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AspectInjector.BuildTask.Processors.ModuleProcessors
{
    internal class SnippetsProcessor : IModuleProcessor
    {
        public static readonly string SnippetsNamespace = "__$_aspect_injector_namespaces";
        private static readonly string SnippetsFilename = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "AspectInjector.Snippets.dll");

        private ModuleDefinition _module;
        private AssemblyDefinition _snippetsAssembly;

        private Dictionary<object, object> _refsMap;

        public SnippetsProcessor()
        {
            _snippetsAssembly = AssemblyDefinition.ReadAssembly(SnippetsFilename,
                new ReaderParameters
                {
                    ReadingMode = Mono.Cecil.ReadingMode.Deferred,
                });
        }

        public void ProcessModule(ModuleDefinition module)
        {
            _module = module;
            _refsMap = new Dictionary<object, object>();

            var snippets = _snippetsAssembly.MainModule.Types.Where(t => t.Name != "<Module>").ToList();
            foreach (var type in snippets)
            {
                GetMappedMember(type);

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
        }

        public TypeDefinition CopyType(TypeDefinition destination, TypeDefinition type)
        {
            var newtype = new TypeDefinition(SnippetsNamespace, type.Name, type.Attributes, CopyReference(type.BaseType));
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
            var newEvent = new EventDefinition(oldEvent.Name, oldEvent.Attributes, CopyReference(oldEvent.EventType));

            newEvent.AddMethod = CopyReference(oldEvent.AddMethod);
            newEvent.RemoveMethod = CopyReference(oldEvent.RemoveMethod);

            _refsMap.Add(oldEvent, newEvent);
            target.Events.Add(newEvent);
            return newEvent;
        }

        private PropertyDefinition CopyProperty(TypeDefinition target, PropertyDefinition prop)
        {
            var newProp = new PropertyDefinition(prop.Name, prop.Attributes, CopyReference(prop.PropertyType));

            newProp.GetMethod = CopyReference(prop.GetMethod);
            newProp.SetMethod = CopyReference(prop.SetMethod);

            _refsMap.Add(prop, newProp);
            target.Properties.Add(newProp);
            return newProp;
        }

        private ParameterDefinition CopyParameterDefinition(ParameterDefinition parmd)
        {
            var npar = new ParameterDefinition(parmd.Name, parmd.Attributes, CopyReference(parmd.ParameterType));
            return npar;
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
                    var ni = ilp.Create(i.OpCode, parMap[(ParameterDefinition)i.Operand]);
                    ilp.SafeReplace(i, ni);
                }

                if (i.Operand is VariableDefinition)
                {
                    var ni = ilp.Create(i.OpCode, varMap[(VariableDefinition)i.Operand]);
                    ilp.SafeReplace(i, ni);
                }

                if (i.Operand is Instruction)
                {
                    var ni = ilp.Create(i.OpCode, instMap[(Instruction)i.Operand]);
                    ilp.SafeReplace(i, ni);
                }

                if (i.Operand is Instruction[])
                {
                    var ni = ilp.Create(i.OpCode, ((Instruction[])i.Operand).Select(ii => instMap[ii]).ToArray());
                    ilp.SafeReplace(i, ni);
                }
            }

            foreach (var h in method.Body.ExceptionHandlers)
            {
                var nh = new ExceptionHandler(h.HandlerType);
                nh.CatchType = h.CatchType == null ? null : CopyReference(h.CatchType);
                nh.FilterStart = h.FilterStart == null ? null : instMap[h.FilterStart];
                nh.HandlerEnd = h.HandlerEnd == null ? null : instMap[h.HandlerEnd];
                nh.HandlerStart = h.HandlerStart == null ? null : instMap[h.HandlerStart];
                nh.TryEnd = h.TryEnd == null ? null : instMap[h.TryEnd];
                nh.TryStart = h.TryStart == null ? null : instMap[h.TryStart];

                newMethod.Body.ExceptionHandlers.Add(nh);
            }

            return newMethod;
        }

        private Instruction CopyInstruction(ILProcessor ilp, Instruction inst)
        {
            var opcode = inst.OpCode;
            var operand = inst.Operand;

            if (operand is MemberReference)
                operand = CopyReference((MemberReference)operand);

            var argTypes = new Type[] { typeof(OpCode) };
            if (operand != null)
                argTypes = argTypes.Concat(new Type[] { operand.GetType() }).ToArray();

            var args = new object[] { opcode };
            if (operand != null)
                args = args.Concat(new object[] { operand }).ToArray();

            var factory = ilp.GetType().GetMethod("Create", argTypes);
            var ni = (Instruction)factory.Invoke(ilp, args);

            return ni;
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
            return new CustomAttribute(CopyReference(attribute.Constructor), attribute.GetBlob());
        }

        private TypeSpecification CopyTypeSpecification(TypeSpecification typeSpec)
        {
            TypeSpecification result;

            if (typeSpec is GenericInstanceType)
                result = new GenericInstanceType(CopyReference(typeSpec.ElementType));
            else if (typeSpec is ArrayType)
                result = new ArrayType(CopyReference(typeSpec.ElementType), ((ArrayType)typeSpec).Rank);
            else throw new NotSupportedException();

            return result;
        }

        private T CopyMember<T>(T reference)
        {
            object result = null;

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

            MemberReference resolvedRef = null;

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

            return (T)(object)_module.GetType().GetMethod("Import", new[] { memberRefType }).Invoke(_module, new[] { memberRef });
        }
    }
}