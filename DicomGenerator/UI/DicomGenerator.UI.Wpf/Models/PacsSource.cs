using CommunityToolkit.Mvvm.ComponentModel;
using DicomGenerator.Core.Enums;

namespace DicomGenerator.UI.Wpf.Models
{
    public partial class PacsSource : ObservableObject
    {
        public static int _num = 0;

        [ObservableProperty]
        public partial string Name { get; set; } = "Новый PACS";

        [ObservableProperty]
        public partial string CallingAe { get; set; } = "UNIEXPERT";

        [ObservableProperty]
        public partial string PacsHost { get; set; } = "127.0.0.1";

        [ObservableProperty]
        public partial string PacsPort { get; set; } = "4242";

        [ObservableProperty]
        public partial string CalledAe { get; set; } = "ORTHANC";

        [ObservableProperty]
        public partial bool IsEnabled { get; set; } = false;

        [ObservableProperty]
        public partial bool IsExpanded { get; set; } = true;

        [ObservableProperty]
        public partial string EchoStatus { get; set; }

        [ObservableProperty]
        public partial PacsLoadingMode LoadingMode { get; set; } = PacsLoadingMode.Auto;


        public PacsSource()
        {
            Name = $"Новый PACS {_num++}";
        }
    }
}
