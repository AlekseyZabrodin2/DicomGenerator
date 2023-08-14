using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomGenerator.Core.GeneratorRules._3_Series
{
    public class SeriesNumberRule : IGeneratorRule<string, int>

    {
        public string Generate(int number)
        {
           
            return $"{number}";
        }
    }
}
