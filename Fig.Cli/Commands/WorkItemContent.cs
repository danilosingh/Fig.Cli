using Markdig;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Fig.Cli.Commands
{
    // Helpers compartilhados entre os comandos de work item.
    internal static class WorkItemContent
    {
        private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseSoftlineBreakAsHardlineBreak()
            .Build();

        public static string ToHtml(string markdownFile)
        {
            return ToHtmlString(File.ReadAllText(markdownFile));
        }

        public static string ToHtmlString(string markdown)
        {
            return Markdown.ToHtml(markdown ?? string.Empty, Pipeline);
        }

        public static JsonPatchOperation Field(string refName, object value)
        {
            return new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/" + refName,
                Value = value
            };
        }

        // Conversão simples de HTML (como vem do ADO) para texto legível, para o `show`.
        public static string HtmlToText(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            var s = Regex.Replace(html, @"(?i)<br\s*/?>", "\n");
            s = Regex.Replace(s, @"(?i)<li[^>]*>", "- ");
            s = Regex.Replace(s, @"(?i)</(p|li|h[1-6]|div|tr|ul|ol)>", "\n");
            s = Regex.Replace(s, "<[^>]+>", string.Empty);
            s = WebUtility.HtmlDecode(s);
            s = Regex.Replace(s, @"[ \t]+\n", "\n");
            s = Regex.Replace(s, @"\n{3,}", "\n\n");
            return s.Trim();
        }
    }
}
