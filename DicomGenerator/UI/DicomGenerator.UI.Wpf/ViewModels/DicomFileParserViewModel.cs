using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DicomGenerator.Core.DicomFileParser;
using DicomGenerator.Core.Enums;
using DicomGenerator.Core.LiteDbModels;
using DicomGenerator.UI.Wpf.DicomFileParser;
using DicomGenerator.UI.Wpf.LiteDbCore;
using DicomGenerator.UI.Wpf.LocalPacsModules;
using DicomGenerator.UI.Wpf.Models;
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
        private PacsLoadingMode _loadingMode;
        private bool _isFolderScanSource;
        private bool _isPacsScanSource;


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
        public partial string OutputText { get; set; } = string.Empty;

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
        public partial PacsSource MainPacsSource { get; set; }

        [ObservableProperty]
        public partial FolderSource MainFolderSource { get; set; } = new();

        [ObservableProperty]
        public partial bool CanCancel { get; set; }

        public PacsLoadingMode LoadingMode
        {
            get => _loadingMode;
            set
            {
                if (SetProperty(ref _loadingMode, value))
                {
                    UpdateLoadingModeDescription();
                }
            }
        }

        [ObservableProperty]
        public partial DatabaseSaveMode SaveMode { get; set; } = DatabaseSaveMode.Full;

        [ObservableProperty]
        public partial string CurrentLoadingMode { get; set; }

        public bool HasPreviewData => !string.IsNullOrWhiteSpace(DatabasePath) && PreviewPatients?.Any() == true;

        public bool IsAllScanSource
        {
            get => IsFolderScanSource &&
            IsPacsScanSource &&
            AdditionalFolderSources.All(x => x.IsEnabled) &&
            AdditionalPacsSources.All(x => x.IsEnabled);

            set
            {
                IsFolderScanSource = value;
                IsPacsScanSource = value;

                foreach (var folder in AdditionalFolderSources)
                    folder.IsEnabled = value;

                foreach (var pacs in AdditionalPacsSources)
                    pacs.IsEnabled = value;

                OnPropertyChanged(nameof(IsAllScanSource));
            }
        }

        public bool IsFolderScanSource
        {
            get => _isFolderScanSource;
            set
            {
                if (SetProperty(ref _isFolderScanSource, value))
                {
                    OnPropertyChanged(nameof(IsAllScanSource));
                }
            }
        }

        public bool IsPacsScanSource
        {
            get => _isPacsScanSource;
            set
            {
                if (SetProperty(ref _isPacsScanSource, value))
                {
                    OnPropertyChanged(nameof(IsAllScanSource));
                }
            }
        }

        [ObservableProperty]
        public partial ObservableCollection<FolderSource> AdditionalFolderSources { get; set; } = new();

        [ObservableProperty]
        public partial ObservableCollection<PacsSource> AdditionalPacsSources { get; set; } = new();

        public ICommand RemoveAdditionalFolderCommand { get; }
        public ICommand RemoveAdditionalPacsCommand { get; }


        public DicomFileParserViewModel()
        {
            _scanner = new DicomFileScanner();
            _localPacsSource = new LocalPacsSource();
            PreviewPatients = new();

            InitialiseConnecting();
            InitialiseScanMode();
            UpdateLoadingModeDescription();

            RemoveAdditionalFolderCommand = new RelayCommand<FolderSource>(RemoveAdditionalFolder);
            RemoveAdditionalPacsCommand = new RelayCommand<PacsSource>(RemoveAdditionalPacs);

            AdditionalFolderSources.CollectionChanged += (s, e) => UpdateAllScanSource();
            AdditionalPacsSources.CollectionChanged += (s, e) => UpdateAllScanSource();
        }


        private void InitialiseConnecting()
        {
            MainPacsSource = new ()
            {
                Name ="Default",
                PacsHost = "127.0.0.1",
                PacsPort = "4242",
                CallingAe = "UNIEXPERT",
                CalledAe = "ORTHANC",
                LoadingMode = PacsLoadingMode.Auto
            };

            MainFolderSource = new ()
            {
                FolderPath = "E:\\DicomGeneratorResult\\"
            };
        }

        private void InitialiseScanMode()
        {
            LoadingMode = PacsLoadingMode.Auto;
            IsAllScanSource = true;
        }

        private void UpdateAllScanSource()
        {
            OnPropertyChanged(nameof(IsAllScanSource));
        }

        [RelayCommand]
        private void BrowseFolder()
        {
            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                MainFolderSource.FolderPath = dialog.FolderName;

                var allFiles = Directory.GetFiles(MainFolderSource.FolderPath, "*.*", SearchOption.AllDirectories);
                OutputText = $"Found {allFiles.Length} files in folder";
            }
        }

        [RelayCommand]
        private void BrowseAdditionalFolder()
        {
            var folder = new FolderSource();

            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                folder.FolderPath = dialog.FolderName;
                folder.IsEnabled = true;

                var allFiles = Directory.GetFiles(folder.FolderPath, "*.*", SearchOption.AllDirectories);
                OutputText = $"Found {allFiles.Length} files in folder";
            }

            folder.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(FolderSource.IsEnabled))
                    UpdateAllScanSource();
            };

            AdditionalFolderSources.Add(folder);
        }

        private void RemoveAdditionalFolder(FolderSource item)
        {
            AdditionalFolderSources.Remove(item);
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

        [RelayCommand]
        public void AddAdditionalPacs()
        {
            var newPacs = new PacsSource();

            newPacs.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PacsSource.IsEnabled))
                    UpdateAllScanSource();
            };
            AdditionalPacsSources.Add(newPacs);
        }

        private void RemoveAdditionalPacs(PacsSource item)
        {
            AdditionalPacsSources.Remove(item);
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
        private async Task ScanDicomAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            OutputText = string.Empty;
            PreviewPatients?.Clear();
            ScannedDicomFiles?.Clear();

            var token = CreateCancellationToken();
            var allResults = new List<DicomFileInfo>();

            var activeFolders = new List<FolderSource>();
            var activePacs = new List<PacsSource>();

            if (IsFolderScanSource && !string.IsNullOrWhiteSpace(MainFolderSource?.FolderPath))
            {
                activeFolders.Add(MainFolderSource);
            }

            if (IsPacsScanSource && MainPacsSource != null)
            {
                activePacs.Add(MainPacsSource);
            }

            activeFolders.AddRange(AdditionalFolderSources.Where(x => x.IsEnabled));
            activePacs.AddRange(AdditionalPacsSources.Where(x => x.IsEnabled));

            var totalSources = activeFolders.Count + activePacs.Count;

            if (totalSources == 0)
            {
                OutputText = "Нет активных источников.";
                return;
            }

            try
            {
                StartBusy($"Сканирование {totalSources} источников...", false);

                var sourceIndex = 0;

                foreach (var folder in activeFolders)
                {
                    token.ThrowIfCancellationRequested();

                    sourceIndex++;

                    BusyMessage =
                        $"Сканирование папки {folder.FolderPath} " +
                        $"({sourceIndex}/{totalSources})";

                    var progress = new Progress<(int Percent, string Message)>(p =>
                    {
                        CurrentProgress = p.Percent;
                        BusyMessage = $"Папка: {p.Message}";
                    });

                    var results = await _scanner.ScanFolderAsync(folder.FolderPath, progress, token);
                    allResults.AddRange(results);
                }

                foreach (var pacs in activePacs)
                {
                    token.ThrowIfCancellationRequested();

                    sourceIndex++;

                    BusyMessage =
                        $"Сканирование PACS {pacs.Name} " +
                        $"({sourceIndex}/{totalSources})";

                    var client = CreateClientFromCollection(pacs);

                    if (client == null)
                    {
                        OutputText += $"\nНе удалось подключиться к {pacs.Name}";
                        continue;
                    }

                    var progress = new Progress<(int Percent, string Message)>(p =>
                    {
                        CurrentProgress = p.Percent;
                        BusyMessage = $"PACS {pacs.Name}: {p.Message}";
                    });

                    var results = await LoadFromPacsAsync(client,pacs.LoadingMode, progress, token);
                    allResults.AddRange(results);
                }

                ScannedDicomFiles = allResults;
                PreviewPatients = BuildPreviewPatients(allResults);

                var stats = GetStatistics(allResults);

                stopwatch.Stop();

                var timeString = FormatTimeSpan(stopwatch.Elapsed);

                OutputText = $"Всего: Пациентов - [{stats.Patients}], " +
                    $"Исследований - [{stats.Studies}], " +
                    $"Серий - [{stats.Series}], " +
                    $"Изображений - [{stats.Images}].\n" +
                    $"Источников: {totalSources} (папок: {activeFolders.Count}, PACS: {activePacs.Count})\n" +
                    $"Время выполнения: {timeString}";
            }
            catch (OperationCanceledException)
            {
                OutputText = "Сканирование отменено пользователем.";
            }
            catch (Exception ex)
            {
                OutputText = $"Ошибка: {ex.Message}";
            }
            finally
            {
                StopBusy();

                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private ObservableCollection<PatientLiteDb> BuildPreviewPatients(List<DicomFileInfo> results)
        {
            var patients = new ObservableCollection<PatientLiteDb>();
            var seenIds = new HashSet<string>();

            foreach (var info in results)
            {
                if (info.Patient != null && seenIds.Add(info.Patient.PatientID))
                {
                    patients.Add(info.Patient);
                }
            }

            return patients;
        }        

        private (int Patients, int Studies, int Series, int Images) GetStatistics(List<DicomFileInfo> results)
        {
            var patients = new HashSet<string>();
            var studies = new HashSet<string>();
            var series = new HashSet<string>();
            var images = new HashSet<string>();

            foreach (var item in results)
            {
                if (!string.IsNullOrWhiteSpace(item.Patient?.PatientID))
                    patients.Add(item.Patient.PatientID);

                if (!string.IsNullOrWhiteSpace(item.Study?.StudyInstanceUid))
                    studies.Add(item.Study.StudyInstanceUid);

                if (!string.IsNullOrWhiteSpace(item.Series?.SeriesInstanceUid))
                    series.Add(item.Series.SeriesInstanceUid);

                if (!string.IsNullOrWhiteSpace(item.Image?.SopInstanceUid))
                    images.Add(item.Image.SopInstanceUid);
            }

            return (
                patients.Count,
                studies.Count,
                series.Count,
                images.Count);
        }

        private async Task<List<DicomFileInfo>> LoadFromPacsAsync(
            IDicomClient client,
            PacsLoadingMode loadingMode,
            IProgress<(int Percent, string Message)> progress,
            CancellationToken token)
        {
            return loadingMode switch
            {
                PacsLoadingMode.Fast => await _localPacsSource.LoadDicomMetadataFastAsync(client, progress, token),
                PacsLoadingMode.Compatible => await _localPacsSource.LoadDicomMetadataAsync(client, progress, token),
                _ => await LoadWithFallbackAsync(client, progress, token)
            };
        }

        private async Task<List<DicomFileInfo>> LoadWithFallbackAsync(
            IDicomClient client,
            IProgress<(int Percent, string Message)> progress,
            CancellationToken token)
        {
            try
            {
                return await _localPacsSource.LoadDicomMetadataFastAsync(client, progress, token);
            }
            catch (DicomNetworkException)
            {
                BusyMessage = "Быстрый режим не поддерживается. Переключение на совместимый...";
                return await _localPacsSource.LoadDicomMetadataAsync(client, progress, token);
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            if (_cancellationTokenSource == null)
                return;

            _cancellationTokenSource.Cancel();
            OutputText = "Операция отменена пользователем.";
        }

        private IDicomClient CreateClient()
        {            
            try
            {
                if (!int.TryParse(MainPacsSource.PacsPort, out var pacsPort))
                {
                    MainPacsSource.EchoStatus = "Invalid port";
                    return null;
                }

                return DicomClientFactory.Create(
                    MainPacsSource.PacsHost,
                    pacsPort,
                    false,
                    MainPacsSource.CallingAe,
                    MainPacsSource.CalledAe);

            }
            catch (Exception ex)
            {
                OutputText = $"Error creating PACS client: {ex.Message}";
                _logger.Error(OutputText);

                return null;
            }
        }

        private IDicomClient CreateClientFromCollection(PacsSource pacsSource)
        {
            try
            {
                if (!int.TryParse(pacsSource.PacsPort, out var pacsPort))
                {
                    pacsSource.EchoStatus = "Invalid port";
                    return null;
                }

                return DicomClientFactory.Create(
                    pacsSource.PacsHost,
                    pacsPort,
                    false,
                    pacsSource.CallingAe,
                    pacsSource.CalledAe);

            }
            catch (Exception ex)
            {
                OutputText = $"Error creating PACS client: {ex.Message}";
                _logger.Error(OutputText);

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

                var saveProgress = CreateProgress();

                OutputText = "Сохранение в БД ...";

                await Task.Run(() =>
                {
                    databaseService.SaveToDatabase(ScannedDicomFiles, SaveMode, saveProgress, token);
                }, token);

                OutputText = $"Завершено! Сохраненно {ScannedDicomFiles.Count} файл(ов).";
            }
            catch (OperationCanceledException)
            {
                OutputText = "Сохранение было отменено.";
            }
            catch (Exception ex)
            {
                OutputText = $"Ошибка парсинга файлов {ScannedDicomFiles.Count}: {ex.Message}";
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
            OutputText = $"Предварительный просмотр очищен.";
            OnPropertyChanged(nameof(HasPreviewData));
        }

        [RelayCommand]
        public async Task EchoAsync()
        {
            if (string.IsNullOrWhiteSpace(MainPacsSource.PacsHost) || string.IsNullOrWhiteSpace(MainPacsSource.PacsPort))
            {
                OutputText = "Please enter Host or Port.";
                MainPacsSource.EchoStatus = OutputText;
                return;
            }

            OutputText = "Connecting ...";
            MainPacsSource.EchoStatus = OutputText;

            try
            {
                var client = CreateClient();

                if (client == null)
                {
                    OutputText = "Не удается создать клиент PACS.";
                    return;
                }

                var echoRequest = new DicomCEchoRequest();
                await client.AddRequestAsync(echoRequest);
                await client.SendAsync();

                OutputText = $"PacS - [ {MainPacsSource.CalledAe} ] connected successful! ({MainPacsSource.PacsHost} : {MainPacsSource.PacsPort})";
                MainPacsSource.EchoStatus = OutputText;
            }
            catch (Exception ex)
            {
                OutputText = $"error: {ex.Message}";
                MainPacsSource.EchoStatus = OutputText;
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

        private IProgress<(int Percent, string Message)> CreateProgress()
        {
            return new Progress<(int Percent, string Message)>(percent =>
            {
                CurrentProgress = percent.Percent;
                BusyMessage = percent.Message;
            });
        }

        private CancellationToken CreateCancellationToken()
        {
            _cancellationTokenSource?.Dispose();

            _cancellationTokenSource = new CancellationTokenSource();
            return _cancellationTokenSource.Token;
        }

        private void StartBusy(string message = "Загрузка ...", bool percentShow = true)
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

        private void UpdateLoadingModeDescription()
        {
            CurrentLoadingMode = LoadingMode switch
            {
                PacsLoadingMode.Auto =>
                    "Автоматически выбирается быстрый режим сканирования.",

                PacsLoadingMode.Fast =>
                    "Один C-FIND запрос, но поддерживается не всеми PACS.",

                PacsLoadingMode.Compatible =>
                    "Поэтапная загрузка. Совместим со всеми PACS",

                _ => string.Empty
            };
        }
    }
}
