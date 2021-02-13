using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("merge", HelpText = "Merge changes form other branch")]
    public class MergeOptions : BaseOptions
    {
        [Value(0, HelpText = "Source branch", Required = false)]
        public string SourceBranch { get; set; }

        [Option('t', "target", HelpText = "Target branch", Required = false)]
        public string TargetBranch { get; set; }
    }
}
