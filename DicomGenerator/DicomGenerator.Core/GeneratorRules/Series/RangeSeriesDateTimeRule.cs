using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DicomGenerator.Core.GeneratorRules.Study;
using FellowOakDicom;

namespace DicomGenerator.Core.GeneratorRules.Series
{
    public sealed class RangeSeriesDateTimeRule : IGeneratorRule<DateTime>
    {
       
        //public RangeSeriesDateTimeRule(DateTime dateTime)
        //{
        //    SeriesTime = dateTime;

        //}
        
        public DateTime SeriesTime { get; }

        public DateTime Generate()
        {
            var random = new Random();

            DateTime nowDate = DateTime.Now;

            int rndHours = random.Next(0, 24);
            int rndMinute = random.Next(0, 60);
            int rndSecond = random.Next(0, 60);

            var seriesTime = SeriesTime.AddHours(random.Next(rndHours)).AddMinutes(random.Next(rndMinute)).AddSeconds(random.Next(rndSecond));

            return seriesTime;

        }
    }
}
