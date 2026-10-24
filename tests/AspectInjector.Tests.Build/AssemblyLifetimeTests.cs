using AspectInjector.Rules;
using FluentIL;
using FluentIL.Common;
using FluentIL.Logging;
using FluentIL.Resolvers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace AspectInjector.Tests.Build
{
    public class AssemblyLifetimeTests : IDisposable
    {
        private readonly string _directory = Path.Combine(Path.GetTempPath(), "AspectInjectorTests", Guid.NewGuid().ToString("N"));

        /// <summary>
        /// Creates an isolated directory for assembly lifetime checks.
        /// </summary>
        public AssemblyLifetimeTests()
        {
            Directory.CreateDirectory(_directory);
        }

        /// <summary>
        /// Verifies file-handle cleanup across successful, skipped, and failed processing.
        /// </summary>
        [Theory]
        [InlineData(PatchResult.Unmodified, true)]
        [InlineData(PatchResult.Modified, true)]
        [InlineData(PatchResult.LoggedError, true)]
        [InlineData(PatchResult.PatchThrows, true)]
        [InlineData(PatchResult.WriteThrows, true)]
        [InlineData(PatchResult.Unmodified, false)]
        [InlineData(PatchResult.Modified, false)]
        public void ProcessingReleasesTargetSymbolsAndDependencies(PatchResult result, bool withSymbols)
        {
            var target = CreateAssembly("Target", withSymbols);
            var dependency = CreateAssembly("Dependency");
            var logger = new TestLogger();
            var patcher = new TestPatcher(logger, result);

            var error = Record.Exception(() => patcher.Process(target, new[] { dependency }, false, false));

            if (result == PatchResult.PatchThrows || result == PatchResult.WriteThrows)
                Assert.NotNull(error);
            else
                Assert.Null(error);

            Assert.True(patcher.PatchCalled);
            Assert.Equal(result == PatchResult.LoggedError, logger.IsErrorThrown);
            AssertUnlocked(target);
            if (withSymbols)
                AssertUnlocked(Path.ChangeExtension(target, ".pdb"));
            AssertUnlocked(dependency);

            if (result == PatchResult.Modified)
            {
                using var assembly = AssemblyDefinition.ReadAssembly(target);
                Assert.Contains(assembly.MainModule.Types, type => type.Name == "AddedByPatcher");
            }
        }

        /// <summary>
        /// Verifies that a borrowed resolver remains usable without retaining the processed target.
        /// </summary>
        [Fact]
        public void BorrowedResolverRemainsUsableAndDoesNotKeepTheDisposedTarget()
        {
            var target = CreateAssembly("Target");
            var dependency = CreateAssembly("Dependency");
            using var resolver = new TrackingResolver();
            resolver.AddSearchDirectory(_directory);
            resolver.AddReference(dependency);
            var patcher = new TestPatcher(new TestLogger(), PatchResult.Unmodified);

            patcher.Process(target, resolver, false, false);

            Assert.Equal(0, resolver.DisposeCount);
            AssertUnlocked(target);
            var reopened = resolver.Resolve(new AssemblyNameReference("Target", new Version(1, 0, 0, 0)));
            Assert.NotSame(patcher.Target, reopened);

            resolver.Dispose();
            AssertUnlocked(target);
            AssertUnlocked(dependency);
        }

        /// <summary>
        /// Verifies that repeated disposal releases cached assemblies and symbols.
        /// </summary>
        [Fact]
        public void ResolverDisposalClosesCachedAssembliesAndCanBeRepeated()
        {
            var dependency = CreateAssembly("Dependency");
            var resolver = new KnownReferencesAssemblyResolver();
            resolver.AddReference(dependency);
            resolver.Resolve(new AssemblyNameReference("Dependency", new Version(1, 0, 0, 0)), new ReaderParameters { ReadSymbols = true });

            resolver.Dispose();
            resolver.Dispose();

            AssertUnlocked(dependency);
            AssertUnlocked(Path.ChangeExtension(dependency, ".pdb"));
        }

        /// <summary>
        /// Checks that resolver reuse observes replacement targets instead of stale metadata.
        /// </summary>
        [Theory]
        [InlineData(PatchResult.Unmodified, false)]
        [InlineData(PatchResult.Modified, false)]
        [InlineData(PatchResult.Unmodified, true)]
        [InlineData(PatchResult.Modified, true)]
        public void BorrowedCachingResolversCanBeReusedAcrossProcessingOperations(PatchResult result, bool cacheParameterizedReads)
        {
            var target = CreateAssembly("Target");
            var dependency = CreateAssembly("Dependency");
            using var resolver = cacheParameterizedReads ? new ExternalCachingResolver() : new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(_directory);
            var patcher = new TestPatcher(new TestLogger(), result);

            patcher.Process(target, resolver, false, false);
            var first = patcher.Target;
            AssertUnlocked(target);
            // Replace the input between operations to detect stale cached metadata too.
            CreateAssembly("Target");

            patcher.Process(target, resolver, false, false);

            Assert.NotSame(first, patcher.Target);
            Assert.NotEqual(first.MainModule.Mvid, patcher.Target.MainModule.Mvid);
            AssertUnlocked(target);
            AssertUnlocked(Path.ChangeExtension(target, ".pdb"));
            resolver.Dispose();
            AssertUnlocked(dependency);
        }

        /// <summary>
        /// Checks that validation errors still dispose an operation-owned resolver.
        /// </summary>
        [Fact]
        public void OwnedResolverIsDisposedBeforeEarlyReturn()
        {
            var target = CreateAssembly("Target");
            var resolver = new TrackingResolver();
            var logger = new TestLogger { IsErrorThrown = true };
            var patcher = new TestPatcher(logger, PatchResult.Unmodified, resolver);

            patcher.Process(target, Array.Empty<string>(), false, false);

            Assert.Equal(1, resolver.DisposeCount);
            Assert.False(patcher.PatchCalled);
        }

        /// <summary>
        /// Creates a minimal fixture assembly with optional portable symbols.
        /// </summary>
        private string CreateAssembly(string name, bool withSymbols = true)
        {
            var path = Path.Combine(_directory, name + ".dll");
            using var assembly = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition(name, new Version(1, 0, 0, 0)), name, ModuleKind.Dll);
            var type = new TypeDefinition("Tests", "Target", TypeAttributes.Public, assembly.MainModule.TypeSystem.Object);
            var method = new MethodDefinition("Run", MethodAttributes.Public | MethodAttributes.Static, assembly.MainModule.TypeSystem.Void);
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            method.DebugInformation.SequencePoints.Add(new SequencePoint(method.Body.Instructions[0], new Document(name + ".cs"))
            {
                StartLine = 1, EndLine = 1, StartColumn = 1, EndColumn = 2
            });
            type.Methods.Add(method);
            assembly.MainModule.Types.Add(type);
            assembly.Write(path, new WriterParameters { WriteSymbols = withSymbols, SymbolWriterProvider = new PortablePdbWriterProvider() });
            return path;
        }

        /// <summary>
        /// Requires exclusive read/write access to the fixture file.
        /// </summary>
        private static void AssertUnlocked(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            Assert.True(stream.CanWrite);
        }

        /// <summary>
        /// Removes generated assembly fixtures after each test.
        /// </summary>
        public void Dispose()
        {
            Directory.Delete(_directory, true);
        }

        public enum PatchResult { Unmodified, Modified, LoggedError, PatchThrows, WriteThrows }

        private sealed class TestPatcher : PatcherBase
        {
            private readonly PatchResult _result;
            private readonly IAssemblyResolver _resolver;
            public bool PatchCalled { get; private set; }
            public AssemblyDefinition Target { get; private set; }

            /// <summary>
            /// Configures the processing outcome and optional caller-owned resolver.
            /// </summary>
            public TestPatcher(ILogger logger, PatchResult result, IAssemblyResolver resolver = null) : base(logger)
            {
                _result = result;
                _resolver = resolver;
            }

            protected override Rule GenericInfoRule => GeneralRules.Info;
            protected override Rule GenericErrorRule => GeneralRules.CompilationMustSecceedIfNoOtherErrors;
            /// <summary>
            /// Uses the supplied resolver when testing ownership, otherwise creates one.
            /// </summary>
            protected override IAssemblyResolver GetResolver(string path, IReadOnlyList<string> references)
                => _resolver ?? base.GetResolver(path, references);

            /// <summary>
            /// Exercises target identity and the configured processing outcome.
            /// </summary>
            protected override bool PatchAssembly(AssemblyDefinition assembly, bool optimize, bool verbose)
            {
                PatchCalled = true;
                Target = assembly;
                Assert.Same(assembly, assembly.MainModule.AssemblyResolver.Resolve(assembly.Name));
                assembly.MainModule.AssemblyResolver.Resolve(new AssemblyNameReference("Dependency", new Version(1, 0, 0, 0)));
                if (_result == PatchResult.PatchThrows)
                    throw new InvalidOperationException("Patch failed");
                if (_result == PatchResult.LoggedError)
                    _log.Log(GeneralRules.CompilationMustSecceedIfNoOtherErrors, null, "Patch failed");
                if (_result == PatchResult.Modified)
                    assembly.MainModule.Types.Add(new TypeDefinition("Tests", "AddedByPatcher", TypeAttributes.Public, assembly.MainModule.TypeSystem.Object));
                if (_result == PatchResult.WriteThrows)
                {
                    var instruction = Instruction.Create(OpCodes.Nop);
                    instruction.OpCode = OpCodes.Call;
                    assembly.MainModule.Types.Single(t => t.Name == "Target").Methods[0].Body.Instructions.Insert(0, instruction);
                }
                return _result == PatchResult.Modified || _result == PatchResult.WriteThrows;
            }
        }

        private sealed class TrackingResolver : KnownReferencesAssemblyResolver
        {
            public int DisposeCount { get; private set; }
            /// <summary>
            /// Counts disposal calls before releasing cached dependencies.
            /// </summary>
            protected override void Dispose(bool disposing)
            {
                DisposeCount++;
                base.Dispose(disposing);
            }
        }

        // Models a caller-owned resolver that caches both IAssemblyResolver overloads.
        private sealed class ExternalCachingResolver : DefaultAssemblyResolver
        {
            private readonly Dictionary<string, AssemblyDefinition> _cache = new Dictionary<string, AssemblyDefinition>();

            /// <summary>
            /// Routes default lookups through the simulated external cache.
            /// </summary>
            public override AssemblyDefinition Resolve(AssemblyNameReference name)
                => Resolve(name, new ReaderParameters());

            /// <summary>
            /// Caches parameterized lookups to model a caller-owned external resolver.
            /// </summary>
            public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
            {
                if (!_cache.TryGetValue(name.FullName, out var assembly))
                    _cache[name.FullName] = assembly = base.Resolve(name, parameters);
                return assembly;
            }

            /// <summary>
            /// Releases assemblies retained by the simulated external cache.
            /// </summary>
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    foreach (var assembly in _cache.Values)
                        assembly.Dispose();
                    _cache.Clear();
                }
                base.Dispose(disposing);
            }
        }

        private sealed class TestLogger : ILogger
        {
            public bool IsErrorThrown { get; set; }
            /// <summary>
            /// Records error diagnostics without emitting test output.
            /// </summary>
            public void Log(Rule rule, SequencePoint point, params string[] messages)
            {
                if (rule.Severity == RuleSeverity.Error)
                    IsErrorThrown = true;
            }
        }
    }
}
