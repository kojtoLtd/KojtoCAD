using System;

namespace KojtoCAD.Updater
{
    public class KojtoCadVersion : IComparable
    {
        public int Major { get; set; }
        public int Year { get; set; }
        public int ElapsedDays { get; set; }
        public int Revision { get; set; }
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            var other = obj as KojtoCadVersion;
            if (other == null)
            {
                throw new ArgumentException("Object is not an instance of KojtoCadVersion.");
            }

            if (other.Major < Major)
            {
                return 1;
            }
            if (other.Major > Major)
            {
                return -1;
            }
            if (other.Year < Year)
            {
                return 1;
            }
            if (other.Year > Year)
            {
                return -1;
            }
            if (other.ElapsedDays < ElapsedDays)
            {
                return 1;
            }
            if (other.ElapsedDays > ElapsedDays)
            {
                return -1;
            }
            if (other.Revision < Revision)
            {
                return 1;
            }
            if (other.Revision > Revision)
            {
                return -1;
            }
            return 0;
        }

        public static bool operator <(KojtoCadVersion first, KojtoCadVersion second)
        {
            return first.CompareTo(second) == -1;
        }

        public static bool operator >(KojtoCadVersion first, KojtoCadVersion second)
        {
            return first.CompareTo(second) == 1;
        }

        public static bool operator >=(KojtoCadVersion first, KojtoCadVersion second)
        {
            return first.CompareTo(second) >= 0;
        }

        public static bool operator <=(KojtoCadVersion first, KojtoCadVersion second)
        {
            return first.CompareTo(second) <= 0;
        }

        public static bool operator ==(KojtoCadVersion first, KojtoCadVersion second)
        {
            if (ReferenceEquals(first, null))
            {
                return ReferenceEquals(second, null);
            }

            return first.CompareTo(second) == 0;
        }

        public static bool operator !=(KojtoCadVersion first, KojtoCadVersion second)
        {
            return !(first == second);
        }

        public override bool Equals(object obj)
        {
            return CompareTo(obj) == 0;
        }

        public override int GetHashCode()
        {
            return Tuple.Create(Major, Year, ElapsedDays, Revision).GetHashCode();
        }

        public override string ToString()
        {
            return $"{Major}.{Year}.{ElapsedDays}.{Revision}";
        }
    }
}
