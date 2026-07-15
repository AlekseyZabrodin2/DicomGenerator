using System.Windows.Controls;
using DicomGenerator.UI.Wpf.ViewModels;

namespace DicomGenerator.UI.Wpf.Views
{
    /// <summary>
    /// Interaction logic for FileSenderUserControl.xaml
    /// </summary>
    public partial class FileSenderUserControl : UserControl
    {
        public FileSenderViewModel ViewModel = new();

        public FileSenderUserControl()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
