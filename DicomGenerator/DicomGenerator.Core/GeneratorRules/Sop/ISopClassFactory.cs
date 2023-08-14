using FellowOakDicom;

namespace DicomGenerator.Core.GeneratorRules.Sop
{
    public interface ISopClassFactory
    {
        DicomDataset GenerateDataset(SopClassUid sopClassUid);
    }
}