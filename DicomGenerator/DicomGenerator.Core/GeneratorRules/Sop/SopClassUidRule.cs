using System;
using FellowOakDicom;

namespace DicomGenerator.Core.GeneratorRules.Sop
{
    public sealed class SopClassUidRule : IGeneratorRule<DicomDataset, SopClassUid>
    {
        private readonly ISopClassFactory _sopClassFactory;

        public SopClassUidRule(ISopClassFactory sopClassFactory)
        {
            _sopClassFactory = sopClassFactory ?? throw new ArgumentNullException(nameof(sopClassFactory));
        }
        public DicomDataset Generate(SopClassUid sopClassUid)
        {
            if (sopClassUid == null)
            {
                throw new ArgumentNullException(nameof(sopClassUid));
            }

            return _sopClassFactory.GenerateDataset(sopClassUid);
        }

    }

}