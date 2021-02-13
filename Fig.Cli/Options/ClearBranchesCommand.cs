using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("cleardevbranches", HelpText = "Clear remove and local development branches")]
    public class ClearBranchesOptions : BaseOptions
    {
        [Option("withoutorigin", HelpText = "Remove the branches without remote origin")]
        public bool RemoveBranchesNoOrigin { get; set; }
    }
}
