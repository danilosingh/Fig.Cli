using System;
using Fig.Cli.Options;

namespace Fig.Cli.Commands
{
    public class GuidCommand : Command<GuidOptions>
    {
        public GuidCommand(GuidOptions opts, FigContext context) : base(opts, context)
        { }

        public override CommandResult Execute()
        {
            Console.WriteLine(Guid.NewGuid().ToString());
            return CommandResult.Ok();
        }
    }
}
