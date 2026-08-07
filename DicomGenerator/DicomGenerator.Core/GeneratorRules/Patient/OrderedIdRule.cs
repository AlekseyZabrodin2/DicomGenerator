using System;
using DicomGenerator.Core.DicomGeneratorModels;

namespace DicomGenerator.Core.GeneratorRules.Patient
{
    public sealed class OrderedIdRule : IGeneratorRule<string, int>
    {
        public string Prefix { get; set; }
        public int IdNumber { get; set; }


        public OrderedIdRule(string prefix, int idNumber)
        {
            Prefix = string.IsNullOrEmpty(prefix) ? "Test_" : prefix;
            IdNumber = idNumber;
        }



        public string Generate(int order)
        {
            if(order < 0)
            {
                order = 0;
            }

            var idNumber = IdNumber + order + 1;

            if (string.IsNullOrWhiteSpace(Prefix))
            {
                throw new InvalidOperationException("Prefix must contain an string");
            }

            var outputPrefix = $"{Prefix}{idNumber:D6}";

            return outputPrefix;
        }
    }
}