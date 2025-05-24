using System.Collections.Generic;
using DicomGenerator.Core.GeneratorRules.Series;
using FellowOakDicom;

namespace DicomGenerator.Core
{
    internal class SerieGenerator
    {
        public IEnumerable<DicomDataset> Generate(
            DicomDataset patientIod, 
            DicomDataset studyIod, 
            SerieGeneratorParameters serieGeneratorParameters)
        {
            var dataSets = new List<DicomDataset>();

            for (var seriesCount = 0; seriesCount < serieGeneratorParameters.SeriesCount; seriesCount++)
            {
                var serieIod = GenerateSerieIod(seriesCount, serieGeneratorParameters);
                var sopGenerator = new SopGenerator();

                dataSets.AddRange(sopGenerator.Generate(patientIod, studyIod, serieIod,serieGeneratorParameters.Modality.SopClassUid, serieGeneratorParameters.SopGeneratorParameters));
            }

            return dataSets;
        }

        private DicomDataset GenerateSerieIod(int seriesCount, SerieGeneratorParameters serieGeneratorParameters)
        {
            var dataset = new DicomDataset();

            CreateSeriesModuleRequed(dataset, seriesCount, serieGeneratorParameters);

            CreateSeriesModuleOptional(dataset, seriesCount, serieGeneratorParameters);

            return dataset;
        }

        private void CreateSeriesModuleRequed(DicomDataset dataset, int seriesCount, SerieGeneratorParameters serieGeneratorParameters)
        {
            dataset.AddOrUpdate(DicomTag.Laterality, serieGeneratorParameters.SeriesLaterality.Generate());
            dataset.AddOrUpdate(DicomTag.Modality, serieGeneratorParameters.Modality.Name);
            dataset.AddOrUpdate(DicomTag.SeriesNumber, serieGeneratorParameters.SeriesNumber.Generate(seriesCount));
            dataset.AddOrUpdate(DicomTag.SeriesInstanceUID, UidGenerator.GenerateSeriesUid());
            dataset.AddOrUpdate(DicomTag.SeriesTime, serieGeneratorParameters.SeriesTime.Generate());
            dataset.AddOrUpdate(DicomTag.AcquisitionTime, serieGeneratorParameters.SeriesTime.Generate());

            var effectiveDoseTag = new DicomTag(0x0031, 0x0201);
            dataset.AddOrUpdate(new DicomDecimalString(effectiveDoseTag, EntranceDoseRandom.GenerateDose()));
        }

        private void CreateSeriesModuleOptional(DicomDataset dataset, int seriesCount, SerieGeneratorParameters serieGeneratorParameters)
        {

        }
    }
}