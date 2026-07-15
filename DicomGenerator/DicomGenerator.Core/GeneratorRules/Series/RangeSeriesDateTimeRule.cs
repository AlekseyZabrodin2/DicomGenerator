using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DicomGenerator.Core.DicomGeneratorModels;
using DicomGenerator.Core.GeneratorRules.Study;
using FellowOakDicom;

namespace DicomGenerator.Core.GeneratorRules.Series
{
    public sealed class RangeSeriesDateTimeRule : IGeneratorRule<DateTime>
    {

        public RangeSeriesDateTimeRule(DateTime startDateTime, DateTime endDateTime)
        {
            MinSeriesTime = startDateTime;
            MaxSeriesTime = endDateTime;            
        }

        public DateTime MaxSeriesTime { get; }
        public DateTime MinSeriesTime { get; }

        public DateTime Generate()
        {
            var random = new Random();

            int rndHours = random.Next(MinSeriesTime.Hour, MaxSeriesTime.Hour);
            int rndMinute = random.Next(MinSeriesTime.Minute, MaxSeriesTime.Minute);

            var seriesTime = MinSeriesTime.AddHours(rndHours).AddMinutes(rndMinute);

            return seriesTime;
        }
    }
}
