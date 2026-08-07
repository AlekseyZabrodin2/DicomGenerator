using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        public partial string BusyMessage { get; set; }

        [ObservableProperty]
        public partial int CurrentProgress { get; set; }

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
            InitialiseServerConnectingProperty();
        }



        private void InitialiseServerConnectingProperty()
        {
            ServerIp = "127.0.0.1";
            //ServerPort = 55100;
            ServerPort = 4242;
            CallingAe = "UniExpert";
            //CalledAe = "LocalScp";
            CalledAe = "Orthanc";
            //FolderPathFull = "D:\\DicomGeneratorResult\\";
            FolderPathFull = "E:\\DicomGeneratorResult\\";
        }


        [RelayCommand]
        private async Task SendDicomImage()
        {
            var stopwatch = Stopwatch.StartNew();

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            var client = DicomClientFactory.Create(ServerIp, ServerPort, false, CallingAe, CalledAe);
            client.ClientOptions.MaximumNumberOfRequestsPerAssociation = 500;

            var dicomFiles = Directory.GetFiles(FolderPathFull, "*.*");

            var fileCount = 0;

            if (dicomFiles.Length == 0)
            {
                ServerOutputText = "Файлов не найдено.";
                return;
            }

            try
            {
                IsBusy = true;

                var scanProgress = CreateProgress();

                var dicomRequests = new List<DicomRequest>(dicomFiles.Length);

                foreach (var filePath in dicomFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var dicomFile = DicomFile.Open(filePath);
                    var storeRequest = new DicomCStoreRequest(dicomFile);

                    storeRequest.OnResponseReceived += (_, response) =>
                    {
                        Interlocked.Increment(ref fileCount);

                        var percent = fileCount * 100 / dicomFiles.Length;

                        scanProgress.Report((percent, $"DICOM файл {fileCount} из {dicomFiles.Length} отправлен успешно."));
                    };

                    dicomRequests.Add(storeRequest);
                }

                await client.AddRequestsAsync(dicomRequests);

                await client.SendAsync(cancellationToken, cancellationMode: DicomClientCancellationMode.ImmediatelyReleaseAssociation);
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
                stopwatch.Stop();
                var elapsedTime = stopwatch.Elapsed;
                var timeString = FormatTimeSpan(elapsedTime);

                IsBusy = false;

                ServerOutputText = $"DICOM файл {fileCount} отправлен успешно. Время выполнения: {timeString}";
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

        private string FormatTimeSpan(TimeSpan time)
        {
            if (time.TotalHours >= 1)
            {
                return $"{time.Hours} ч {time.Minutes} мин {time.Seconds} сек";
            }
            if (time.TotalMinutes >= 1)
            {
                return $"{time.Minutes} мин {time.Seconds} сек";
            }
            return $"{time.TotalSeconds:F1} сек";
        }

        private IProgress<(int Percent, string Message)> CreateProgress()
        {
            return new Progress<(int Percent, string Message)>(percent =>
            {
                CurrentProgress = percent.Percent;
                BusyMessage = percent.Message;
            });
        }
    }
}
