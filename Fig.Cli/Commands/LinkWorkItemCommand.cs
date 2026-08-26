using Fig.Cli.Helpers;
using Fig.Cli.Options;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System;
using System.IO;
using System.Linq;

namespace Fig.Cli.Commands
{
    public class LinkWorkItemCommand : AzureDevOpsCommand<LinkWorkItemOptions>
    {
        private const string HyperlinkRel = "Hyperlink";

        private readonly WorkItemTrackingHttpClient client;

        public LinkWorkItemCommand(LinkWorkItemOptions opts, FigContext context) : base(opts, context)
        {
            client = AzureContext.Connection.GetClient<WorkItemTrackingHttpClient>();
        }

        public override CommandResult Execute()
        {
            if (Options.Id <= 0)
            {
                throw new FigException("Invalid work item id.");
            }

            if (string.IsNullOrWhiteSpace(Options.Target))
            {
                throw new FigException("Informe o caminho no repositório ou a URL do link.");
            }

            var isAbsoluteUrl = IsAbsoluteUrl(Options.Target);
            var url = isAbsoluteUrl ? Options.Target.Trim() : BuildRepositoryUrl(Options.Target);
            var title = ResolveTitle(isAbsoluteUrl);

            var workItem = client.GetWorkItemAsync(Options.Id, expand: WorkItemExpand.Relations).Result;
            var existing = workItem.Relations?.FirstOrDefault(
                c => c.Rel == HyperlinkRel && string.Equals(c.Url, url, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                return Ok($"O work item #{Options.Id} já tem esse link: {url}");
            }

            var patch = new JsonPatchDocument
            {
                new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/relations/-",
                    Value = new
                    {
                        rel = HyperlinkRel,
                        url,
                        attributes = string.IsNullOrWhiteSpace(title) ? null : new { comment = title }
                    }
                }
            };

            client.UpdateWorkItemAsync(patch, Options.Id).Wait();

            var label = string.IsNullOrWhiteSpace(title) ? url : $"{title} → {url}";

            return Ok($"Link adicionado ao work item #{Options.Id}: {label}");
        }

        private static bool IsAbsoluteUrl(string target)
        {
            return Uri.TryCreate(target.Trim(), UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        private string BuildRepositoryUrl(string target)
        {
            var path = NormalizePath(target);

            var fullPath = Path.Combine(Context.RootDirectory, path);

            if (!File.Exists(fullPath) && !Directory.Exists(fullPath))
            {
                throw new FigException(
                    $"Caminho não encontrado: {path}. Confira o caminho, ou passe uma URL absoluta.");
            }

            if (!GitHelper.IsTracked(path, Context.RootDirectory))
            {
                WriteLine($"Atenção: {path} ainda não está versionado. " +
                    "O link só vai abrir depois que o arquivo for commitado e chegar na branch default.");
            }

            var projectUrl = Context.Options.ProjectUrl?.TrimEnd('/');
            var projectName = Context.Options.ProjectName;
            var repositoryName = Context.Options.RepositoryName;

            if (string.IsNullOrEmpty(projectUrl) || string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(repositoryName))
            {
                throw new FigException(
                    "ProjectUrl, ProjectName e RepositoryName precisam estar no .fig/.conf para montar o link do repositório.");
            }

            var url = $"{projectUrl}/{projectName}/_git/{repositoryName}?path={Uri.EscapeDataString("/" + path)}";

            if (!string.IsNullOrWhiteSpace(Options.Branch))
            {
                url += $"&version=GB{Uri.EscapeDataString(Options.Branch.Trim())}";
            }

            return url;
        }

        private static string NormalizePath(string target)
        {
            var path = target.Trim().Replace('\\', '/');

            while (path.StartsWith("./"))
            {
                path = path.Substring(2);
            }

            return path.TrimStart('/');
        }

        private string ResolveTitle(bool isAbsoluteUrl)
        {
            if (!string.IsNullOrWhiteSpace(Options.Title))
            {
                return Options.Title.Trim();
            }

            return isAbsoluteUrl ? null : Path.GetFileName(NormalizePath(Options.Target));
        }
    }
}
