using Fig.Cli.Helpers;
using Fig.Cli.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fig.Cli.Commands
{
    public class ClearBranchesCommand : AzureDevOpsCommand<ClearBranchesOptions>
    {
        public ClearBranchesCommand(ClearBranchesOptions opts, FigContext context) : base(opts, context)
        {
        }

        public override CommandResult Execute()
        {
            GitHelper.Fetch(prune: false, showCommand: false);

            if (GitHelper.HasChanges(false, false))
                return Fail("Has uncommited changes in current branch");

            var branches = GitHelper.GetLocalBranches().Where(c => c.StartsWith("dev/")).ToList();
            var deletingBranches = new List<string>();
            var maxLenght = branches.Max(c => c.Length) + 2;

            foreach (var item in branches)
            {
                bool hasRemoteOrigin = GitHelper.BranchHasRemoteOrigin(item);
                bool isSyncronized = false;

                if (hasRemoteOrigin)
                {
                    GitHelper.Checkout(item, showCommand: false, writeOutput: false);
                    isSyncronized = GitHelper.IsSynchronized();
                }

                WriteLine(item.CompleteWithEmptySpaces(maxLenght) + (!hasRemoteOrigin ? "[No remote origin]" : (isSyncronized ? "[Syncronized]" : "[No syncronized]")));

                if ((hasRemoteOrigin && isSyncronized) || (!hasRemoteOrigin && Options.RemoveBranchesNoOrigin))
                    deletingBranches.Add(item);
            }

            WriteBreakLine();

            if (!deletingBranches.Any())
                return Ok("No branches for deleting.");

            if (!Confirm("Confirm deleted follow branches: \n{0}", string.Join(Environment.NewLine, deletingBranches)))
                return Canceled();

            GitHelper.Checkout("master", showCommand: false, writeOutput: false);

            foreach (var item in deletingBranches)
            {
                WriteLine($"Deleting branch {item} {(GitHelper.DeleteBranch(item, false, false) ? "[OK]" : "[Fail]")}");
            }

            return Ok();
        }
    }
}
