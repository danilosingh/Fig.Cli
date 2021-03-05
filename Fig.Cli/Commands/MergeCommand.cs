using Fig.Cli.Helpers;
using Fig.Cli.Options;

namespace Fig.Cli.Commands
{
    public class MergeCommand : GitCommand<MergeOptions>
    {
        public MergeCommand(MergeOptions opts, FigContext context) : base(opts, context)
        {
        }

        public override CommandResult Execute()
        {
            if (string.IsNullOrEmpty(Options.SourceBranch))
                throw new FigException("Source branch is required.");

            var targetBranch = Options.TargetBranch ?? GitHelper.GetCurrentBranchName();

            if (targetBranch == Options.SourceBranch)
                throw new FigException("Source branch equal target branch.");

            if (!Confirm($"Confirm merge {Options.SourceBranch} to {targetBranch}?"))
                return Canceled();

            if (GitHelper.MergeInProgress())
            {
                if (GitHelper.ContainsConflicts())
                    return Fail("Current merge contains conflicts.");

                if (!Confirm("Confirm commit merge?"))
                    return Canceled();
            }
            else
            {
                AddStep(() => GitHelper.Checkout(Options.SourceBranch, true));                
                AddStep(() => GitHelper.Checkout(targetBranch, true));
                AddStep(() => GitHelper.Merge(Options.SourceBranch));
            }

            AddStep(() => GitHelper.Commit($"Merge {Options.SourceBranch} to {targetBranch}", true));
            AddStep(() => GitHelper.Sync());

            return ExecuteSteps();
        }
    }
}
