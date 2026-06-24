using Fig.Cli.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System.IO;
using System.Linq;

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

        // Hook para campos especificos do tipo (ex: Severity do Bug). Padrao: nada.
        protected virtual void AddExtraFields(JsonPatchDocument patch)
        {
        }

        public override CommandResult Execute()
        {
            if (string.IsNullOrWhiteSpace(Options.Title))
                throw new FigException("Title is required.");

            // Idempotencia por referencia externa: se ja existe um work item com a tag
            // <ref>, retorna ele em vez de duplicar (ex: reabrir o mesmo ticket de suporte).
            if (!string.IsNullOrWhiteSpace(Options.ExternalRef))
            {
                var existing = FindByRef(Options.ExternalRef);

                if (existing != null)
                {
                    var existingUrl = $"{Context.Options.ProjectUrl}/_workitems/edit/{existing.Id}";
                    return Ok("Ja existe um work item para a referencia '{0}': #{1} {2}", Options.ExternalRef, existing.Id, existingUrl);
                }
            }

            var patch = new JsonPatchDocument
            {
                WorkItemContent.Field("System.Title", Options.Title)
            };

            if (!string.IsNullOrWhiteSpace(Options.ExternalRef))
                patch.Add(WorkItemContent.Field("System.Tags", Options.ExternalRef));

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

            AddExtraFields(patch);

            var created = client.CreateWorkItemAsync(patch, Context.Options.ProjectName, WorkItemType).Result;
            var url = $"{Context.Options.ProjectUrl}/_workitems/edit/{created.Id}";

            return Ok("{0} #{1} criado: {2}", WorkItemType, created.Id, url);
        }

        private WorkItem FindByRef(string externalRef)
        {
            var safe = externalRef.Replace("'", "''");
            var query = $"SELECT [System.Id] FROM WorkItems WHERE [System.TeamProject] = '{Context.Options.ProjectName}' AND [System.Tags] CONTAINS '{safe}'";
            var result = client.QueryByWiqlAsync(new Wiql { Query = query }).Result;

            if (result.WorkItems == null || !result.WorkItems.Any())
                return null;

            var ids = result.WorkItems.Select(w => w.Id).Take(50).ToList();
            var items = client.GetWorkItemsAsync(ids, new[] { "System.Id", "System.Tags" }).Result;

            // CONTAINS e substring; confirma a tag exata (evita 'SUS-629' casar com 'SUS-6290').
            return items.FirstOrDefault(i => HasExactTag(i, externalRef));
        }

        private static bool HasExactTag(WorkItem workItem, string tag)
        {
            if (workItem.Fields == null || !workItem.Fields.TryGetValue("System.Tags", out var value) || value == null)
                return false;

            return value.ToString()
                .Split(';')
                .Select(t => t.Trim())
                .Any(t => string.Equals(t, tag, System.StringComparison.OrdinalIgnoreCase));
        }
    }
}
