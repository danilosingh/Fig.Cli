using Fig.Cli.Helpers;
using Fig.Cli.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System;

namespace Fig.Cli.Commands
{
    public class MergePullRequestCommand : AzureDevOpsCommand<MergePullRequestOptions>
    {
        public MergePullRequestCommand(MergePullRequestOptions opts, FigContext context) : base(opts, context)
        {
        }

        public override CommandResult Execute()
        {
            if (string.IsNullOrEmpty(Options.TargetBranch))
                Options.TargetBranch = "master";

            var gitHttpClient = AzureContext.Connection.GetClient<GitHttpClient>();

            var project = AzureProjectHelper.FindDefaultProject(AzureContext);
            var repo = AzureGitHelper.FindRepository(AzureContext, project.Id, Context.Options.RepositoryName);
            var pullRequest = gitHttpClient.GetPullRequestAsync(repo.Id, Options.PullRequestId).Result;

            if (pullRequest == null)
                return Fail("Pull request not found.");

            if (pullRequest.Status != PullRequestStatus.Completed)
                return Fail("Pull request not completed");

            var topicBranchName = pullRequest.SourceRefName.Replace("refs/heads/", "") + "-on-" + Options.TargetBranch.Replace("/", "-");
            var pullRequestTargetBranch = pullRequest.TargetRefName.Replace("refs/heads/", "");

            if (pullRequestTargetBranch == Options.TargetBranch)
                return Fail($"The pull request branch is same target branch [{Options.TargetBranch}]");

            if (!Confirm($"Confirm merge {pullRequestTargetBranch} to {Options.TargetBranch}? Will be created {topicBranchName}"))
                return Canceled();

            if (!GitHelper.MergeInProgress())
            {
                AddStep(() => GitHelper.Checkout(pullRequestTargetBranch, true));
                AddStep(() => GitHelper.CreateBranch(Options.TargetBranch, topicBranchName, true));
                AddStep(() => GitHelper.Merge(pullRequestTargetBranch));
            }

            AddStep(() => GitHelper.Commit($"Merge {pullRequestTargetBranch} to {Options.TargetBranch}", checkHasChangesBeforeCommit: true));
            AddStep(() => GitHelper.PushBranch());
            AddStep(() => GitHelper.Sync());
            AddStep(() => CreatePullRequest(gitHttpClient, repo.Id, pullRequest, topicBranchName, Options.TargetBranch, pullRequestTargetBranch));

            return ExecuteSteps();
        }

        private CommandResult CreatePullRequest(GitHttpClient client, Guid repoId, GitPullRequest pullRequestBase, string topicBranchName, string targetBranch, string pullRequestTargetBranch)
        {
            var pullRequest = client.CreatePullRequestAsync(new GitPullRequest()
            {
                SourceRefName = AzureGitHelper.WithRefsAndHeadsPrefix(topicBranchName),
                TargetRefName = AzureGitHelper.WithRefsAndHeadsPrefix(targetBranch),
                Title = $"Merge {pullRequestTargetBranch} to {Options.TargetBranch}",
                Description = $"Originated by Pull Request {pullRequestBase.PullRequestId} - {pullRequestBase.Title}"
            }, repoId).Result;

            var url = AzureGitHelper.BuildUrl(Context, "/pullrequest/" + pullRequest.PullRequestId);
            BrowserHelper.OpenUrl(url);

            return Ok();
        }
    }
}
