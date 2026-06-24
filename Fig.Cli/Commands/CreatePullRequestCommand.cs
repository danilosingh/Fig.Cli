using Fig.Cli.Extensions;
using Fig.Cli.Helpers;
using Fig.Cli.Options;
using Fig.Cli.TeamFoundation.Helpers;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fig.Cli.Commands
{
    public class CreatePullRequestCommand : AzureDevOpsCommand<CreatePullRequestOptions>
    {
        private readonly GitHttpClient gitClient;
        private readonly WorkItemTrackingHttpClient workItemTrackingClient;

        public CreatePullRequestCommand(CreatePullRequestOptions opts, FigContext context) : base(opts, context)
        {
            gitClient = AzureContext.Connection.GetClient<GitHttpClient>();
            workItemTrackingClient = AzureContext.Connection.GetClient<WorkItemTrackingHttpClient>();
        }

        public override CommandResult Execute()
        {
            var project = AzureProjectHelper.FindDefaultProject(AzureContext);
            var repo = AzureGitHelper.FindRepository(AzureContext, project.Id, Context.Options.RepositoryName);

            var source = AzureGitHelper.WithoutRefsAndHeadsPrefix(Options.SourceBranch ?? GitHelper.GetCurrentBranchName());
            var target = AzureGitHelper.WithoutRefsAndHeadsPrefix(Options.TargetBranch ?? AzureGitHelper.WithoutRefsPrefix(repo.DefaultBranch));
            var sourceRef = "refs/heads/" + source;
            var targetRef = "refs/heads/" + target;

            // Idempotente: se ja existe um PR ativo source->target, retorna ele em vez
            // de criar um duplicado (seguro para o resume do fluxo /dev).
            var existing = gitClient.GetPullRequestsAsync(repo.Id, new GitPullRequestSearchCriteria
            {
                SourceRefName = sourceRef,
                TargetRefName = targetRef,
                Status = PullRequestStatus.Active
            }).Result.FirstOrDefault();

            if (existing != null)
                return Ok("PR #{0} ja existe: {1}", existing.PullRequestId, BuildPrUrl(project.Name, repo.Name, existing.PullRequestId));

            var title = Options.Title;

            if (string.IsNullOrWhiteSpace(title))
                title = GitHelper.GetLastCommitSubject();

            if (string.IsNullOrWhiteSpace(title))
                title = source;

            var created = gitClient.CreatePullRequestAsync(new GitPullRequest
            {
                SourceRefName = sourceRef,
                TargetRefName = targetRef,
                Title = title,
                Description = Options.Description,
                IsDraft = Options.Draft
            }, repo.Id).Result;

            MoveWorkItemToReview(source);

            return Ok("PR #{0} criado: {1}", created.PullRequestId, BuildPrUrl(project.Name, repo.Name, created.PullRequestId));
        }

        // Ao abrir o PR, o item de backlog sai de Committed e entra em Review (aguardando
        // revisao/merge). Resolve o id pelo nome da branch (dev/{pbi|bug|feature}-<id>).
        // Nao mexe em itens ja em Done/Removed/Review.
        private void MoveWorkItemToReview(string sourceBranch)
        {
            var id = WorkItemIdFromBranch(sourceBranch);

            if (id <= 0)
                return;

            var workItem = workItemTrackingClient.GetWorkItemAsync(id, expand: WorkItemExpand.All).Result;
            var type = workItem.GetField<string>("System.WorkItemType");

            if (type != "Product Backlog Item" && type != "Bug")
                return;

            var state = workItem.GetField<string>("System.State");

            if (state == "Done" || state == "Removed" || state == "Review")
                return;

            AzureWorkItemHelpers.ChangeState(workItemTrackingClient, id, "Review");
        }

        private static int WorkItemIdFromBranch(string branch)
        {
            var match = Regex.Match(branch ?? string.Empty, @"(?:^|/)(?:pbi|bug|feature|branch)-(\d+)", RegexOptions.IgnoreCase);

            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }

        private string BuildPrUrl(string projectName, string repoName, int pullRequestId)
        {
            return $"{Context.Options.ProjectUrl}/{projectName}/_git/{repoName}/pullrequest/{pullRequestId}";
        }
    }
}
