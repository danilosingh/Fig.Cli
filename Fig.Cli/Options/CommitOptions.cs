using CommandLine;

namespace Fig.Cli.Options
{
    [Verb("commit", HelpText = "Commit changes on current branch (using -am).")]
    public class CommitOptions : BaseOptions
    {        
        [Option('m', "message", HelpText = "Message from commit")]
        public string Message { get; set; }

        public CommitOptions()
        { }

        public CommitOptions(string message)
        {
            Message = message;
        }
    }
}
