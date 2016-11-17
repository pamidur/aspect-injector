using Mono.Cecil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectInjector.Core.Models
{
    public class FQN : IEquatable<FQN>, IEquatable<string>
    {
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
            return other == null ? false : ToString() == other.ToString();
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
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            var fqn = $"{Namespace}.{Name}";

            if (Arguments != null && Arguments.Any())
                fqn = $"{fqn}[{string.Join(",", Arguments.Select(a => $"[{a.ToString()}]"))}]";

            fqn = $"{fqn}, {AssemblyName}";

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
                var sep = type.IsGenericParameter ? '*' : '+';
                fqn.Name = $"{type.Name}{sep}{fqn.Name}";
            }

            fqn.Namespace = type.Namespace;
            fqn.AssemblyName = type.Assembly.GetName().Name;

            return fqn;
        }

        public static FQN FromTypeReference(TypeReference type)
        {
            var fqn = new FQN
            {
                Name = type.Name
            };

            var args = ((GenericInstanceType)type).GenericArguments.Select(g => FromTypeReference(g));

            if (type.IsGenericInstance)
                fqn.Arguments = ((GenericInstanceType)type).GenericArguments.Select(g => FromTypeReference(g)).ToList();

            while (type.IsNested)
            {
                type = type.DeclaringType;
                fqn.Name = $"{type.Name}+{fqn.Name}";
            }

            fqn.Namespace = type.Namespace;
            fqn.AssemblyName = type.Resolve().Module.Assembly.Name.Name;

            return fqn;
        }
    }
}