# Fig.Cli (`fig`)

CLI interno do time pra automatizar o fluxo de desenvolvimento: branches de work item,
pull requests, scripts/migrations de banco e criação de Work Items no Azure DevOps.

Distribuído como executável único (`fig`), multiplataforma (Linux, macOS, Windows).

---

## Instalação

O `fig` é um binário **single-file self-contained** (não precisa de .NET instalado pra rodar).

- **Linux / macOS:** copie o binário publicado para um diretório no `PATH` e dê permissão:
  ```bash
  cp fig /usr/local/bin/fig && chmod +x /usr/local/bin/fig
  fig --help
  ```
- **Windows:** use o instalador MSI (`Fig.Cli.Setup`) ou coloque `fig.exe` num diretório do `PATH`.

Para gerar o binário, veja [Publicar](#publicar).

---

## Configuração

O `fig` lê a configuração de um arquivo **`.fig/.conf`** (JSON) na raiz do repositório. Ele
**contém segredos** (PAT do Azure DevOps, string de conexão com banco **local**) — mantenha-o no `.gitignore`.

Crie/edite com:

```bash
fig init                          # inicializa o .fig/.conf no repositório
fig set-option <chave> <valor>    # define uma opção
```

Principais chaves:

| Chave | Descrição |
| --- | --- |
| `ProjectUrl` | URL da organização Azure DevOps (ex: `https://dev.azure.com/spedy`) |
| `ProjectName` | Nome do projeto (ex: `spedy`) |
| `Pat` | Personal Access Token do Azure DevOps (**segredo**) |
| `RepositoryId` / `RepositoryName` | Repositório alvo |
| `DeveloperId` / `DeveloperName` | Identificação do dev (usada no nome/header de scripts) |
| `DbScriptPath` | Pasta dos scripts SQL de migração |
| `DbServer` / `DbName` / `DbUserName` / `DbPassword` / `DbProvider` | Conexão do banco **local** pro `migratedb` |
| `DbMigrationsTable` | Tabela de controle de migrações |
| `JiraBaseUrl` | URL do Jira Cloud (ex: `https://spedy.atlassian.net`) |
| `JiraEmail` | E-mail da conta Jira do dev |
| `JiraToken` | API token do Jira (**segredo**) — gere em `id.atlassian.com` → Security → API tokens |

---

## Comandos

### Fluxo de trabalho (Git + Azure DevOps)

| Comando | O que faz |
| --- | --- |
| `fig init` | Inicializa o Fig.Cli no repositório |
| `fig start <id>` | Cria a branch da feature a partir de um work item |
| `fig done` | Finaliza o trabalho em progresso, sincroniza e abre o PR |
| `fig sync` | Sincroniza a branch atual com o remoto |
| `fig merge` | Faz merge de outra branch |
| `fig rebase` | Rebase a partir de outra branch |
| `fig release` | Configura a versão de release |
| `fig pr [<título>] [-t <target>] [-d <desc>] [--draft]` | **Cria** o PR via API (headless) da branch atual → target (default: branch padrão do repo); idempotente — retorna o PR ativo se já existir |
| `fig pullr` | Abre a **página** de criação de Pull Request no navegador (branch atual → master) |
| `fig mergepull` | Faz merge das mudanças do PR de destino em outra branch |
| `fig cleardevbranches` | Remove branches de desenvolvimento locais e remotas |
| `fig findinbranches` | Busca um termo em todas as branches locais |

### Banco de dados

| Comando | O que faz |
| --- | --- |
| `fig script` | Cria um novo script de migração (nome + header automáticos) |
| `fig template` | Cria um novo template de script |
| `fig migratedb` | Aplica as migrações pendentes no banco |
| `fig runscripts` | Roda os scripts de uma pasta |

### Work Items (Azure DevOps)

| Comando | O que faz |
| --- | --- |
| `fig pbi <título> [--desc-file <md>] [--ac-file <md>] [--parent <id>]` | Cria um Product Backlog Item (sem `--desc-file` = captura só com título, nasce em New) |
| `fig bug <título> [--desc-file <md>] [--ac-file <md>] [--parent <id>]` | Cria um Bug (sem `--desc-file` = captura só com título, nasce em New) |
| `fig feature <título> --desc-file <md> [--ac-file <md>] [--parent <id>]` | Cria uma Feature |
| `fig task <parent-id> <título> [--desc-file <md>]` | Cria uma Task sob um work item pai |
| `fig edit <id> [--title <t>] [--desc-file <md>] [--ac-file <md>]` | Edita um work item existente; atualização parcial |
| `fig show <id>` | Mostra um work item (título, corpo, critérios) em texto |
| `fig comment <id> "<texto>"` (ou `--file <md>`) | Adiciona um comentário ao work item |
| `fig list [--mine] [--state <s>] [--type <t>] [--top <n>]` | Lista work items (`--mine` = atribuídos a você) |

### Jira (Suporte / Sustentação)

Ponte com o Jira Cloud pra trazer demanda de suporte (ex: `SUS-629`, `SUP-…`) pro ADO. Auth por **API token** do próprio dev (`JiraBaseUrl`/`JiraEmail`/`JiraToken` no `.fig/.conf`); o token age com as permissões do dev e **nunca** é impresso.

| Comando | O que faz |
| --- | --- |
| `fig jira <KEY>` | Lê o issue (título, descrição, tipo, status) em texto |
| `fig jira <KEY> --json` | Igual, em JSON (pra consumo por script/agente) |
| `fig jira <KEY> --comment "<texto>"` | Adiciona um comentário ao issue |
| `fig jira <KEY> --transition "<status>"` | Move o issue para um status pelo nome (ex: `"Em andamento"`) |

Fluxo típico (suporte → dev): `fig jira SUS-629` (lê) → cria o Bug/PBI no ADO a partir do conteúdo → `fig start <id>` → `fig jira SUS-629 --comment "Tratado no ADO #<id>"` `--transition "Em andamento"`.

### Utilitários

| Comando | O que faz |
| --- | --- |
| `fig set-option <chave> <valor>` | Define uma opção no `.fig/.conf` |
| `fig guid` | Gera um GUID |
| `fig --version` | Mostra a versão |
| `fig help <comando>` | Ajuda detalhada de um comando |

---

## Criando Work Items (`pbi` / `bug`)

Os comandos `pbi` e `bug` criam o work item no Azure DevOps a partir de arquivos **Markdown**,
que são convertidos para HTML (via [Markdig](https://github.com/xoofx/markdig)) e gravados nos
campos nativos:

| | Campo "corpo" | Campo "critérios" |
| --- | --- | --- |
| `pbi` | `System.Description` | `Microsoft.VSTS.Common.AcceptanceCriteria` |
| `bug` | `Microsoft.VSTS.TCM.ReproSteps` | `Microsoft.VSTS.Common.AcceptanceCriteria` |

Exemplo:

```bash
fig pbi "Bloquear cancelamento de NFS-e fora do prazo (via API)" \
  --desc-file /tmp/pbi-desc.md \
  --ac-file   /tmp/pbi-ac.md \
  --parent    1234            # Feature pai (opcional)
```

O `fig` imprime o id e o link do item criado.

Para **editar** um item existente (atualização parcial — só os campos passados; detecta PBI vs
Bug automaticamente):

```bash
fig edit 4720 --ac-file /tmp/pbi-ac.md     # atualiza só os critérios de aceitação
fig edit 4720 --title "Novo título"        # atualiza só o título
```

**Dica de formatação:** a conversão usa *hard line breaks* (cada quebra de linha vira `<br>`).
Escreva parágrafos de prosa em **uma linha só**; use linhas separadas só onde a quebra é
desejada (listas, cenários `Dado/Quando/Então`).

Org, projeto e PAT vêm do `.fig/.conf` — o PAT **nunca** é impresso.

---

## Publicar

Gera o executável único self-contained para o RID alvo:

```bash
dotnet publish Fig.Cli/Fig.Cli.csproj -c Release -r <RID> \
  -p:PublishSingleFile=true --self-contained true -o <saída>
```

RIDs por sistema:

| Sistema | RID | Saída |
| --- | --- | --- |
| **Linux** (x64) | `linux-x64` | `fig` |
| **Linux** (ARM) | `linux-arm64` | `fig` |
| **macOS** (Apple Silicon) | `osx-arm64` | `fig` |
| **macOS** (Intel) | `osx-x64` | `fig` |
| **Windows** (x64) | `win-x64` | `fig.exe` |
| **Windows** (ARM) | `win-arm64` | `fig.exe` |

Exemplos:

```bash
# macOS Apple Silicon
dotnet publish Fig.Cli/Fig.Cli.csproj -c Release -r osx-arm64 \
  -p:PublishSingleFile=true --self-contained true -o ./publish/osx-arm64
cp ./publish/osx-arm64/fig /usr/local/bin/fig && chmod +x /usr/local/bin/fig

# Linux
dotnet publish Fig.Cli/Fig.Cli.csproj -c Release -r linux-x64 \
  -p:PublishSingleFile=true --self-contained true -o ./publish/linux-x64

# Windows (PowerShell)
dotnet publish Fig.Cli/Fig.Cli.csproj -c Release -r win-x64 `
  -p:PublishSingleFile=true --self-contained true -o ./publish/win-x64
```

No **Windows**, a distribuição oficial é via instalador MSI em `Fig.Cli.Setup` (WiX) — abra a
solução e gere o pacote, ou rode o `fig.exe` publicado direto.

---

## Desenvolvimento

Requer o **.NET 8 SDK**.

```bash
dotnet build Fig.Cli/Fig.Cli.csproj          # compila
dotnet run --project Fig.Cli -- <comando>    # roda sem publicar
```

Estrutura:

```text
Fig.Cli/
├── Options/        # definição de args por comando ([Verb], [Option], [Value])
├── Commands/       # implementação de cada comando
├── AzureDevOps/    # integração ADO (auth por PAT, clients, helpers)
├── Helpers/        # utilitários gerais (Git, etc.)
├── Program.cs      # entrypoint: parse dos args e dispatch
└── FigContext.cs   # carrega o .fig/.conf
```

**Adicionar um comando novo:** crie `Options/XOptions.cs` (com `[Verb("x")]`) e
`Commands/XCommand.cs` (herdando `Command<XOptions>` ou `AzureDevOpsCommand<XOptions>` se
falar com o ADO), e registre ambos em `Program.cs` (no array `optionTypes` e no `Dispatch`).
