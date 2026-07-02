using CommandLine;
using Fig.Cli.Commands;
using Fig.Cli.Extensions;
using Fig.Cli.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fig.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            var exitCode = 0;

            try
            {
                var optionTypes = new[]
                {
                    typeof(InitOptions), typeof(StartWorkItemOptions), typeof(DoneWorkItemOptions),
                    typeof(SyncOptions), typeof(MergeOptions), typeof(RebaseOptions),
                    typeof(ReleaseOptions), typeof(PullRequestOptions), typeof(MergePullRequestOptions),
                    typeof(ScriptOptions), typeof(ScriptTemplateOptions), typeof(MigrateDbOptions),
                    typeof(RunScriptsOptions), typeof(ClearBranchesOptions), typeof(FindInBranchesOptions),
                    typeof(SetOptions), typeof(GuidOptions), typeof(CreatePbiOptions),
                    typeof(CreateBugOptions), typeof(EditWorkItemOptions), typeof(ShowWorkItemOptions),
                    typeof(CommentWorkItemOptions), typeof(ListWorkItemOptions), typeof(CreateFeatureOptions),
                    typeof(CreateTaskOptions), typeof(CreatePullRequestOptions), typeof(JiraOptions)
                };

                CommandResult result = null;

                Parser.Default.ParseArguments(args, optionTypes)
                    .WithParsed(opts => result = Dispatch((BaseOptions)opts))
                    .WithNotParsed(_ => result = new CommandResult(false));

                result ??= new CommandResult(false);

                if (!result.IsValid)
                {
                    exitCode = 1;
                }

                if (!string.IsNullOrEmpty(result.Message))
                {
                    ConsoleWrite(result.Message);
                }
            }
            catch (FigException ex)
            {
                ConsoleWrite(ex.Message);
                exitCode = 1;
            }
            catch (Exception ex)
            {
                ConsoleWrite(GetFormattedException(ex));
                exitCode = 1;
            }

            return exitCode;
        }

        private static CommandResult Dispatch(BaseOptions opts)
        {
            switch (opts)
            {
                case InitOptions o: return CommandFactory.Execute<InitCommand>(o);
                case StartWorkItemOptions o: return CommandFactory.Execute<StartWorkItemCommand>(o);
                case DoneWorkItemOptions o: return CommandFactory.Execute<DoneWorkItemCommand>(o);
                case SyncOptions o: return CommandFactory.Execute<SyncCommand>(o);
                case MergeOptions o: return CommandFactory.Execute<MergeCommand>(o);
                case RebaseOptions o: return CommandFactory.Execute<RebaseCommand>(o);
                case ReleaseOptions o: return CommandFactory.Execute<ReleaseCommand>(o);
                case PullRequestOptions o: return CommandFactory.Execute<PullRequestCommand>(o);
                case MergePullRequestOptions o: return CommandFactory.Execute<MergePullRequestCommand>(o);
                case ScriptOptions o: return CommandFactory.Execute<ScriptCommand>(o);
                case ScriptTemplateOptions o: return CommandFactory.Execute<ScriptTemplateCommand>(o);
                case MigrateDbOptions o: return CommandFactory.Execute<MigrateDbCommand>(o);
                case RunScriptsOptions o: return CommandFactory.Execute<RunScriptsCommand>(o);
                case ClearBranchesOptions o: return CommandFactory.Execute<ClearBranchesCommand>(o);
                case FindInBranchesOptions o: return CommandFactory.Execute<FindInBranchesCommand>(o);
                case SetOptions o: return CommandFactory.Execute<SetOptionCommand>(o);
                case GuidOptions o: return CommandFactory.Execute<GuidCommand>(o);
                case CreatePbiOptions o: return CommandFactory.Execute<CreatePbiCommand>(o);
                case CreateBugOptions o: return CommandFactory.Execute<CreateBugCommand>(o);
                case EditWorkItemOptions o: return CommandFactory.Execute<EditWorkItemCommand>(o);
                case ShowWorkItemOptions o: return CommandFactory.Execute<ShowWorkItemCommand>(o);
                case CommentWorkItemOptions o: return CommandFactory.Execute<CommentWorkItemCommand>(o);
                case ListWorkItemOptions o: return CommandFactory.Execute<ListWorkItemCommand>(o);
                case CreateFeatureOptions o: return CommandFactory.Execute<CreateFeatureCommand>(o);
                case CreateTaskOptions o: return CommandFactory.Execute<CreateTaskCommand>(o);
                case CreatePullRequestOptions o: return CommandFactory.Execute<CreatePullRequestCommand>(o);
                case JiraOptions o: return CommandFactory.Execute<JiraCommand>(o);
                default: return new CommandResult(false);
            }
        }

        private static string GetFormattedException(Exception ex)
        {
            var exceptions = new List<Exception>() { ex };

            while (ex.InnerException != null)
            {
                exceptions.Add(ex.InnerException);
                ex = ex.InnerException;
            }

            var figException = exceptions.FirstOrDefault(c => c is FigException);

            if (figException != null)
            {
                return figException.Message;
            }

            return "Error: " + string.Join(Environment.NewLine,
                exceptions.AsEnumerable().Reverse()
                .Select(c => $"{c.Message} on {c.StackTrace}"));
        }

        private static void ConsoleWrite(string message, params object[] args)
        {
            if (string.IsNullOrEmpty(message))
                return;

            // Nunca tratar `message` como composite format string quando não há args:
            // conteúdo buscado (Jira/ADF, JSON, corpos com `{`) faria Console.WriteLine
            // lançar FormatException. Só interpreta placeholders quando args foram passados.
            if (args == null || args.Length == 0)
                Console.WriteLine(message);
            else
                Console.WriteLine(message, args);
        }
    }
}
