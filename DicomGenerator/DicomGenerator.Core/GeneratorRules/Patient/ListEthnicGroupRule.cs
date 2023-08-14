using System;
using System.Collections.Generic;

namespace DicomGenerator.Core.GeneratorRules.Patient
{
    public sealed class ListEthnicGroupRule : IGeneratorRule<string>
    {
        private static readonly IDictionary<int, string> _groupDictionary = new Dictionary<int, string>
        {
            {0, "Russian"}, {1, "Belarus"}, {2, "China"}
        };

        public string Generate()
        {
            var random = new Random();

            return _groupDictionary[random.Next(0, _groupDictionary.Count - 1)];
        }
    }
}