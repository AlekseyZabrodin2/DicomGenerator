using System;
using DicomGenerator.Core.GeneratorRules.Series;

namespace DicomGenerator.Core.GeneratorRules.Study
{
    public sealed class RangeStudyDateTimeRule : IGeneratorRule<DateTime>
    {
        public RangeStudyDateTimeRule(DateTime minimumDateTime, DateTime maximumDateTime)
        {
            if (minimumDateTime >= maximumDateTime)
            {
                throw new ArgumentException("Minimum date and time must be less then maximum date and time");
            }

            MinimumDateTime = minimumDateTime;
            MaximumDateTime = maximumDateTime;
        }

        public DateTime MinimumDateTime { get; }

        public DateTime MaximumDateTime { get; }

        public DateTime Generate()
        {
            var random = new Random();

            //var timeDiff = (int)(MaximumDateTime - MinimumDateTime).TotalDays;

            //var days = random.Next(0, timeDiff);

            //var dateTime = MinimumDateTime.AddDays(days).AddHours(random.Next(0, 24)).AddMinutes(random.Next(0, 60));

            //return dateTime;

            DateTime nowDate = DateTime.Today.AddYears(1);

           
            int rndYear = random.Next(2020, nowDate.Year);
            int rndMonth = random.Next(1, 12);
            int rndDay = random.Next(1, 28);
            int rndHours = random.Next(0, 24);
            int rndMinute = random.Next(0, 60);
            int rndSecond = random.Next(0, 60);

            var studyDate = new DateTime(rndYear, rndMonth, rndDay, rndHours, rndMinute, rndSecond);

            

            return studyDate;


        }

    }
}
