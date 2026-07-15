using System;
using FellowOakDicom;

namespace DicomGenerator.Core.DicomFileParser
{
    public static class DicomExtensions
    {
        public static string GetStringValue(this DicomDataset dataset, DicomTag tag, string defaultValue = "")
        {
            return dataset.TryGetSingleValue<string>(tag, out var value)
                ? value ?? defaultValue
                : defaultValue;
        }

        public static DateTime? GetDateTimeValue(this DicomDataset dataset, DicomTag tag)
        {
            return dataset.TryGetSingleValue<DateTime>(tag, out var value)
                ? value
                : (DateTime?)null;
        }

        public static double? GetDoubleValue(this DicomDataset dataset, DicomTag tag)
        {
            return dataset.TryGetSingleValue<double>(tag, out var value)
                ? value
                : (double?)null;
        }

        public static int? GetIntValue(this DicomDataset dataset, DicomTag tag)
        {
            return dataset.TryGetSingleValue<int>(tag, out var value)
                ? value
                : (int?)null;
        }
    }
}
