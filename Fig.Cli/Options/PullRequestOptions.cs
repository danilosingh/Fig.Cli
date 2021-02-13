using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("pullr", HelpText = "Open Pull Request page from current branch to master.")]
    public class PullRequestOptions : BaseOptions
    {
        [Option('t', "target", HelpText = "Target branch", Required = false)]
        public string TargetBranch { get; set; }

        public PullRequestOptions()
        { }

        public PullRequestOptions(string targetBranch)
        {
            TargetBranch = targetBranch;
        }
    }
}
