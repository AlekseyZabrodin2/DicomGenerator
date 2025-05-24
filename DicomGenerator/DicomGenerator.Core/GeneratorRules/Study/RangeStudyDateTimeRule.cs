using System;

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

            var daysDiff = (int)(MaximumDateTime - MinimumDateTime).TotalDays;

            var days = random.Next(0, daysDiff);

            var dateTime = MinimumDateTime.AddDays(days).AddHours(random.Next(0, 24)).AddMinutes(random.Next(0, 60));

            return dateTime;
        }

    }
}
