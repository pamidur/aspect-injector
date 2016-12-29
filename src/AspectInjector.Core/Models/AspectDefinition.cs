using AspectInjector.Broker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector.Core.Models
{
    public class AspectDefinition
    {
        public List<Effect> Effects { get; set; }
        public AspectUsage Usage { get; set; }
    }
}