using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using DicomGenerator.Core;
using DicomGenerator.Core.GeneratorRules._3_Series;
using DicomGenerator.Core.GeneratorRules.Series;
using DicomGenerator.Core.GeneratorRules.Patient;
using DicomGenerator.Core.GeneratorRules.Sop;
using DicomGenerator.Core.GeneratorRules.Study;
using FellowOakDicom;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DicomGenerator.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
           var serieParametersDx = new SerieGeneratorParameters(
                    new SeriesLateralityRule(),
                    Modality.Dx,
                    new SeriesNumberRule(),
                    new RangeSeriesDateTimeRule(),
                    new SopGeneratorParameters(
                        new SopClassUidRule(new SopClassFactory(@"D:\Develop\DicomGeneratorTestData\")))
                    {
                        SopCount = 1
                    }
                )
           {
               SeriesCount = 1
           };

            var serieParametersMg = new SerieGeneratorParameters(
                    new SeriesLateralityRule(),
                    Modality.Mg,
                    new SeriesNumberRule(),
                    new RangeSeriesDateTimeRule(),
                    new SopGeneratorParameters(
                        new SopClassUidRule(new SopClassFactory(@"D:\Develop\DicomGeneratorTestData\")))
                    {
                        SopCount = 0
                    }
                )
            {
                SeriesCount = 1
            };

            var serieParametersSr = new SerieGeneratorParameters(
                    new SeriesLateralityRule(),
                    Modality.Sr,
                    new SeriesNumberRule(),
                    new RangeSeriesDateTimeRule(),
                    new SopGeneratorParameters(
                            new SopClassUidRule(new SopClassFactory(@"D:\Develop\DicomGeneratorTestData\")))
                    {
                        SopCount = 0
                    }
                )
            {
                SeriesCount = 1
            };

            var studyParameters = new StudyGeneratorParameters(
                new RangeStudyDateTimeRule(DateTime.Now, DateTime.Now.AddDays(10)),
                new RandomAccessionNumberRule(),
                new List<SerieGeneratorParameters>{serieParametersDx,serieParametersMg,serieParametersSr}
            )
            {
                StudiesCount = 1
            };

            var patientParameters = new PatientGeneratorParameters(
                new DicomEncodingRule(Encoding.UTF8),
                new RandomNameRule(),
                new OrderedIdRule(),
                new RandomSexRule(),
                new RandomPatientBirthDateRule(new DateTime()),
                new List<StudyGeneratorParameters>{studyParameters}
            )
            {
                PatientsCount = 1,
                EthnicGroupRule = new ListEthnicGroupRule(),
                PatientAge = new PatientAgeRule()
            };

            var patientGenerator = new PatientGenerator();
            var dataSets =
                patientGenerator.Generate(patientParameters);

            foreach (var dataset in dataSets)
            {
                var dicomFIle = new DicomFile(dataset);
                dicomFIle.Save(Guid.NewGuid().ToString());
            }

        }

        [TestMethod]
        public void GenerateName()
        {
            var name = new RandomNameRule();

            var result = name.Generate();


            Console.WriteLine(name);
        }

        [TestMethod]
        public void GenerateLaterality()
        {
            var laterality = new SeriesLateralityRule();

            var result = laterality.Generate();


            Console.WriteLine(result);
        }

        [TestMethod]
        public void GenerateStudyDate()
        {
          var studyDate = new RangeStudyDateTimeRule(DateTime.MinValue, DateTime.MaxValue);

          var study = studyDate.Generate();

          Console.WriteLine(study);
          
        }

        [TestMethod]
        public void GenerateSerialTime()
        {
            //var seriasTime = new RangeSeriesDateTimeRule();

            var studyTime = new RangeSeriesDateTimeRule();

            var seriasTime = studyTime.Generate();


            Console.WriteLine(seriasTime);
        }
    }
}

