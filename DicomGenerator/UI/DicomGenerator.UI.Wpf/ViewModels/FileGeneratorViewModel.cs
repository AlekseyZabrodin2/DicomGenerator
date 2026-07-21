using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DicomGenerator.Core.DicomGeneratorModels;
using DicomGenerator.Core.GeneratorRules._3_Series;
using DicomGenerator.Core.GeneratorRules.Patient;
using DicomGenerator.Core.GeneratorRules.Series;
using DicomGenerator.Core.GeneratorRules.Sop;
using DicomGenerator.Core.GeneratorRules.Study;
using FellowOakDicom;

namespace DicomGenerator.UI.Wpf.ViewModels
{
    public partial class FileGeneratorViewModel : ObservableObject
    {
        private StudyGeneratorParameters _studyParameters;
        private PatientGeneratorParameters _patientParameters;
        private PatientGenerator _patientGenerator;
        private SerieGeneratorParameters _serieParameters;
        private SerieGeneratorParameters _serieParametersSr;
        private SerieGeneratorParameters _serieParametersMg;
        private SerieGeneratorParameters _serieParametersDx;
        private SopGeneratorParameters _sopGeneratorParameters;
        private DicomEncodingRule _defaultEncoding = new DicomEncodingRule(Encoding.UTF8);
        private CancellationTokenSource _cancellationTokenSource;
        public bool _useBirthDatePatient;
        public int _sumCounts;
        private readonly string _pathToSave = @"D:\DicomGeneratorResult";


        // Path to TestData in Debug
        private readonly string _pathToTestData = @"D:\Develop\DicomGeneratorTestData\";


        // Path to TestData after Install
        //private readonly string _pathToTestData = @"C:\Program Files (x86)\DicomGenerator\DicomGeneratorTestData\";



        public ObservableCollection<DicomEncodingRule> Encodings { get; }

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        public partial string BusyMessage { get; set; }

        [ObservableProperty]
        public partial int CurrentProgress { get; set; }

        [ObservableProperty]
        public partial bool PercentShow { get; set; }

        [ObservableProperty]
        public partial bool CanCancel { get; set; }

        [ObservableProperty]
        public partial bool WasException { get; set; }

        [ObservableProperty]
        public partial DicomEncodingRule ChooseCod { get; set; }

        [ObservableProperty]
        public partial List<GenderItem> GenderPatient { get; set; }

        [ObservableProperty]
        public partial bool UsePeriodBirthDatePatient { get; set; }

        [ObservableProperty]
        public partial DateTime BirthDatePatient { get; set; }

        [ObservableProperty]
        public partial DateTime PeriodBirthDatePatient { get; set; }

        [ObservableProperty]
        public partial DateTime SelectedBirthDatePatient { get; set; }

        [ObservableProperty]
        public partial DateTime StartDatePatient { get; set; }

        [ObservableProperty]
        public partial DateTime EndDatePatient { get; set; }

        [ObservableProperty]
        public partial DateTime StartTimePatient { get; set; }

        [ObservableProperty]
        public partial DateTime EndTimePatient { get; set; }

        [ObservableProperty]
        public partial int SetPatientsCount { get; set; }

        [ObservableProperty]
        public partial int SetStudiesCount { get; set; }

        [ObservableProperty]
        public partial int SetSeriesCount { get; set; }

        [ObservableProperty]
        public partial int SopSrCount { get; set; }

        [ObservableProperty]
        public partial int SopDxCount { get; set; }

        [ObservableProperty]
        public partial int SopMgCount { get; set; }

        [ObservableProperty]
        public partial string IdPatient { get; set; }

        [ObservableProperty]
        public partial string LastNamePatient { get; set; }

        [ObservableProperty]
        public partial string NamePatient { get; set; }

        [ObservableProperty]
        public partial string MiddleNamePatient { get; set; }

        [ObservableProperty]
        public partial string AddressPatient { get; set; }

        [ObservableProperty]
        public partial int PhonePatient { get; set; }

        [ObservableProperty]
        public partial string WorkPlacePatient { get; set; }

        [ObservableProperty]
        public partial string InfoPatient { get; set; }


        [ObservableProperty]
        public partial string OutputText { get; set; }
                
        public bool UseBirthDatePatient
        {
            get => _useBirthDatePatient;
            set
            {
                UsePeriodBirthDatePatient = !value;
                SetProperty(ref _useBirthDatePatient, value);
            }
        }

        [ObservableProperty]
        public partial GenderItem? SelectedGender { get; set; }



        public FileGeneratorViewModel()
        {
            InitialiseProperties();

            Encodings = new ObservableCollection<DicomEncodingRule>
            {
                new DicomEncodingRule(Encoding.Latin1),
                new DicomEncodingRule(Encoding.ASCII),
                _defaultEncoding
            };

            ChooseCod = _defaultEncoding;
        }



        private void InitialiseProperties()
        {
            GenderPatient = new()
            {
                new GenderItem { DisplayName = "Man",    Code = "M" },
                new GenderItem { DisplayName = "Female", Code = "F" },
                new GenderItem { DisplayName = "Other",  Code = "O" },
                new GenderItem { DisplayName = "Empty",  Code = null }
            };

            SelectedGender = GenderPatient[3];
            UseBirthDatePatient = false;
            UsePeriodBirthDatePatient = !UseBirthDatePatient;
            BirthDatePatient = DateTime.Now;
            PeriodBirthDatePatient = DateTime.Now.AddYears(-50);
            StartDatePatient = DateTime.Now.AddMonths(-3);
            EndDatePatient = DateTime.Now;
            EndTimePatient = DateTime.Now;
        }



        private DateTime SelectedPatientBirthDate()
        {
            if (UseBirthDatePatient)
            {
                return BirthDatePatient;
            }

            return PeriodBirthDatePatient;
        }

        [RelayCommand]
        private async Task GenerateFiles()
        {
            var stopwatch = Stopwatch.StartNew();

            var token = CreateCancellationToken();

            var processed = 0;

            try
            {
                StartBusy("Генерация файлов ...");
                
                var progress = CreateProgress();

                await Task.Run(() =>
                {

                    _serieParametersDx = new SerieGeneratorParameters(
                      new SeriesLateralityRule(),
                      Modality.Dx,
                      new SeriesNumberRule(),
                      new RangeSeriesDateTimeRule(StartTimePatient, EndTimePatient),
                      new SopGeneratorParameters(new SopClassUidRule(new SopClassFactory(_pathToTestData)))
                      {
                          SopCount = SopDxCount
                      })
                    { SeriesCount = SetSeriesCount };

                    _serieParametersMg = new SerieGeneratorParameters(
                        new SeriesLateralityRule(),
                        Modality.Mg,
                        new SeriesNumberRule(),
                        new RangeSeriesDateTimeRule(StartTimePatient, EndTimePatient),
                        new SopGeneratorParameters(new SopClassUidRule(new SopClassFactory(_pathToTestData)))
                        {
                            SopCount = SopMgCount
                        })
                    { SeriesCount = SetSeriesCount };

                    _serieParametersSr = new SerieGeneratorParameters(
                        new SeriesLateralityRule(),
                        Modality.Sr,
                        new SeriesNumberRule(),
                        new RangeSeriesDateTimeRule(StartTimePatient, EndTimePatient),
                        new SopGeneratorParameters(new SopClassUidRule(new SopClassFactory(_pathToTestData)))
                        {
                            SopCount = SopSrCount
                        })
                    { SeriesCount = SetSeriesCount };

                    _studyParameters = new StudyGeneratorParameters(
                        new RangeStudyDateTimeRule(StartDatePatient, EndDatePatient),
                        new RandomAccessionNumberRule(),
                        new List<SerieGeneratorParameters>() { _serieParametersDx, _serieParametersMg, _serieParametersSr });

                    _patientParameters = new PatientGeneratorParameters(
                        ChooseCod,
                        new RandomNameRule(LastNamePatient, NamePatient, MiddleNamePatient),
                        new OrderedIdRule(IdPatient),
                        new RandomSexRule(SelectedGender.Code),
                        new PatientAddressRule(AddressPatient),
                        new PatientCommentsRule(InfoPatient),
                        new PatientTelephoneRule(PhonePatient),
                        new RandomPatientBirthDateRule(SelectedPatientBirthDate(), UsePeriodBirthDatePatient),
                        new List<StudyGeneratorParameters>() { _studyParameters })
                    {
                        EthnicGroupRule = new ListEthnicGroupRule(),
                        PatientAge = new PatientAgeRule(),
                        PatientsCount = SetPatientsCount
                    };

                    _patientParameters.PatientsCount = SetPatientsCount;
                    _studyParameters.StudiesCount = SetStudiesCount;
                    _serieParametersDx.SeriesCount = SetSeriesCount;
                    _serieParametersMg.SeriesCount = SetSeriesCount;
                    _serieParametersSr.SeriesCount = SetSeriesCount;

                    _sumCounts = SetPatientsCount * (SetStudiesCount * (SetSeriesCount * (SopDxCount + SopMgCount + SopSrCount)));

                    _patientGenerator = new PatientGenerator();

                    DirectoryInfo directoryInfo = new DirectoryInfo(_pathToSave);

                    if (!directoryInfo.Exists)
                    {
                        MessageBox.Show("              Folder not found. \nBut i'm smart and did it for you !");

                        directoryInfo.Create();
                    }

                    progress?.Report((0, "Генерация файлов ..."));

                    var total = _patientParameters.PatientsCount;

                    for (var patientIndex = 0; patientIndex < total; patientIndex++)
                    {
                        var dataSets = _patientGenerator.Generate(patientIndex, _patientParameters, token);

                        foreach (var dataset in dataSets)
                        {
                            var dicomFIle = new DicomFile(dataset);

                            dicomFIle.Save(Path.Combine(_pathToSave, Guid.NewGuid().ToString()));                            
                        }

                        processed++;
                        var percent = (int)((double)processed * 100 / total);
                        progress?.Report((percent, string.Empty));
                    }
                });
            }
            catch (OperationCanceledException)
            {
                WasException = true;
                OutputText = $"Генерация была отменена, сгенерировано {processed} файлов.";
            }
            catch (Exception ex)
            {
                WasException = true;
                OutputText = ex.Message;
            }
            finally
            {
                stopwatch.Stop();
                var elapsedTime = stopwatch.Elapsed;
                var timeString = FormatTimeSpan(elapsedTime);

                StopBusy();
                if(!WasException)
                    OutputText = $"Выполнено, сгенерировано {_sumCounts} фалов.  Время выполнения: {timeString}";

                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
                WasException = false;
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

        [RelayCommand]
        private void CleanUpFields()
        {
            SetPatientsCount = 0;
            SetStudiesCount = 0;
            SetSeriesCount = 0;
            SopMgCount = 0;
            SopDxCount = 0;
            SopSrCount = 0;

            IdPatient = string.Empty;
            LastNamePatient = string.Empty;
            NamePatient = string.Empty;
            MiddleNamePatient = string.Empty;
            SelectedGender = GenderPatient[3];

            UseBirthDatePatient = false;
            BirthDatePatient = DateTime.Now;
            PeriodBirthDatePatient = DateTime.Now.AddYears(-50);

            AddressPatient = string.Empty;
            PhonePatient = 0;
            InfoPatient = string.Empty;

            StartDatePatient = DateTime.Now.AddMonths(-3);
            EndDatePatient = DateTime.Now;
            StartTimePatient = DateTime.Now;
            StartTimePatient = DateTime.Parse("00:00");
            EndTimePatient = DateTime.Now;

            ChooseCod = _defaultEncoding;

            OutputText = "All fields are cleared !!!";
        }

        private IProgress<(int Percent, string Message)> CreateProgress()
        {
            return new Progress<(int Percent, string Message)>(percent =>
            {
                CurrentProgress = percent.Percent;
                BusyMessage = percent.Message;
            });
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
        }
    }
}
