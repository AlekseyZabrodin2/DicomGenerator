using System;

namespace DicomGenerator.Core
{
    public class SerieGeneratorParameters
    {
        public SerieGeneratorParameters(IGeneratorRule<string> seriesLaterality, Modality modality, IGeneratorRule<string,int> seriesNumber, 
            IGeneratorRule<DateTime> seriesTime, SopGeneratorParameters sopGeneratorParameters)
        {
            SeriesLaterality = seriesLaterality ?? throw new ArgumentNullException(nameof(seriesLaterality));
            Modality = modality;
            SeriesNumber = seriesNumber ?? throw new ArgumentNullException(nameof(seriesNumber));
            SeriesTime = seriesTime ?? throw new ArgumentNullException(nameof(seriesTime));
            SopGeneratorParameters = sopGeneratorParameters ?? throw new ArgumentNullException(nameof(sopGeneratorParameters));
        }

        public SopGeneratorParameters SopGeneratorParameters { get; }
        public Modality Modality { get; }

        public int SeriesCount { get; set; }
        
        public IGeneratorRule<string> SeriesLaterality { get; set; }

        public IGeneratorRule<string,int> SeriesNumber { get; }

        public IGeneratorRule<DateTime> SeriesTime { get; set; }
    }
}