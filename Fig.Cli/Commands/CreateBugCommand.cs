using Fig.Cli.Options;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;

namespace Fig.Cli.Commands
{
    public class CreateBugCommand : CreateWorkItemCommandBase<CreateBugOptions>
    {
        public CreateBugCommand(CreateBugOptions opts, FigContext context) : base(opts, context)
        {
        }

        protected override string WorkItemType => "Bug";

        // No processo Scrum, o corpo do Bug vai em Repro Steps.
        protected override string BodyFieldRefName => "Microsoft.VSTS.TCM.ReproSteps";

        // Severidade e campo nativo do ADO (Microsoft.VSTS.Common.Severity), nao texto no corpo.
        protected override void AddExtraFields(JsonPatchDocument patch)
        {
            if (string.IsNullOrWhiteSpace(Options.Severity))
                return;

            patch.Add(WorkItemContent.Field("Microsoft.VSTS.Common.Severity", MapSeverity(Options.Severity)));
        }

        private static string MapSeverity(string severity)
        {
            switch (severity.Trim().ToLowerInvariant())
            {
                case "critica":
                case "crítica":
                case "critical":
                    return "1 - Critical";
                case "alta":
                case "high":
                    return "2 - High";
                case "media":
                case "média":
                case "medium":
                    return "3 - Medium";
                case "baixa":
                case "low":
                    return "4 - Low";
                default:
                    throw new FigException($"Severidade invalida: '{severity}'. Use Critica, Alta, Media ou Baixa.");
            }
        }
    }
}
