[中文](./readme-cn.md)

This package provides Lazy-like functionality.

1. Put `LazyAttribute` attribure **on getonly property**, like:

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

2. The compiled DLL such as:

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

3. But `LazyAttribute` use a Dictionary caches fields.


