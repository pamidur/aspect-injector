using AspectInjector.BuildTask.Contracts;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AspectInjector.BuildTask.Processors.ModuleProcessors
{
    class SnippetsProcessor : IModuleProcessor
    {
        private static readonly string _namespace = "__$_aspect_injector_namespaces";
        private static readonly string _snippetsFilename = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "AspectInjector.Snippets.dll");

        private ModuleDefinition _module;
        private AssemblyDefinition _snippetsAssembly;

        private Dictionary<MemberReference, MemberReference> _refsMap;


        public SnippetsProcessor()
        {
            _snippetsAssembly = AssemblyDefinition.ReadAssembly(_snippetsFilename,
                new ReaderParameters
                {
                    ReadingMode = Mono.Cecil.ReadingMode.Deferred,
                });
        }


        public void ProcessModule(ModuleDefinition module)
        {
            _module = module;
            _refsMap = new Dictionary<MemberReference, MemberReference>();

            var snippets = _snippetsAssembly.MainModule.Types.Where(t => t.Name != "<Module>").ToList();
            foreach (var type in snippets)
            {
                GetMappedMember(type);

                foreach (var field in type.Fields)
                    GetMappedMember(field);

                foreach (var method in type.Methods)
                    GetMappedMember(method);
            }
        }

        public TypeDefinition MigrateType(TypeDefinition destination, TypeDefinition type)
        {
            var newtype = new TypeDefinition(_namespace, type.Name, type.Attributes, MigrateReference(type.BaseType));

            if (destination == null)
                _module.Types.Add(newtype);
            else
                destination.NestedTypes.Add(newtype);

            return newtype;
        }

        private FieldDefinition MigrateField(TypeDefinition newtype, FieldDefinition field)
        {
            var newField = new FieldDefinition(field.Name, field.Attributes, MigrateReference(field.FieldType));
            newtype.Fields.Add(newField);
            return newField;
        }

        private MethodDefinition MigrateMethod(TypeDefinition newtype, MethodDefinition method)
        {


            var newMethod = new MethodDefinition(method.Name, method.Attributes, method.GenericParameters.Any(gp=>gp.) MigrateReference(method.ReturnType));  
            newtype.Methods.Add(newMethod);
            return newMethod;
        }

        //private void MigrateCustomAttributes(IMemberDefinition member, IEnumerable<CustomAttribute> attributes)
        //{
        //    reference

        //    foreach(var attribute in attributes)
        //    {
        //        var newAttr = new CustomAttribute(attribute.)
        //    }

        //    member.CustomAttributes.Add()
        //}  

        private T CreateMappedMember<T>(T reference) where T : MemberReference
        {
            if (reference is TypeReference)
                return (T)(object)MigrateType((TypeDefinition)GetMappedMember(reference.DeclaringType), (TypeDefinition)(object)reference);

            if (reference is MethodReference)
                return (T)(object)MigrateMethod((TypeDefinition)GetMappedMember(reference.DeclaringType), (MethodDefinition)(object)reference);

            if (reference is FieldReference)
                return (T)(object)MigrateField((TypeDefinition)GetMappedMember(reference.DeclaringType), (FieldDefinition)(object)reference);

            throw new NotSupportedException();
        }

        private T GetMappedMember<T>(T reference) where T : MemberReference
        {
            if (reference == null)
                return null;

            MemberReference resolvedRef = ((dynamic)reference).Resolve();

            if (resolvedRef == null || resolvedRef.Module.Assembly != _snippetsAssembly)
                return reference;

            MemberReference existingType;
            if (_refsMap.TryGetValue(resolvedRef, out existingType))
                return (T)existingType;

            var member = CreateMappedMember(resolvedRef);
            _refsMap.Add(resolvedRef, member);
            return (T)member;
        }

        private T MigrateReference<T>(T reference)
            where T : MemberReference
        {
            var memberRef = GetMappedMember(reference);
            var memberRefType = typeof(T);

            return (T)(object)((dynamic)_module).Import(memberRef);

            //return (T)(object) _module.GetType().GetMethod("Import", new[] { memberRefType }).Invoke(_module, new[] { memberRef });
        }
    }
}
