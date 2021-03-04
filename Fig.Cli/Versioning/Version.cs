using Fig.Cli.Helpers;
using Newtonsoft.Json;
using System;
using System.IO;

namespace Fig.Cli.Versioning
{
    public class VersionInfo
    {
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Patch { get; set; }
        public string LastScript { get; set; }

        public VersionInfo()
        { }

        public VersionInfo(string versionString)
        {
            var versionArray = versionString.Split(".");

            if (versionArray.Length >= 1)
                Major = Convert.ToInt32(versionArray[0]);

            if (versionArray.Length >= 2)
                Minor = Convert.ToInt32(versionArray[1]);

            if (versionArray.Length >= 3)
                Patch = Convert.ToInt32(versionArray[2]);
        }

        public override string ToString()
        {
            return $"{Major}.{Minor}.{Patch}";
        }

        public void NextMinor()
        {
            Minor++;
        }

        public static VersionInfo LoadVersion()
        {
            var versionFileName = StringHelper.ConcatPath(FigContext.Instance.FigDirectory, @".version");

            if (!File.Exists(versionFileName))
                return null;

            return JsonConvert.DeserializeObject<VersionInfo>(File.ReadAllText(versionFileName));
        }
    }
}
