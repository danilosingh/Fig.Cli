using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("comment", HelpText = "Add a comment to a work item")]
    public class CommentWorkItemOptions : BaseOptions
    {
        [Value(0, Required = true, HelpText = "Work item id")]
        public int Id { get; set; }

        [Value(1, Required = false, HelpText = "Comment text (markdown)")]
        public string Text { get; set; }

        [Option("file", HelpText = "Markdown file with the comment")]
        public string File { get; set; }
    }
}
