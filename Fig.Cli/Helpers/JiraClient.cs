using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Fig.Cli.Helpers
{
    // Cliente minimo da REST API do Jira Cloud (v2). Auth Basic = email:apiToken.
    // A v2 devolve a description como string (wiki markup) — mais simples de extrair
    // que a v3 (ADF/JSON). O token e lido do .fig/.conf e nunca e impresso.
    public class JiraClient
    {
        private readonly HttpClient http;
        private readonly string baseUrl;

        public JiraClient(string baseUrl, string email, string token)
        {
            this.baseUrl = baseUrl.TrimEnd('/');
            http = new HttpClient();
            var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{email}:{token}"));
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basic);
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public JiraIssue GetIssue(string key, IList<string> extraFieldIds = null)
        {
            var ids = (extraFieldIds ?? new List<string>()).Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList();
            var fieldList = "summary,description,issuetype,status" + (ids.Any() ? "," + string.Join(",", ids) : "");
            var url = $"{baseUrl}/rest/api/2/issue/{key}?fields={fieldList}" + (ids.Any() ? "&expand=names" : "");

            var resp = http.GetAsync(url).Result;
            EnsureSuccess(resp, $"ler o issue {key}");

            var root = JObject.Parse(resp.Content.ReadAsStringAsync().Result);
            var fields = root["fields"];
            var names = root["names"] as JObject;
            var descToken = fields?["description"];

            var issue = new JiraIssue
            {
                Key = key,
                Summary = (string)fields?["summary"],
                Description = descToken == null || descToken.Type == JTokenType.Null
                    ? null
                    : (descToken.Type == JTokenType.String ? (string)descToken : descToken.ToString()),
                Type = (string)fields?["issuetype"]?["name"],
                Status = (string)fields?["status"]?["name"],
                Url = $"{baseUrl}/browse/{key}",
                ExtraFields = new List<JiraField>()
            };

            foreach (var id in ids)
            {
                var value = FlattenValue(fields?[id]);

                if (string.IsNullOrWhiteSpace(value))
                    continue;

                issue.ExtraFields.Add(new JiraField
                {
                    Id = id,
                    Name = (string)names?[id] ?? id,
                    Value = value.Trim()
                });
            }

            return issue;
        }

        // Achata um valor de campo do Jira (que pode ser string, objeto {value/name/...},
        // array, etc) num texto legivel. Generico — nao assume tipo de custom field.
        private static string FlattenValue(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return null;

            switch (token.Type)
            {
                case JTokenType.String:
                    return (string)token;
                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.Boolean:
                case JTokenType.Date:
                    return token.ToString();
                case JTokenType.Array:
                    return string.Join(", ", ((JArray)token).Select(FlattenValue).Where(s => !string.IsNullOrWhiteSpace(s)));
                case JTokenType.Object:
                    var o = (JObject)token;
                    foreach (var prop in new[] { "value", "name", "displayName", "emailAddress" })
                        if (o[prop] != null && o[prop].Type == JTokenType.String)
                            return (string)o[prop];
                    return o.ToString(Formatting.None);
                default:
                    return token.ToString();
            }
        }

        public void AddComment(string key, string body)
        {
            var payload = new JObject { ["body"] = body };
            var content = new StringContent(payload.ToString(), Encoding.UTF8, "application/json");
            var resp = http.PostAsync($"{baseUrl}/rest/api/2/issue/{key}/comment", content).Result;
            EnsureSuccess(resp, $"comentar no issue {key}");
        }

        // Transiciona o issue para um status pelo NOME (match no destino da transicao,
        // case-insensitive, exato ou contido). Retorna o nome do status destino.
        public string Transition(string key, string statusName)
        {
            var resp = http.GetAsync($"{baseUrl}/rest/api/2/issue/{key}/transitions").Result;
            EnsureSuccess(resp, $"listar transicoes de {key}");

            var transitions = JObject.Parse(resp.Content.ReadAsStringAsync().Result)["transitions"] as JArray ?? new JArray();

            var match = transitions.FirstOrDefault(t => NameMatches((string)t["to"]?["name"], statusName) || NameMatches((string)t["name"], statusName));

            if (match == null)
            {
                var available = string.Join(", ", transitions.Select(t => $"\"{(string)t["to"]?["name"]}\""));
                throw new FigException($"Transicao para '{statusName}' nao disponivel em {key}. Disponiveis: {available}");
            }

            var payload = new JObject { ["transition"] = new JObject { ["id"] = (string)match["id"] } };
            var content = new StringContent(payload.ToString(), Encoding.UTF8, "application/json");
            var result = http.PostAsync($"{baseUrl}/rest/api/2/issue/{key}/transitions", content).Result;
            EnsureSuccess(result, $"transicionar {key}");

            return (string)match["to"]?["name"];
        }

        private static bool NameMatches(string actual, string wanted)
        {
            if (string.IsNullOrEmpty(actual) || string.IsNullOrEmpty(wanted))
                return false;

            return string.Equals(actual, wanted, StringComparison.OrdinalIgnoreCase)
                || actual.IndexOf(wanted, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void EnsureSuccess(HttpResponseMessage resp, string action)
        {
            if (resp.IsSuccessStatusCode)
                return;

            var body = resp.Content.ReadAsStringAsync().Result;

            if (body != null && body.Length > 300)
                body = body.Substring(0, 300) + "...";

            throw new FigException($"Falha ao {action}: HTTP {(int)resp.StatusCode}. {body}");
        }
    }

    public class JiraIssue
    {
        public string Key { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public string Url { get; set; }
        public List<JiraField> ExtraFields { get; set; }
    }

    public class JiraField
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
