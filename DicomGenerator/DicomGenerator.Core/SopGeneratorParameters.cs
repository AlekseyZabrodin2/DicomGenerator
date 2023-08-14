using DicomGenerator.Core.GeneratorRules.Sop;

namespace DicomGenerator.Core
{
    public class SopGeneratorParameters
    {
        public SopGeneratorParameters(SopClassUidRule sopClassUidRule)
        {
            SopClassUidRule = sopClassUidRule;
        }

        public int SopCount { get; set; }

        public SopClassUidRule SopClassUidRule { get; }
    }
}