using System;
using System.Text;

namespace DicomGenerator.Core.GeneratorRules.Study
{
    public sealed class RandomAccessionNumberRule : IGeneratorRule<string>
    {
        public int Lenght { get; set; } = 16;
        
        public string Generate()
        {
            var random = new Random();

            var accessionNumber = new StringBuilder(Lenght);

            for (var index = 0; index < Lenght; index++)
            {
                accessionNumber.Append(random.Next(0, 10));
            }
            
            return accessionNumber.ToString();
        }
    }
}
