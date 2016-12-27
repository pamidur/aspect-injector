using Mono.Cecil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AspectInjector.Core.Models
{
    public class FQN : IEquatable<FQN>, IEquatable<string>
    {
        private static readonly Regex _fqnMatch = new Regex(@"^([^\,\[]+)(\[.+\])?,\s?(.+)$", RegexOptions.Compiled);

        [JsonProperty("asm")]
        public string AssemblyName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("ns")]
        public string Namespace { get; set; }

        [JsonProperty("arg")]
        public List<FQN> Arguments { get; set; }

        public bool Equals(FQN other)
        {
            return object.Equals(other, null) ? false : ToString() == other.ToString();
        }

        public bool Equals(string other)
        {
            return other == null ? false : ToString() == other;
        }

        public override bool Equals(object obj)
        {
            if (obj is string)
                return Equals((string)obj);
            if (obj is FQN)
                return Equals((FQN)obj);

            return false;
        }

        public override int GetHashCode()
        {
            return AssemblyName.GetHashCode();
        }

        public override string ToString()
        {
            var fqn = $"{Namespace}.{Name}";

            if (Arguments != null && Arguments.Any())
                fqn = $"{fqn}[{string.Join(",", Arguments.Select(a => $"[{a.ToString()}]"))}]";

            fqn = $"{fqn}, {AssemblyName}";

            return fqn;
        }

        public TypeReference ToTypeReference(IAssemblyResolver resolver, ModuleDefinition reference)
        {
            var asm = resolver.Resolve(AssemblyName);

            var nestedPath = Name.Split(new[] { '+' });

            var maintype = asm.Modules.SelectMany(m => m.Types).FirstOrDefault(t => t.Name == nestedPath[0]);

            if (maintype == null)
                throw new Exception($"Cannot find type {nestedPath[0]}.");

            TypeReference exactType = maintype;

            foreach (var name in nestedPath.Skip(1))
            {
                TypeReference newRef = null;

                if (exactType.IsDefinition)
                    newRef = ((TypeDefinition)exactType).NestedTypes.FirstOrDefault(t => t.Name == name);

                if (newRef == null && exactType.HasGenericParameters)
                    newRef = exactType.GenericParameters.FirstOrDefault(g => g.Name == name);

                if (newRef == null)
                    throw new Exception($"Cannot find type {name}.");

                exactType = newRef;
            }

            var tr = new TypeReference(exactType.Namespace ?? "", exactType.Name, reference, exactType.Scope);

            tr.DeclaringType = exactType.DeclaringType;

            if (Arguments != null && Arguments.Any())
            {
                var args = Arguments.Select(a => a.ToTypeReference(resolver, reference));

                var gps = args.Where(a => a is GenericParameter).ToList();
                var gas = args.Except(gps).ToList();

                if (gas.Any())
                {
                    var gtr = new GenericInstanceType(tr);
                    gas.ForEach(ga => gtr.GenericArguments.Add(ga));
                    tr = gtr;
                }

                gps.Cast<GenericParameter>().ToList().ForEach(gp => tr.GenericParameters.Add(gp));
                //gps.Cast<GenericParameter>().ToList().ForEach(gp => tr.GenericParameters.Add(new GenericParameter(gp.Name, tr)));
            }

            return tr;
        }

        public static FQN FromString(string fqnString)
        {
            if (string.IsNullOrEmpty(fqnString))
                return null;

            var match = _fqnMatch.Match(fqnString);

            if (!match.Success)
                throw new Exception("Cannot parse FQN string");

            var fullname = match.Groups[1].Value;
            var parameters = match.Groups[2].Value;
            var assemblyName = match.Groups[3].Value;

            var nsSeparator = match.Groups[1].Value.LastIndexOf('.');

            var fqn = new FQN
            {
                Name = fullname.Substring(nsSeparator + 1),
                Namespace = fullname.Substring(0, nsSeparator),
                AssemblyName = assemblyName
            };

            if (!string.IsNullOrWhiteSpace(parameters))
            {
                var paramStrings = new List<string>();

                parameters = parameters.Substring(1, parameters.Length - 2);

                var stack = new Stack<char>();

                int prs = 0;

                foreach (char c in parameters)
                {
                    if (!((c == ',' || c == ' ' || c == '[') && prs == 0) && !(c == ']' && prs == 1))
                        stack.Push(c);

                    if (c == '[') prs++;
                    else if (c == ']')
                    {
                        prs--;
                        if (prs == 0)
                        {
                            paramStrings.Add(new string(stack.ToArray().Reverse().ToArray()));
                            stack.Clear();
                        }
                    }
                }

                if (prs != 0)
                    throw new Exception("Cannot parse FQN string");

                fqn.Arguments = paramStrings.Select(s => FromString(s)).ToList();
            }

            return fqn;
        }

        public static FQN FromType(Type type)
        {
            var fqn = new FQN
            {
                Name = type.Name
            };

            if (type.IsGenericType)
                fqn.Arguments = type.GetGenericArguments().Select(g => FromType(g)).ToList();

            while (type.IsNested)
            {
                type = type.DeclaringType;
                fqn.Name = $"{type.Name}+{fqn.Name}";
            }

            fqn.Namespace = type.Namespace;
            fqn.AssemblyName = type.Assembly.GetName().FullName;

            return fqn;
        }

        public static FQN FromTypeReference(TypeReference type)
        {
            if (type == null)
                return null;

            var fqn = new FQN
            {
                Name = type.Name
            };

            if (type.IsGenericInstance)
                fqn.Arguments = ((GenericInstanceType)type).GenericArguments.Select(g => FromTypeReference(g)).ToList();
            else if (type.HasGenericParameters)
                fqn.Arguments = type.GenericParameters.Select(g => FromTypeReference(g)).ToList();

            if (type.IsGenericInstance && type.HasGenericParameters)
                throw new Exception("Check this out!");

            while (type.IsNested)
            {
                type = type.DeclaringType;
                fqn.Name = $"{type.Name}+{fqn.Name}";
            }

            fqn.Namespace = type.Namespace;
            fqn.AssemblyName = type.Resolve().Module.Assembly.Name.FullName;

            return fqn;
        }

        public static bool operator ==(FQN a, FQN b)
        {
            if (object.Equals(a, null) || object.Equals(b, null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(FQN a, FQN b)
        {
            return !(a == b);
        }
    }
}