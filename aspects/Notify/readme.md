This package provides simple INotifyPropertyChanged aspect. 
Put ```[Notify]``` attribure on your properties or a whole class. You can use ```[NotifyAlso]``` attributes to notify other dependant properties.

[![Nuget](https://img.shields.io/nuget/v/Aspects.Notify?label=nuget&logo=nuget&style=flat-square)](https://www.nuget.org/packages/Aspects.Notify)

```c#
class TestClass
{
    [Notify]
    [NotifyAlso(nameof(FullName))]
    public string FirstName { get; set; }

    [Notify]
    [NotifyAlso(nameof(FullName))]
    public string LastName { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}

public void Code()
{
    var target = new TestClass();
    target.LastName = "Yu"; // <-- will raise ```PropertyChanged``` for ```LastName``` and ```FullName```     
}
```
