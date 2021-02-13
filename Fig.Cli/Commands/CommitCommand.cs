using Fig.Cli.Helpers;
using Fig.Cli.Options;

namespace Fig.Cli.Commands
{
    public class CommitCommand : GitCommand<CommitOptions>
    {
        public CommitCommand(CommitOptions opts, FigContext context) : base(opts, context)
        { }

        public override CommandResult Execute()
        {
            return GitHelper.Commit(Options.Message, true) ? Ok() : Fail();
        }
    }
}
