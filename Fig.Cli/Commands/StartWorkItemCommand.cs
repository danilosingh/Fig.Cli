using Fig.Cli.Helpers;
using Fig.Cli.Options;
using Fig.Cli.Versioning;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fig.Cli.Commands
{
    public class StartWorkItemCommand : AzureDevOpsCommand<StartWorkItemOptions>
    {
        private readonly WorkItemTrackingHttpClient workItemTrackingClient;
        private readonly GitHttpClient gitClient;

        public StartWorkItemCommand(StartWorkItemOptions opts, FigContext context) : base(opts, context)
        {
            workItemTrackingClient = AzureContext.Connection.GetClient<WorkItemTrackingHttpClient>();
            gitClient = AzureContext.Connection.GetClient<GitHttpClient>();
        }

        public override CommandResult Execute()
        {
            if (Options.WorkItemId <= 0)
                throw new ArgumentException("Invalid workitem ID.");

            var project = AzureProjectHelper.FindDefaultProject(AzureContext);
            var repo = AzureGitHelper.FindRepository(AzureContext, project.Id, Context.Options.RepositoryName);
            var defaultBranchName = AzureGitHelper.WithoutRefsPrefix(repo.DefaultBranch);
            var defaultBranch = gitClient.GetRefsAsync(repo.Id, filter: defaultBranchName).Result.First();
            var workitem = GetWorkItem(Options.WorkItemId);
            var relatedWorkitems = GetAllRelatedWorkdItems(workitem);
            var workItemsReleaseVersion = GetReleaseVersionFromWorkItems(relatedWorkitems);

            if (!string.IsNullOrEmpty(Options.Release) &&
                !string.IsNullOrEmpty(workItemsReleaseVersion) &&
                workItemsReleaseVersion != Options.Release)
                throw new ArgumentException("The reported release is different from the linked release in the workitems");

            var releaseBranchName = workItemsReleaseVersion ?? Options.Release;
            var parentWorkItem = relatedWorkitems.FirstOrDefault();
            var releaseBranch = GetReleaseBranch(repo.Id, releaseBranchName);
            var branchName = GetBranchName(parentWorkItem, releaseBranch);
            var branchPatch = "heads/dev/" + branchName;
            var fullBranchPatch = "refs/" + branchPatch;
            var newBranch = gitClient.GetRefsAsync(repo.Id, filter: branchPatch).Result.FirstOrDefault();

            if (newBranch == null || !newBranch.Name.EndsWith(branchPatch))
            {
                if (!Confirm("Confirm branch creation: {0} [Enter]", branchName))
                    return Canceled();

                newBranch = CreateBranch(repo, releaseBranch ?? defaultBranch, fullBranchPatch, branchName);
            }

            LinkWorkItems(project, repo, newBranch, branchName, relatedWorkitems);
            StartFirstTask(workitem, relatedWorkitems);
            ConfigureLocalGit(branchName);

            if (!Options.NoPrune)
                ClearLocalBranches();

            var result = Ok();

            if (Options.RunDbScripts)
            {
                result = RunDataBaseScripts();
            }

            return result;
        }

        private CommandResult RunDataBaseScripts()
        {
            var version = VersionInfo.LoadVersion();

            if (version == null)
            {
                return CommandResult.Ok();
            }

            var scriptsOpts = new RunScriptsOptions()
            {
                GreaterThan = version.LastScript,
                Server = Context.Options.DbServer,
                UserName = Context.Options.DbUserName,
                Password = Context.Options.DbPassword,
                Database = Context.Options.DbName,
                ScriptsDirectory = Path.Combine(Context.RootDirectory, Context.Options.DbScriptPath),
                SupressConfirmation = true,
                Provider = Context.Options.DbProvider,
            };

            return CommandFactory.Execute<RunScriptsCommand>(scriptsOpts);
        }

        private GitRef GetReleaseBranch(Guid repoId, string releaseBranchName)
        {
            return !string.IsNullOrEmpty(releaseBranchName) ? gitClient.GetRefsAsync(repoId, filter: "heads/release/" + releaseBranchName).Result.FirstOrDefault() : null;
        }

        private void ClearLocalBranches()
        {
            GitHelper.Prune();
        }

        private string GetReleaseVersionFromWorkItems(List<WorkItem> relatedWorkitems)
        {
            var versions = relatedWorkitems.Select(c => c.GetField<string>("Unicus.Release")).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();

            if (versions.Count > 1)
                throw new ArgumentException("Related workd items with different releases");

            return versions.FirstOrDefault();
        }

        private object GetFirstReleaseBranchByCommit(Guid repoId, GitCommitRef lastCommit, List<GitRef> releaseBranches)
        {
            foreach (var releaseBranch in releaseBranches)
            {
                var newBranchVersionDescriptor = new GitVersionDescriptor() { VersionType = GitVersionType.Branch, Version = AzureGitHelper.WithoutRefsAndHeadsPrefix(releaseBranch.Name) };
                var criteria = new GitQueryCommitsCriteria() { Ids = new List<string>() { lastCommit.CommitId }, Top = 1, ItemVersion = newBranchVersionDescriptor };

                if (gitClient.GetCommitsAsync(repoId, criteria, top: 1).Result.Any())
                    return releaseBranch;
            }

            return null;
        }

        private GitRef CreateBranch(GitRepository repo, GitRef sourceRef, string fullBranchName, string branchName)
        {
            GitRefUpdateResult refCreateResult = gitClient.UpdateRefsAsync(
                   new GitRefUpdate[] { new GitRefUpdate() {
                    OldObjectId = new string('0', 40),
                    NewObjectId = sourceRef.ObjectId,
                    Name = fullBranchName,
                } },
                   repositoryId: repo.Id).Result.First();

            return gitClient.GetRefsAsync(repo.Id, filter: "heads/dev/" + branchName).Result.First();
        }

        private void StartFirstTask(WorkItem workItem, List<WorkItem> relatedWorkitems)
        {
            WorkItem task = null;

            if (workItem.GetField<string>("System.WorkItemType") == "Task")
            {
                if (workItem.GetField<string>("System.State") == "To Do")
                    task = workItem;
            }
            else
            {
                var tasks = relatedWorkitems.Where(c => c.GetField<string>("System.WorkItemType") == "Task").ToList();

                if (tasks.Any(c => c.GetField<string>("System.State") == "In Progress"))
                    return;

                task = tasks.FirstOrDefault(c => c.GetField<string>("System.State") == "To Do");
            }

            if (task == null)
                return;

            var patchDocument = new JsonPatchDocument
            {
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.State",
                    Value = "In Progress"
                }
            };

            workItemTrackingClient.UpdateWorkItemAsync(patchDocument, (int)task.Id).Wait();
        }

        private List<WorkItem> GetAllRelatedWorkdItems(WorkItem workitem)
        {
            var relatedWorkitems = new List<WorkItem> { workitem };
            var parentFound = false;

            while (!parentFound)
            {
                var parentRelation = workitem.Relations.FirstOrDefault(c => c.Rel == "System.LinkTypes.Hierarchy-Reverse");

                if (workitem.GetField<string>("System.WorkItemType") == "Feature" || parentRelation == null)
                {
                    parentFound = true;
                    continue;
                }

                var parentId = Convert.ToInt32(parentRelation.Url.Substring(parentRelation.Url.LastIndexOf('/') + 1));
                workitem = GetWorkItem(parentId);
                relatedWorkitems.Add(workitem);
            }

            var childs = GetChildsWorkItems(workitem);
            relatedWorkitems.AddRange(childs.Where(c => !relatedWorkitems.Any(d => Convert.ToInt32(d.Fields["System.Id"]) == Convert.ToInt32(c.Fields["System.Id"]))));
            return OrderWorkItems(relatedWorkitems);
        }

        private void ConfigureLocalGit(string branchName)
        {
            GitHelper.ExecuteCommads(
                "git fetch -p",
                "git checkout dev/" + branchName,
                "git pull");
        }

        private void LinkWorkItems(TeamProjectReference project, GitRepository repo, GitRef newBranch, string branchName, List<WorkItem> relatedWorkitems)
        {
            var patchDocument = new JsonPatchDocument();
            var gitUri = $"vstfs:///Git/Ref/{project.Id}%2F{repo.Id}%2FGBdev%2F{branchName}";
            patchDocument.Add(
               new JsonPatchOperation()
               {
                   Operation = Operation.Add,
                   Path = "/relations/-",
                   Value = new
                   {
                       rel = "ArtifactLink",
                       url = gitUri,
                       attributes = new { name = "Branch" }
                   }
               }
            );

            foreach (var workItem in relatedWorkitems)
            {
                if (!workItem.Relations.Any(c => c.Rel == "ArtifactLink" && c.Url == gitUri))
                    workItemTrackingClient.UpdateWorkItemAsync(patchDocument, (int)workItem.Id).Wait();
            }
        }

        private string GetBranchName(WorkItem workItem, GitRef releaseBranch)
        {
            var branchName = string.Empty;
            switch (workItem.Fields["System.WorkItemType"])
            {
                case "Feature": branchName = "feature-"; break;
                case "Product Backlog Item": branchName = "pbi-"; break;
                case "Bug": branchName = "bug-"; break;
                default: branchName = "branch-"; break;
            }

            branchName += workItem.Id.ToString();

            if (releaseBranch != null)
            {
                var releaseBranchName = releaseBranch.Name.Remove(0, releaseBranch.Name.IndexOf("release/")).Replace("/", "-");
                branchName += "-on-" + releaseBranchName;
            }

            return branchName;
        }

        private List<WorkItem> OrderWorkItems(List<WorkItem> relatedWorkitems)
        {
            return relatedWorkitems.OrderBy(c =>
                {
                    switch (c.Fields["System.WorkItemType"])
                    {
                        case "Feature": return 0;
                        case "Product Backlog Item": return 1;
                        case "Bug": return 2;
                        case "Task": return 3;
                        default:
                            return 3;
                    }
                })
                .ToList();
        }

        private IList<WorkItem> GetChildsWorkItems(WorkItem workItem)
        {
            var items = new List<WorkItem>();
            var ids = GetRelationIds(workItem, "System.LinkTypes.Hierarchy-Forward");

            if (ids.Count > 0)
            {
                foreach (var workItemChild in GetWorkItems(ids))
                {
                    items.Add(workItemChild);

                    if ((string)workItemChild.Fields["System.WorkItemType"] == "Task")
                        continue;

                    var grandChilds = GetChildsWorkItems(workItemChild);

                    foreach (var grandChild in grandChilds)
                    {
                        if (!items.Any(c => Convert.ToInt32(c.Fields["System.Id"]) == Convert.ToInt32(grandChild.Fields["System.Id"])))
                        {
                            items.Add(grandChild);
                        }
                    }
                }
            }

            return items;
        }

        private WorkItem GetWorkItem(int id)
        {
            return PrepareWorkItem(workItemTrackingClient.GetWorkItemAsync(id, expand: WorkItemExpand.All).Result);
        }

        private IList<WorkItem> GetWorkItems(IEnumerable<int> ids)
        {
            var workItems = workItemTrackingClient.GetWorkItemsAsync(ids, expand: WorkItemExpand.All).Result;
            workItems.ForEach(c => PrepareWorkItem(c));
            return workItems;
        }

        private WorkItem PrepareWorkItem(WorkItem workItem)
        {
            if (workItem.Relations == null)
            {
                workItem.Relations = new List<WorkItemRelation>();
            }

            return workItem;
        }

        private IList<int> GetRelationIds(WorkItem workItem, string relationName)
        {
            return workItem.Relations.Where(c => c.Rel == relationName)
                .Select(c => Convert.ToInt32(c.Url.Substring(c.Url.LastIndexOf('/') + 1)))
                .ToList();
        }
    }
}
