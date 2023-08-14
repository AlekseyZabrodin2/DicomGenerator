using System;
using System.Collections.Generic;

namespace DicomGenerator.Core.GeneratorRules.Sop
{
    public sealed class SopClassUid : IEquatable<SopClassUid>
    {
       private SopClassUid(string uid, string name)
        {
            if (string.IsNullOrWhiteSpace(uid))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(uid));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            Uid = uid;
            Name = name;
        }

        public static SopClassUid MgPresentation { get; } = new SopClassUid("1.2.840.10008.5.1.4.1.1.1.2", "Digital Mammography X-Ray Image Storage - For Presentation");

        public static SopClassUid DxPresentation { get; } = new SopClassUid("1.2.840.10008.5.1.4.1.1.1.1", "Digital X-Ray Image Storage - For Presentation");

        public static SopClassUid SrBasic { get; } = new SopClassUid("1.2.840.10008.5.1.4.1.1.88.11", "Basic Text SR");

        public static SopClassUid Unknown { get; } = new SopClassUid("Unknown", "Unsupported sop class");

        private static readonly IDictionary<string, SopClassUid> _sopClassUids = new Dictionary<string, SopClassUid>
        {
            {DxPresentation.Uid, DxPresentation}, {MgPresentation.Uid, MgPresentation}, {SrBasic.Uid, SrBasic}
        };

        public string Uid { get; }
        public string Name { get; }

        public bool Equals(SopClassUid other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Uid == other.Uid;
        }

        public static SopClassUid FromUidString(string stringUid)
        {
            if (string.IsNullOrWhiteSpace(stringUid))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(stringUid));
            }

            return _sopClassUids.TryGetValue(stringUid, out var sopClassUid) ? sopClassUid : Unknown;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || (obj is SopClassUid other && Equals(other));
        }

        public override int GetHashCode()
        {
            return Uid != null ? Uid.GetHashCode() : 0;
        }

        public static bool operator ==(SopClassUid left, SopClassUid right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SopClassUid left, SopClassUid right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}