using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("feature", HelpText = "Create a Feature in Azure DevOps")]
    public class CreateFeatureOptions : BaseOptions, ICreateWorkItemOptions
    {
        [Value(0, Required = true, HelpText = "Work item title")]
        public string Title { get; set; }

        [Option("desc-file", Required = true, HelpText = "Markdown file with the Description")]
        public string DescFile { get; set; }

        [Option("ac-file", HelpText = "Markdown file with the Acceptance Criteria")]
        public string AcFile { get; set; }

        [Option("parent", HelpText = "Parent Epic id (optional)")]
        public int? Parent { get; set; }
    }
}
