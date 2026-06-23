using Fig.Cli.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System.IO;

namespace Fig.Cli.Commands
{
    public class EditWorkItemCommand : AzureDevOpsCommand<EditWorkItemOptions>
    {
        private readonly WorkItemTrackingHttpClient client;

        public EditWorkItemCommand(EditWorkItemOptions opts, FigContext context) : base(opts, context)
        {
            client = AzureContext.Connection.GetClient<WorkItemTrackingHttpClient>();
        }

        public override CommandResult Execute()
        {
            if (Options.Id <= 0)
                throw new FigException("Invalid work item id.");

            var workItem = client.GetWorkItemAsync(Options.Id).Result;
            var type = workItem.Fields.TryGetValue("System.WorkItemType", out var t) ? t as string : null;

            // Bug usa Repro Steps; o resto (PBI, Feature, etc.) usa Description.
            var bodyField = type == "Bug" ? "Microsoft.VSTS.TCM.ReproSteps" : "System.Description";

            var patch = new JsonPatchDocument();

            if (!string.IsNullOrEmpty(Options.Title))
                patch.Add(WorkItemContent.Field("System.Title", Options.Title));

            if (!string.IsNullOrEmpty(Options.DescFile))
            {
                if (!File.Exists(Options.DescFile))
                    throw new FigException($"Description file not found: {Options.DescFile}");

                patch.Add(WorkItemContent.Field(bodyField, WorkItemContent.ToHtml(Options.DescFile)));
            }

            if (!string.IsNullOrEmpty(Options.AcFile))
            {
                if (!File.Exists(Options.AcFile))
                    throw new FigException($"Acceptance criteria file not found: {Options.AcFile}");

                patch.Add(WorkItemContent.Field("Microsoft.VSTS.Common.AcceptanceCriteria", WorkItemContent.ToHtml(Options.AcFile)));
            }

            if (patch.Count == 0)
                throw new FigException("Nothing to update. Pass --title, --desc-file or --ac-file.");

            var updated = client.UpdateWorkItemAsync(patch, Options.Id).Result;
            var url = $"{Context.Options.ProjectUrl}/_workitems/edit/{updated.Id}";

            return Ok("{0} #{1} atualizado: {2}", type ?? "Work item", updated.Id, url);
        }
    }
}
