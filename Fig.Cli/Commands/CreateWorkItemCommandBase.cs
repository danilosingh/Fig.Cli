using Fig.Cli.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System.IO;

namespace Fig.Cli.Commands
{
    public abstract class CreateWorkItemCommandBase<T> : AzureDevOpsCommand<T>
        where T : ICreateWorkItemOptions
    {
        private readonly WorkItemTrackingHttpClient client;

        protected CreateWorkItemCommandBase(T opts, FigContext context) : base(opts, context)
        {
            client = AzureContext.Connection.GetClient<WorkItemTrackingHttpClient>();
        }

        protected abstract string WorkItemType { get; }

        // Campo que recebe o corpo principal (Description para PBI, ReproSteps para Bug).
        protected abstract string BodyFieldRefName { get; }

        public override CommandResult Execute()
        {
            if (string.IsNullOrWhiteSpace(Options.Title))
                throw new FigException("Title is required.");

            var patch = new JsonPatchDocument
            {
                WorkItemContent.Field("System.Title", Options.Title)
            };

            // Corpo e opcional: sem --desc-file e uma captura so-titulo (item nasce
            // em New pra triagem depois). Se um caminho foi informado, ele tem que existir.
            if (!string.IsNullOrEmpty(Options.DescFile))
            {
                if (!File.Exists(Options.DescFile))
                    throw new FigException($"Description file not found: {Options.DescFile}");

                patch.Add(WorkItemContent.Field(BodyFieldRefName, WorkItemContent.ToHtml(Options.DescFile)));
            }

            if (!string.IsNullOrEmpty(Options.AcFile))
            {
                if (!File.Exists(Options.AcFile))
                    throw new FigException($"Acceptance criteria file not found: {Options.AcFile}");

                patch.Add(WorkItemContent.Field("Microsoft.VSTS.Common.AcceptanceCriteria", WorkItemContent.ToHtml(Options.AcFile)));
            }

            if (Options.Parent.HasValue)
            {
                patch.Add(new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/relations/-",
                    Value = new
                    {
                        rel = "System.LinkTypes.Hierarchy-Reverse",
                        url = $"{Context.Options.ProjectUrl}/_apis/wit/workItems/{Options.Parent.Value}"
                    }
                });
            }

            var created = client.CreateWorkItemAsync(patch, Context.Options.ProjectName, WorkItemType).Result;
            var url = $"{Context.Options.ProjectUrl}/_workitems/edit/{created.Id}";

            return Ok("{0} #{1} criado: {2}", WorkItemType, created.Id, url);
        }
    }
}
