using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("mergepull", HelpText = "Merge pull request destination branch changes to another branch")]
    public class MergePullRequestOptions : BaseOptions
    {
        [Value(0, HelpText = "Pull request ID", Required = true)]
        public int PullRequestId { get; set; }

        [Value(1, HelpText = "Target branch", Required = false)]
        public string TargetBranch { get; set; }
    }
}
