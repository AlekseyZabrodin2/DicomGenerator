using System;
using System.Globalization;
using System.Numerics;

namespace DicomGenerator.Core
{
    public static class UidGenerator
    {
            private const string _manufacturerUid = "1.2.826.0.1.3680043.2.564";

            /// <summary>
            /// Generates the study uid.
            /// </summary>
            /// <returns></returns>
            public static string GenerateStudyUid()
            {
                return GenerateUid();
            }

            /// <summary>
            /// Generates the series uid.
            /// </summary>
            /// <returns></returns>
            public static string GenerateSeriesUid()
            {
                return GenerateUid();
            }

            /// <summary>
            /// Generates the image uid.
            /// </summary>
            /// <returns></returns>
            public static string GenerateImageUid()
            {
                return GenerateUid();
            }

            /// <summary>
            /// Generates biopsy target UID.
            /// </summary>
            public static string GenerateTargetUid()
            {
                return GenerateUid();
            }

            /// <summary>
            /// Generates the uid.
            /// </summary>
            /// <returns>UID</returns>
            private static string GenerateUid()
            {
                //var uniqueIdBytes = Guid.NewGuid().ToByteArray();
                //var uniqueIdIntPart = (uint)(uniqueIdBytes[3] << 24 | uniqueIdBytes[2] << 16 | uniqueIdBytes[1] << 8 | uniqueIdBytes[0]);
                //var dateTime = DateTime.Now;

                //return $"{_manufacturerUid}.{dateTime:yyyyMMdd}.{dateTime:hhmmss}.{dateTime:fff}.{uniqueIdIntPart}";
                var guid = Guid.NewGuid();

                var octets = guid.ToByteArray();
                var littleEndianOrder = new byte[]
                { octets[15], octets[14], octets[13], octets[12], octets[11], octets[10], octets[9], octets[8],
                    octets[6], octets[7], octets[4], octets[5], octets[0], octets[1], octets[2], octets[3], 0 };

                return "2.25." + new BigInteger(littleEndianOrder).ToString(CultureInfo.InvariantCulture);
            }
    }
}