using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("link", HelpText = "Add a hyperlink to a work item (repository path or absolute URL)")]
    public class LinkWorkItemOptions : BaseOptions
    {
        [Value(0, Required = true, HelpText = "Work item id")]
        public int Id { get; set; }

        [Value(1, Required = true, HelpText = "Repository path (docs/specs/app/file.md) or absolute URL (https://...)")]
        public string Target { get; set; }

        [Option("title", HelpText = "Label shown in the work item links tab")]
        public string Title { get; set; }

        [Option("branch", HelpText = "Pin a repository link to a branch (default: repository default branch)")]
        public string Branch { get; set; }
    }
}
