using AspectInjector.Broker;
using Mono.Cecil;
using System;

namespace AspectInjector.BuildTask.Models
{
    internal class AspectDefinition : IEquatable<AspectDefinition>
    {
        public TypeDefinition AdviceClassType { get; set; }

        public string NameFilter { get; set; }

        public AccessModifiers AccessModifierFilter { get; set; }

        public object RoutableData { get; set; }

        public bool Equals(AspectDefinition other)
        {
            return
                this.AdviceClassType.Equals(other.AdviceClassType) &&
                this.NameFilter.Equals(other.NameFilter) &&
                this.AccessModifierFilter.Equals(other.AccessModifierFilter) &&
                this.RoutableData.Equals(other.RoutableData);
        }

        public override bool Equals(object obj)
        {
            return obj is AspectDefinition ? Equals((AspectDefinition)obj) : false;
        }

        public override int GetHashCode()
        {
            return
                this.AdviceClassType.GetHashCode() +
                (this.NameFilter == null ? 0 : this.NameFilter.GetHashCode()) +
                this.AccessModifierFilter.GetHashCode() +
                (this.RoutableData == null ? 0 : this.RoutableData.GetHashCode());
        }
    }
}
