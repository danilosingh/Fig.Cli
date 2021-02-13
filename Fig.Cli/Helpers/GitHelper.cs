using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fig.Cli.Helpers
{
    public static class GitHelper
    {
        public static bool Pull(bool showCommand = true)
        {
            return ExecuteCommad("git pull", showCommand).IsSuccess;
        }

        public static bool Push(bool showCommand = true)
        {
            return ExecuteCommad("git push", showCommand).IsSuccess;
        }

        public static IList<string> GetLocalBranches()
        {
            var result = ExecuteCommad("git branch", false, true, false);
            var branches = result.Output.Split("\n");
            return branches.Select(c =>
            {
                c = c.Trim();

                if (c.StartsWith("* "))
                    c = c.Remove(0, 2);

                return c;

            }).ToList();
        }

        public static bool Fetch(bool prune = false, bool showCommand = true)
        {
            return ExecuteCommad("git fetch" + (prune ? " -p" : ""), showCommand).IsSuccess;
        }

        public static string GetCurrentBranchName()
        {
            return ExecuteCommad("git rev-parse --abbrev-ref HEAD", false).Output?.Trim('\n');
        }

        public static bool Sync(bool showCommand = true)
        {
            if (!Pull(showCommand) || !Push(showCommand))
                return false;

            return IsSynchronized();
        }

        public static bool IsSynchronized()
        {
            var revUpStream = ExecuteCommad("git rev-parse @{u}", false, false, false).Output;
            var rev = ExecuteCommad("git rev-parse HEAD", false, false, false).Output;

            return rev == revUpStream;
        }

        public static bool ContainsConflicts()
        {
            return !IsBlankOutput(ExecuteCommad($"git ls-files -u", false).Output);
        }

        public static bool DeleteBranch(string branchName, bool showCommand = true, bool writeOutput = true)
        {
            return ExecuteCommad("git branch -d " + branchName, showCommand, true, writeOutput).IsSuccess;
        }

        private static bool IsBlankOutput(string output)
        {
            return string.IsNullOrEmpty(output?.Trim('\n'));
        }

        public static bool Merge(string sourceBranch, bool showCommand = true)
        {
            return ExecuteCommad($"git merge {sourceBranch}", showCommand).IsSuccess;
        }

        public static bool Rebase(string sourceBranch, bool showCommand = true)
        {
            return ExecuteCommad($"git rebase {sourceBranch}", showCommand).IsSuccess;
        }

        public static bool MergeInProgress()
        {
            return File.Exists($@"{FigContext.Instance.RootDirectory}\.git\MERGE_HEAD");
        }

        public static bool PushBranch(bool showCommand = true)
        {
            return ExecuteCommad($"git push -u origin {GetCurrentBranchName()}", showCommand).IsSuccess;
        }

        public static bool RebaseInProgress()
        {
            return Directory.Exists($@"{FigContext.Instance.RootDirectory}\.git\rebase-apply") ||
                Directory.Exists($@"{FigContext.Instance.RootDirectory}\.git\rebase-merge");
        }

        public static bool AddAllFiles(bool showCommand = true)
        {
            return ExecuteCommad($"git add --all", showCommand).IsSuccess;
        }

        public static string FindRootDirectory()
        {
            var current = Directory.GetCurrentDirectory();

            while (!string.IsNullOrEmpty(current))
            {
                if (Directory.Exists(Path.Combine(current, ".git")))
                {
                    break;
                }

                current = Directory.GetParent(current)?.FullName;
            }

            return current;
        }

        public static bool Commit(string commitMessage, bool checkHasChangesBeforeCommit = false, bool showCommand = true)
        {
            if (checkHasChangesBeforeCommit && !HasChanges(showCommand))
                return true;

            return ExecuteCommad(string.Format("git commit -am \"{0}\"", commitMessage), showCommand).IsSuccess;
        }

        public static bool HasChanges(bool showCommand = true, bool writeOutput = true)
        {
            var result = ExecuteCommad("git status", showCommand, writeOutput: writeOutput);
            return result.Output != null && !result.Output.Contains("nothing to commit");
        }

        public static bool BranchHasRemoteOrigin(string branchName)
        {
            return !ExecuteCommad("git show-branch remotes/origin/" + branchName, false, writeOutput: false).Output.Contains("fatal:");
        }

        public static bool Checkout(string targetBranch, bool pull = false, bool fetch = false, bool showCommand = true, bool writeOutput = true)
        {
            if (fetch)
                ExecuteCommad("git fetch", showCommand);

            var result = ExecuteCommad(string.Format("git checkout {0}", targetBranch), showCommand, writeOutput: writeOutput).IsSuccess;

            if (!result)
                return false;

            return !pull || Pull();
        }

        public static bool CreateBranch(string sourceName, string newBranchName, bool checkExists = false, bool showCommand = true)
        {
            if (!Checkout(sourceName, true))
                return false;

            return CreateBranch(newBranchName, checkExists, showCommand);
        }

        public static bool CreateBranch(string branchName, bool checkExists = false, bool showCommand = true)
        {
            if (checkExists && BranchExists(branchName, showCommand))
                return Checkout(branchName, true);

            return ExecuteCommad(string.Format("git checkout -b {0}", branchName), showCommand).IsSuccess;
        }

        public static bool BranchExists(string name, bool showCommand = true)
        {
            if (!name.StartsWith("refs/heads/"))
                name = "refs/heads/" + name;

            var result = ExecuteCommad("git show-ref " + name, showCommand);
            return !string.IsNullOrEmpty(result.Output?.Trim('\n'));
        }

        public static bool CherryPick(IEnumerable<string> commits, bool showCommand = true)
        {
            if (!commits.Any())
                return true;

            return ExecuteCommad($"git cherry-pick {string.Join(" ", commits)}", showCommand).IsSuccess;
        }

        public static bool ContainsCommit(string commitId, bool showCommand = true)
        {
            return ExecuteCommad($"git branch --contains {commitId} | grep {GetCurrentBranchName()}", showCommand).IsSuccess;
        }

        public static void Prune(bool showCommand = true)
        {
            ExecuteCommad("git remote prune origin", showCommand);
        }

        private static CmdResult ExecuteCommad(string command, bool showCommand, bool useCommandIndicator = true, bool writeOutput = true)
        {
            return CmdHelper.ExecuteCommand(command, FigContext.Instance.RootDirectory, showCommand, writeOutput, useCommandIndicator);
        }

        public static CmdResult ExecuteCommads(params string[] command)
        {
            foreach (var item in command)
            {
                var result = ExecuteCommad(item, true);

                if (!result.IsSuccess)
                    return result;
            }

            return new CmdResult() { ExitCode = 0 };
        }

        public static string GetRemoteRepositoryName()
        {
            var result = ExecuteCommad("git remote get-url origin", false);
            if (!result.IsSuccess || string.IsNullOrEmpty(result.Output))
            {
                return null;
            }
            var lastDashIndex = result.Output.LastIndexOf('/') + 1;
            var endIndex = result.Output.IndexOf(".", lastDashIndex);
            if (endIndex < 0)
            {
                endIndex = result.Output.Length - 1;
            }
            return result.Output.Substring(lastDashIndex, endIndex - lastDashIndex);
        }
    }
}
