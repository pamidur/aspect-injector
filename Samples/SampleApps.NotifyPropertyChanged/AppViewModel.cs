using SampleApps.NotifyPropertyChanged.Aspects;
namespace SampleApps.NotifyPropertyChanged
{
    
    public class AppViewModel
    {
        [Notify(NotifyAlso = "FullName")]
        public string FirstName { get; set; }

        [Notify(NotifyAlso = "FullName")]
        public string LastName { get; set; }

        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }
    }
}
