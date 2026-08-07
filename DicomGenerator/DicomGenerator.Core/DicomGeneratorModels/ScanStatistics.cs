namespace DicomGenerator.Core.DicomGeneratorModels
{
    public class ScanStatistics
    {
        public int Patients { get; set; }
        public int Studies { get; set; }
        public int Series { get; set; }
        public int Images { get; set; }

        public static ScanStatistics operator +(ScanStatistics left, ScanStatistics right)
        {
            return new ScanStatistics
            {
                Patients = left.Patients + right.Patients,
                Studies = left.Studies + right.Studies,
                Series = left.Series + right.Series,
                Images = left.Images + right.Images
            };
        }
    }
}
