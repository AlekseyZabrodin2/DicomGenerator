using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using Microsoft.Win32;

namespace DicomGenerator.UI.Wpf.ViewModels
{
    public partial class FileSenderViewModel : ObservableObject
    {
        private string _folderPathFull = string.Empty;
        private CancellationTokenSource _cancellationTokenSource;


        [ObservableProperty]
        public partial string ServerIp { get; set; }

        [ObservableProperty]
        public partial int ServerPort { get; set; }

        [ObservableProperty]
        public partial string CallingAe { get; set; }

        [ObservableProperty]
        public partial string CalledAe { get; set; }

        [ObservableProperty]
        public partial string ServerOutputText { get; set; }       

        [ObservableProperty]
        public partial string FolderPathShort { get; set; }

        [ObservableProperty]
        public partial Visibility IsVisible { get; set; }

        public string FolderPathFull
        {
            get => _folderPathFull;
            set
            {
                SetProperty(ref _folderPathFull, value);
                BuildShortPath(_folderPathFull);
            }
        }




        public FileSenderViewModel()
        {
            IsVisible = Visibility.Collapsed;

            InitialiseServerConnectingProperty();
        }



        private void InitialiseServerConnectingProperty()
        {
            ServerIp = "127.0.0.1";
            ServerPort = 55100;
            CallingAe = "UniExpert";
            CalledAe = "LocalScp";
            FolderPathFull = "D:\\DicomGeneratorResult\\";
        }


        [RelayCommand]
        private async Task SendDicomImage()
        {
            IsVisible = Visibility.Visible;

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            var options = new DicomClientOptions { AssociationRequestTimeoutInMs = 10000 };

            var client = DicomClientFactory.Create(ServerIp, ServerPort, false, CallingAe, CalledAe);

            var dicomFiles = Directory.GetFiles(FolderPathFull, "*.*");

            var fileCount = 0;

            foreach (var filePath in dicomFiles)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    ServerOutputText = "Отправка файлов отменена пользователем.";
                    break;
                }

                var dicomFile = DicomFile.Open(filePath);
                var storeRequest = new DicomCStoreRequest(dicomFile);

                await client.AddRequestAsync(storeRequest);

                fileCount++;

                try
                {
                    await client.SendAsync(cancellationMode: DicomClientCancellationMode.ImmediatelyReleaseAssociation);
                    ServerOutputText = $"DICOM файл {fileCount} отправлен успешно.";
                }
                catch (OperationCanceledException)
                {
                    ServerOutputText = $"Отправка DICOM файла {fileCount} была отменена.";
                    return;
                }
                catch (Exception ex)
                {
                    ServerOutputText = $"Ошибка: {ex.Message}";
                    return;
                }
                finally
                {
                    IsVisible = Visibility.Collapsed;
                }
            }
        }

        [RelayCommand]
        private void CancaelSendingImage()
        {
            _cancellationTokenSource?.Cancel();
        }

        [RelayCommand]
        public void BrowseDatabase()
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Выберите путь к папке"
            };

            if (dialog.ShowDialog() == true)
            {
                FolderPathFull = dialog.FolderName;
            }
        }

        private string BuildShortPath(string fullPath, int keepFolders = 3)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                return string.Empty;

            // Нормализуем разделители и убираем хвостовые '\'
            var normalized = fullPath.Replace('/', '\\').TrimEnd('\\');

            // Для коротких путей ничего не делаем
            var parts = normalized.Split('\\', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length <= keepFolders + 1) // +1 = имя файла
            {
                FolderPathShort = normalized;
                return normalized;
            }

            var fileName = parts[^1];
            var startIndex = Math.Max(0, parts.Length - (keepFolders + 1));
            var tail = string.Join("\\", parts[startIndex..]);

            // Если есть диск (C:) или UNC, префикс всё равно делаем через "..."
            FolderPathShort = $@"...\{tail}";

            return FolderPathShort;
        }
    }
}
