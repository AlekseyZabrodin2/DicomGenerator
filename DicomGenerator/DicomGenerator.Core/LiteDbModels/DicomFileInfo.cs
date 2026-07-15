using System.Collections.Generic;

namespace DicomGenerator.Core.LiteDbModels
{
    public class DicomFileInfo
    {
        public PatientLiteDb Patient {  get; set; }
        public StudyLiteDb Study { get; set; }
        public SeriesLiteDb Series { get; set; }
        public ImageLiteDb Image { get; set; }
    }
}
