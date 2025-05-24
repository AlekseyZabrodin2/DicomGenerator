using System;
using System.Collections.Generic;
using DicomGenerator.Core.GeneratorRules.Study;
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
            dataset.AddOrUpdate(DicomTag.OperatorsName, OperatorsNameGenerator.GenerateOperatorsName());
            dataset.AddOrUpdate(DicomTag.StudyID, StudyIdGenerator.GenerateStudyId());
            dataset.AddOrUpdate(DicomTag.Manufacturer, "AutoGenFILE");


            BodyPartRandom.GenerateRegion();
            BodyPartRandom.GenerateBodyPositions();

            var regionItem = new DicomDataset
            {
                { DicomTag.CodeValue, BodyPartRandom.RegionCodeValue },
                { DicomTag.CodingSchemeDesignator, BodyPartRandom.RegionCodingSchemeDesignator },
                { DicomTag.CodeMeaning, BodyPartRandom.RegionCodeMeaning }
            };
            dataset.AddOrUpdate(DicomTag.AnatomicRegionSequence, regionItem);

            var viewItem = new DicomDataset
            {
                { DicomTag.CodeValue, BodyPartRandom.ViewCodeValue },
                { DicomTag.CodingSchemeDesignator, BodyPartRandom.ViewCodingSchemeDesignator },
                { DicomTag.CodeMeaning, BodyPartRandom.ViewCodeMeaning },
                { DicomTag.ContextUID, BodyPartRandom.ViewContextUID }
            };
            dataset.AddOrUpdate(DicomTag.ViewCodeSequence, viewItem);
            dataset.AddOrUpdate(DicomTag.BodyPartExamined, BodyPartRandom.BodyPartExamined);
            dataset.AddOrUpdate(DicomTag.ImageLaterality, BodyPartRandom.GenerateLaterality());

            return dataset;
        }
    }
}