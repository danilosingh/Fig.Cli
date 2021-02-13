using Fig.Cli.Helpers;
using Fig.Cli.Options;

namespace Fig.Cli.Commands
{
    public class SyncCommand : GitCommand<SyncOptions>
    {
        public SyncCommand(SyncOptions opts, FigContext context) : base(opts, context)
        { }

        public override CommandResult Execute()
        {
            return GitHelper.Sync(true) ? Ok() : Fail("Sync failed");
        }
    }
}
