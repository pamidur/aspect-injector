This package provides simple freezable pattern implementation. 
Put ```[Freezable]``` attribure on your properties or a whole class. 

Then cast your objects to ```IFreezable``` when you need to make it frozen or check its status.

```c#
[Freezable]
class TestClass
{
    public string Data { get; set; }
}

public void Code()
{
    var target = new TestClass();
    target.Data = "test1";
    ((IFreezable)target).Freeze();
    target.Data = "test2"; // <-- will throw exception
}
```