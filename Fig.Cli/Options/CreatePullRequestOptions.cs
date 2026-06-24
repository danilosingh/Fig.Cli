using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("pr", HelpText = "Create a Pull Request from the current branch (headless, via API)")]
    public class CreatePullRequestOptions : BaseOptions
    {
        [Value(0, HelpText = "PR title (default: last commit subject)")]
        public string Title { get; set; }

        [Option('t', "target", HelpText = "Target branch (default: repository default branch)")]
        public string TargetBranch { get; set; }

        [Option('s', "source", HelpText = "Source branch (default: current branch)")]
        public string SourceBranch { get; set; }

        [Option('d', "description", HelpText = "PR description")]
        public string Description { get; set; }

        [Option("draft", HelpText = "Create the PR as draft")]
        public bool Draft { get; set; }
    }
}
