[English](./readme.md)

这个包提供了类似 Lazy 的功能。

1. 假如有这样一个属性，并且你想延迟初始化：

    ```csharp
    class TestClass
    {
        public ServiceA ServiceA { get; } = new ServiceA(DateTime.Now);
    }
    ```

2. 使用`LazyAttribute`，并将`ServiceA`定义为**只读属性**，如下：

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

3. 编译后的 DLL 实现似下面的效果：

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

4. 但是`LazyAttribute`使用`Dictionary`缓存结果。


