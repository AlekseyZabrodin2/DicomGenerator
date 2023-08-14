using System;
using System.Collections;
using System.Collections.Generic;
using DicomGenerator.Core.GeneratorRules.Patient;

namespace DicomGenerator.Core
{
    public class PatientGeneratorParameters
    {
        public PatientGeneratorParameters(DicomEncodingRule dicomEncodingRule,
            IGeneratorRule<string> nameRule, 
            IGeneratorRule<string,int> idRule,
            IGeneratorRule<string> sex, 
            IGeneratorRule<DateTime> patientBirthDate,
            IList<StudyGeneratorParameters> studyGeneratorParameters)
        {
            DicomEncodingRule = dicomEncodingRule;
            NameRule = nameRule ?? throw new ArgumentNullException(nameof(nameRule));
            IdRule = idRule ?? throw new ArgumentNullException(nameof(idRule));
            Sex = sex ?? throw new ArgumentNullException(nameof(sex));
            PatientBirthDate = patientBirthDate ?? throw new ArgumentNullException(nameof(patientBirthDate));
            StudyGeneratorParameters = studyGeneratorParameters ?? throw new ArgumentNullException(nameof(studyGeneratorParameters));
        }

        public int PatientsCount { get; set; }

        public DicomEncodingRule DicomEncodingRule { get; }

        public IGeneratorRule<string> NameRule { get; }

        public IGeneratorRule<string,int> IdRule { get; }

        public IGeneratorRule<string> Sex { get; }

        public IGeneratorRule<DateTime> PatientBirthDate { get; }

        public IList<StudyGeneratorParameters> StudyGeneratorParameters { get; }

        public IGeneratorRule<string> EthnicGroupRule { get; set; }

        public IGeneratorRule<string> PatientAge { get; set; }



    }
}