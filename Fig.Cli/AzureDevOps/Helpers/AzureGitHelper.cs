using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Linq;

namespace Fig.Cli
{
    public static class AzureGitHelper
    {
        public static GitRepository FindRepository(AzureDevOpsClientContext context, string repositoryName)
        {
            var project = AzureProjectHelper.FindDefaultProject(context);

            if (!FindRepository(context, project.Id, repositoryName, out GitRepository repo))
            {
                throw new Exception("No repositories available. Create a repo in this project and run the sample again.");
            }

            return repo;
        }

        public static GitRepository FindRepository(AzureDevOpsClientContext context, Guid projectId, string repositoryName)
        {
            if (!FindRepository(context, projectId, repositoryName, out GitRepository repo))
            {
                throw new Exception("No repositories available. Create a repo in this project and run the sample again.");
            }

            return repo;
        }

        public static string BuildUrl(FigContext context, string relativePath, params object[] args)
        {
            var baseUrl = context.Options.ProjectUrl + "/" + context.Options.ProjectName + "/_git/" + context.Options.RepositoryName;

            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }

            if (relativePath.StartsWith("/"))
            {
                relativePath = relativePath.Remove(0, 1);
            }

            return string.Format(baseUrl + relativePath, args);
        }

        public static Guid? FindRepositoryId(AzureDevOpsClientContext context, Guid projectId, string repositoryName)
        {
            var repository = FindRepository(context, projectId, repositoryName);
            return repository?.Id;
        }

        public static bool FindRepository(AzureDevOpsClientContext context, Guid projectId, string repositoryName, out GitRepository repo)
        {
            // Check if we already have a repo loaded
            VssConnection connection = context.Connection;
            GitHttpClient gitClient = connection.GetClient<GitHttpClient>();

            // Check if an ID was already set (this could have been provided by the caller)
            if (!context.TryGetValue("repository" + repositoryName, out Guid repoId))
            {
                repo = gitClient.GetRepositoriesAsync(projectId).Result.FirstOrDefault(c => c.Name == repositoryName);
            }
            else
            {
                repo = gitClient.GetRepositoryAsync(repoId.ToString()).Result;
            }

            if (repo != null)
            {
                context.SetValue("repository" + repositoryName, repo);
            }
            else
            {
                // create a project here?
                throw new Exception("No repos available for running the sample.");
            }

            return repo != null;
        }

        public static string WithoutRefsPrefix(string refName)
        {
            if (!refName.StartsWith("refs/"))
            {
                return refName;
            }

            return refName.Remove(0, "refs/".Length);
        }

        public static string WithoutRefsAndHeadsPrefix(string refName)
        {
            return WithoutRefsPrefix(refName).Replace("heads/", "");
        }

        public static string WithRefsAndHeadsPrefix(string refName)
        {
            return !refName.StartsWith("refs/heads/") ? "refs/heads/" + refName : refName;
        }

        public static GitRef CreateBranch(this GitHttpClient gitClient, Guid repoId, string sourceRefName, string fullBranchName)
        {
            var sourceRef = gitClient.GetBranch(repoId, sourceRefName);
            return gitClient.CreateBranch(repoId, sourceRef, fullBranchName);
        }

        public static GitRef GetBranch(this GitHttpClient gitClient, Guid repoId, string branch)
        {
            return gitClient.GetRefsAsync(repoId, filter: "heads/" + WithoutRefsAndHeadsPrefix(branch)).Result.FirstOrDefault();
        }

        public static bool HasBranch(this GitHttpClient gitClient, Guid repoId, string branch)
        {
            return gitClient.GetBranch(repoId, branch) != null;
        }

        public static GitRef CreateBranch(this GitHttpClient gitClient, Guid repoId, GitRef sourceRef, string fullBranchName)
        {
            GitRefUpdateResult refCreateResult = gitClient.UpdateRefsAsync(
                   new GitRefUpdate[] { new GitRefUpdate() {
                    OldObjectId = new string('0', 40),
                    NewObjectId = sourceRef.ObjectId,
                    Name = WithRefsAndHeadsPrefix(fullBranchName),
                } },
                   repositoryId: repoId).Result.First();

            return gitClient.GetRefsAsync(repoId, filter: "heads/" + WithoutRefsAndHeadsPrefix(fullBranchName)).Result.First();
        }
    }
}
