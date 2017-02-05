//using AspectInjector.Broker;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.ComponentModel;

//namespace AspectInjector.Tests.General
//{
//    [TestClass]
//    public class ImportingTypesTests
//    {
//    }

//    [Notify]
//    public class AppViewModel
//    {
//        public string FirstName { get; set; }

//        public string LastName { get; set; }

//        public string FullName
//        {
//            get
//            {
//                return FirstName + " " + LastName;
//            }
//        }
//    }

//    [NamedCut(typeof(NotifyPropertyChangedAspect))]
//    internal class NotifyAttribute : Attribute
//    {
//        public string NotifyAlso { get; set; }
//    }

//    [Mixin(typeof(INotifyPropertyChanged))]
//    internal class NotifyPropertyChangedAspect : INotifyPropertyChanged
//    {
//        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

//        [Advice(Advice.Type.After, Advice.Target.Setter)]
//        public void AfterSetter(
//            [Advice.Argument(Advice.Argument.Source.Instance)] object source,
//            [Advice.Argument(Advice.Argument.Source.Name)] string propName,
//            [Advice.Argument(Advice.Argument.Source.RoutableData)] Attribute[] data
//            )
//        {
//            PropertyChanged(source, new PropertyChangedEventArgs(propName));

//            var additionalPropName = (data[0] as NotifyAttribute).NotifyAlso;

//            if (!string.IsNullOrEmpty(additionalPropName))
//            {
//                PropertyChanged(source, new PropertyChangedEventArgs(additionalPropName));
//            }
//        }
//    }
//}