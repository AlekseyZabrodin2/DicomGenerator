using System.Collections.Generic;
using FellowOakDicom;

namespace DicomGenerator.Core
{
    public class PatientGenerator
    {
        public IEnumerable<DicomDataset> Generate(
            PatientGeneratorParameters patientGeneratorParameters)
        {
            var dataSets = new List<DicomDataset>();

            for (var patientIndex = 0; patientIndex < patientGeneratorParameters.PatientsCount; patientIndex++)
            {
                //Generate PatientIod
                var patientIod = GeneratePatientIod(patientIndex, patientGeneratorParameters);
                var studyGenerator = new StudyGenerator();

                foreach (var studyParameters in patientGeneratorParameters.StudyGeneratorParameters)
                {
                    dataSets.AddRange(studyGenerator.Generate(patientIod, studyParameters));
                }
            }

            return dataSets;
        }

        private DicomDataset GeneratePatientIod(int patientIndex, PatientGeneratorParameters patientGeneratorParameters)
        {
            var dataset = new DicomDataset();

            CreatePatientModuleRequied(dataset, patientIndex, patientGeneratorParameters);

            CreatePatientModuleOptional(dataset, patientIndex, patientGeneratorParameters);

            CreatePatientStudyModule(dataset, patientIndex, patientGeneratorParameters);

            return dataset;
        }

        private void CreatePatientModuleRequied(DicomDataset dataset, int patientIndex, PatientGeneratorParameters patientGeneratorParameters)
        {
            dataset.AddOrUpdate(DicomTag.SpecificCharacterSet, patientGeneratorParameters.DicomEncodingRule.Generate());

            dataset.AddOrUpdate(DicomTag.PatientName, patientGeneratorParameters.NameRule.Generate());
            dataset.AddOrUpdate(DicomTag.PatientID, patientGeneratorParameters.IdRule.Generate(patientIndex));
            dataset.AddOrUpdate(DicomTag.PatientSex, patientGeneratorParameters.Sex.Generate());
            dataset.AddOrUpdate(DicomTag.PatientBirthDate, patientGeneratorParameters.PatientBirthDate.Generate());
            dataset.AddOrUpdate(DicomTag.PatientAddress, patientGeneratorParameters.PatientRandomAddress.Generate());
            dataset.AddOrUpdate(DicomTag.PatientComments, patientGeneratorParameters.PatientRandomComments.Generate());
            dataset.AddOrUpdate(DicomTag.PatientTelephoneNumbers, patientGeneratorParameters.PatientRandomTelephone.Generate());
        }

        private void CreatePatientModuleOptional(DicomDataset dataset, int patientIndex, PatientGeneratorParameters patientGeneratorParameters)
        {
            if (patientGeneratorParameters.EthnicGroupRule != null)
            {
                dataset.AddOrUpdate(DicomTag.EthnicGroup, patientGeneratorParameters.EthnicGroupRule.Generate());
            }
        }

        private void CreatePatientStudyModule(DicomDataset dataset, int patientIndex, PatientGeneratorParameters patientGeneratorParameters)
        {
            if (patientGeneratorParameters.PatientAge != null)
            {
                dataset.AddOrUpdate(DicomTag.PatientAge, patientGeneratorParameters.PatientAge.Generate());
            }
        }
    }
}