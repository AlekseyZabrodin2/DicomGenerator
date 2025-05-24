namespace DicomGenerator.Core.GeneratorRules
{
    public class SeriesNumberRule : IGeneratorRule<string, int>

    {
        public string Generate(int number)
        {
           
            return $"{number}";
        }
    }
}
