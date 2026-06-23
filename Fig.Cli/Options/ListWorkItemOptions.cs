using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("list", HelpText = "List work items (use --mine for items assigned to you)")]
    public class ListWorkItemOptions : BaseOptions
    {
        [Option("mine", HelpText = "Only items assigned to me")]
        public bool Mine { get; set; }

        [Option("state", HelpText = "Filter by state (e.g. \"In Progress\")")]
        public string State { get; set; }

        [Option("type", HelpText = "Filter by work item type (e.g. Bug)")]
        public string Type { get; set; }

        [Option("top", Default = 30, HelpText = "Max items to return")]
        public int Top { get; set; }
    }
}
