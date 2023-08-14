using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using DicomGenerator.Core;
using DicomGenerator.Core.GeneratorRules._3_Series;
using DicomGenerator.Core.GeneratorRules.Patient;
using DicomGenerator.Core.GeneratorRules.Series;
using DicomGenerator.Core.GeneratorRules.Sop;
using DicomGenerator.Core.GeneratorRules.Study;
using FellowOakDicom;
using Prism.Commands;
using Prism.Mvvm;


namespace DicomGenerator.UI.Wpf
{
    public class MainViewModel : BindableBase
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

        private int _patientsCount;
        private int _studiesCount;
        private int _seriesCount;
        private int _sopDxCount;
        private int _sopMgCount;
        private int _sopSrCount;

        private Visibility _isVisible;

        public int sumCounts;

        // Path to TestData and Save File in Debug
        //private readonly string _pathToSave = @"D:\Develop\DicomGeneratorTestData\TestResult";

        //private readonly string _pathToTestData = @"D:\Develop\DicomGeneratorTestData\";


        // Path to TestData and Save File after Install
        private readonly string _pathToSave = @"D:\DicomGeneratorResult";

        private readonly string _pathToTestData = @"C:\Program Files (x86)\DicomGenerator\DicomGeneratorTestData\";


        public MainViewModel()
        {
            var defaultEncoding = new DicomEncodingRule(Encoding.UTF8);

            Encodings = new ObservableCollection<DicomEncodingRule>
            {
                new DicomEncodingRule(Encoding.Latin1),
                new DicomEncodingRule(Encoding.ASCII),
                defaultEncoding
            };

            ChooseCod = defaultEncoding;

            IsVisible = Visibility.Collapsed;
        }


        public int SetPatientsCount
        {
            get
            {
                return _patientsCount;
            }
            set
            {
                SetProperty(ref _patientsCount, value);
            }
        }

        public int SetStudiesCount
        {
            get
            {
                return _studiesCount;
            }
            set
            {
                SetProperty(ref _studiesCount, value);
            }
        }

        public int SetSeriesCount
        {
            get
            {
                return _seriesCount;
            }
            set
            {
                SetProperty(ref _seriesCount, value);
            }
        }

        public int SopSrCount
        {
            get
            {
                return _sopSrCount;
            }
            set
            {
                SetProperty(ref _sopSrCount, value);
            }
        }

        public int SopDxCount
        {
            get
            {
                return _sopDxCount;
            }
            set
            {
                SetProperty(ref _sopDxCount, value);
            }
        }

        public int SopMgCount
        {
            get
            {
                return _sopMgCount;
            }
            set
            {
                SetProperty(ref _sopMgCount, value);
            }
        }


        public DicomEncodingRule ChooseCod
        {
            get
            {
                return _encoding;
            }
            set
            {
                SetProperty(ref _encoding, value);
            }
        }

        public ObservableCollection<DicomEncodingRule> Encodings { get; }


        private string _updateText;

        public string UpdateText
        {
            get
            {
                return _updateText;
            }
            set
            {
                SetProperty(ref _updateText, value);
            }
        }


        public Visibility IsVisible
        {
            get => _isVisible;

            set
            {
                SetProperty(ref _isVisible, value);
            }
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
              new RangeSeriesDateTimeRule(),
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
                    new RangeSeriesDateTimeRule(),
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
                    new RangeSeriesDateTimeRule(),
                    new SopGeneratorParameters(
                        new SopClassUidRule(new SopClassFactory(_pathToTestData)))
                    {
                        SopCount = SopSrCount
                    })
                { SeriesCount = SetSeriesCount };


                _studyParameters = new StudyGeneratorParameters(
                    new RangeStudyDateTimeRule(DateTime.Now, DateTime.Now.AddHours(10)),
                    new RandomAccessionNumberRule(),
                    new List<SerieGeneratorParameters>() { _serieParametersDx, _serieParametersMg, _serieParametersSr });

                _patientParameters = new PatientGeneratorParameters(
                    ChooseCod,
                    new RandomNameRule(),
                    new OrderedIdRule(),
                    new RandomSexRule(),
                    new RandomPatientBirthDateRule(DateTime.Now),
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

            UpdateText = "All fields are cleared !!!";

        }







    }
}