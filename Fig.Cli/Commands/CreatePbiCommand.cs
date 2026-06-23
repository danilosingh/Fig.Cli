using Fig.Cli.Options;

namespace Fig.Cli.Commands
{
    public class CreatePbiCommand : CreateWorkItemCommandBase<CreatePbiOptions>
    {
        public CreatePbiCommand(CreatePbiOptions opts, FigContext context) : base(opts, context)
        {
        }

        protected override string WorkItemType => "Product Backlog Item";

        protected override string BodyFieldRefName => "System.Description";
    }
}
