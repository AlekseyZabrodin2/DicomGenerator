using System.Collections.Generic;
using System.Threading;
using FellowOakDicom;

namespace DicomGenerator.Core.DicomGeneratorModels
{
    public class PatientGenerator
    {
        public IEnumerable<DicomDataset> Generate(int patientIndex,
            PatientGeneratorParameters patientGeneratorParameters,
            CancellationToken cancellationToken = default)
        {
            var dataSets = new List<DicomDataset>();

            var patientIod = GeneratePatientIod(patientIndex, patientGeneratorParameters, cancellationToken);
            var studyGenerator = new StudyGenerator();

            cancellationToken.ThrowIfCancellationRequested();

            foreach (var studyParameters in patientGeneratorParameters.StudyGeneratorParameters)
            {
                cancellationToken.ThrowIfCancellationRequested();

                dataSets.AddRange(studyGenerator.Generate(patientIod, studyParameters));
            }
            return dataSets;
        }

        private DicomDataset GeneratePatientIod(int patientIndex, 
            PatientGeneratorParameters patientGeneratorParameters,
            CancellationToken cancellationToken = default)
        {
            var dataset = new DicomDataset();

            CreatePatientModuleRequied(dataset, patientIndex, patientGeneratorParameters, cancellationToken);
            CreatePatientModuleOptional(dataset, patientGeneratorParameters, cancellationToken);
            CreatePatientStudyModule(dataset, patientGeneratorParameters, cancellationToken);

            return dataset;
        }

        private void CreatePatientModuleRequied(DicomDataset dataset, int patientIndex, 
            PatientGeneratorParameters patientGeneratorParameters,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            dataset.AddOrUpdate(DicomTag.SpecificCharacterSet, patientGeneratorParameters.DicomEncodingRule.Generate());

            dataset.AddOrUpdate(DicomTag.PatientName, patientGeneratorParameters.NameRule.Generate());
            dataset.AddOrUpdate(DicomTag.PatientID, patientGeneratorParameters.IdRule.Generate(patientIndex));
            dataset.AddOrUpdate(DicomTag.PatientSex, patientGeneratorParameters.Sex.Generate());
            dataset.AddOrUpdate(DicomTag.PatientBirthDate, patientGeneratorParameters.PatientBirthDate.Generate());
            dataset.AddOrUpdate(DicomTag.PatientAddress, patientGeneratorParameters.PatientRandomAddress.Generate());
            dataset.AddOrUpdate(DicomTag.PatientComments, patientGeneratorParameters.PatientRandomComments.Generate());
            dataset.AddOrUpdate(DicomTag.PatientTelephoneNumbers, patientGeneratorParameters.PatientRandomTelephone.Generate());
        }

        private void CreatePatientModuleOptional(DicomDataset dataset, 
            PatientGeneratorParameters patientGeneratorParameters,
            CancellationToken cancellationToken = default)
        {
            if (patientGeneratorParameters.EthnicGroupRule != null)
            {
                cancellationToken.ThrowIfCancellationRequested();

                dataset.AddOrUpdate(DicomTag.EthnicGroup, patientGeneratorParameters.EthnicGroupRule.Generate());
            }
        }

        private void CreatePatientStudyModule(DicomDataset dataset, 
            PatientGeneratorParameters patientGeneratorParameters,
            CancellationToken cancellationToken = default)
        {
            if (patientGeneratorParameters.PatientAge != null)
            {
                cancellationToken.ThrowIfCancellationRequested();

                dataset.AddOrUpdate(DicomTag.PatientAge, patientGeneratorParameters.PatientAge.Generate());
            }
        }
    }
}