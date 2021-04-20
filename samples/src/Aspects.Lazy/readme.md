This package provides Lazy property.
Put `LazyAttribute` attribure on your get only properties. 

```csharp
class TestClass
{
    [Lazy]
    public ServiceA ServiceA => new ServiceA(DateTime.Now);
}
```

