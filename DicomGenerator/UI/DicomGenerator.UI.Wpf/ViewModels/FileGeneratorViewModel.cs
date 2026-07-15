using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
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
        public bool _useBirthDatePatient;
        private string? _gender; 
        public int _sumCounts;
        private readonly string _pathToSave = @"D:\DicomGeneratorResult";


        // Path to TestData in Debug
        private readonly string _pathToTestData = @"D:\Develop\DicomGeneratorTestData\";


        // Path to TestData after Install
        //private readonly string _pathToTestData = @"C:\Program Files (x86)\DicomGenerator\DicomGeneratorTestData\";



        public ObservableCollection<DicomEncodingRule> Encodings { get; }

        [ObservableProperty]
        public partial Visibility IsVisible { get; set; }

        [ObservableProperty]
        public partial DicomEncodingRule ChooseCod { get; set; }

        [ObservableProperty]
        public partial List<string> GenderPatient { get; set; }

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
        public partial string AdressPatient { get; set; }

        [ObservableProperty]
        public partial int PhonePatient { get; set; }

        [ObservableProperty]
        public partial string WorkPlasePatient { get; set; }

        [ObservableProperty]
        public partial string InfoPatient { get; set; }


        [ObservableProperty]
        public partial string UpdateText { get; set; }



        public bool UseBirthDatePatient
        {
            get => _useBirthDatePatient;
            set
            {
                UsePeriodBirthDatePatient = _useBirthDatePatient;
                SetProperty(ref _useBirthDatePatient, value);
            }
        }

        public string? SelectedGender
        {
            get => _gender;

            set
            {
                if (value == GenderPatient[0])
                {
                    value = "M";
                }
                else if (value == GenderPatient[1])
                {
                    value = "F";
                }
                else if (value == GenderPatient[2])
                {
                    value = "O";
                }
                else if (value == GenderPatient[3])
                {
                    value = null;
                }
                SetProperty(ref _gender, value);
            }
        }









        public FileGeneratorViewModel()
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
        }



        private void InitialiseProperties()
        {
            GenderPatient = new List<string> { "Man", "Female", "Other", "string - Empty" };
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
            IsVisible = Visibility.Visible;

            await Task.Factory.StartNew(() =>
            {

                _serieParametersDx = new SerieGeneratorParameters(
              new SeriesLateralityRule(),
              Modality.Dx,
              new SeriesNumberRule(),
              new RangeSeriesDateTimeRule(StartTimePatient, EndTimePatient),
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
                    new RangeSeriesDateTimeRule(StartTimePatient, EndTimePatient),
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
                    new RangeSeriesDateTimeRule(StartTimePatient, EndTimePatient),
                    new SopGeneratorParameters(
                        new SopClassUidRule(new SopClassFactory(_pathToTestData)))
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
                    new RandomSexRule(SelectedGender),
                    new PatientAddressRule(AdressPatient),
                    new PatientCommentsRule(InfoPatient),
                    new PatientTelephoneRule(PhonePatient),
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

                _sumCounts = SetPatientsCount * (SetStudiesCount * (SetSeriesCount * (SopDxCount + SopMgCount + SopSrCount)));

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

                if (_sumCounts == 1)
                {
                    UpdateText = $"Done, generated {_sumCounts} file !!!";
                }

                if (_sumCounts > 1)
                {
                    UpdateText = $"Done, generated {_sumCounts} files !!!";
                }

                IsVisible = Visibility.Collapsed;

            });
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

            AdressPatient = string.Empty;
            PhonePatient = 0;
            InfoPatient = string.Empty;

            StartDatePatient = DateTime.Now.AddMonths(-3);
            EndDatePatient = DateTime.Now;
            StartTimePatient = DateTime.Now;
            StartTimePatient = DateTime.Parse("00:00");
            EndTimePatient = DateTime.Now;

            ChooseCod = _defaultEncoding;

            UpdateText = "All fields are cleared !!!";
        }
    }
}
