using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("start", HelpText = "Start work item with new branch feature")]
    public class StartWorkItemOptions : BaseOptions
    {
        [Value(0, HelpText = "Work Item ID")]
        public int WorkItemId { get; set; }

        [Option('r', "release", HelpText = "Release source from create branch")]
        public string Release { get; set; }

        [Option("noprune", HelpText = "Disable clear local branches")]
        public bool NoPrune { get; set; }

        [Option("runscripts", HelpText = "Run script from database scripts folder", Default = true)]
        public bool RunDbScripts { get; set; }
    }
}
