using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
