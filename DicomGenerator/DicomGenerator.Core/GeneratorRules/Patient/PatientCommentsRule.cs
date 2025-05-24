using System;
using System.Collections.Generic;

namespace DicomGenerator.Core.GeneratorRules.Patient
{
    public class PatientCommentsRule : IGeneratorRule<string>
    {
        public PatientCommentsRule(string getCommentsPatient)
        {
            GetCommentsPatient = getCommentsPatient;
        }

        public string GetCommentsPatient { get; }


        private static IDictionary<int, string> _patientComments = new Dictionary<int, string>
        {
            {0, "Ультразвуковое обследование суставов."},
            {1, "Рентгенографическое исследование."},
            {2, "Ретроградная цистография, с контрастированием"},
            {3, "Подозрение или наличие признаков травмы."},
            {4, "Обзорная рентгенография органов брюшной полости в одной проекции."}
        };

        public string Generate()
        {
            var random = new Random();

            if (GetCommentsPatient != null)
            {
                return GetCommentsPatient;
            }

            var result = _patientComments[random.Next(0, _patientComments.Count)];

            return result;
        }
    }
}
