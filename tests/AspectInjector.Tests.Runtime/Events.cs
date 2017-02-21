namespace AspectInjector.Tests.Runtime
{
    public static class Events
    {
        public static readonly string TestMethodEnter = "TestMethodEnter";
        public static readonly string TestMethodExit = "TestMethodExit";
        public static readonly string TestConstructorEnter = "TestConstructorEnter";
        public static readonly string TestConstructorExit = "TestConstructorExit";
        public static readonly string TestIteratorMethodEnter = "TestIteratorMethodEnter";
        public static readonly string TestIteratorMethodExit = "TestIteratorMethodExit";
        public static readonly string TestAsyncMethodEnter = "TestAsyncMethodEnter";
        public static readonly string TestAsyncMethodExit = "TestAsyncMethodExit";
        public static readonly string TestPropertySetterEnter = "TestPropertySetterEnter";
        public static readonly string TestPropertySetterExit = "TestPropertySetterExit";
        public static readonly string TestPropertyGetterEnter = "TestPropertyGetterEnter";
        public static readonly string TestPropertyGetterExit = "TestPropertyGetterExit";
        public static readonly string TestEventAddEnter = "TestEventAddEnter";
        public static readonly string TestEventAddExit = "TestEventAddExit";
        public static readonly string TestEventRemoveEnter = "TestEventRemoveEnter";
        public static readonly string TestEventRemoveExit = "TestEventRemoveExit";
    }
}