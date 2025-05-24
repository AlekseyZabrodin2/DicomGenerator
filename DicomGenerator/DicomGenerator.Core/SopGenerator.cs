using System.Collections.Generic;
using DicomGenerator.Core.GeneratorRules.Sop;
using FellowOakDicom;

namespace DicomGenerator.Core
{
    internal class SopGenerator
    {
       
        internal IEnumerable<DicomDataset> Generate(DicomDataset patientIod, DicomDataset studyIod,
            DicomDataset serieIod, SopClassUid modalitySopClassUid, SopGeneratorParameters sopGeneratorParameters)
        {
            var dataSets = new List<DicomDataset>();

            for (var sopCount = 0; sopCount < sopGeneratorParameters.SopCount; sopCount++)
            {
                var sopIod = GenerateSopIod(serieIod,sopGeneratorParameters,modalitySopClassUid);
                
                dataSets.Add(CombineDatasets(patientIod, studyIod, serieIod, sopIod));
            }

            return dataSets;
        }

        private DicomDataset CombineDatasets(DicomDataset patientIod, DicomDataset studyIod, DicomDataset serieIod, DicomDataset sopIod)
        {
            var dataSet = new DicomDataset();

            patientIod.CopyTo(dataSet);
            studyIod.CopyTo(dataSet);
            serieIod.CopyTo(dataSet);
            sopIod.CopyTo(dataSet);

            return dataSet;
        }

        private DicomDataset GenerateSopIod(DicomDataset serieIod, SopGeneratorParameters sopGeneratorParameters,SopClassUid sopClassUid)
        {
           return sopGeneratorParameters.SopClassUidRule.Generate(sopClassUid);
        }
    }
}
