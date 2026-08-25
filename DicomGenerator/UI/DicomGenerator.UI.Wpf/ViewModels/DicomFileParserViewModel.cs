using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DicomGenerator.Core.DicomFileParser;
using DicomGenerator.Core.Enums;
using DicomGenerator.Core.LiteDbModels;
using DicomGenerator.UI.Wpf.Configuration;
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
        private readonly AppConfigService _appConfigService = new();
        private readonly DicomFileScanner _scanner;
        private readonly LocalPacsSource _localPacsSource;
        private ObservableCollection<PatientLiteDb> _previewPatients;
        private string _databasePath = string.Empty;
        private string _errorMessage = string.Empty;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource? _busyAnimationCts; 
        private CancellationTokenSource _elapsedTimerCts;
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
        public partial PacsSource MainPacsSource { get; set; } = new();

        [ObservableProperty]
        public partial FolderSource MainFolderSource { get; set; } = new();

        [ObservableProperty]
        public partial string EchoStatus { get; set; }

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
                    MainFolderSource.IsEnabled = _isFolderScanSource;
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
                    MainPacsSource.IsEnabled = _isPacsScanSource;
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
        public event Action BusyAnimationStarted;
        public event Action BusyAnimationStopped;


        public DicomFileParserViewModel()
        {            
            _scanner = new DicomFileScanner();
            _localPacsSource = new LocalPacsSource();
            PreviewPatients = new();

            InitialiseConnecting();
            _ = LoadConfigAsync();
            InitialiseScanMode();
            UpdateLoadingModeDescription();

            RemoveAdditionalFolderCommand = new AsyncRelayCommand<FolderSource>(RemoveAdditionalFolder);
            RemoveAdditionalPacsCommand = new AsyncRelayCommand<PacsSource>(RemoveAdditionalPacs);

            AdditionalFolderSources.CollectionChanged += (s, e) => UpdateAllScanSource();
            AdditionalPacsSources.CollectionChanged += (s, e) => UpdateAllScanSource();
        }


        private void InitialiseConnecting()
        {
            MainPacsSource = new ()
            {
                Name = "Pacs",
                PacsHost = "127.0.0.1",
                PacsPort = "4242",
                CallingAe = "UNIEXPERT",
                CalledAe = "ORTHANC",
                LoadingMode = PacsLoadingMode.Auto,
                IsEnabled = true
            };

            MainFolderSource = new ()
            {
                FolderPath = "E:\\DicomGeneratorResult\\",
                IsEnabled = true
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

        public async Task LoadConfigAsync()
        {
            var config = await _appConfigService.LoadAsync();

            DatabasePath = config.Sources.DataBasePath;
            LoadFolderSources(config.Sources.Folders);
            LoadPacsSources(config.Sources.Pacs);
        }

        private void LoadFolderSources(List<FolderConfig> folders)
        {
            if (folders.Count == 0)
                return;

            var first = folders[0];

            MainFolderSource.FolderPath = first.Path;
            IsFolderScanSource = first.IsEnabled;

            for (var i = 1; i < folders.Count; i++)
            {
                var config = folders[i];

                AdditionalFolderSources.Add(new FolderSource
                {
                    FolderPath = config.Path,
                    IsEnabled = config.IsEnabled
                });
            }
        }

        private void LoadPacsSources(List<PacsConfig> pacs)
        {
            if (pacs.Count == 0)
                return;

            var first = pacs[0];

            MainPacsSource.Name = first.Name;
            MainPacsSource.CallingAe = first.CallingAe;
            MainPacsSource.PacsHost = first.PacsHost;
            MainPacsSource.PacsPort = first.PacsPort;
            MainPacsSource.CalledAe = first.CalledAe;
            IsPacsScanSource = first.IsEnabled;
            MainPacsSource.LoadingMode = first.LoadingMode;

            for (var i = 1; i < pacs.Count; i++)
            {
                var config = pacs[i];

                AdditionalPacsSources.Add(new PacsSource
                {
                    Name = config.Name,
                    CallingAe = config.CallingAe,
                    PacsHost = config.PacsHost,
                    PacsPort = config.PacsPort,
                    CalledAe = config.CalledAe,
                    IsEnabled = config.IsEnabled,
                    LoadingMode = config.LoadingMode
                });
            }
        }

        [RelayCommand]
        private async Task BrowseFolder()
        {
            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                MainFolderSource.FolderPath = dialog.FolderName;

                var allFiles = Directory.GetFiles(MainFolderSource.FolderPath, "*.*", SearchOption.AllDirectories);
                SetOutputInfo($"Found {allFiles.Length} files in folder");
            }

            await SaveConfigAsync();
        }

        [RelayCommand]
        private async Task BrowseAdditionalFolder()
        {
            var folder = new FolderSource();

            var dialog = new OpenFolderDialog();
            if (dialog.ShowDialog() == true)
            {
                folder.FolderPath = dialog.FolderName;
                folder.IsEnabled = true;

                var allFiles = Directory.GetFiles(folder.FolderPath, "*.*", SearchOption.AllDirectories);
                SetOutputInfo($"Found {allFiles.Length} files in folder");
            }

            folder.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(FolderSource.IsEnabled))
                    UpdateAllScanSource();
            };

            AdditionalFolderSources.Add(folder);

            await SaveConfigAsync();
        }

        private async Task RemoveAdditionalFolder(FolderSource item)
        {
            AdditionalFolderSources.Remove(item);
            await SaveConfigAsync();
        }        

        [RelayCommand]
        private async Task BrowseDatabase()
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

            await SaveConfigAsync();
        }

        [RelayCommand]
        public async Task AddAdditionalPacs()
        {
            var newPacs = new PacsSource();

            newPacs.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PacsSource.IsEnabled))
                    UpdateAllScanSource();
            };
            AdditionalPacsSources.Add(newPacs);
            await SaveConfigAsync();
        }

        private async Task RemoveAdditionalPacs(PacsSource item)
        {
            AdditionalPacsSources.Remove(item);
            await SaveConfigAsync();
        }

        private DicomDatabaseService CreateDatabaseService()
        {
            if (string.IsNullOrWhiteSpace(DatabasePath))
            {
                SetOutputInfo("Не выбран файл базы данных");

                throw new InvalidOperationException(OutputText);
            }

            var context = new LiteDbContext(DatabasePath);

            return new DicomDatabaseService(context);
        }

        [RelayCommand]
        private async Task ScanDicomAsync()
        {
            var stopwatch = Stopwatch.StartNew();

            _errorMessage = string.Empty;
            OutputText = string.Empty;
            PreviewPatients?.Clear();
            ScannedDicomFiles?.Clear();

            var token = CreateCancellationToken();
            var animationToken = CreateCancellationTokenAnimation();
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
                SetOutputInfo("Нет активных источников.");
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

                    SetBusyInfo($"Сканирование архива {folder.FolderPath} " +
                    $"({sourceIndex}/{totalSources})");

                    SetBusyState(BusyMessage, true);

                    var progress = new Progress<(int Percent, string Message)>(p =>
                    {
                        CurrentProgress = p.Percent;
                        BusyMessage = $"Архив: сканирование - {folder.FolderPath} {p.Message}";
                    });

                    StartBusyAnimation("Получение данных из Архива ");

                    var results = await _scanner.ScanFolderAsync(folder.FolderPath, progress, token);
                    allResults.AddRange(results);
                }

                foreach (var pacs in activePacs)
                {
                    token.ThrowIfCancellationRequested();

                    sourceIndex++;

                    var displayName = GetDisplayName(pacs);

                    SetBusyInfo($"Сканирование - {displayName}");

                    SetBusyState(BusyMessage, false);

                    var client = CreateClientFromCollection(pacs);

                    if (client == null)
                    {
                        SetOutputWarning($"\nНе удалось подключиться к {displayName}");
                        continue;
                    }

                    var progress = new Progress<(int Percent, string Message)>(p =>
                    {
                        CurrentProgress = p.Percent;
                        BusyMessage = $"{displayName} {p.Message}";
                    });

                    try
                    {
                        StartBusyAnimation("Получение данных из PACS ");

                        var results = await LoadFromPacsAsync(displayName, client, LoadingMode, progress, token, animationToken);
                        allResults.AddRange(results);
                    }
                    finally
                    {
                        StopElapsedTimer();
                    }
                }
                
                stopwatch.Stop();
            }
            catch (OperationCanceledException)
            {
                _errorMessage = "\n\nСканирование отменено пользователем.";
                SetOutputError(_errorMessage);
            }
            catch (Exception ex)
            {
                _errorMessage = $"\n\nОшибка Pacs: {ex.Message}";
                SetOutputError(_errorMessage);
            }
            finally
            {
                StopBusyAnimation();
                ScannedDicomFiles = allResults;
                PreviewPatients = BuildPreviewPatients(allResults);

                var stats = GetStatistics(allResults);

                var timeString = FormatTimeSpan(stopwatch.Elapsed);

                SetOutputInfo($"Всего: Пациентов - [{stats.Patients}], " +
                    $"Исследований - [{stats.Studies}], " +
                    $"Серий - [{stats.Series}], " +
                    $"Изображений - [{stats.Images}].\n" +
                    $"Источников: {totalSources} (папок: {activeFolders.Count}, PACS: {activePacs.Count})\n" +
                    $"Время выполнения: {timeString}" +
                    $"{_errorMessage}");

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
            string displayName,
            IDicomClient client,
            PacsLoadingMode loadingMode,
            IProgress<(int Percent, string Message)> progress,
            CancellationToken token,
            CancellationTokenSource animationToken)
        {
            switch (loadingMode)
            {
                case PacsLoadingMode.Fast:
                    StartElapsedTimer(displayName);
                    var fastResult = await _localPacsSource.LoadDicomMetadataFastAsync(
                        client, progress, token, animationToken);
                    return fastResult;

                case PacsLoadingMode.Compatible:
                    SetBusyState(BusyMessage, true);
                    return await _localPacsSource.LoadDicomMetadataAsync(
                        client, progress, token, animationToken);

                default:
                    return await LoadWithFallbackAsync(
                        displayName, client, progress, token, animationToken);
            }
        }

        private async Task<List<DicomFileInfo>> LoadWithFallbackAsync(
            string displayName,
            IDicomClient client,
            IProgress<(int Percent, string Message)> progress,
            CancellationToken token,
            CancellationTokenSource animationToken)
        {
            try
            {
                StartElapsedTimer(displayName);
                return await _localPacsSource.LoadDicomMetadataFastAsync(client, progress, token, animationToken);
            }
            catch (DicomNetworkException ex)
            {
                SetBusyState("",true);
                SetBusyWarning("Быстрый режим не поддерживается. Переключение на совместимый...", ex);
                return await _localPacsSource.LoadDicomMetadataAsync(client, progress, token, animationToken);
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            if (_cancellationTokenSource == null)
                return;

            _cancellationTokenSource.Cancel();
            SetOutputInfo("Операция отменена пользователем.");
        }

        private IDicomClient CreateClient(PacsSource pacs)
        {            
            try
            {
                if (!int.TryParse(pacs.PacsPort, out var pacsPort))
                {
                    pacs.EchoStatus = "Invalid port";
                    return null;
                }

                return DicomClientFactory.Create(
                    pacs.PacsHost,
                    pacsPort,
                    false,
                    pacs.CallingAe,
                    pacs.CalledAe);

            }
            catch (Exception ex)
            {
                SetOutputError($"Error creating PACS client: {ex.Message}", ex);
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
                SetOutputError($"Error creating PACS client: {ex.Message}", ex);
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

                SetOutputInfo("Сохранение в БД ...");

                await Task.Run(() =>
                {
                    databaseService.SaveToDatabase(ScannedDicomFiles, SaveMode, saveProgress, token);
                }, token);

                SetOutputInfo($"Завершено! Сохраненно {ScannedDicomFiles.Count} файл(ов).");
            }
            catch (OperationCanceledException)
            {
                SetOutputError("Сохранение было отменено.");
            }
            catch (Exception ex)
            {
                SetOutputError($"Ошибка парсинга файлов {ScannedDicomFiles.Count}: {ex.Message}", ex);
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
            SetOutputInfo($"Предварительный просмотр очищен.");
            OnPropertyChanged(nameof(HasPreviewData));
        }

        [RelayCommand]
        public async Task EchoAsync()
        {
            if (string.IsNullOrWhiteSpace(MainPacsSource.PacsHost) || string.IsNullOrWhiteSpace(MainPacsSource.PacsPort))
            {
                SetOutputInfo("Please enter Host or Port.");
                MainPacsSource.EchoStatus = OutputText;
                return;
            }

            SetOutputInfo("Connecting ...");
            MainPacsSource.EchoStatus = OutputText;

            try
            {
                var client = CreateClient(MainPacsSource);

                if (client == null)
                {
                    SetOutputWarning("Не удается создать клиент PACS.");
                    return;
                }

                DicomStatus responseStatus = null;
                var echoRequest = new DicomCEchoRequest();

                echoRequest.OnResponseReceived += (request, response) =>
                {
                    responseStatus = response.Status;
                };

                await client.AddRequestAsync(echoRequest);
                await client.SendAsync();

                if (responseStatus?.State == DicomState.Success)
                {
                    SetOutputInfo($"PacS - [ {client.CalledAe} ] connected successful! ({client.Host} : {client.Port})");
                    await SaveConfigAsync();
                }
                else
                {
                    SetOutputWarning($"PACS вернул статус: {responseStatus}");
                }

                MainPacsSource.EchoStatus = OutputText;
            }
            catch (Exception ex)
            {
                SetOutputError($"error: {ex.Message}", ex);
                MainPacsSource.EchoStatus = OutputText;
            }
        }

        [RelayCommand]
        public async Task EchoAdditionalPacsAsync(PacsSource pacs)
        {
            if (string.IsNullOrWhiteSpace(pacs.PacsHost) || string.IsNullOrWhiteSpace(pacs.PacsPort))
            {
                SetOutputInfo("Please enter Host or Port.");
                pacs.EchoStatus = OutputText;
                return;
            }

            try
            {
                var client = CreateClient(pacs);

                if (client == null)
                {
                    SetOutputWarning("Не удается создать клиент PACS.");
                    return;
                }

                DicomStatus responseStatus = null;
                var echoRequest = new DicomCEchoRequest();

                echoRequest.OnResponseReceived += (request, response) =>
                {
                    responseStatus = response.Status;
                };

                await client.AddRequestAsync(echoRequest);
                await client.SendAsync();

                if (responseStatus?.State == DicomState.Success)
                {
                    SetOutputInfo($"PacS - [ {pacs.CalledAe} ] connected successful! ({pacs.PacsHost} : {pacs.PacsPort})");
                    pacs.EchoStatus = OutputText;
                    await SaveConfigAsync();
                }
                else
                {
                    SetOutputWarning($"PACS вернул статус: {responseStatus}");
                    pacs.EchoStatus = OutputText;
                }

                pacs.EchoStatus = OutputText;
            }
            catch (Exception ex)
            {
                SetOutputError($"error: {ex.Message}", ex);
                pacs.EchoStatus = OutputText;
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

        private CancellationTokenSource CreateCancellationTokenAnimation()
        {
            _busyAnimationCts?.Dispose();

            _busyAnimationCts = new CancellationTokenSource();
            return _busyAnimationCts;
        }

        private void StartBusy(string message = "Загрузка ...", bool percentShow = true)
        {
            BusyMessage = message;
            CurrentProgress = 0;
            IsBusy = true;
            CanCancel = IsBusy;
            PercentShow = percentShow;
        }

        private void SetBusyState(string message, bool percentShow, int progress = 0)
        {
            BusyMessage = message;
            PercentShow = percentShow;
            CurrentProgress = progress;
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

        private void SetBusyInfo(string message)
        {
            BusyMessage = message;
            _logger.Info(BusyMessage);
        }

        private void SetBusyError(string message, Exception? ex = null)
        {
            BusyMessage = message;
            _logger.Error(ex, BusyMessage);
        }

        private void SetBusyWarning(string message, Exception? ex = null)
        {
            BusyMessage = message;
            _logger.Warn(ex, BusyMessage);
        }

        private void SetOutputInfo(string message)
        {
            OutputText = message;
            _logger.Info(OutputText);
        }

        private void SetOutputError(string message, Exception? ex = null)
        {
            OutputText = message;
            _logger.Error(ex, OutputText);
        }

        private void SetOutputWarning(string message, Exception? ex = null)
        {
            OutputText = message;
            _logger.Warn(ex, OutputText);
        }

        public string GetDisplayName(PacsSource pacs)
        {
            if (pacs == null)
                return string.Empty;

            return $"{pacs.Name}-[ {pacs.CallingAe} ]";
        }

        private async Task SaveConfigAsync()
        {
            var config = new AppConfig();

            config.Sources.DataBasePath = DatabasePath;

            config.Sources.Folders.Add(new FolderConfig
            {
                Path = MainFolderSource.FolderPath,
                IsEnabled = MainFolderSource.IsEnabled
            });

            foreach (var folder in AdditionalFolderSources)
            {
                config.Sources.Folders.Add(new FolderConfig
                {
                    Path = folder.FolderPath,
                    IsEnabled = folder.IsEnabled
                });
            }

            config.Sources.Pacs.Add(new PacsConfig
            {
                Name = MainPacsSource.Name,
                CallingAe = MainPacsSource.CallingAe,
                PacsHost = MainPacsSource.PacsHost,
                PacsPort = MainPacsSource.PacsPort,
                CalledAe = MainPacsSource.CalledAe,
                IsEnabled = MainPacsSource.IsEnabled,
                LoadingMode = MainPacsSource.LoadingMode
            });

            foreach (var pacs in AdditionalPacsSources)
            {
                config.Sources.Pacs.Add(new PacsConfig
                {
                    Name = pacs.Name,
                    CallingAe = pacs.CallingAe,
                    PacsHost = pacs.PacsHost,
                    PacsPort = pacs.PacsPort,
                    CalledAe = pacs.CalledAe,
                    IsEnabled = pacs.IsEnabled,
                    LoadingMode = pacs.LoadingMode
                });
            }

            await _appConfigService.SaveAsync(config);
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

        private void StartBusyAnimation(string message)
        {
            BusyAnimationStarted?.Invoke();
            BusyMessage = $"{message}";
        }

        private void StopBusyAnimation()
        {
            BusyAnimationStopped?.Invoke();

            _busyAnimationCts?.Cancel();
            _busyAnimationCts?.Dispose();
            _busyAnimationCts = null;
        }

        private void StartElapsedTimer(string displayName)
        {
            _elapsedTimerCts?.Cancel();
            _elapsedTimerCts?.Dispose();

            _elapsedTimerCts = new CancellationTokenSource();

            _ = UpdateElapsedTimerAsync(displayName, _elapsedTimerCts.Token);
        }

        private async Task UpdateElapsedTimerAsync(
            string displayName,
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    BusyMessage = $"{displayName} Получение данных ... {stopwatch.Elapsed:mm\\:ss}";

                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void StopElapsedTimer()
        {
            _elapsedTimerCts?.Cancel();
            _elapsedTimerCts?.Dispose();
            _elapsedTimerCts = null;
        }
    }
}
