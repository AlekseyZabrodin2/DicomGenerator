using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DicomGenerator.Core.GeneratorRules.Patient
{
    public sealed class RandomNameRule : IGeneratorRule<string>
    {
        public RandomNameRule(string lastName, string patientName, string middleName)
        {
            GetPatientName = $"{lastName}^{patientName}^{middleName}";
        }


        private static readonly IDictionary<int, string> _russianName = new Dictionary<int, string>
        {
            {0, "Андрей"},
            {1, "Борис"},
            {2, "Виталик"},
            {3, "Георгий"},
            {4, "Дмитрий"},
            {5, "Егор"},
            {6, "Сергей"},
            {7, "Ярик"},
            {8, "Павел"},
            {9, "Марк"}
        };

        private static readonly IDictionary<int, string> _russianMiddlename = new Dictionary<int, string>
        {
            {0, "Дмитреевич"},
            {1, "Аркадьевич"},
            {2, "Иваныч"},
            {3, "Семенович"},
            {4, "Вольфович"},
            {5, "Ильич"},
            {6, "Михайлович"},
            {7, "Григорьевич"},
            {8, "Борисович"},
            {9, "Юрьевич"}
        };

        private static readonly IDictionary<int, string> _russianSecondname = new Dictionary<int, string>
        {
            {0, "Покровский"},
            {1, "Лебединский"},
            {2, "Дубов"},
            {3, "Голицын"},
            {4, "Селиверстов"},
            {5, "Сибирцев"},
            {6, "Дунаевский"},
            {7, "Ржевский"},
            {8, "Чацкий"},
            {9, "Ростов"}
        };

        private static readonly IDictionary<int, string> _latinName = new Dictionary<int, string>
        {
            {0, "Arkadiy"},
            {1, "Boris"},
            {2, "Vadim"},
            {3, "Gleb"},
            {4, "Daniil"},
            {5, "Egor"},
            {6, "Ivan"},
            {7, "Kirill"},
            {8, "Roman"},
            {9, "Vasiliy"}
        };

        private static readonly IDictionary<int, string> _latinMiddlename = new Dictionary<int, string>
        {
            {0, "Aleksandrovich"},
            {1, "Victorovich"},
            {2, "Yuryevich"},
            {3, "Vasilyevich"},
            {4, "Vladimirovich"},
            {5, "Mikhaylovich"},
            {6, "Nikolayevich"},
            {7, "Anatolyevich"},
            {8, "Borisovich"},
            {9, "Ivanovich"}
        };

        private static readonly IDictionary<int, string> _latinSecondname = new Dictionary<int, string>
        {
            {0, "Babushkin"},
            {1, "Dudinsky"},
            {2, "Vorobyov"},
            {3, "Zaytsev"},
            {4, "Kovalsky"},
            {5, "Lapshin"},
            {6, "Tikhonov"},
            {7, "Kasyanov"},
            {8, "Stroyev"},
            {9, "Akopyan"}
        };
        
        public string GetPatientName { get;  }

        public string Generate()
        {
            var random = new Random();

            if (!string.IsNullOrWhiteSpace(Regex.Replace(GetPatientName, @"\^+", "")))
            {
                return GetPatientName;
            }
            
            //Russian Patient
            var rusName = _russianName[random.Next(0, _russianName.Count)];

            var rusMiddleName = _russianMiddlename[random.Next(0, _russianMiddlename.Count)];

            var rusSecondName = _russianSecondname[random.Next(0, _russianSecondname.Count)];

            var rusPatient = $"{rusSecondName}^{rusName}^{rusMiddleName}";

            //Latin Patient
            var latinName = _latinName[random.Next(0, _latinName.Count)];

            var latinMiddleName = _latinMiddlename[random.Next(0, _latinMiddlename.Count)];

            var latinSecondName = _latinSecondname[random.Next(0, _latinSecondname.Count)];

            var latinPatient = $"{latinSecondName}^{latinName}^{latinMiddleName}";

            //Return result
            string[] array = { rusPatient, latinPatient };

            var resultPatient = array[random.Next(0, 2)];

            return resultPatient;
            
        }
    }
}