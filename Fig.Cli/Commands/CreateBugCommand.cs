using Fig.Cli.Options;

namespace Fig.Cli.Commands
{
    public class CreateBugCommand : CreateWorkItemCommandBase<CreateBugOptions>
    {
        public CreateBugCommand(CreateBugOptions opts, FigContext context) : base(opts, context)
        {
        }

        protected override string WorkItemType => "Bug";

        // No processo Scrum, o corpo do Bug vai em Repro Steps.
        protected override string BodyFieldRefName => "Microsoft.VSTS.TCM.ReproSteps";
    }
}
