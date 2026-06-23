using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("edit", HelpText = "Edit an existing work item (PBI or Bug)")]
    public class EditWorkItemOptions : BaseOptions
    {
        [Value(0, Required = true, HelpText = "Work item id")]
        public int Id { get; set; }

        [Option("title", HelpText = "New title")]
        public string Title { get; set; }

        [Option("desc-file", HelpText = "Markdown file for the body (Description for PBI, Repro Steps for Bug)")]
        public string DescFile { get; set; }

        [Option("ac-file", HelpText = "Markdown file with the Acceptance Criteria")]
        public string AcFile { get; set; }
    }
}
