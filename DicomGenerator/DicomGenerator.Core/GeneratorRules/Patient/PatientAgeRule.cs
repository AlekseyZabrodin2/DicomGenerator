using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomGenerator.Core.GeneratorRules.Patient
{
    public sealed class PatientAgeRule : IGeneratorRule<string>
    {
        private static IDictionary<int, string> _patientAge = new Dictionary<int, string>
        {
            {0, "018Y"},
            {1, "030Y"},
            {2, "028Y"},
            {3, "015Y"},
            {4, "045Y"},
            {5, "077Y"},
        };

        public string Generate()
        {
            var random = new Random();

            return _patientAge[random.Next(0, _patientAge.Count)];
        }
    }
}
