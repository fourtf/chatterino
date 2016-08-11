using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Chatterino.Common
{
    public class VersionNumber
    {
        public int Major { get; private set; } = 0;
        public int Minor { get; private set; } = 0;
        public int Build { get; private set; } = 0;
        public int Revision { get; private set; } = 0;

        private VersionNumber() { }

        public static VersionNumber Parse(string version)
        {
            Match match = Regex.Match(version, @"(?<major>\d+)(?<minor>\.\d+)?(?<build>\.\d+)?(?<revision>\.\d+)?");

            if (match.Success)
            {
                VersionNumber v = new VersionNumber();

                var major = match.Groups["major"];
                v.Major = int.Parse(major.Value, CultureInfo.InvariantCulture);

                var minor = match.Groups["minor"];
                if (minor.Success)
                {
                    v.Minor = int.Parse(minor.Value.Substring(1), CultureInfo.InvariantCulture);
                }

                var build = match.Groups["build"];
                if (build.Success)
                {
                    v.Build = int.Parse(build.Value.Substring(1), CultureInfo.InvariantCulture);
                }

                var revision = match.Groups["revision"];
                if (revision.Success)
                {
                    v.Revision = int.Parse(revision.Value.Substring(1), CultureInfo.InvariantCulture);
                }
                
                return v;
            }

            throw new FormatException($"{version} is not a valid version number.");
        }

        public bool IsNewerThan(VersionNumber other)
        {
            return Major > other.Major ||
                Minor > other.Minor ||
                Build > other.Build ||
                Revision > other.Revision;
        }

        public override string ToString()
        {
            string v = "";

            if (Revision != 0)
            {
                v = "." + Build + "." + Revision;
            }
            else if (Build != 0)
            {
                v = "." + Build;
            }

            v = Major + "." + Minor + v;

            return v;
        }
    }
}
