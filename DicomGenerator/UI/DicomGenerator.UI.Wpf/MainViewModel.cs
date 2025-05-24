using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using DicomGenerator.Core;
using DicomGenerator.Core.GeneratorRules;
using DicomGenerator.Core.GeneratorRules.Patient;
using DicomGenerator.Core.GeneratorRules.Series;
using DicomGenerator.Core.GeneratorRules.Sop;
using DicomGenerator.Core.GeneratorRules.Study;
using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using Prism.Commands;


namespace DicomGenerator.UI.Wpf
{
    partial class MainViewModel : ObservableObject
    {
        private PatientGeneratorParameters _patientParameters;
        private PatientGenerator _patientGenerator;
        private DicomEncodingRule _encoding;
        private IGeneratorRule<string> _nameRule;
        private IGeneratorRule<string, int> _idRule;
        private IGeneratorRule<string> _sex;
        private IGeneratorRule<DateTime> _patientBirthDate;
        private IList<StudyGeneratorParameters> _StudyGeneratorParameters;

        private StudyGeneratorParameters _studyParameters;
        private IGeneratorRule<DateTime> _studyDateTimeGeneratorRule;
        private IGeneratorRule<string> _accessionNumber;
        private IList<SerieGeneratorParameters> _SerieGeneratorParameters;

        private SerieGeneratorParameters _serieParameters;
        private SerieGeneratorParameters _serieParametersSr;
        private SerieGeneratorParameters _serieParametersMg;
        private SerieGeneratorParameters _serieParametersDx;
        private SopGeneratorParameters _sopGeneratorParameters;
        private Modality _modality;
        private IGeneratorRule<string> _seriesLaterality;
        private IGeneratorRule<string, int> _seriesNumber;
        private IGeneratorRule<DateTime> _seriesTime;
        private CancellationTokenSource _cancellationTokenSource;
        private DicomEncodingRule _defaultEncoding = new DicomEncodingRule(Encoding.UTF8);

        private int _patientsCount;
        private int _studiesCount;
        private int _seriesCount;
        private int _sopDxCount;
        private int _sopMgCount;
        private int _sopSrCount;

        private Visibility _isVisible;

        public int sumCounts;

        // Path to TestData and Save File in Debug
        private readonly string _pathToSave = @"D:\Develop\DicomGeneratorTestData\TestResult";

        private readonly string _pathToTestData = @"D:\Develop\DicomGeneratorTestData\";


        // Path to TestData and Save File after Install
        //private readonly string _pathToSave = @"D:\DicomGeneratorResult";

        //private readonly string _pathToTestData = @"C:\Program Files (x86)\DicomGenerator\DicomGeneratorTestData\";


        public MainViewModel()
        {
            Encodings = new ObservableCollection<DicomEncodingRule>
            {
                new DicomEncodingRule(Encoding.Latin1),
                new DicomEncodingRule(Encoding.ASCII),
                _defaultEncoding
            };

            ChooseCod = _defaultEncoding;

            IsVisible = Visibility.Collapsed;

            InitialiseProperties();
            InitialiseServerConnectingProperty();
        }



        [ObservableProperty]
        public partial int SetPatientsCount {  get; set; }

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
        public partial DicomEncodingRule ChooseCod { get; set; }

        public ObservableCollection<DicomEncodingRule> Encodings { get; }

        [ObservableProperty]
        public partial string UpdateText { get; set; }

        [ObservableProperty]
        public partial Visibility IsVisible { get; set; }

        [ObservableProperty]
        public partial string CreateIdPatient { get; set; }

        [ObservableProperty]
        public partial string CreateLastNamePatient { get; set; }

        [ObservableProperty]
        public partial string CreateNamePatient { get; set; }

        [ObservableProperty]
        public partial string CreateMiddleNamePatient { get; set; }

        [ObservableProperty]
        public partial DateTime CreateBirthDatePatient { get; set; }

        public bool _useBirthDatePatient;
        public bool UseBirthDatePatient 
        {
            get => _useBirthDatePatient;
            set
            {
                UsePeriodBirthDatePatient = _useBirthDatePatient;
                SetProperty(ref _useBirthDatePatient, value);
            }
        }

        [ObservableProperty]
        public partial DateTime CreatePeriodBirthDatePatient { get; set; }

        [ObservableProperty]
        public partial DateTime SelectedBirthDatePatient { get; set; }

        [ObservableProperty]
        public partial bool UsePeriodBirthDatePatient { get; set; }

        [ObservableProperty]
        public partial List<string> CreateGenderPatient { get; set; }

        private string? _gender;
        public string? SelectedGender
        {
            get => _gender;

            set
            {
                if (value == CreateGenderPatient[0])
                {
                    value = "M";
                }
                else if (value == CreateGenderPatient[1])
                {
                    value = "F";
                }
                else if (value == CreateGenderPatient[2])
                {
                    value = "O";
                }
                else if (value == CreateGenderPatient[3])
                {
                    value = null;
                }
                SetProperty(ref _gender, value);
            }
        }

        [ObservableProperty]
        public partial string CreateAdressPatient { get; set; }

        [ObservableProperty]
        public partial int CreatePhonePatient { get; set; }

        [ObservableProperty]
        public partial string CreateWorkPlasePatient { get; set; }

        [ObservableProperty]
        public partial string CreateInfoPatient { get; set; }

        [ObservableProperty]
        public partial DateTime CreateStartDatePatient { get; set; }

        [ObservableProperty]
        public partial DateTime CreateEndDatePatient { get; set; }

        [ObservableProperty]
        public partial DateTime CreateStartTimePatient { get; set; }

        [ObservableProperty]
        public partial DateTime CreateEndTimePatient { get; set; }

        [ObservableProperty]
        public partial string CreateServerIp { get; set; }

        [ObservableProperty]
        public partial int CreateServerPort { get; set; }

        [ObservableProperty]
        public partial string CreateCallingAe { get; set; }

        [ObservableProperty]
        public partial string CreateCalledAe { get; set; }

        [ObservableProperty]
        public partial string CreateSourceFolderPath { get; set; }

        [ObservableProperty]
        public partial string ServerOutputText { get; set; }





        private void InitialiseProperties()
        {
            CreateGenderPatient = new List<string> { "Man", "Female", "Other", "string - Empty" };
            UseBirthDatePatient = false;
            UsePeriodBirthDatePatient = !UseBirthDatePatient;
            CreateBirthDatePatient = DateTime.Now;
            CreatePeriodBirthDatePatient = DateTime.Now.AddYears(-50);
            CreateStartDatePatient = DateTime.Now.AddMonths(-3);
            CreateEndDatePatient = DateTime.Now;
            CreateEndTimePatient = DateTime.Now;
        }

        private void InitialiseServerConnectingProperty()
        {
            CreateServerIp = "127.0.0.1";
            CreateServerPort = 55100;
            CreateCallingAe = "UniExpert";
            CreateCalledAe = "LocalScp";
            CreateSourceFolderPath = "D:\\DicomGeneratorResult\\";
        }

        private DateTime SelectedPatientBirthDate()
        {
            if (UseBirthDatePatient)
            {
                return CreateBirthDatePatient;
            }

            return CreatePeriodBirthDatePatient;
        }


        private DelegateCommand generateFile_Click;
        public ICommand GenerateFile_Click => generateFile_Click ??= new DelegateCommand(PerformGenerateFile_Click);

        private async void PerformGenerateFile_Click()
        {
            IsVisible = Visibility.Visible;

            await Task.Factory.StartNew(() =>
            {

                _serieParametersDx = new SerieGeneratorParameters(
              new SeriesLateralityRule(),
              Modality.Dx,
              new SeriesNumberRule(),
              new RangeSeriesDateTimeRule(CreateStartTimePatient, CreateEndTimePatient),
              new SopGeneratorParameters(
                  new SopClassUidRule(new SopClassFactory(_pathToTestData)))
              {
                  SopCount = SopDxCount
              })
                { SeriesCount = SetSeriesCount };


                _serieParametersMg = new SerieGeneratorParameters(
                    new SeriesLateralityRule(),
                    Modality.Mg,
                    new SeriesNumberRule(),
                    new RangeSeriesDateTimeRule(CreateStartTimePatient, CreateEndTimePatient),
                    new SopGeneratorParameters(
                        new SopClassUidRule(new SopClassFactory(_pathToTestData)))
                    {
                        SopCount = SopMgCount
                    })
                { SeriesCount = SetSeriesCount };

                _serieParametersSr = new SerieGeneratorParameters(
                    new SeriesLateralityRule(),
                    Modality.Sr,
                    new SeriesNumberRule(),
                    new RangeSeriesDateTimeRule(CreateStartTimePatient, CreateEndTimePatient),
                    new SopGeneratorParameters(
                        new SopClassUidRule(new SopClassFactory(_pathToTestData)))
                    {
                        SopCount = SopSrCount
                    })
                { SeriesCount = SetSeriesCount };


                _studyParameters = new StudyGeneratorParameters(
                    new RangeStudyDateTimeRule(CreateStartDatePatient, CreateEndDatePatient),
                    new RandomAccessionNumberRule(),
                    new List<SerieGeneratorParameters>() { _serieParametersDx, _serieParametersMg, _serieParametersSr });

                _patientParameters = new PatientGeneratorParameters(
                    ChooseCod,
                    new RandomNameRule(CreateLastNamePatient, CreateNamePatient, CreateMiddleNamePatient),                    
                    new OrderedIdRule(CreateIdPatient),
                    new RandomSexRule(SelectedGender),
                    new PatientAddressRule(CreateAdressPatient),
                    new PatientCommentsRule(CreateInfoPatient),
                    new PatientTelephoneRule(CreatePhonePatient),
                    new RandomPatientBirthDateRule(SelectedPatientBirthDate(), UsePeriodBirthDatePatient),
                    new List<StudyGeneratorParameters>() { _studyParameters }
                )
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

                sumCounts = SetPatientsCount * (SetStudiesCount * (SetSeriesCount * (SopDxCount + SopMgCount + SopSrCount)));

                _patientGenerator = new PatientGenerator();

                var dataSets = _patientGenerator.Generate(_patientParameters);

                foreach (var dataset in dataSets)
                {
                    var dicomFIle = new DicomFile(dataset);

                    DirectoryInfo directoryInfo = new DirectoryInfo(_pathToSave);

                    if (!directoryInfo.Exists)
                    {
                        MessageBox.Show("              Folder not found. \nBut i'm smart and did it for you !");

                        directoryInfo.Create();
                    }

                    dicomFIle.Save(Path.Combine(_pathToSave, Guid.NewGuid().ToString()));

                }

                if (sumCounts == 1)
                {
                    UpdateText = $"Done, generated {sumCounts} file !!!";
                }

                if (sumCounts > 1)
                {
                    UpdateText = $"Done, generated {sumCounts} files !!!";
                }

                IsVisible = Visibility.Collapsed;

            });
        }


        private DelegateCommand cleanUp_Click;
        public ICommand CleanUp_Click => cleanUp_Click = new DelegateCommand(PerformCleanUp_Click);

        private void PerformCleanUp_Click()
        {
            SetPatientsCount = 0;
            SetStudiesCount = 0;
            SetSeriesCount = 0;
            SopMgCount = 0;
            SopDxCount = 0;
            SopSrCount = 0;

            CreateIdPatient = string.Empty;
            CreateLastNamePatient = string.Empty;
            CreateNamePatient = string.Empty;
            CreateMiddleNamePatient = string.Empty;
            SelectedGender = CreateGenderPatient[3];

            UseBirthDatePatient = false;
            CreateBirthDatePatient = DateTime.Now;
            CreatePeriodBirthDatePatient = DateTime.Now.AddYears(-50);

            CreateAdressPatient = string.Empty;
            CreatePhonePatient = 0;
            CreateInfoPatient = string.Empty;

            CreateStartDatePatient = DateTime.Now.AddMonths(-3);
            CreateEndDatePatient = DateTime.Now;
            CreateStartTimePatient = DateTime.Now;
            CreateStartTimePatient = DateTime.Parse("00:00");
            CreateEndTimePatient = DateTime.Now;

            ChooseCod = _defaultEncoding;

            UpdateText = "All fields are cleared !!!";
        }

        private DelegateCommand sendDicomImageAsync;
        public ICommand SendDicomImage => sendDicomImageAsync = new DelegateCommand(PerformSendDicomImage);

        private void PerformSendDicomImage()
        {
            SendDicomImageAsync(CreateServerIp, CreateServerPort, CreateSourceFolderPath, CreateCallingAe, CreateCalledAe);
        }

        private async Task SendDicomImageAsync(string serverIp, int serverPort, string sourceFolderPath, string callingAe, string calledAe)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            var options = new DicomClientOptions { AssociationRequestTimeoutInMs = 10000 };

            var client = DicomClientFactory.Create(serverIp, serverPort, false, callingAe, calledAe);

            var dicomFiles = Directory.GetFiles(sourceFolderPath, "*.*");

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
            }
        }

        private DelegateCommand cancelSending;
        public ICommand CancaelSendingImage => cancelSending = new DelegateCommand(PerformCancaelSendingImage);

        private void PerformCancaelSendingImage()
        {
            _cancellationTokenSource?.Cancel();
        }



    }
}