using System.Windows;

namespace Heathmill.FixAT.ATOrderBook.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new Client.ViewModel.MainWindowViewModel();
        }
    }
}
