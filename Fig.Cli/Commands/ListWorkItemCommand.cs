using Fig.Cli.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System.Linq;
using System.Text;

namespace Fig.Cli.Commands
{
    public class ListWorkItemCommand : AzureDevOpsCommand<ListWorkItemOptions>
    {
        private readonly WorkItemTrackingHttpClient client;

        public ListWorkItemCommand(ListWorkItemOptions opts, FigContext context) : base(opts, context)
        {
            client = AzureContext.Connection.GetClient<WorkItemTrackingHttpClient>();
        }

        public override CommandResult Execute()
        {
            var wiql = new StringBuilder("SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = @project");

            if (Options.Mine)
                wiql.Append(" AND [System.AssignedTo] = @Me");
            if (!string.IsNullOrEmpty(Options.State))
                wiql.Append($" AND [System.State] = '{Options.State.Replace("'", "''")}'");
            if (!string.IsNullOrEmpty(Options.Type))
                wiql.Append($" AND [System.WorkItemType] = '{Options.Type.Replace("'", "''")}'");

            wiql.Append(" ORDER BY [System.ChangedDate] DESC");

            var queryResult = client.QueryByWiqlAsync(new Wiql { Query = wiql.ToString() }, Context.Options.ProjectName).Result;
            var ids = queryResult.WorkItems.Select(w => w.Id).Take(Options.Top).ToList();

            if (ids.Count == 0)
                return Ok("Nenhum work item encontrado.");

            var fields = new[] { "System.Id", "System.WorkItemType", "System.State", "System.Title" };
            var items = client.GetWorkItemsAsync(ids, fields: fields).Result;

            foreach (var wi in items)
            {
                string F(string k)
                {
                    return wi.Fields.TryGetValue(k, out var v) ? v?.ToString() : string.Empty;
                }

                WriteLine($"#{wi.Id,-6} [{F("System.WorkItemType"),-20}] {F("System.State"),-14} {F("System.Title")}");
            }

            return Ok("{0} item(ns).", ids.Count);
        }
    }
}
