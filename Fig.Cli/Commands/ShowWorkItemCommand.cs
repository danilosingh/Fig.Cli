using Fig.Cli.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System.Linq;
using System.Text;

namespace Fig.Cli.Commands
{
    public class ShowWorkItemCommand : AzureDevOpsCommand<ShowWorkItemOptions>
    {
        private readonly WorkItemTrackingHttpClient client;

        public ShowWorkItemCommand(ShowWorkItemOptions opts, FigContext context) : base(opts, context)
        {
            client = AzureContext.Connection.GetClient<WorkItemTrackingHttpClient>();
        }

        public override CommandResult Execute()
        {
            if (Options.Id <= 0)
                throw new FigException("Invalid work item id.");

            var wi = client.GetWorkItemAsync(Options.Id, expand: WorkItemExpand.All).Result;

            string Field(string key)
            {
                return wi.Fields.TryGetValue(key, out var v) ? v?.ToString() : null;
            }

            var type = Field("System.WorkItemType");
            var bodyField = type == "Bug" ? "Microsoft.VSTS.TCM.ReproSteps" : "System.Description";
            var body = WorkItemContent.HtmlToText(Field(bodyField));
            var ac = WorkItemContent.HtmlToText(Field("Microsoft.VSTS.Common.AcceptanceCriteria"));

            var parentId = "-";
            var parent = wi.Relations?.FirstOrDefault(r => r.Rel == "System.LinkTypes.Hierarchy-Reverse");
            if (parent != null)
                parentId = parent.Url.Substring(parent.Url.LastIndexOf('/') + 1);

            var sb = new StringBuilder();
            sb.AppendLine($"# {Field("System.Title")}");
            sb.AppendLine($"Id: {wi.Id} | Tipo: {type} | Estado: {Field("System.State")} | Parent: {parentId}");
            sb.AppendLine();
            sb.AppendLine(type == "Bug" ? "## Repro Steps" : "## Descrição");
            sb.AppendLine(body);

            if (!string.IsNullOrWhiteSpace(ac))
            {
                sb.AppendLine();
                sb.AppendLine("## Critérios de Aceitação");
                sb.AppendLine(ac);
            }

            WriteLine(sb.ToString());
            return Ok(null);
        }
    }
}
