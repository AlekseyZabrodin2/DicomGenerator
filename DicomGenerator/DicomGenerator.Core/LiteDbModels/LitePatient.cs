using System;
using DicomGenerator.Core.Enums;
using LiteDB;

namespace DicomGenerator.Core.LiteDbModels
{
    public sealed class LitePatient
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public string PatientId { get; set; } = default!;

        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;

        public PatientSex Sex { get; set; } = PatientSex.Other;
        public DateTime? BirthDate { get; set; }

        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public string Occupation { get; set; } = string.Empty;
    }
}
