using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("task", HelpText = "Create a Task under a parent work item")]
    public class CreateTaskOptions : BaseOptions
    {
        [Value(0, Required = true, HelpText = "Parent work item id")]
        public int Parent { get; set; }

        [Value(1, Required = true, HelpText = "Task title")]
        public string Title { get; set; }

        [Option("desc-file", HelpText = "Markdown file with the Description (optional)")]
        public string DescFile { get; set; }
    }
}
