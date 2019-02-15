//using AspectInjector.Tests.Assets;
//using ILVerify;
//using System;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Reflection.PortableExecutable;
//using Xunit;

//namespace AspectInjector.Tests.General
//{
//    public class Resolver : ResolverBase
//    {
//        public Resolver()
//        {
//            LocalDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//            SystemDir = Path.GetDirectoryName(typeof(string).Assembly.Location);

//            References = AppDomain.CurrentDomain.GetAssemblies().Where(a=>!a.IsDynamic).ToArray();
//        }
//        public string LocalDir { get; }
//        public string SystemDir { get; }
//        public Assembly[] References { get; }

//        protected override PEReader ResolveCore(string simpleName)
//        {
//            if (!simpleName.EndsWith(".dll")) simpleName += ".dll";

//            var path = References.FirstOrDefault(r => Path.GetFileName(r.Location) == simpleName)?.Location;

//            if (!File.Exists(path)) path = Path.Combine(LocalDir, simpleName);
//            if (!File.Exists(path)) path = Path.Combine(SystemDir, simpleName);
//            if (!File.Exists(path)) return null;

//            var fm = new FileStream(path, FileMode.Open, FileAccess.Read);
//            return new PEReader(fm);
//        }
//    }

//    public class PETest
//    {
//        private readonly Resolver _resolver;
//        private readonly Verifier _verifier;
//        private readonly AssemblyName _systemModule = new AssemblyName("mscorlib");

//        public PETest()
//        {
//            _resolver = new Resolver();
//            _verifier = new Verifier(_resolver);
//            _verifier.SetSystemModuleName(_systemModule);
//        }

//        [Fact]
//        public void IL_Runtime_Is_CorrectGeneral()
//        {
//            var assembly = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
//            var result = _verifier.Verify(_resolver.Resolve(assembly)).ToArray();
//            Assert.Empty(result);
//        }

//        [Fact]
//        public void IL_RuntimeAssets_Is_CorrectGeneral()
//        {
//            var assembly = Path.GetFileName(typeof(TestLog).Assembly.Location);
//            var result = _verifier.Verify(_resolver.Resolve(assembly)).ToArray();
//            Assert.Empty(result);
//        }
//    }
//}