using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("migratedb", HelpText = "Migrate Database.")]
    public class MigrateDbOptions : RunScriptsOptions
    {
        [Option('t', "table", HelpText = "Migrations Table Name", Default = "migrations")]
        public string MigrationsTable { get; set; }
    }
}
