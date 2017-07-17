using System.Windows;

namespace AspectInjector.SampleApps.NotifyPropertyChanged
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = new AppViewModel();
        }
    }
}
