using Fig.Cli.Helpers;
using Fig.Cli.Options;

namespace Fig.Cli.Commands
{
    public class PullRequestCommand : GitCommand<PullRequestOptions>
    {
        public PullRequestCommand(PullRequestOptions opts, FigContext context) : base(opts, context)
        { }

        public override CommandResult Execute()
        {
            var currentBranch = GitHelper.GetCurrentBranchName();
            var targetBranch = Options.TargetBranch ?? "master";

            if (currentBranch.Contains("-on-"))
            {
                var targetBranchFromCurrentBranch = currentBranch.Substring(currentBranch.LastIndexOf("-on-") + 4).Replace("release-", "release/");

                if (targetBranch?.ToLower() != "master" && targetBranch != targetBranchFromCurrentBranch)
                    return Fail("Differents Target branch");

                targetBranch = targetBranchFromCurrentBranch;
            }

            var url = AzureGitHelper.BuildUrl(Context, "/pullrequestcreate?sourceRef={0}&targetRef={1}", currentBranch, targetBranch);
            BrowserHelper.OpenUrl(url);

            return CommandResult.Ok();
        }
    }
}
