using Fig.Cli.Options;

namespace Fig.Cli.Commands
{
    public class SetOptionCommand : Command<SetOptions>
    {
        public SetOptionCommand(SetOptions opts, FigContext context) : base(opts, context)
        {
            EnsureConfiguration();
        }

        public override CommandResult Execute()
        {
            Context.SetOption(Options.Name, Options.Value);
            Context.SaveOptions();
            return Ok();
        }
    }
}
