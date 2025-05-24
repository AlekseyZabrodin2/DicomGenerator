using System;

namespace DicomGenerator.Core.GeneratorRules.Study
{
    internal class StudyIdGenerator
    {

        public static string GenerateStudyId()
        {
            var studyId = $"STD-{DateTime.Now.ToString("yyMMddHHmmss")}";

            return studyId;
        }
    }
}
