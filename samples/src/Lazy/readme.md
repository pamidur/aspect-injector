[中文](./readme-cn.md)

This package provides Lazy-like functionality.

1. Suppose there is such a property and you want to delay the initialization:

    ```csharp
    class TestClass
    {
        public ServiceA ServiceA { get; } = new ServiceA(DateTime.Now);
    }
    ```

2. Use `LazyAttribute`, and define `ServiceA` as a **read-only property**, as follows:

    ```csharp
    class TestClass
    {
        [Lazy]
        public ServiceA ServiceA => new ServiceA(DateTime.Now);
    }
    ```

    or

    ```csharp
    class TestClass
    {
        [Lazy]
        public ServiceA ServiceA
        {
            get { return new ServiceA(DateTime.Now); }
        }
    }
    ```

3. The compiled DLL such as:

    ```csharp
    class TestClass
    {
        private ServiceA _serviceA;

        [Lazy]
        public ServiceA ServiceA
        {
            get { return _serviceA ?? (_serviceA = new ServiceA(DateTime.Now)); }
        }
    }
    ```

4. But `LazyAttribute` use a Dictionary caches fields.


