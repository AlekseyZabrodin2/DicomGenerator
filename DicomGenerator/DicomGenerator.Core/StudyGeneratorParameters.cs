using System;
using System.Collections;
using System.Collections.Generic;

namespace DicomGenerator.Core
{
    public class StudyGeneratorParameters
    {
        public StudyGeneratorParameters(IGeneratorRule<DateTime> studyDateTimeGeneratorRule, IGeneratorRule<string> accessionNumber, IList<SerieGeneratorParameters> serieGeneratorParameters)
        {
            StudyDateTimeGeneratorRule = studyDateTimeGeneratorRule ?? throw new ArgumentNullException(nameof(studyDateTimeGeneratorRule));
            AccessionNumber = accessionNumber ?? throw new ArgumentNullException(nameof(accessionNumber));
            SerieGeneratorParameters = serieGeneratorParameters ?? throw new ArgumentNullException(nameof(serieGeneratorParameters));
        }

        public int StudiesCount { get; set; }

        public IGeneratorRule<DateTime> StudyDateTimeGeneratorRule { get; set; }
        
        public IGeneratorRule<string> AccessionNumber { get; }
        public IList<SerieGeneratorParameters> SerieGeneratorParameters { get; }
    }
}