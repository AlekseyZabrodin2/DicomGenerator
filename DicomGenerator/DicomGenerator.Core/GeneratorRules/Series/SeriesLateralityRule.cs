using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomGenerator.Core.GeneratorRules.Series
{
    public sealed class SeriesLateralityRule : IGeneratorRule<string>
    {
        private static IDictionary<int, string> _laterality = new Dictionary<int, string>
        {
            {0,"R"},
            {1,"L"},
            {2,"B"},
            {3,"U"},
        };
        
        public string Generate()
        {
            var random = new Random();

            return _laterality[random.Next(0, _laterality.Count)];
        }
    }
}
