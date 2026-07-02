using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("script", HelpText = "Create new script")]
    public class ScriptOptions : BaseOptions
    {
        [Value(0, HelpText = "Template name", Required = false)]
        public string TemplateName { get; set; }

        [Option("noopen", HelpText = "Cria o script sem abrir o arquivo no editor padrão (ex: DBeaver)")]
        public bool NoOpen { get; set; }
    }
}
