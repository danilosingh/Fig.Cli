using Fig.Cli.Helpers;
using Fig.Cli.Options;
using Newtonsoft.Json.Linq;

namespace Fig.Cli.Commands
{
    public class JiraCommand : Command<JiraOptions>
    {
        public JiraCommand(JiraOptions opts, FigContext context) : base(opts, context)
        {
            EnsureConfiguration();
        }

        public override CommandResult Execute()
        {
            var baseUrl = Context.Options.JiraBaseUrl;
            var email = Context.Options.JiraEmail;
            var token = Context.Options.JiraToken;

            if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                throw new FigException("Jira nao configurado. Defina JiraBaseUrl, JiraEmail e JiraToken no .fig/.conf (use fig set-option).");

            var client = new JiraClient(baseUrl, email, token);
            var issue = client.GetIssue(Options.Key);

            if (Options.Json)
            {
                var outJson = new JObject
                {
                    ["key"] = issue.Key,
                    ["summary"] = issue.Summary,
                    ["description"] = issue.Description,
                    ["type"] = issue.Type,
                    ["status"] = issue.Status,
                    ["url"] = issue.Url
                };

                // "{0}" pra nao tratar as chaves do JSON como format placeholders.
                WriteLine("{0}", outJson.ToString());
            }
            else
            {
                WriteLine("Key:    {0}", issue.Key);
                WriteLine("Tipo:   {0}", issue.Type);
                WriteLine("Status: {0}", issue.Status);
                WriteLine("URL:    {0}", issue.Url);
                WriteLine("Titulo: {0}", issue.Summary);
                WriteBreakLine();
                WriteLine("{0}", string.IsNullOrEmpty(issue.Description) ? "(sem descricao)" : issue.Description);
            }

            if (!string.IsNullOrEmpty(Options.Comment))
            {
                client.AddComment(Options.Key, Options.Comment);
                WriteLine("Comentario adicionado em {0}.", Options.Key);
            }

            if (!string.IsNullOrEmpty(Options.Transition))
            {
                var to = client.Transition(Options.Key, Options.Transition);
                WriteLine("Status de {0} -> {1}.", Options.Key, to);
            }

            // Sem "Completed." no fim: o comando ja imprimiu tudo (e mantem o --json limpo).
            return Ok(null);
        }
    }
}
