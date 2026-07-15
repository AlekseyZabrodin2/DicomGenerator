using System;
using DicomGenerator.Core.Enums;

namespace DicomGenerator.Core.LiteDbModels
{
    public class PatientLiteDb
    {
        public string? Id { get; set; }

        public string PatientID { get; set; } = default!;

        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;

        /// <summary>
        /// Stored computed field: "{LastName} {FirstName} {MiddleName}".
        /// Indexed for full-name search.
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        public PatientSex Sex { get; set; } = PatientSex.Other;
        public DateTime? BirthDate { get; set; }
        public string Age { get; set; }

        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public string Occupation { get; set; } = string.Empty;
    }
}
