using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("runscripts", HelpText = "Run scripts from a folder")]
    public class RunScriptsOptions : BaseOptions
    {
        [Option('s',"server", HelpText = "Server Instance - for SqlServer")]
        public string Server { get; internal set; }
        [Option('d', "db", HelpText = "Database name")]
        public string Database { get; set; }
        [Option('u', "user", HelpText = "User")]
        public string UserName { get; set; }
        [Option('p', "password", HelpText = "Password")]
        public string Password { get; set; }
        [Option('d', "directory", HelpText = "Scripts directory")]
        public string ScriptsDirectory { get; set; }
        [Option("supressconfirm", HelpText = "Script path")]
        public bool SupressConfirmation { get; set; }
        [Option("ignoredirectory", HelpText = "Ignore directory exists")]
        public bool IgnoreDirectoryExists { get; set; }
        [Option("greaterthan", HelpText = "Run scripts greater than")]
        public string GreaterThan { get; set; }
        [Option("provider", HelpText = "Database provider - PostgreSQl(default), SqlServer")]
        public string Provider { get; set; }
    }
}
