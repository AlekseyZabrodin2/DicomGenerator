using System;
using LiteDB;

namespace DicomGenerator.Core.LiteDbModels
{
    public sealed class LiteStudy
    {
        [BsonId]
        public ObjectId Id { get; set; } // Внутренний уникальный идентификатор исследования

        public string StudyInstanceUid { get; set; } = default!;
        public string StudyId { get; set; } = string.Empty;

        // Связь с пациентом
        public ObjectId PatientId { get; set; } = ObjectId.Empty; // Внутренний ID пациента

        public string SnapshotLastName { get; set; } = string.Empty;
        public string SnapshotFirstName { get; set; } = string.Empty;
        public string SnapshotPatronymic { get; set; } = string.Empty;
        public string SnapshotPatientId { get; set; } = string.Empty;
        public DateTime? PatientBirthDate { get; set; }

        public string AccessionNumber { get; set; } = string.Empty;
        public string[] BodyParts { get; set; } = Array.Empty<string>(); // Области исследования

        public DateTime StudyDateTime { get; set; }

        public double EffectiveDosemSv { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
