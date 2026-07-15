using System;
using LiteDB;

namespace DicomGenerator.Core.LiteDbModels
{
    public class SeriesLiteDb
    {
        public ObjectId? Id { get; set; }

        public string SeriesInstanceUid { get; set; } = default!;

        public string StudyInstanceUid { get; set; } = default!;

        public string OperatorName { get; set; } = string.Empty;
        public string Modality { get; set; } = string.Empty;
        public string BodyPartExamined { get; set; } = string.Empty;

        public DateTime? SeriesDateTime { get; set; }
    }
}
