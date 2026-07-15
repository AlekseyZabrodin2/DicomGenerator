using System.Windows.Controls;
using DicomGenerator.UI.Wpf.ViewModels;

namespace DicomGenerator.UI.Wpf.Views
{
    /// <summary>
    /// Interaction logic for DicomFileParserUserControl.xaml
    /// </summary>
    public partial class DicomFileParserUserControl : UserControl
    {
        public DicomFileParserViewModel ViewModel = new();

        public DicomFileParserUserControl()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
