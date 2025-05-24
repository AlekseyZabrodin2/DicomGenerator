using System;
using System.Collections.Generic;

namespace DicomGenerator.Core.GeneratorRules.Patient
{
    public class PatientAddressRule : IGeneratorRule<string>
    {
        public PatientAddressRule(string getAdressPatient)
        {
            GetAdressPatient = getAdressPatient;
        }

        public string GetAdressPatient { get; }


        private static readonly IDictionary<int, string> _rusAddress = new Dictionary<int, string>
        {
            {0, "Минск, ул. Ровды, д.15, кв.69"},
            {1, "ул. Нестерова 49, Минск"},
            {2, "ул. Денисовская 8, Минск"},
            {3, "пр. Независимости 154, Минск"},
            {4, "ул. Кульман 14, Минск 220100"}
        };

        public string Generate()
        {
            var random = new Random();

            if (GetAdressPatient != null)
            {
                return GetAdressPatient;
            }

            return _rusAddress[random.Next(0, _rusAddress.Count)];
        }
    }
}
