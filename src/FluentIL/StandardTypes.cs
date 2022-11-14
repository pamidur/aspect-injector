using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluentIL
{
    public class StandardType
    {
        public static readonly StandardType MethodBase = new StandardType("System.Reflection", "MethodBase");
        public static readonly StandardType Attribute = new StandardType("System", "Attribute");
        public static readonly StandardType Type = new StandardType("System", "Type");
        public static readonly StandardType RuntimeTypeHandle = new StandardType("System", "RuntimeTypeHandle", isValueType: true);
        public static readonly StandardType RuntimeMethodHandle = new StandardType("System", "RuntimeMethodHandle", isValueType: true);
        public static readonly StandardType AsyncStateMachineAttribute = new StandardType("System.Runtime.CompilerServices", "AsyncStateMachineAttribute");
        public static readonly StandardType IteratorStateMachineAttribute = new StandardType("System.Runtime.CompilerServices", "IteratorStateMachineAttribute");
        public StandardType(string @namespace, string name, 
            bool isValueType = false, bool isArray = false, 
            IReadOnlyList<StandardType> elements = null,
            IReadOnlyList<string> assemblyHints = null
            )
        {
            Namespace = @namespace;
            Name = name;
            IsValueType = isValueType;
            IsArray = isArray;
            Elements = elements ?? new StandardType[] { };
            AssemblyHints = assemblyHints ?? new string[] { };
        }

        public string Namespace { get; }
        public string Name { get; }
        public bool IsValueType { get; }
        public bool IsArray { get; }
        public IReadOnlyList<StandardType> Elements { get; }
        public IReadOnlyList<string> AssemblyHints { get; }

        public override string ToString()
        {
            var fullname = new StringBuilder().Append(Namespace).Append(".").Append(Name);
            if (Elements.Count > 0)
            {
                fullname.Append("<");
                var last = Elements.Last();
                foreach (var element in Elements)
                {
                    fullname.Append(element.ToString());
                    if (element != last)
                        fullname.Append(",");
                }
                fullname.Append(">");
            }
            if (IsArray)
                fullname.Append("[]");

            return fullname.ToString();
        }

        public StandardType MakeArray()
        {
            return new StandardType(Namespace, Name, IsValueType, true, Elements, AssemblyHints);
        }
    }   
}