using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("release", HelpText = "Configure release version")]
    public class ReleaseOptions : BaseOptions
    {
        [Value(0, HelpText = "Release name", Required = true)]
        public string ReleaseName { get; set; }

        [Option("baseBranch", HelpText = "Base branch (Default = master)", Required = false)]
        public string BaseBranch { get; set; }
    }
}
