Imports AspectInjector.Broker
Imports AspectInjector.Tests.Assets
Imports Xunit

Namespace AspectInjector.Tests.VBRuntime

    Public Class Issue_0182

        <Fact>
        Public Async Sub AdviceBefore_Methods_Passes()
            Dim vb = New TestClassVbAspect()
            Dim result = Await vb.TestAsyncMethod("Test")
            Assert.Equal("Test", result)
        End Sub

        Partial Friend NotInheritable Class TestClassVbAspect
            <InjectInstanceAspect>
            Public Async Function TestAsyncMethod(Of T)(ByVal obj As T) As Task(Of T)
                Return Await Task.FromResult(obj)
            End Function
        End Class

        <AttributeUsage(AttributeTargets.All, AllowMultiple:=True)>
        <Injection(GetType(InstanceAspect))>
        Public Class InjectInstanceAspect
            Inherits Attribute
        End Class

        <Aspect(Scope.PerInstance)>
        Public Class InstanceAspect
            Inherits TestAspectBase

            <Advice(Kind.After)>
            Public Sub AspectMethod()
            End Sub

        End Class

    End Class

End Namespace

