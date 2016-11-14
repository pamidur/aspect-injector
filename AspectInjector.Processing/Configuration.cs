using AspectInjector.Contracts;
using AspectInjector.Defaults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspectInjector
{
    public interface IRegisterAdviceType<T>
        where T : IAdvice
    {
    }

    public class Configuration
    {
        public ILogger Log { get; set; } = new DefaultLogger();

        public ICollection<Type> Processors { get; set; } = new List<Type>();

        public void RegisterAdviceType<T>()
    }
}