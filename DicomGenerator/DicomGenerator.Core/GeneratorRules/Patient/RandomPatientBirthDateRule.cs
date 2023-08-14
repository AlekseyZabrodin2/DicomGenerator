using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomGenerator.Core.GeneratorRules.Patient
{
    public sealed class RandomPatientBirthDateRule : IGeneratorRule<DateTime>
    {
        public RandomPatientBirthDateRule(DateTime birthDate)
        {
            BirthDate = birthDate;
        }
        
        public DateTime BirthDate { get; }

        public DateTime Generate()
        {
            var random = new Random();

            DateTime nowDate = DateTime.Now;

            int rndYear = random.Next(1950, nowDate.Year);
            int rndMonth = random.Next(1, 12);
            int rndDay = random.Next(1, 28);

            var birthDate = new DateTime(rndYear,rndMonth,rndDay);

            return birthDate;


        }
    }
}
