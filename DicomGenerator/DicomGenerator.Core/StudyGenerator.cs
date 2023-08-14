using System;
using System.Collections.Generic;
using FellowOakDicom;

namespace DicomGenerator.Core
{
    internal class StudyGenerator
    {
        public IEnumerable<DicomDataset> Generate(
            DicomDataset patientIod, 
            StudyGeneratorParameters studyGeneratorParameters)
        {
            var dataSets = new List<DicomDataset>();

            for (var studyCount = 0; studyCount < studyGeneratorParameters.StudiesCount; studyCount++)
            {
                var studyIod = GenerateStudyIod(studyGeneratorParameters);
                var seriesGenerator = new SerieGenerator();

                foreach (var serieGeneratorParameter in studyGeneratorParameters.SerieGeneratorParameters)
                {
                    dataSets.AddRange(seriesGenerator.Generate(patientIod, studyIod, serieGeneratorParameter));
                }
            }

            return dataSets;
        }

        private DicomDataset GenerateStudyIod(StudyGeneratorParameters studyGeneratorParameters)
        {
            var dataset = new DicomDataset();

            var studyDateTime = studyGeneratorParameters.StudyDateTimeGeneratorRule.Generate();

            dataset.AddOrUpdate(DicomTag.StudyDate, studyDateTime.Date);
            dataset.AddOrUpdate(DicomTag.StudyTime, new DateTime(studyDateTime.TimeOfDay.Ticks));
            dataset.AddOrUpdate(DicomTag.AccessionNumber, studyGeneratorParameters.AccessionNumber.Generate());
            dataset.AddOrUpdate(DicomTag.StudyInstanceUID, UidGenerator.GenerateStudyUid());

            return dataset;

            
        }
    }
}