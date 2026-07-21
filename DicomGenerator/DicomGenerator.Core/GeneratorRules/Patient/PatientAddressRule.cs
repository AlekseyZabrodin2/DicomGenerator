using System;
using System.Collections.Generic;
using DicomGenerator.Core.DicomGeneratorModels;

namespace DicomGenerator.Core.GeneratorRules.Patient
{
    public class PatientAddressRule : IGeneratorRule<string>
    {
        public string GetAddressPatient { get; }

        public PatientAddressRule(string getAddressPatient)
        {
            GetAddressPatient = getAddressPatient;
        }

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

            if (GetAddressPatient != null)
            {
                return GetAddressPatient;
            }

            return _rusAddress[random.Next(0, _rusAddress.Count)];
        }
    }
}
