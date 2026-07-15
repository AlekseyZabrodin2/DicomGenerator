using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DicomGenerator.Core.DicomFileParser;
using DicomGenerator.Core.Enums;
using DicomGenerator.Core.LiteDbModels;
using DicomGenerator.UI.Wpf.DicomFileParser;
using DicomGenerator.UI.Wpf.LiteDbCore;
using DicomGenerator.UI.Wpf.LocalPacsModules;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using Microsoft.Win32;
using NLog;

namespace DicomGenerator.UI.Wpf.ViewModels
{
    public partial class DicomFileParserViewModel : ObservableObject
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly DicomFileScanner _scanner;
        private readonly LocalPacsSource _localPacsSource;
        private ObservableCollection<PatientLiteDb> _previewPatients;
        private string _databasePath = string.Empty; 
        private CancellationTokenSource _cancellationTokenSource;


        public ObservableCollection<PatientLiteDb> PreviewPatients
        {
            get => _previewPatients;
            set
            {
                if (SetProperty(ref _previewPatients, value))
                {
                    OnPropertyChanged(nameof(HasPreviewData));
                }
            }
        }

        public string DatabasePath
        {
            get => _databasePath;
            set
            {
                SetProperty(ref _databasePath, value);
                BuildShortPath(_databasePath);
                OnPropertyChanged(nameof(HasPreviewData));
            }
        }

        [ObservableProperty]
        public partial string DatabasePathShort { get; set; }

        [ObservableProperty]
        public partial List<DicomFileInfo> ScannedDicomFiles { get; set; } = new();

        [ObservableProperty]
        public partial string FolderPath { get; set; }

        [ObservableProperty]
        public partial string OutputText { get; set; }

        [ObservableProperty]
        public partial int ProgressValue { get; set; }

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        public partial string BusyMessage { get; set; }

        [ObservableProperty]
        public partial int CurrentProgress { get; set; }

        [ObservableProperty]
        public partial bool PercentShow { get; set; }

        [ObservableProperty]
        public partial string PacsHost { get; set; } = "127.0.0.1";

        [ObservableProperty]
        public partial string PacsPort { get; set; } = "4242";

        [ObservableProperty]
        public partial string CallingAe { get; set; } = "UNIEXPERT";

        [ObservableProperty]
        public partial string CalledAe { get; set; } = "ORTHANC";

        [ObservableProperty]
        public partial string EchoStatus { get; set; }

        [ObservableProperty]
        public partial bool CanCancel { get; set; }

        [ObservableProperty]
        public partial DataSourceType SelectedDataSource { get; set; } = DataSourceType.Folder;

        public bool HasPreviewData => !string.IsNullOrWhiteSpace(DatabasePath) && PreviewPatients?.Any() == true;



        public DicomFileParserViewModel()
        {
            var dbPath = Path.Combine(
                "D:\\Develop\\DataBaseGenerator\\DataBaseGenerator\\bin\\Debug\\net9.0-windows10.0.17763.0\\LiteDataBase",
                "patients.db");
                                    
            _scanner = new DicomFileScanner();
            _localPacsSource = new LocalPacsSource();
            PreviewPatients = new();
        }



        [RelayCommand]
        private void BrowseFolder()
        {
            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                FolderPath = dialog.FolderName;

                var allFiles = Directory.GetFiles(FolderPath, "*.*", SearchOption.AllDirectories);
                OutputText = $"Found {allFiles.Length} files in folder";
            }
        }

        [RelayCommand]
        private void BrowseDatabase()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Выберите файл базы данных",
                Filter = "LiteDB database (*.db)|*.db|All files (*.*)|*.*",
                CheckFileExists = true
            };


            if (dialog.ShowDialog() == true)
            {
                DatabasePath = dialog.FileName;
            }
        }

        private DicomDatabaseService CreateDatabaseService()
        {
            if (string.IsNullOrWhiteSpace(DatabasePath))
            {
                OutputText = "Не выбран файл базы данных";

                throw new InvalidOperationException(OutputText);
            }

            var context = new LiteDbContext(DatabasePath);

            return new DicomDatabaseService(context);
        }

        [RelayCommand]
        private async Task ScanDicom()
        {
            if (SelectedDataSource == DataSourceType.Folder)
                await ScanFolderAsync();

            if (SelectedDataSource == DataSourceType.PACS)
                await ScanDatasetAsync();
        }

        private async Task ScanFolderAsync()
        {
            PreviewPatients?.Clear();
            ScannedDicomFiles?.Clear();

            if (string.IsNullOrWhiteSpace(FolderPath))
            {
                OutputText = "Please select a folder first.";
                return;
            }

            var token = CreateCancellationToken();

            try
            {
                StartBusy();

                var scanProgress = CreateProgress("Сканирование исследований ... ");

                ScannedDicomFiles = await _scanner.ScanFolderAsync(FolderPath, scanProgress, token);
                PreviewPatients = new ObservableCollection<PatientLiteDb>(_scanner.PreviewPatients);

                OutputText = $"Пациентов - [{_scanner.PatientsCount}], " +
                    $"Исследований - [{_scanner.StudiesCount}]," +
                    $" Серий - [{_scanner.SeriesCount}], " +
                    $"Изибражений - [{_scanner.ImagesCount}].";
            }
            catch (OperationCanceledException)
            {
                OutputText = "Scanning was cancelled.";
            }
            catch (Exception ex)
            {
                OutputText = $"Error: {ex.Message}";
            }
            finally
            {
                StopBusy();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private async Task ScanDatasetAsync()
        {
            PreviewPatients?.Clear();
            ScannedDicomFiles?.Clear();

            if (_localPacsSource == null)
            {
                OutputText = "Pacs client not connect";
                return;
            }

            var token = CreateCancellationToken();

            try
            {
                StartBusy();

                var loadProgress = CreateProgress("Сканирование исследований ... ");

                if (!int.TryParse(PacsPort, out var port))
                {
                    OutputText = "Invalid PACS port.";
                    return;
                }

                var client = CreateClient(PacsHost, port, CallingAe, CalledAe);

                if (client == null)
                {
                    OutputText = "Unable to create PACS client.";
                    return;
                }

                ScannedDicomFiles = await _localPacsSource.LoadDicomMetadataAsync(client, loadProgress, token);

                PreviewPatients = _localPacsSource.PreviewPatients;

                OutputText = $"Пациентов - [{_localPacsSource.PatientsCount}], " +
                    $"Исследований - [{_localPacsSource.StudiesCount}]," +
                    $" Серий - [{_localPacsSource.SeriesCount}], " +
                    $"Изибражений - [{_localPacsSource.ImagesCount}].";
            }
            catch (OperationCanceledException)
            {
                OutputText = "Scanning was cancelled.";
            }
            catch (Exception ex)
            {
                OutputText = $"Error: {ex.Message}";
            }
            finally
            {
                StopBusy();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            if (_cancellationTokenSource == null)
                return;

            _cancellationTokenSource.Cancel();
            OutputText = "Operation cancelled by user.";
        }

        private IDicomClient CreateClient(string host, int port, string callingAe, string calledAe)
        {
            try
            {
                var client = DicomClientFactory.Create(
                    host,
                    port,
                    false,
                    callingAe,
                    calledAe);

                _logger.Info($"PACS client created: {PacsHost}:{PacsPort}");                

                return client;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error creating PACS client: {ex.Message}");

                return null;
            }
        }

        [RelayCommand]
        public async Task SaveToDatabase()
        {
            if (ScannedDicomFiles == null || !ScannedDicomFiles.Any())
                return;

            var token = CreateCancellationToken();

            try
            {
                StartBusy();

                using var databaseService = CreateDatabaseService();

                var saveProgress = CreateProgress ($"Сохранение в БД ... ");

                OutputText = "Saving in DB ...";

                await Task.Run(() =>
                {
                    databaseService.SaveToDatabase(ScannedDicomFiles, saveProgress, token);
                }, token);

                OutputText = $"Completed! Saved {ScannedDicomFiles.Count} files.";
            }
            catch (OperationCanceledException)
            {
                OutputText = "Saving was cancelled.";
            }
            catch (Exception ex)
            {
                OutputText = $"Error parsing {ScannedDicomFiles.Count}: {ex.Message}";
            }
            finally 
            {
                StopBusy();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        [RelayCommand]
        private void CleanPreviewPatient()
        {
            if (!ScannedDicomFiles.Any())
                return;

            PreviewPatients = new();
            ScannedDicomFiles = new();
            OutputText = $"Preview is clear.";
            OnPropertyChanged(nameof(HasPreviewData));
        }

        [RelayCommand]
        public async Task EchoAsync()
        {
            if (string.IsNullOrWhiteSpace(PacsHost) || string.IsNullOrWhiteSpace(PacsPort))
            {
                OutputText = "Please enter Host and Port.";
                EchoStatus = OutputText;
                return;
            }

            OutputText = "Connecting...";
            EchoStatus = OutputText;

            try
            {
                if (!int.TryParse(PacsPort, out var port))
                {
                    OutputText = "Invalid PACS port.";
                    return;
                }
                var client = CreateClient(PacsHost, port, CallingAe, CalledAe);

                if (client == null)
                {
                    OutputText = "Unable to create PACS client.";
                    return;
                }

                var echoRequest = new DicomCEchoRequest();
                await client.AddRequestAsync(echoRequest);
                await client.SendAsync();

                OutputText = $"PacS - [ {CalledAe} ] connection successful! ({PacsHost} : {PacsPort})";
                EchoStatus = OutputText;
            }
            catch (Exception ex)
            {
                OutputText = $"Error: {ex.Message}";
                EchoStatus = OutputText;
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
                DatabasePathShort = normalized;
                return normalized;
            }

            var fileName = parts[^1];
            var startIndex = Math.Max(0, parts.Length - (keepFolders + 1));
            var tail = string.Join("\\", parts[startIndex..]);

            // Если есть диск (C:) или UNC, префикс всё равно делаем через "..."
            DatabasePathShort = $@"...\{tail}";

            return DatabasePathShort;
        }

        private IProgress<int> CreateProgress(string message)
        {
            return new Progress<int>(percent =>
            {
                CurrentProgress = percent;
                BusyMessage = message;
            });
        }

        private CancellationToken CreateCancellationToken()
        {
            _cancellationTokenSource?.Dispose();

            _cancellationTokenSource = new CancellationTokenSource();
            return _cancellationTokenSource.Token;
        }

        private void StartBusy(string message = "Загрузка...", bool percentShow = true)
        {
            BusyMessage = message;
            CurrentProgress = 0;
            IsBusy = true;
            CanCancel = IsBusy;
            PercentShow = percentShow;
        }

        private void StopBusy()
        {
            IsBusy = false;
            BusyMessage = string.Empty;
            PercentShow = false;
            CanCancel = IsBusy;
            CurrentProgress = 0;
            OnPropertyChanged(nameof(HasPreviewData));
        }
    }
}
