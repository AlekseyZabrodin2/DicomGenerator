using System;
using System.Collections.Generic;
using System.Linq;
using DicomGenerator.Core.LiteDbModels;
using LiteDB;

namespace DicomGenerator.Core.Extensions
{
    public static class DicomModelExtensions
    {
        // ==================== PATIENT ====================

        public static PatientLiteDb ToPatientLiteDb(this LitePatient patient, string patientId = null)
        {
            if (patient == null)
                return null;

            return new PatientLiteDb
            {
                Id = patient.Id?.ToString() ?? patientId ?? ObjectId.NewObjectId().ToString(),
                PatientID = patient.PatientId ?? string.Empty,
                LastName = patient.LastName ?? string.Empty,
                FirstName = patient.FirstName ?? string.Empty,
                MiddleName = patient.MiddleName ?? string.Empty,
                FullName = $"{patient.LastName ?? string.Empty} {patient.FirstName ?? string.Empty} {patient.MiddleName ?? string.Empty}".Trim(),
                Sex = patient.Sex,
                BirthDate = patient.BirthDate,
                Age = patient.BirthDate.HasValue
                    ? $"{DateTime.Now.Year - patient.BirthDate.Value.Year:000}Y"
                    : string.Empty,
                Address = patient.Address ?? string.Empty,
                Phone = patient.Phone ?? string.Empty,
                Comments = patient.Comments ?? string.Empty,
                Occupation = patient.Occupation ?? string.Empty
            };
        }

        public static LitePatient ToLitePatient(this PatientLiteDb patient)
        {
            if (patient == null)
                return null;

            return new LitePatient
            {
                Id = !string.IsNullOrEmpty(patient.Id)
                    ? new ObjectId(patient.Id)
                    : ObjectId.NewObjectId(),
                PatientId = patient.PatientID ?? string.Empty,
                LastName = patient.LastName ?? string.Empty,
                FirstName = patient.FirstName ?? string.Empty,
                MiddleName = patient.MiddleName ?? string.Empty,
                Sex = patient.Sex,
                BirthDate = patient.BirthDate,
                Address = patient.Address ?? string.Empty,
                Phone = patient.Phone ?? string.Empty,
                Comments = patient.Comments ?? string.Empty,
                Occupation = patient.Occupation ?? string.Empty
            };
        }

        public static List<PatientLiteDb> ToPatientLiteDbList(this IEnumerable<LitePatient> patients)
        {
            return patients?.Select(p => p.ToPatientLiteDb()).ToList() ?? new List<PatientLiteDb>();
        }

        public static List<LitePatient> ToLitePatientList(this IEnumerable<PatientLiteDb> patients)
        {
            return patients?.Select(p => p.ToLitePatient()).ToList() ?? new List<LitePatient>();
        }

        // ==================== STUDY ====================

        public static StudyLiteDb ToStudyLiteDb(this LiteStudy study)
        {
            if (study == null)
                return null;

            return new StudyLiteDb
            {
                Id = study.Id,
                StudyInstanceUid = study.StudyInstanceUid ?? string.Empty,
                StudyId = study.StudyId ?? string.Empty,
                PatientId = study.PatientId != ObjectId.Empty ? study.PatientId : null,
                SnapshotLastName = study.SnapshotLastName ?? string.Empty,
                SnapshotFirstName = study.SnapshotFirstName ?? string.Empty,
                SnapshotPatronymic = study.SnapshotPatronymic ?? string.Empty,
                SnapshotPatientId = study.SnapshotPatientId ?? string.Empty,
                PatientBirthDate = study.PatientBirthDate,
                AccessionNumber = study.AccessionNumber ?? string.Empty,
                BodyParts = study.BodyParts ?? Array.Empty<string>(),
                StudyDateTime = study.StudyDateTime,
                EffectiveDosemSv = study.EffectiveDosemSv,
                Status = study.Status ?? string.Empty
            };
        }

        public static LiteStudy ToLiteStudy(this StudyLiteDb study)
        {
            if (study == null)
                return null;

            return new LiteStudy
            {
                Id = study.Id ?? ObjectId.NewObjectId(),
                StudyInstanceUid = study.StudyInstanceUid ?? string.Empty,
                StudyId = study.StudyId ?? string.Empty,
                PatientId = study.PatientId ?? ObjectId.Empty,
                SnapshotLastName = study.SnapshotLastName ?? string.Empty,
                SnapshotFirstName = study.SnapshotFirstName ?? string.Empty,
                SnapshotPatronymic = study.SnapshotPatronymic ?? string.Empty,
                SnapshotPatientId = study.SnapshotPatientId ?? string.Empty,
                PatientBirthDate = study.PatientBirthDate,
                AccessionNumber = study.AccessionNumber ?? string.Empty,
                BodyParts = study.BodyParts ?? Array.Empty<string>(),
                StudyDateTime = study.StudyDateTime ?? DateTime.MinValue,
                EffectiveDosemSv = study.EffectiveDosemSv,
                Status = study.Status ?? string.Empty
            };
        }

        public static List<StudyLiteDb> ToStudyLiteDbList(this IEnumerable<LiteStudy> studies)
        {
            return studies?.Select(s => s.ToStudyLiteDb()).ToList() ?? new List<StudyLiteDb>();
        }

        public static List<LiteStudy> ToLiteStudyList(this IEnumerable<StudyLiteDb> studies)
        {
            return studies?.Select(s => s.ToLiteStudy()).ToList() ?? new List<LiteStudy>();
        }

        // ==================== SERIES ====================

        public static SeriesLiteDb ToSeriesLiteDb(this LiteSeries series)
        {
            if (series == null)
                return null;

            return new SeriesLiteDb
            {
                Id = series.Id,
                SeriesInstanceUid = series.SeriesInstanceUid ?? string.Empty,
                StudyInstanceUid = series.StudyInstanceUid ?? string.Empty,
                OperatorName = series.OperatorName ?? string.Empty,
                Modality = series.Modality ?? string.Empty,
                BodyPartExamined = series.BodyPartExamined ?? string.Empty,
                SeriesDateTime = series.SeriesDateTime
            };
        }

        public static LiteSeries ToLiteSeries(this SeriesLiteDb series)
        {
            if (series == null)
                return null;

            return new LiteSeries
            {
                Id = series.Id ?? ObjectId.NewObjectId(),
                SeriesInstanceUid = series.SeriesInstanceUid ?? string.Empty,
                StudyInstanceUid = series.StudyInstanceUid ?? string.Empty,
                OperatorName = series.OperatorName ?? string.Empty,
                Modality = series.Modality ?? string.Empty,
                BodyPartExamined = series.BodyPartExamined ?? string.Empty,
                SeriesDateTime = series.SeriesDateTime ?? DateTime.MinValue
            };
        }

        public static List<SeriesLiteDb> ToSeriesLiteDbList(this IEnumerable<LiteSeries> series)
        {
            return series?.Select(s => s.ToSeriesLiteDb()).ToList() ?? new List<SeriesLiteDb>();
        }

        public static List<LiteSeries> ToLiteSeriesList(this IEnumerable<SeriesLiteDb> series)
        {
            return series?.Select(s => s.ToLiteSeries()).ToList() ?? new List<LiteSeries>();
        }

        // ==================== IMAGE ====================

        public static ImageLiteDb ToImageLiteDb(this LiteImage image)
        {
            if (image == null)
                return null;

            return new ImageLiteDb
            {
                Id = image.Id,
                SopInstanceUid = image.SopInstanceUid ?? string.Empty,
                SeriesInstanceUid = image.SeriesInstanceUid ?? string.Empty,
                StudyInstanceUid = image.StudyInstanceUid ?? string.Empty,
                BodyPart = image.BodyPart ?? string.Empty,
                Projection = image.Projection ?? string.Empty,
                Laterality = image.Laterality ?? string.Empty,
                InstanceDose = image.InstanceDose,
                AcquisitionTime = image.AcquisitionTime
            };
        }

        public static LiteImage ToLiteImage(this ImageLiteDb image)
        {
            if (image == null)
                return null;

            return new LiteImage
            {
                Id = image.Id ?? ObjectId.NewObjectId(),
                SopInstanceUid = image.SopInstanceUid ?? string.Empty,
                SeriesInstanceUid = image.SeriesInstanceUid ?? string.Empty,
                StudyInstanceUid = image.StudyInstanceUid ?? string.Empty,
                BodyPart = image.BodyPart ?? string.Empty,
                Projection = image.Projection ?? string.Empty,
                Laterality = image.Laterality ?? string.Empty,
                InstanceDose = image.InstanceDose,
                AcquisitionTime = image.AcquisitionTime ?? DateTime.MinValue
            };
        }

        public static List<ImageLiteDb> ToImageLiteDbList(this IEnumerable<LiteImage> images)
        {
            return images?.Select(i => i.ToImageLiteDb()).ToList() ?? new List<ImageLiteDb>();
        }

        public static List<LiteImage> ToLiteImageList(this IEnumerable<ImageLiteDb> images)
        {
            return images?.Select(i => i.ToLiteImage()).ToList() ?? new List<LiteImage>();
        }
    }
}