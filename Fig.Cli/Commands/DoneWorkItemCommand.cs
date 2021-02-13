using Fig.Cli.Options;
using Fig.Cli.TeamFoundation.Helpers;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fig.Cli.Commands
{
    public class DoneWorkItemCommand : AzureDevOpsCommand<DoneWorkItemOptions>
    {
        private readonly WorkItemTrackingHttpClient workItemTrackingClient;

        public DoneWorkItemCommand(DoneWorkItemOptions opts, FigContext context) : base(opts, context)
        {
            workItemTrackingClient = AzureContext.Connection.GetClient<WorkItemTrackingHttpClient>();
        }

        public override CommandResult Execute()
        {
            var workItems = GetWorkItemsInProgress();

            if (!Confirm(workItems))
                return Canceled();

            AddStep(Options.Commit, () => Commit());
            AddStep(() => ChangeWorkItemsState(workItems));
            AddStep(!Options.NoSync, () => Sync());
            AddStep(!Options.NoPullRequest, () => OpenPullRequest(workItems));

            return ExecuteSteps();
        }

        private bool Confirm(IList<WorkItem> workItems)
        {
            ListWorkItems(workItems);
            return Confirm("Confirm done task(s)?");
        }

        private CommandResult Commit()
        {
            return CommandFactory.Execute<CommitCommand>(new CommitOptions(Options.CommitMessage));
        }

        private CommandResult OpenPullRequest(IList<WorkItem> workItems)
        {
            var parentWorkItens = workItems.Select(c => workItemTrackingClient.GetWorkItemAsync(c.GetParentId(), expand: WorkItemExpand.All).Result).ToList();
            var release = parentWorkItens.Select(c => c.GetField<string>("Unicus.Release")).FirstOrDefault();

            if (!string.IsNullOrEmpty(release) && HasReleaseBranch(release))
                release = "release/" + release;
            else
                release = null;

            return CommandFactory.Execute<PullRequestCommand>(new PullRequestOptions(release));
        }

        private bool HasReleaseBranch(string release)
        {
            var project = AzureProjectHelper.FindDefaultProject(AzureContext);
            var repo = AzureGitHelper.FindRepository(AzureContext, project.Id, Context.Options.RepositoryName);
            return AzureContext.Connection.GetClient<GitHttpClient>().GetRefsAsync(repo.Id, filter: "heads/release/" + release).Result.FirstOrDefault() != null;
        }

        private CommandResult Sync()
        {
            return CommandFactory.Execute<SyncCommand>(new SyncOptions());
        }

        private CommandResult ChangeWorkItemsState(IList<WorkItem> workItems)
        {
            foreach (var item in workItems)
            {
                if (!item.Fields.ContainsKey("Microsoft.VSTS.Common.Activity"))
                    TfsWorkItemHelpers.ChangeField(workItemTrackingClient, (int)item.Id, "Microsoft.VSTS.Common.Activity", "Development");

                TfsWorkItemHelpers.ChangeState(workItemTrackingClient, (int)item.Id, "Done");
            }

            return Ok();
        }

        private void ListWorkItems(IList<WorkItem> workItems)
        {
            Console.WriteLine(string.Join(Environment.NewLine, workItems.Select(c => c.GetField<string>("System.Id") + "-" + c.GetField<string>("System.Title"))));
        }

        private bool WorkItemsFromDifferentParents(List<WorkItem> workItems)
        {
            return workItems.Select(c => c.GetParentId()).Distinct().Count() > 1;
        }

        private IList<WorkItem> GetWorkItemsInProgress()
        {
            var project = AzureProjectHelper.FindDefaultProject(AzureContext);

            var query = $"SELECT [System.Id], [System.Title] FROM WorkItems " +
                $"WHERE [System.TeamProject] = '{project.Name}' " +
                $"AND [System.AssignedTo] = '{ConvertUserName()}' " +
                $"AND [System.State] = 'In Progress' ";

            if (Options.WorkItemId > 0)
                query += "AND [System.Id] = " + Options.WorkItemId.ToString();

            var queryResult = workItemTrackingClient.QueryByWiqlAsync(new Wiql() { Query = query }).Result;

            if (!queryResult.WorkItems.Any())
                throw new ArgumentException("No items in progress");

            var workItems = queryResult.WorkItems.Select(c => workItemTrackingClient.GetWorkItemAsync(c.Id, expand: WorkItemExpand.All).Result).ToList();

            if (WorkItemsFromDifferentParents(workItems))
                throw new ArgumentException("The workitems in progress are of different parents");

            return workItems;
        }
    }
}
