using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DicomGenerator.Core.DicomGeneratorModels;

namespace DicomGenerator.Core.GeneratorRules.Patient
{
    public sealed class RandomSexRule : IGeneratorRule<string>
    {
        public RandomSexRule( string getGender)
        {
            SetGender = getGender;
        }

        public string SetGender { get; }

        private static IList<string> _patientSex = new List<string> { "M", "F", "O" };

        public string Generate()
        {
            var random = new Random();

            if (SetGender != null)
            {
                if (SetGender == "M")
                {
                    return _patientSex[0];
                }
                else if (SetGender == "F")
                {
                    return _patientSex[1];
                }
                else if (SetGender == "O")
                {
                    return _patientSex[2];
                }
            }

            var selectGender = _patientSex[random.Next(_patientSex.Count)];

            return selectGender;
        }
    }
}
