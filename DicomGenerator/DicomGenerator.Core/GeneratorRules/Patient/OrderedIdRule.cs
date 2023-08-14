using System;

namespace DicomGenerator.Core.GeneratorRules.Patient
{
    public sealed class OrderedIdRule : IGeneratorRule<string, int>
    {
        public string Prefix { get; set; } = "Test";

        public string Generate(int order)
        {
            if (string.IsNullOrWhiteSpace(Prefix))
            {
                throw new InvalidOperationException("Prefix must contain an string");
            }

            return $"{Prefix}_{order}";
        }
    }
}