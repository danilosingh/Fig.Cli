using Fig.Cli.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System.IO;

namespace Fig.Cli.Commands
{
    public class CommentWorkItemCommand : AzureDevOpsCommand<CommentWorkItemOptions>
    {
        private readonly WorkItemTrackingHttpClient client;

        public CommentWorkItemCommand(CommentWorkItemOptions opts, FigContext context) : base(opts, context)
        {
            client = AzureContext.Connection.GetClient<WorkItemTrackingHttpClient>();
        }

        public override CommandResult Execute()
        {
            if (Options.Id <= 0)
                throw new FigException("Invalid work item id.");

            var markdown = !string.IsNullOrEmpty(Options.File)
                ? File.ReadAllText(Options.File)
                : Options.Text;

            if (string.IsNullOrWhiteSpace(markdown))
                throw new FigException("Comment text is required (inline or --file).");

            var request = new CommentCreate { Text = WorkItemContent.ToHtmlString(markdown) };
            client.AddCommentAsync(request, Context.Options.ProjectName, Options.Id).Wait();

            return Ok("Comentário adicionado ao #{0}.", Options.Id);
        }
    }
}
