using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("template", HelpText = "Create new template script")]
    public class ScriptTemplateOptions : BaseOptions
    {
        [Value(0, HelpText = "Template name", Required = true)]
        public string TemplateName { get; set; }
        public string Script { get; set; }
    }
}
