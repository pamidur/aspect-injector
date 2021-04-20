[English](./readme.md)

这个包提供了类似Lazy的功能。


1. 在**只读属性**上添加`LazyAttribute`，如下：

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

2. 编译后的 DLL 类似下面的效果：

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

3. 但是`LazyAttribute`使用`Dictionary`缓存结果。


