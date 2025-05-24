using System;
using System.Collections.Generic;

namespace DicomGenerator.Core.GeneratorRules.Study
{
    public class BodyPartRandom
    {
        public static List<string> _itemFinger = new() { "7569003", "SCT", "Finger" };
        public static List<string> _itemKnee = new() { "72696002", "SCT", "Knee" };
        public static List<string> _itemShoulder = new() { "16982005", "SCT", "Shoulder" };
        public static List<string> _itemWrist = new() { "8205005", "SCT", "Wrist" };
        public static List<string> _itemSternum = new() { "56873002", "SCT", "Sternum" };

        private static IDictionary<int, List<string>> _anatomicRegion = new Dictionary<int, List<string>>
        {
            { 0, _itemFinger },
            { 1, _itemKnee },
            { 2, _itemShoulder },
            { 3, _itemWrist },
            { 4, _itemSternum }
        };


        public static List<string> _positionsAnteroPosterior = new () { "399348003", "SCT", "antero-posterior", "208066586250159497743618000768743441649" };
        public static List<string> _positionsPosteroAnterior = new () { "272479007", "SCT", "postero-anterior", "54474552544190154445668312014970236256" };

        private static IDictionary<int, List<string>> _viewPositions = new Dictionary<int, List<string>>
        {
            { 0, _positionsAnteroPosterior },
            { 1, _positionsPosteroAnterior }
        };

        private static IDictionary<int, string> _laterality = new Dictionary<int, string>
        {
            {0, "L"},
            {1, "R"}
        };



        public static string RegionCodeValue { get; set; }
        public static string RegionCodingSchemeDesignator { get; set; }
        public static string RegionCodeMeaning { get; set; }
        public static string BodyPartExamined { get; set; }

        public static string ViewCodeValue { get; set; }
        public static string ViewCodingSchemeDesignator { get; set; }
        public static string ViewCodeMeaning { get; set; }
        public static string ViewContextUID { get; set; }



        public static void GenerateRegion()
        {
            var random = new Random();

            var region = _anatomicRegion[random.Next(0, _anatomicRegion.Count)];

            RegionCodeValue = region[0];
            RegionCodingSchemeDesignator = region[1];
            RegionCodeMeaning = region[2];

            BodyPartExamined = region[2].ToUpper();
        }

        public static void GenerateBodyPositions()
        {
            var random = new Random();

            var bodyPositions = _viewPositions[random.Next(0, _viewPositions.Count)];

            ViewCodeValue = bodyPositions[0];
            ViewCodingSchemeDesignator = bodyPositions[1];
            ViewCodeMeaning = bodyPositions[2];
            ViewContextUID = bodyPositions[3];
        }

        public static string GenerateLaterality()
        {
            var random = new Random();
            var laterality = _laterality[random.Next(0, _laterality.Count)];
            return laterality;
        }
    }
}
