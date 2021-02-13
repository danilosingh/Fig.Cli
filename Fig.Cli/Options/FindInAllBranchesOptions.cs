using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("findinbranches", HelpText = "Find in all local branches")]
    public class FindInBranchesOptions : BaseOptions
    {
        [Option("filename", HelpText = "Full file name", Required =true)]
        public string FileName { get; set; }
        
        [Option("search", HelpText = "Search text", Required = true)]
        public string SearchText { get; set; }

        [Option("onlydevbranches", HelpText = "Only dev branches")]
        public bool OnlyDevBranches { get; set; }
    }
}
