using CommandLine;

namespace Fig.Cli.Options
{
    public class ProjectOptions : BaseOptions
    {
        [Option("add", HelpText = "Add new project")]
        public bool Add { get; set; }
        [Option("edit", HelpText = "Edit existing project")]
        public bool Edit { get; set; }
        [Option("del", HelpText = "Delete existing project")]
        public bool Delete { get; set; }
        [Option("show", HelpText = "Show all projects")]
        public bool Show { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
