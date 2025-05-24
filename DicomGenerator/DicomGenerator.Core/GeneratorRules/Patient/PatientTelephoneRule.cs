using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomGenerator.Core.GeneratorRules.Patient
{
    public class PatientTelephoneRule : IGeneratorRule<string>
    {
        public PatientTelephoneRule(int getTelephonePatient)
        {
            GetTelephonePatient = getTelephonePatient.ToString();
        }

        public string GetTelephonePatient { get; }



        private static IDictionary<int, string> _patientPhone = new Dictionary<int, string>
        {
            {0, "+4202364291"},
            {1, "+2661077588474"},
            {2, "+2813799498405"},
            {3, "+2958791516805"},
            {4, "+59852280549"}
        };

        public string Generate()
        {
            var random = new Random();

            if (GetTelephonePatient != "0")
            {
                var telephoneNumber = $"+{GetTelephonePatient}";

                return telephoneNumber;
            }

            var result = _patientPhone[random.Next(0, _patientPhone.Count)];

            return result;
        }
    }
}
