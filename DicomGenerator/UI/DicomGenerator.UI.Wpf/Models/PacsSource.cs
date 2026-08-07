using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DicomGenerator.Core.Enums;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;

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
            Name = $"Новый PACS {_num++}" ;
        }


        [RelayCommand]
        private async Task EchoAsync()
        {
            var client = DicomClientFactory.Create(PacsHost, int.Parse(PacsPort), false, CallingAe, CalledAe);
            var echoRequest = new DicomCEchoRequest();
            try
            {
                await client.AddRequestAsync(echoRequest);
                await client.SendAsync();
                EchoStatus = "Pacs successfully connected";
            }
            catch(Exception ex)
            {
                EchoStatus = $"Failed - [{ex.Message}]";
            }
        }
    }
}
