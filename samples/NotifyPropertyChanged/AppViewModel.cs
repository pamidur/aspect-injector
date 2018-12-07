namespace AspectInjector.SampleApps.NotifyPropertyChanged
{
    public class AppViewModel
    {
        [Notify(NotifyAlso = nameof(Fullname))]
        public string FirstName { get; set; }

        [Notify(NotifyAlso = nameof(Fullname))]
        public string LastName { get; set; }

        public string Fullname => $"{FirstName} {LastName}";
    }
}