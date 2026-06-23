using Fig.Cli.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System.IO;

namespace Fig.Cli.Commands
{
    public class CreateTaskCommand : AzureDevOpsCommand<CreateTaskOptions>
    {
        private readonly WorkItemTrackingHttpClient client;

        public CreateTaskCommand(CreateTaskOptions opts, FigContext context) : base(opts, context)
        {
            client = AzureContext.Connection.GetClient<WorkItemTrackingHttpClient>();
        }

        public override CommandResult Execute()
        {
            if (Options.Parent <= 0)
                throw new FigException("Invalid parent id.");
            if (string.IsNullOrWhiteSpace(Options.Title))
                throw new FigException("Title is required.");

            var patch = new JsonPatchDocument
            {
                WorkItemContent.Field("System.Title", Options.Title)
            };

            if (!string.IsNullOrEmpty(Options.DescFile))
            {
                if (!File.Exists(Options.DescFile))
                    throw new FigException($"Description file not found: {Options.DescFile}");

                patch.Add(WorkItemContent.Field("System.Description", WorkItemContent.ToHtml(Options.DescFile)));
            }

            patch.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/relations/-",
                Value = new
                {
                    rel = "System.LinkTypes.Hierarchy-Reverse",
                    url = $"{Context.Options.ProjectUrl}/_apis/wit/workItems/{Options.Parent}"
                }
            });

            var created = client.CreateWorkItemAsync(patch, Context.Options.ProjectName, "Task").Result;
            var url = $"{Context.Options.ProjectUrl}/_workitems/edit/{created.Id}";

            return Ok("Task #{0} criada sob #{1}: {2}", created.Id, Options.Parent, url);
        }
    }
}
