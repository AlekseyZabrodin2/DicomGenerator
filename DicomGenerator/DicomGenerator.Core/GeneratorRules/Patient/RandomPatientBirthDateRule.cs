using System;

namespace DicomGenerator.Core.GeneratorRules.Patient
{
    public sealed class RandomPatientBirthDateRule : IGeneratorRule<DateTime>
    {
        public RandomPatientBirthDateRule(DateTime birthDate, bool useRandomDate)
        {
            BirthDate = birthDate;
            UseRandomDate = useRandomDate;
        }
        
        public DateTime BirthDate { get; }
        public bool UseRandomDate { get; }

        public DateTime Generate()
        {
            var random = new Random();

            if (UseRandomDate)
            {
                int rndYear = random.Next(BirthDate.Year, DateTime.Now.Year);
                int rndMonth = random.Next(1, 12);
                int rndDay = random.Next(1, DateTime.DaysInMonth(rndYear, rndMonth));

                var birthDate = new DateTime(rndYear, rndMonth, rndDay);
                return birthDate;
            }            

            return BirthDate;
        }
    }
}
