This package provides check string property length.
Put `CheckLengthAttribute` attribure on your string properties. 

```csharp
class TestClass
{
    [StringLengthCheck(10, 1)]
    public string Name { get; set; }
}
```

