using System;
using System.Diagnostics;
using System.Globalization;
using DicomGenerator.Core.Enums;
using DicomGenerator.Core.LiteDbModels;
using FellowOakDicom;
using LiteDB;

namespace DicomGenerator.Core.DicomFileParser
{
    public class DicomParser
    {
        public DicomFileInfo ParseFile(string filePath)
        {
            try
            {
                var dicomFile = DicomFile.Open(filePath);
                var dataset = dicomFile.Dataset;


                var patient = new PatientLiteDb()
                {
                    Id = ObjectId.NewObjectId().ToString(),
                    PatientID = dataset.GetStringValue(DicomTag.PatientID),
                    LastName = ParseDicomName(dataset.GetStringValue(DicomTag.PatientName, string.Empty)).lastName,
                    FirstName = ParseDicomName(dataset.GetStringValue(DicomTag.PatientName, string.Empty)).firstName,
                    MiddleName = ParseDicomName(dataset.GetStringValue(DicomTag.PatientName, string.Empty)).middleName,
                    BirthDate = dataset.GetDateTimeValue(DicomTag.PatientBirthDate),
                    Sex = ParsePatientSex(dataset.GetStringValue(DicomTag.PatientSex, string.Empty)),
                    Phone = dataset.GetStringValue(DicomTag.PatientTelephoneNumbers, string.Empty),
                    Address = dataset.GetStringValue(DicomTag.PatientAddress, string.Empty),
                    Comments = dataset.GetStringValue(DicomTag.PatientComments, string.Empty),
                    Age = CalculateAge(dataset.GetDateTimeValue(DicomTag.PatientBirthDate))
                };

                patient.FullName = $"{patient.LastName} {patient.FirstName} {patient.MiddleName}".Trim();

                var study = new StudyLiteDb()
                {
                    Id = ObjectId.NewObjectId(),
                    StudyInstanceUid = dataset.GetStringValue(DicomTag.StudyInstanceUID, string.Empty),
                    StudyId = dataset.GetStringValue(DicomTag.StudyID, string.Empty),
                    PatientId = new ObjectId(patient.Id),
                    SnapshotLastName = patient.LastName,
                    SnapshotFirstName = patient.FirstName,
                    SnapshotPatronymic = patient.MiddleName,
                    SnapshotPatientId = patient.PatientID,
                    PatientBirthDate = patient.BirthDate,
                    AccessionNumber = dataset.GetStringValue(DicomTag.AccessionNumber, string.Empty),
                    BodyParts = GetBodyParts(dataset),
                    StudyDateTime = GetStudyDateTime(dataset)
                };

                var series = new SeriesLiteDb()
                {
                    Id = ObjectId.NewObjectId(),
                    SeriesInstanceUid = dataset.GetStringValue(DicomTag.SeriesInstanceUID, string.Empty),
                    StudyInstanceUid = study.StudyInstanceUid,
                    OperatorName = dataset.GetStringValue(DicomTag.OperatorsName, string.Empty),
                    Modality = dataset.GetStringValue(DicomTag.Modality, string.Empty),
                    BodyPartExamined = dataset.GetStringValue(DicomTag.BodyPartExamined, string.Empty),
                    SeriesDateTime = GetSeriesDateTime(dataset)
                };

                var image = new ImageLiteDb()
                {
                    Id = ObjectId.NewObjectId(),
                    SopInstanceUid = dataset.GetStringValue(DicomTag.SOPInstanceUID, string.Empty),
                    SeriesInstanceUid = series.SeriesInstanceUid,
                    StudyInstanceUid = study.StudyInstanceUid,
                    BodyPart = dataset.GetStringValue(DicomTag.BodyPartExamined, string.Empty),
                    Projection = dataset.GetStringValue(DicomTag.ViewCodeSequence, string.Empty),
                    Laterality = dataset.GetStringValue(DicomTag.ImageLaterality, string.Empty),
                    InstanceDose = dataset.GetDoubleValue(DicomTag.EntranceDose) ?? 0,
                    AcquisitionTime = GetAcquisitionTime(dataset)
                };

                return new DicomFileInfo
                {
                    Patient = patient,
                    Study = study,
                    Series = series,
                    Image = image
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error parsing {filePath}: {ex.Message}");
                return null;
            }
        }

        public DicomFileInfo ParseDataset(DicomDataset dataset)
        {
            try
            {
                var patient = new PatientLiteDb()
                {
                    Id = ObjectId.NewObjectId().ToString(),
                    PatientID = dataset.GetStringValue(DicomTag.PatientID),
                    LastName = ParseDicomName(dataset.GetStringValue(DicomTag.PatientName, string.Empty)).lastName,
                    FirstName = ParseDicomName(dataset.GetStringValue(DicomTag.PatientName, string.Empty)).firstName,
                    MiddleName = ParseDicomName(dataset.GetStringValue(DicomTag.PatientName, string.Empty)).middleName,
                    BirthDate = dataset.GetDateTimeValue(DicomTag.PatientBirthDate),
                    Sex = ParsePatientSex(dataset.GetStringValue(DicomTag.PatientSex, string.Empty)),
                    Phone = dataset.GetStringValue(DicomTag.PatientTelephoneNumbers, string.Empty),
                    Address = dataset.GetStringValue(DicomTag.PatientAddress, string.Empty),
                    Comments = dataset.GetStringValue(DicomTag.PatientComments, string.Empty),
                    Age = CalculateAge(dataset.GetDateTimeValue(DicomTag.PatientBirthDate))
                };

                patient.FullName = $"{patient.LastName} {patient.FirstName} {patient.MiddleName}".Trim();


                var study = new StudyLiteDb()
                {
                    Id = ObjectId.NewObjectId(),
                    StudyInstanceUid = dataset.GetStringValue(DicomTag.StudyInstanceUID, string.Empty),
                    StudyId = dataset.GetStringValue(DicomTag.StudyID, string.Empty),
                    PatientId = new ObjectId(patient.Id),
                    SnapshotLastName = patient.LastName,
                    SnapshotFirstName = patient.FirstName,
                    SnapshotPatronymic = patient.MiddleName,
                    SnapshotPatientId = patient.PatientID,
                    PatientBirthDate = patient.BirthDate,
                    AccessionNumber = dataset.GetStringValue(DicomTag.AccessionNumber, string.Empty),
                    BodyParts = GetBodyParts(dataset),
                    StudyDateTime = GetStudyDateTime(dataset)
                };

                var series = new SeriesLiteDb()
                {
                    Id = ObjectId.NewObjectId(),
                    SeriesInstanceUid = dataset.GetStringValue(DicomTag.SeriesInstanceUID, string.Empty),
                    StudyInstanceUid = study.StudyInstanceUid,
                    OperatorName = dataset.GetStringValue(DicomTag.OperatorsName, string.Empty),
                    Modality = dataset.GetStringValue(DicomTag.Modality, string.Empty),
                    BodyPartExamined = dataset.GetStringValue(DicomTag.BodyPartExamined, string.Empty),
                    SeriesDateTime = GetSeriesDateTime(dataset)
                };

                var image = new ImageLiteDb()
                {
                    Id = ObjectId.NewObjectId(),
                    SopInstanceUid = dataset.GetStringValue(DicomTag.SOPInstanceUID, string.Empty),
                    SeriesInstanceUid = series.SeriesInstanceUid,
                    StudyInstanceUid = study.StudyInstanceUid,
                    BodyPart = dataset.GetStringValue(DicomTag.BodyPartExamined, string.Empty),
                    Projection = dataset.GetStringValue(DicomTag.ViewCodeSequence, string.Empty),
                    Laterality = dataset.GetStringValue(DicomTag.ImageLaterality, string.Empty),
                    InstanceDose = dataset.GetDoubleValue(DicomTag.EntranceDose) ?? 0,
                    AcquisitionTime = GetAcquisitionTime(dataset)
                };

                return new DicomFileInfo
                {
                    Patient = patient,
                    Study = study,
                    Series = series,
                    Image = image
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }


        private (string lastName, string firstName, string middleName) ParseDicomName(string dicomName)
        {
            if (string.IsNullOrWhiteSpace(dicomName))
                return (string.Empty, string.Empty, string.Empty);

            var parts = dicomName.Split('^');
            return (
                lastName: parts.Length > 0 ? parts[0].Trim() : string.Empty,
                firstName: parts.Length > 1 ? parts[1].Trim() : string.Empty,
                middleName: parts.Length > 2 ? parts[2].Trim() : string.Empty
            );
        }

        private PatientSex ParsePatientSex(string sex)
        {
            if (string.IsNullOrWhiteSpace(sex))
                return PatientSex.Other;

            return sex.ToUpper() switch
            {
                "M" => PatientSex.Male,
                "F" => PatientSex.Female,
                "O" => PatientSex.Other,
                _ => PatientSex.Other
            };
        }

        private string CalculateAge(DateTime? birthDate)
        {
            if (!birthDate.HasValue)
                return string.Empty;

            var age = DateTime.Now.Year - birthDate.Value.Year;
            return $"{age:000}Y";
        }

        private string[] GetBodyParts(DicomDataset dataset)
        {
            var bodyPart = dataset.GetStringValue(DicomTag.BodyPartExamined, string.Empty);
            return string.IsNullOrWhiteSpace(bodyPart)
                ? Array.Empty<string>()
                : new[] { bodyPart };
        }

        private DateTime? GetStudyDateTime(DicomDataset dataset)
        {
            var date = dataset.GetStringValue(DicomTag.StudyDate, string.Empty);
            var time = dataset.GetStringValue(DicomTag.StudyTime, string.Empty);

            if (string.IsNullOrWhiteSpace(date))
                return null;

            if (DateTime.TryParse($"{date} {time}", out var result))
                return result;

            if (DateTime.TryParse(date, out var resultDate))
                return resultDate;

            return null;
        }

        private DateTime? GetSeriesDateTime(DicomDataset dataset)
        {
            var date = dataset.GetStringValue(DicomTag.SeriesDate, string.Empty);
            var time = dataset.GetStringValue(DicomTag.SeriesTime, string.Empty);

            if (string.IsNullOrWhiteSpace(date))
                return null;

            if (DateTime.TryParse($"{date} {time}", out var result))
                return result;

            if (DateTime.TryParse(date, out var resultDate))
                return resultDate;

            return null;
        }

        private DateTime? GetAcquisitionTime(DicomDataset dataset)
        {
            var timeStr = dataset.GetStringValue(DicomTag.AcquisitionTime);

            if (string.IsNullOrWhiteSpace(timeStr))
                return null;

            // Парсим время формата "HHmmss" или "HHmmss.fff"
            if (DateTime.TryParseExact(timeStr, new[] { "HHmmss", "HHmmss.fff" },
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }

            return null;
        }
    }
}
