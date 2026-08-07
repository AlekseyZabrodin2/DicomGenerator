using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace DicomGenerator.UI.Wpf.Models
{
    public partial class FolderSource : ObservableObject
    {
        [ObservableProperty]
        public partial string FolderName { get; set; }

        [ObservableProperty]
        public partial string FolderPath { get; set; }

        [ObservableProperty]
        public partial bool IsEnabled { get; set; } = false;



        [RelayCommand]
        private void Browse()
        {
            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                FolderPath = dialog.FolderName;
            }
        }
    }
}
