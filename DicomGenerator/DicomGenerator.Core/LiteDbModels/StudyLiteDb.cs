using System;
using LiteDB;

namespace DicomGenerator.Core.LiteDbModels
{
    public class StudyLiteDb
    {
        public ObjectId? Id { get; set; }

        public string StudyInstanceUid { get; set; } = default!;
        public string StudyId { get; set; } = string.Empty;

        // Internal patient identifier used by storage implementation
        public ObjectId? PatientId { get; set; }

        // Patient snapshot at study time
        public string SnapshotLastName { get; set; } = string.Empty;
        public string SnapshotFirstName { get; set; } = string.Empty;
        public string SnapshotPatronymic { get; set; } = string.Empty;
        public string SnapshotPatientId { get; set; } = string.Empty;
        public DateTime? PatientBirthDate { get; set; }

        public string AccessionNumber { get; set; } = string.Empty;
        public string[] BodyParts { get; set; } = Array.Empty<string>();

        [BsonIgnore]
        public string BodyPartsDisplay => BodyParts != null ? string.Join(", ", BodyParts) : string.Empty;

        public DateTime? StudyDateTime { get; set; }
        public double EffectiveDosemSv { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
