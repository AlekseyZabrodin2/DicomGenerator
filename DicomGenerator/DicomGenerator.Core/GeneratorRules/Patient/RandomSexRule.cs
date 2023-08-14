using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomGenerator.Core.GeneratorRules.Patient
{
    public sealed class RandomSexRule : IGeneratorRule<string>
    {
        private static IDictionary<int, string> _patientSex = new Dictionary<int, string>
        {
            {0, "M"},
            {1, "F"},
            {2, "O"},
        };

        public string Generate()
        {
            var random = new Random();

            return _patientSex[random.Next(0, _patientSex.Count)];
        }
    }
}
