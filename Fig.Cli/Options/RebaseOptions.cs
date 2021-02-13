using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("rebase", HelpText = "Rebase from other branch")]
    public class RebaseOptions : BaseOptions
    {
        [Value(0, HelpText = "Source branch", Required = true)]
        public string SourceBranch { get; set; }

        [Option('t', "target", HelpText = "Target branch", Required = false)]
        public string TargetBranch { get; set; }
    }
}
