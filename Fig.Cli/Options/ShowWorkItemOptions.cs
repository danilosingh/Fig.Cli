using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("show", HelpText = "Show a work item (title, body, acceptance criteria)")]
    public class ShowWorkItemOptions : BaseOptions
    {
        [Value(0, Required = true, HelpText = "Work item id")]
        public int Id { get; set; }
    }
}
