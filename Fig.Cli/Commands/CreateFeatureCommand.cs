using Fig.Cli.Options;

namespace Fig.Cli.Commands
{
    public class CreateFeatureCommand : CreateWorkItemCommandBase<CreateFeatureOptions>
    {
        public CreateFeatureCommand(CreateFeatureOptions opts, FigContext context) : base(opts, context)
        {
        }

        protected override string WorkItemType => "Feature";

        protected override string BodyFieldRefName => "System.Description";
    }
}
