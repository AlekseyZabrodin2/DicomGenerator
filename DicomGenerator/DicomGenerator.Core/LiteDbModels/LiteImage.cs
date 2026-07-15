using System;
using LiteDB;

namespace DicomGenerator.Core.LiteDbModels
{
    public sealed class LiteImage
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string SopInstanceUid { get; set; } = default!;

        public string SeriesInstanceUid { get; set; } = default!;
        public string StudyInstanceUid { get; set; } = default!;

        public string BodyPart { get; set; } = string.Empty;
        public string Projection { get; set; } = string.Empty;
        public string Laterality { get; set; } = string.Empty;
        public double InstanceDose { get; set; }

        public DateTime AcquisitionTime { get; set; }
    }
}
