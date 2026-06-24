using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("pbi", HelpText = "Create a Product Backlog Item in Azure DevOps")]
    public class CreatePbiOptions : BaseOptions, ICreateWorkItemOptions
    {
        [Value(0, Required = true, HelpText = "Work item title")]
        public string Title { get; set; }

        [Option("desc-file", HelpText = "Markdown file with the Description (## Problema ...). Optional: omit for a title-only capture (New state, triage later)")]
        public string DescFile { get; set; }

        [Option("ac-file", HelpText = "Markdown file with the Acceptance Criteria (Cenarios)")]
        public string AcFile { get; set; }

        [Option("parent", HelpText = "Parent Feature id (optional)")]
        public int? Parent { get; set; }

        [Option("ref", HelpText = "Referencia externa (ex: ticket de suporte SUS-629) — gravada como tag; torna a criacao idempotente (nao duplica)")]
        public string ExternalRef { get; set; }
    }
}
