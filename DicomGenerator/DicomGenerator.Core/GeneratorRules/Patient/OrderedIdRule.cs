using System;

namespace DicomGenerator.Core.GeneratorRules.Patient
{
    public sealed class OrderedIdRule : IGeneratorRule<string, int>
    {
        public OrderedIdRule(string prefix)
        {
            Prefix = string.IsNullOrEmpty(prefix) ? "Test" : prefix;
        }


        public string Prefix { get; set; }

        public string Generate(int order)
        {
            var numberId = order + 1;

            if (string.IsNullOrWhiteSpace(Prefix))
            {
                throw new InvalidOperationException("Prefix must contain an string");
            }

            var outputPrefix = $"{Prefix}_{numberId:D6}";

            return outputPrefix;
        }
    }
}