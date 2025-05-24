using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomGenerator.Core.GeneratorRules.Study
{
    internal class OperatorsNameGenerator
    {

        private static IDictionary<int, string> _operatorsName = new Dictionary<int, string>
        {
            {0, "Dr. Smith"},
            {1, "Dr. Johnson"},
            {2, "Dr. Brown"},
            {3, "Dr. Davis"},
            {4, "Dr. Wilson"},
            {5, "Dr. Miller"},
            {6, "Dr. Moore"},
            {7, "Dr. Taylor"},
            {8, "Dr. Anderson"},
            {9, "Dr. White"}
        };


        public static string GenerateOperatorsName()
        {
            var random = new Random();

            var operatorsName = _operatorsName[random.Next(0, _operatorsName.Count)];

            return operatorsName;
        }
    }
}
