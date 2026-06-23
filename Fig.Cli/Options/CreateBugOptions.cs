using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("bug", HelpText = "Create a Bug in Azure DevOps")]
    public class CreateBugOptions : BaseOptions, ICreateWorkItemOptions
    {
        [Value(0, Required = true, HelpText = "Work item title")]
        public string Title { get; set; }

        [Option("desc-file", HelpText = "Markdown file with the Repro Steps (## Sintoma ...). Optional: omit for a title-only capture (New state, triage later)")]
        public string DescFile { get; set; }

        [Option("ac-file", HelpText = "Markdown file with the Definicao de corrigido")]
        public string AcFile { get; set; }

        [Option("parent", HelpText = "Parent Feature id (optional)")]
        public int? Parent { get; set; }
    }
}
