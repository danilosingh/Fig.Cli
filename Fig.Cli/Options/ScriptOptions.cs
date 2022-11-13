using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("script", HelpText = "Create new script")]
    public class ScriptOptions : BaseOptions
    {
        [Value(0, HelpText = "Template name", Required = false)]
        public string TemplateName { get; set; }
    }
}
