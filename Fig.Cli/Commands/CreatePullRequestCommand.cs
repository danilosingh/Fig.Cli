using Fig.Cli.Helpers;
using Fig.Cli.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System.Linq;

namespace Fig.Cli.Commands
{
    public class CreatePullRequestCommand : AzureDevOpsCommand<CreatePullRequestOptions>
    {
        private readonly GitHttpClient gitClient;

        public CreatePullRequestCommand(CreatePullRequestOptions opts, FigContext context) : base(opts, context)
        {
            gitClient = AzureContext.Connection.GetClient<GitHttpClient>();
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

            return Ok("PR #{0} criado: {1}", created.PullRequestId, BuildPrUrl(project.Name, repo.Name, created.PullRequestId));
        }

        private string BuildPrUrl(string projectName, string repoName, int pullRequestId)
        {
            return $"{Context.Options.ProjectUrl}/{projectName}/_git/{repoName}/pullrequest/{pullRequestId}";
        }
    }
}
