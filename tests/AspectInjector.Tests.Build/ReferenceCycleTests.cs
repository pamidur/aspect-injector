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
    public sealed class ReferenceCycleTests : IDisposable
    {
        private readonly string _directory = Path.Combine(Path.GetTempPath(), "AspectInjectorCycleTests", Guid.NewGuid().ToString("N"));
        private string TargetPath => Path.Combine(_directory, "Target.dll");
        private string DependencyPath => Path.Combine(_directory, "Dependency.dll");

        /// <summary>
        /// Creates an isolated directory for reference-cycle fixtures.
        /// </summary>
        public ReferenceCycleTests() => Directory.CreateDirectory(_directory);

        /// <summary>
        /// Verifies that dependency back-references reuse the active target on every exit path.
        /// </summary>
        [Theory]
        [InlineData(false, Outcome.Unmodified)]
        [InlineData(false, Outcome.Modified)]
        [InlineData(false, Outcome.Throws)]
        [InlineData(true, Outcome.Unmodified)]
        [InlineData(true, Outcome.Modified)]
        [InlineData(true, Outcome.Throws)]
        public void CycleResolvesTheActiveTargetWithoutOpeningItAgain(bool owned, Outcome outcome)
        {
            WriteFixture(1);
            using var resolver = CreateResolver();
            var patcher = new CyclePatcher(resolver, outcome);

            var error = Record.Exception(() => Process(patcher, resolver, owned));

            if (outcome == Outcome.Throws)
                Assert.Equal("Expected patch failure", Assert.IsType<InvalidOperationException>(error).Message);
            else
                Assert.Null(error);
            Assert.NotNull(patcher.Root);
            Assert.Equal(0, resolver.TargetLookups);
            Assert.Equal(owned ? 1 : 0, resolver.DisposeCount);
            AssertUnlocked(TargetPath);
            if (owned)
                AssertUnlocked(DependencyPath);
            else
            {
                Assert.Same(patcher.Dependency, resolver.Resolve(Name("Dependency")));
                Assert.Throws<IOException>(() => AssertUnlocked(DependencyPath));
                // The temporary registration must be gone on success and on exceptions.
                Assert.NotSame(patcher.Target, resolver.Resolve(Name("Target")));
            }
            resolver.Dispose();
            AssertUnlocked(TargetPath);
            AssertUnlocked(DependencyPath);
            if (outcome == Outcome.Modified)
            {
                using var output = AssemblyDefinition.ReadAssembly(TargetPath);
                Assert.Contains(output.MainModule.Types, type => type.Name == "AddedByPatcher");
            }
        }

        /// <summary>
        /// Verifies that reused dependencies resolve the latest target after replacement.
        /// </summary>
        [Theory]
        [InlineData(Outcome.Unmodified, false)]
        [InlineData(Outcome.Modified, false)]
        [InlineData(Outcome.Throws, false)]
        [InlineData(Outcome.Unmodified, true)]
        [InlineData(Outcome.Modified, true)]
        [InlineData(Outcome.Throws, true)]
        public void BorrowedCycleDependenciesCanBePreloadedAndReusedWithReplacedTargets(Outcome outcome, bool preload)
        {
            WriteFixture(1);
            using var resolver = CreateResolver();
            AssemblyDefinition preloaded = null;
            if (preload)
            {
                preloaded = resolver.Resolve(Name("Dependency"));
                _ = preloaded.MainModule.MetadataResolver;
            }
            var first = new CyclePatcher(resolver, outcome);
            var firstError = Record.Exception(() => Process(first, resolver, owned: false));
            if (outcome == Outcome.Throws)
                Assert.IsType<InvalidOperationException>(firstError);
            else
                Assert.Null(firstError);
            AssertUnlocked(TargetPath);
            if (preload) Assert.Same(preloaded, first.Dependency);

            WriteFixture(2, writeDependency: false);
            var second = new CyclePatcher(resolver, Outcome.Modified);
            Process(second, resolver, owned: false);

            Assert.NotSame(first.Target, second.Target);
            Assert.NotEqual(first.Target.MainModule.Mvid, second.Target.MainModule.Mvid);
            Assert.Same(first.Dependency, second.Dependency);
            Assert.Equal(2, second.Root.Fields.Single(field => field.Name == "Generation").Constant);
            Assert.Equal(0, resolver.TargetLookups);
            Assert.Equal(0, resolver.DisposeCount);
            AssertUnlocked(TargetPath);
            Assert.NotSame(second.Target, resolver.Resolve(Name("Target")));
            resolver.Dispose();
            AssertUnlocked(TargetPath);
            AssertUnlocked(DependencyPath);
        }

        /// <summary>
        /// Checks that temporary registration preserves an existing caller-owned cache entry.
        /// </summary>
        [Fact]
        public void TargetRegistrationDoesNotReplaceAnExistingCallerCacheEntry()
        {
            WriteFixture(1);
            using var resolver = CreateResolver();
            // A caller-owned in-memory target should survive a temporary operation registration.
            var cached = resolver.Resolve(Name("Target"), new ReaderParameters { InMemory = true });
            WriteFixture(2, writeDependency: false);
            var patcher = new CyclePatcher(resolver, Outcome.Modified);

            Process(patcher, resolver, owned: false);

            Assert.Equal(2, patcher.Root.Fields.Single(field => field.Name == "Generation").Constant);
            Assert.Same(cached, resolver.Resolve(Name("Target")));
            Assert.NotSame(cached, patcher.Target);
            Assert.Equal(1, cached.MainModule.GetType("Test.Root").Fields[0].Constant);
            AssertUnlocked(TargetPath);
        }

        /// <summary>
        /// Checks that a failed nested operation restores the outer target registration.
        /// </summary>
        [Fact]
        public void NestedProcessingRestoresTheOuterTargetRegistration()
        {
            WriteFixture(1);
            using var resolver = CreateResolver();
            var nestedPath = Path.Combine(_directory, "NestedTarget.dll");
            File.Copy(TargetPath, nestedPath);
            var inner = new CyclePatcher(resolver, Outcome.Throws);
            var outer = new CyclePatcher(resolver, Outcome.Modified, target =>
            {
                var error = Record.Exception(() => inner.Process(nestedPath, resolver, false, false));
                Assert.Equal("Expected patch failure", Assert.IsType<InvalidOperationException>(error).Message);
                Assert.NotSame(target, inner.Target);
                var root = inner.Dependency.MainModule.GetType("Test.Middle").BaseType.Resolve();
                Assert.Same(target.MainModule.GetType("Test.Root"), root);
            });

            Process(outer, resolver, owned: false);

            Assert.Equal(0, resolver.TargetLookups);
            AssertUnlocked(TargetPath);
            AssertUnlocked(nestedPath);
            Assert.NotSame(outer.Target, resolver.Resolve(Name("Target")));
        }

        /// <summary>
        /// Creates a resolver that tracks unexpected target loads.
        /// </summary>
        private TrackingResolver CreateResolver()
        {
            var resolver = new TrackingResolver();
            resolver.AddSearchDirectory(_directory);
            resolver.AddReference(DependencyPath);
            return resolver;
        }

        /// <summary>
        /// Exercises either the resolver-owning or resolver-borrowing processing overload.
        /// </summary>
        private void Process(CyclePatcher patcher, TrackingResolver resolver, bool owned)
        {
            if (owned) patcher.Process(TargetPath, new[] { DependencyPath }, false, false);
            else patcher.Process(TargetPath, resolver, false, false);
        }

        /// <summary>
        /// Creates a valid inheritance chain spanning cyclic assembly references.
        /// </summary>
        private void WriteFixture(int generation, bool writeDependency = true)
        {
            using var target = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("Target", new Version(1, 0, 0, 0)), "Target", ModuleKind.Dll);
            using var dependency = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("Dependency", new Version(1, 0, 0, 0)), "Dependency", ModuleKind.Dll);
            var root = new TypeDefinition("Test", "Root", TypeAttributes.Public, target.MainModule.TypeSystem.Object);
            // Distinguish freshly loaded targets from stale cached metadata.
            root.Fields.Add(new FieldDefinition("Generation", FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault, target.MainModule.TypeSystem.Int32) { Constant = generation });
            target.MainModule.Types.Add(root);
            var middle = new TypeDefinition("Test", "Middle", TypeAttributes.Public, dependency.MainModule.ImportReference(root));
            dependency.MainModule.Types.Add(middle);
            target.MainModule.Types.Add(new TypeDefinition("Test", "Leaf", TypeAttributes.Public, target.MainModule.ImportReference(middle)));
            target.Write(TargetPath);
            if (writeDependency) dependency.Write(DependencyPath);
        }

        /// <summary>
        /// Builds an assembly reference matching the generated fixture version.
        /// </summary>
        private static AssemblyNameReference Name(string name) => new AssemblyNameReference(name, new Version(1, 0, 0, 0));
        /// <summary>
        /// Requires exclusive read/write access to the fixture file.
        /// </summary>
        private static void AssertUnlocked(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            Assert.True(stream.CanWrite);
        }

        /// <summary>
        /// Removes generated reference-cycle fixtures after each test.
        /// </summary>
        public void Dispose() => Directory.Delete(_directory, recursive: true);
        public enum Outcome { Unmodified, Modified, Throws }

        /// <summary>
        /// Checks cycle resolution before applying a configured patch outcome or nested callback.
        /// </summary>
        private sealed class CyclePatcher(TrackingResolver resolver, Outcome outcome, Action<AssemblyDefinition> afterResolve = null) : PatcherBase(new QuietLogger())
        {
            public AssemblyDefinition Target { get; private set; }
            public AssemblyDefinition Dependency { get; private set; }
            public TypeDefinition Root { get; private set; }
            protected override Rule GenericInfoRule => GeneralRules.Info;
            protected override Rule GenericErrorRule => GeneralRules.CompilationMustSecceedIfNoOtherErrors;
            /// <summary>
            /// Uses the tracking resolver for the resolver-owning overload.
            /// </summary>
            protected override IAssemblyResolver GetResolver(string assemblyFile, IReadOnlyList<string> references) => resolver;
            /// <summary>
            /// Checks dependency back-references before applying the configured fixture outcome.
            /// </summary>
            protected override bool PatchAssembly(AssemblyDefinition target, bool optimize, bool verbose)
            {
                Target = target;
                Dependency = target.MainModule.AssemblyResolver.Resolve(Name("Dependency"));
                Root = Dependency.MainModule.GetType("Test.Middle").BaseType.Resolve();
                Assert.Same(target.MainModule.GetType("Test.Root"), Root);
                Assert.Same(target, Dependency.MainModule.AssemblyResolver.Resolve(target.Name));
                Assert.Same(target, Dependency.MainModule.AssemblyResolver.Resolve(target.Name, new ReaderParameters()));
                afterResolve?.Invoke(target);
                if (outcome == Outcome.Throws) throw new InvalidOperationException("Expected patch failure");
                if (outcome == Outcome.Modified)
                    target.MainModule.Types.Add(new TypeDefinition("Test", "AddedByPatcher", TypeAttributes.Public, target.MainModule.TypeSystem.Object));
                return outcome == Outcome.Modified;
            }
        }

        private sealed class TrackingResolver : KnownReferencesAssemblyResolver
        {
            public int TargetLookups { get; private set; }
            public int DisposeCount { get; private set; }
            /// <summary>
            /// Counts target loads that bypass the scoped target registration.
            /// </summary>
            protected override AssemblyDefinition LookupAssembly(AssemblyNameReference name, ReaderParameters parameters)
            {
                if (name.Name == "Target") TargetLookups++;
                return base.LookupAssembly(name, parameters);
            }
            /// <summary>
            /// Counts disposal calls before releasing cached assemblies.
            /// </summary>
            protected override void Dispose(bool disposing)
            {
                DisposeCount++;
                base.Dispose(disposing);
            }
        }

        private sealed class QuietLogger : ILogger
        {
            public bool IsErrorThrown => false;
            /// <summary>
            /// Suppresses diagnostics for the reference-cycle fixture.
            /// </summary>
            public void Log(Rule rule, SequencePoint point, params string[] messages) { }
        }
    }
}
