using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("jira", HelpText = "Read a Jira issue (and optionally comment / transition its status)")]
    public class JiraOptions : BaseOptions
    {
        [Value(0, Required = true, HelpText = "Jira issue key (e.g. SUS-629)")]
        public string Key { get; set; }

        [Option("comment", HelpText = "Add a comment to the issue")]
        public string Comment { get; set; }

        [Option("transition", HelpText = "Transition the issue to a status by name (e.g. \"Em andamento\")")]
        public string Transition { get; set; }

        [Option("json", HelpText = "Output the issue as JSON (key/summary/description/type/status/url)")]
        public bool Json { get; set; }
    }
}
