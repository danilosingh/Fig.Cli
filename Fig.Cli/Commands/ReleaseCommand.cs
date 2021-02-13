using Fig.Cli.Helpers;
using Fig.Cli.Options;
using Fig.Cli.Versioning;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Fig.Cli.Commands
{
    public class ReleaseCommand : AzureDevOpsCommand<ReleaseOptions>
    {
        private readonly GitHttpClient gitClient;

        public ReleaseCommand(ReleaseOptions opts, FigContext context) : base(opts, context)
        {
            gitClient = AzureContext.Connection.GetClient<GitHttpClient>();
        }

        public override CommandResult Execute()
        {
            if (Options.ReleaseName.Contains("/"))
            {
                return Fail("Invalid release name");
            }

            var baseBranch = Options.BaseBranch ?? GetBaseBranch(Options.ReleaseName);
            var versionInfo = new VersionInfo(Options.ReleaseName);
            var repo = AzureGitHelper.FindRepository(AzureContext, Context.Options.RepositoryName);
            var releaseBranchName = "release/" + Options.ReleaseName;

            if (gitClient.HasBranch(repo.Id, releaseBranchName))
            {
                return Fail("Release already exists.");
            }

            if (!Confirm($"Confirm created branch {releaseBranchName} from {baseBranch}?"))
            {
                return Canceled();
            }

            AddStep(() => gitClient.CreateBranch(repo.Id, baseBranch, releaseBranchName));
            AddStep(() => ConfigureNextReleaseOnMaster(gitClient, repo.Id, versionInfo));

            return ExecuteSteps();
        }

        private string GetBaseBranch(string releaseName)
        {
            var versionArray = releaseName.Split('.');

            if (versionArray.Length < 3)
            {
                throw new ArgumentException("Invalid release name. (Valid name: 1.0.0)");
            }

            int path = Convert.ToInt32(versionArray[2]);

            return path == 0 ? "master" : $"release/{versionArray[0]}.{versionArray[1]}.{path - 1}";
        }

        private bool ConfigureNextReleaseOnMaster(GitHttpClient gitClient, Guid repoId, VersionInfo versionInfo)
        {
            var nextVersion = new VersionInfo(versionInfo.ToString());
            nextVersion.NextMinor();
            nextVersion.LastScript = GetLastScriptName();

            var topicBranchName = "dev/config-release-" + nextVersion.ToString();

            gitClient.CreateBranch(repoId, "master", topicBranchName);

            if (!GitHelper.Checkout(topicBranchName, true, true))
            {
                return false;
            }

            SaveFileVersion(nextVersion);

            if (!GitHelper.AddAllFiles() ||
                !GitHelper.Commit($"Inicialização da release {nextVersion}") ||
                !GitHelper.Sync())
                return false;

            CreatePullRequest(gitClient, repoId, topicBranchName, "master", nextVersion);

            return true;
        }

        private void CreatePullRequest(GitHttpClient client, Guid repoId, string sourceBranch, string targetBranch, VersionInfo versionInfo)
        {
            client.CreatePullRequestAsync(new GitPullRequest()
            {
                SourceRefName = AzureGitHelper.WithRefsAndHeadsPrefix(sourceBranch),
                TargetRefName = AzureGitHelper.WithRefsAndHeadsPrefix(targetBranch),
                Title = $"Inicialização da release {versionInfo}"
            }, repoId).Wait();
        }

        private string GetLastScriptName()
        {
            string path = StringHelper.ConcatPath(Context.RootDirectory, Context.Options.DbScriptPath);
            return Directory.GetFiles(path).Select(c => Path.GetFileName(c)).OrderByDescending(c => c).FirstOrDefault();
        }

        private void SaveFileVersion(VersionInfo versionInfo)
        {
            var json = new JavaScriptSerializer().Serialize(versionInfo);
            File.WriteAllText(Context.FigDirectory + @"\.version", json, Encoding.UTF8);
        }
    }
}
