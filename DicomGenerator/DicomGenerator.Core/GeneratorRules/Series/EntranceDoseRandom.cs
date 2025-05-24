using System;
using System.Collections.Generic;

namespace DicomGenerator.Core.GeneratorRules.Series
{
    internal class EntranceDoseRandom
    {
        private static IDictionary<int, decimal> _doseValues = new Dictionary<int, decimal>
        {
            {0, 0.120m},
            {1, 0.250m},
            {2, 0.080m},
            {3, 0.300m},
            {4, 0.100m},
            {5, 0.450m},
            {6, 0.220m},
            {7, 0.500m},
            {8, 0.050m},
            {9, 0.380m}
        };

        public static decimal GenerateDose()
        {
            var random = new Random();
            var dose = _doseValues[random.Next(0, _doseValues.Count)];
            return dose * 1000;
        }
    }
}
