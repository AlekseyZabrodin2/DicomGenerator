using System;
using DicomGenerator.Core.GeneratorRules.Sop;

namespace DicomGenerator.Core
{
    public sealed class Modality
    {
        private Modality(string name, SopClassUid sopClassUid)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            Name = name;
            SopClassUid = sopClassUid ?? throw new ArgumentNullException(nameof(sopClassUid));
        }

        public string Name { get; }
        public SopClassUid SopClassUid { get; }

        public static Modality Mg { get; } = new Modality("MG", SopClassUid.MgPresentation);

        public static Modality Dx { get; } = new Modality("DX", SopClassUid.DxPresentation);

        public static Modality Sr { get; } = new Modality("SR", SopClassUid.SrBasic);
    }
}