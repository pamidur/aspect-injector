using AspectInjector.Broker;
using static AspectInjector.Broker.AspectBase;

namespace AspectInjector.Core.Models
{
    internal class ChildrenFilter
    {
        public string NameFilter { get; set; }
        public AccessModifier AccessModifierFilter { get; set; }
    }
}