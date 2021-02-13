namespace Fig.Cli.Commands
{
    public abstract class GitCommand<T> : Command<T>
    {
        protected GitCommand(T opts, FigContext context) : base(opts, context)
        { }
    }
}
