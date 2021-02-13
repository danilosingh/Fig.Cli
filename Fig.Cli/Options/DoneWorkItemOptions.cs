using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("done", HelpText = "Done In Progress work, sync changes and open pull request")]
    public class DoneWorkItemOptions : BaseOptions
    {
        [Value(0, HelpText = "Work Item ID", Required = false)]
        public int WorkItemId { get; set; }

        [Option("nopullr", HelpText = "No open pull request")]
        public bool NoPullRequest { get; set; }

        [Option("nosync", HelpText = "Disable Sync")]
        public bool NoSync { get; set; }

        [Option('c', "commit", HelpText = "Commit all changes")]
        public string CommitMessage { get; set; }

        public bool Commit { get { return !string.IsNullOrEmpty(CommitMessage); } }
    }
}
