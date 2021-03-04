using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("set-option", HelpText = "Set Option in Fig config.")]
    public class SetOptions : BaseOptions
    {
        [Value(0,  HelpText = "Name of option")]
        public string Name { get; set; }

        [Value(1, HelpText = "Value to set")]
        public string Value { get; set; }
    }
}
