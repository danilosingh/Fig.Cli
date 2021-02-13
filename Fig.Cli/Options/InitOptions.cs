using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("init", HelpText = "Init Fig.Cli in a repository.")]
    public class InitOptions : BaseOptions
    {
        [Option('l', "url", HelpText = "Azure DevOps Team Project URL")]
        public string ProjectUrl { get; set; }

        [Option('n', "project", HelpText = "Project Name")]
        public string ProjectName { get; set; }

        [Option('u', "userName", HelpText = "UserName")]
        public string UserName { get; set; }

        [Option('p', "password", HelpText = "Password")]
        public string Password { get; set; }

        [Option("pat", HelpText = "PAT (Azure DevOps)")]
        public string Pat { get; set; }

        [Option("devid", HelpText = "Developer ID")]
        public string DeveloperId { get; set; }

        [Option("devname", HelpText = "Developer Name")]
        public string DeveloperName { get; set; }

        [Option("dbspath", HelpText = "Relative database script path")]
        public string DbScriptPath { get; set; }

        [Option("dbuser", HelpText = "Database username")]
        public string DbUserName { get; set; }

        [Option("dbpassword", HelpText = "Database password")]
        public string DbPassword { get; set; }

        [Option("dbserver", HelpText = "Database server")]
        public string DbServer { get; set; }

        [Option("dbname", HelpText = "Database name")]
        public string DbName { get; set; }

        [Option("dbprovider", HelpText = "Database provider", Default = DbProviders.PostgreSql)]
        public string DbProvider { get; set; }

    }
}
