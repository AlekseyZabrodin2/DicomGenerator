using System.Windows.Controls;
using DicomGenerator.UI.Wpf.ViewModels;

namespace DicomGenerator.UI.Wpf.Views
{
    /// <summary>
    /// Interaction logic for FileGeneratorUserControl.xaml
    /// </summary>
    public partial class FileGeneratorUserControl : UserControl
    {
        public FileGeneratorViewModel ViewModel = new();

        public FileGeneratorUserControl()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
