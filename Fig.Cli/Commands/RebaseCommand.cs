using Fig.Cli.Helpers;
using Fig.Cli.Options;
using System;

namespace Fig.Cli.Commands
{
    public class RebaseCommand : GitCommand<RebaseOptions>
    {
        public RebaseCommand(RebaseOptions opts, FigContext context) : base(opts, context)
        {
        }

        public override CommandResult Execute()
        {
            if (string.IsNullOrEmpty(Options.SourceBranch))
                throw new ArgumentException("Source branch is required.");

            var targetBranch = Options.TargetBranch ?? GitHelper.GetCurrentBranchName();

            if (targetBranch == Options.SourceBranch)
                throw new ArgumentException("Source branch equal target branch.");

            if (!Confirm($"Confirm rebase {Options.SourceBranch} to {targetBranch}?"))
                return Canceled();

            if (GitHelper.RebaseInProgress())
            {
                if (GitHelper.ContainsConflicts())
                    return Fail("Current rebase contains conflicts.");

                if (!Confirm("Confirm commit rebase?"))
                    return Canceled();
            }
            else
            {
                AddStep(() => GitHelper.Checkout(Options.SourceBranch));
                AddStep(() => GitHelper.Pull());
                AddStep(() => GitHelper.Checkout(targetBranch));
                AddStep(() => GitHelper.Rebase(Options.SourceBranch));
            }

            AddStep(() => GitHelper.Commit($"Rebase {Options.SourceBranch} to {targetBranch}", true));
            AddStep(() => GitHelper.Sync());

            return ExecuteSteps();
        }
    }
}
